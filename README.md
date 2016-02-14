# RT.PerformanceTimers

I don't have the fancier versions of Visual Studio with built-in instrumentation, so I wanted to create a *simple*, efficient tool for tracking performance of various bits of code. This library is the result.

## How to Use It

It could not be any simpler:

```
var t = new PerformanceTimer("MyTimer");
t.Start();
// Do stuff...
t.Finish();

Console.WriteLine(t.Name + ": " + t.Count.ToString() + " calls in " + t.TotalDurationMS().ToString() + "ms.");
```

If you have a group of timers, you can store them in a class derived from `RT.PerformanceTimers`, which allows you to use reflection to iterate through them:

```
class MyTimers : RT.PerformanceTimers {
    public readonly RT.PerformanceTimer MyTimer1 = new RT.PerformanceTimer();
    public readonly RT.PerformanceTimer MyTimer2 = new RT.PerformanceTimer();
}
...

var timers = new MyTimers();

timers.MyTimer1.Start();
// Do stuff...
timers.MyTimer1.Finish();

timers.MyTimer2.Start();
// Do other stuff...
timers.MyTimer2.Finish();

foreach(var t in timers) {
	Console.WriteLine(t.Name + ": " + t.Count.ToString() + " calls in " + t.TotalDurationMS().ToString() + "ms.");
}
```

Originally I used a `Dictionary<string, PerformanceTimer>` for my collections of timers, but for my use case, the overhead of dictionary hash lookups added far too much overhead to my code. At the same time, I didn't want to lose the semantics of being able to easily loop through the timers in the collection (in my case, usually to write them to a database log). Yes, reflection has some overhead as well, but it's only used to process the results, not for the timing operations themselves.

Incidentally, I used `Finish()` rather than `Stop()` simply because it makes Intellisense faster by two keystrokes. :)

In addition to using explicit Start/Finish calls, you can use the `Time(Action action)` method to directly time a specific call.

```
myTimer.Time( () => { 
	// Do stuff
});
```

I've released this code under an MIT license because I thought it might help someone else. The first version of this was written a *VERY LONG* time ago (before .NET 2.0), so I've just recently switched it to use the `Stopwatch` class rather than relying on direct calls to the Windows system timer. Frankly, I could just use that class if it tracked the number of times `Start()` was called, but it does not.

I have some other code (implemented as extension methods) to send the results of an `RT.PerformanceTimers` instance to JSON or a database table, but those bits are so specific to my own stack that I didn't include them here.

I have no grand plans for expanding this library, it serves my needs, but if you have any suggestions, please open an issue here on GitHub. Thanks!
