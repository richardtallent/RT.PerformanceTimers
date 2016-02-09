using System;

namespace RT {

	/// <summary>
	/// http://msdn.microsoft.com/en-us/library/ms182161.aspx
	/// </summary>
	[System.Security.SuppressUnmanagedCodeSecurity] internal static class SafeNativeMethods {
	
		/// <summary>
		/// Provides a much higher-resolution timer than Environment.TickCount
		/// </summary>
		[System.Runtime.InteropServices.DllImport("Kernel32.dll")] public static extern bool QueryPerformanceCounter(out long perfcount);		

		/// <summary>
		/// Returns the number of QueryPerformanceCounter() values per second
		/// </summary>
		[System.Runtime.InteropServices.DllImport("Kernel32.dll")] public static extern bool QueryPerformanceFrequency(out long freq);

	}

	/// <summary>
	/// This timer class can be used to test the performance of critical code blocks with
	/// low overhead and high precision. Sets of PerformanceTimers should be implemented
	/// as properties of a class inheriting from PerformanceTimers.
	/// 
	/// Note that the Start() / Finish() methods are not thread-safe -- timer instances
	/// shared between threads should only be called using the Time() method so overlapping
	/// timer calls won't impact one another.
	/// </summary>
	public sealed class PerformanceTimer {
		
		/// <summary>
		/// Preserves the start time when Start() is called, and is used only by Finish().
		/// </summary>
		private long _lastStartTime;
		private long _lastFinishTime;

		/// <summary>
		/// Optional, allows this to be easily emitted to a database, web page, etc.
		/// </summary>
		public string Name			{ get; set; }

		/// <summary>
		/// Total run time (in ticks) of this timer instance so far (using Start() or Time()).
		/// </summary>
		public long TotalDuration	{ get; private set; }

		/// <summary>
		/// Total number of times this timer has been *started* so far (using Start() or Time()).
		/// </summary>
		public int Count			{ get; private set; }

		// Utility read-only properties for display of statistics
		public long AverageDuration		{ get { return Count==0 ? 0 : TotalDuration / Count; } }
		public long AverageDurationMS	{ get { return Count==0 ? 0 : TotalDurationMS / Count; } }
		public long TotalDurationMS		{ get { return ConvertToMS(TotalDuration); } }

		public long LastDurationMS		{ get { return ConvertToMS(_lastFinishTime - _lastStartTime); } }

		/* *************************************************************************** */
		// Constructor and Public methods.
		/* *************************************************************************** */

		public PerformanceTimer(string name = null) {
			this.Name = name;
		}

		/// <summary>
		/// Starts the timer and increments the counter. 
		/// </summary>
		public void Start() {
			if( (_lastStartTime > 0) && (_lastFinishTime == _lastStartTime) )
			{
				// Timer did not finish, was called again, probably recursively.
				// Call Finish() so the timing is cumulative, and don't increment
				// the count.
				Finish();
				_lastStartTime = _lastFinishTime;		// Avoid another system call, make timing continuous
			} else {
				Count++;
				_lastStartTime = GetCurrentTimerValue();
			}
			_lastFinishTime = _lastStartTime;
		}

		/// <summary>
		/// Adds the number of ticks since the most recent Start() call.
		/// </summary>
		public void Finish() {
			_lastFinishTime = GetCurrentTimerValue();
			TotalDuration += _lastFinishTime - _lastStartTime;
		}

		/// <summary>
		/// Alternative method to separate Start/Finish calls. Use only with complex blocks of code with
		/// few iterations, otherwise the overhead of passing the delegate action may be significant.
		/// For performance, this inlines its own version of Start/Finish that doesn't rely on _lastStartTime / _lastFinishTime.
		/// Returns itself to allow for fluid calls.
		/// Usage:
		///		myTimer.Time( () => { Dostuff; } );
		/// </summary>
		public PerformanceTimer Time(Action action) {
			Count++;
			var start = GetCurrentTimerValue();
			action();
			TotalDuration += GetCurrentTimerValue() - start;
			return this;
		}

		/// <summary>
		/// Resets everything but the name.
		/// </summary>
		public void Reset() {
			_lastStartTime = 0;
			_lastFinishTime = 0;
			TotalDuration = 0;
			Count = 0;
		}

		/* *************************************************************************** */
		// Private methods
		/* *************************************************************************** */

		private static long GetCurrentTimerValue() {
			long result;
			SafeNativeMethods.QueryPerformanceCounter(out result);
			return result;
		}

		private static long ConvertToMS(long duration) {
			long ticksPerSecond;
			SafeNativeMethods.QueryPerformanceFrequency(out ticksPerSecond);
			long ticksPerMS = ticksPerSecond / 1000L;
			return duration / ticksPerMS;
		}

	}

}