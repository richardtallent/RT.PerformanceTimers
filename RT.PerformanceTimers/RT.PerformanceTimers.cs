using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace RT {

	/// <summary>
	/// Groups of performance timers should inherit from this, add their own Timer fields (ideally read-only)
	/// and reference them directly.
	/// 
	/// This was previously implemented as a Dictionary, but the hash code lookups were far too slow to use
	/// this to time critical code in loops. Enumerating the properties to emit the result is slower than
	/// the dictionary (as it uses reflection), but since this is only called once, when all of the timing
	/// is complete (say, at the end of a page render), the difference is negligible.
	/// 
	/// Note that a PerformanceTimers subclass can also have fields that are instances of *other* subclasses
	/// of PerformanceTimers -- the reflection calls will loop through them when emitting the list of timers.
	/// This allows a long list of timers to be grouped, and allows some composition of groups of timers to
	/// overcome C# limitations of single inheritance.
	/// </summary>
	public class PerformanceTimers {

		/// <summary>
		/// Intended to be used when this instance is a property of another PerformanceTimers instance,
		/// allows all of the timers to be emitted with a common name prefix.
		/// </summary>
		public string NamePrefix { get; set; } = string.Empty;

		public void Reset() {
			foreach(var timer in Timers) timer.Reset();
		}

		/// <summary>
		/// Returns a list of all PerformanceTimer fields that are either members of this class
		/// or are fields of PerformanceTimer-derived fields. This recurses, allowing PerformanceTimers
		/// subclasses to have PerformanceTimers fields, which can then also have PerformanceTimers
		/// fields, etc. Implemented as IReadOnlyCollection<> because AFAIK C# doesn't currently 
		/// support multiple yield return statements to allow for an IEnumerable<> implementation.
		/// </summary>
		public IReadOnlyCollection<PerformanceTimer> Timers {
			get {
				// Initialize the list with the PerformanceTimer fields
				var timers = new List<PerformanceTimer>(MemberTimers);
				// Loop through PerformanceTimers-derived fields to add additional timers from them.
				var listsOfTimers = from timerGroup in MemberTimerGroups select timerGroup.Timers;
				foreach(var otherTimerList in listsOfTimers) {
					timers.AddRange(otherTimerList);
				}
				// Return a collection that the caller can't modify.
				return timers.AsReadOnly();
			}
		}

		/// <summary>
		/// Private only, returns the fields of this class that are of type PerformanceTimer
		/// </summary>
		private IEnumerable<PerformanceTimer> MemberTimers {
			get {
				return this.GetType().GetTypeInfo().DeclaredFields
					.Where(fieldinfo => fieldinfo.FieldType == typeof(PerformanceTimer))
					.Select(fieldinfo => (PerformanceTimer)fieldinfo.GetValue(this));
			}
		}

		/// <summary>
		/// Private only, returns the fields of this class that are *subclasses* of type PerformanceTimers.
		/// </summary>
		private IEnumerable<PerformanceTimers> MemberTimerGroups {
			get {
				return this.GetType().GetTypeInfo().DeclaredFields
					.Where(fieldinfo => typeof(PerformanceTimers).IsAssignableFrom(fieldinfo.FieldType))
					.Select(fieldinfo => (PerformanceTimers)fieldinfo.GetValue(this));
			}
		}

	}

}