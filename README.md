# RT.PerformanceTimers

I don't have the fancier versions of Visual Studio with built-in instrumentation, so I wanted to create a *simple*, efficient tool for tracking performance of various bits of code. This library is the result.

This library basically creates a wrapper around the `Stopwatch` class to make it more useful for testing code performance. The primary improvements over `Stopwatch` are that it tracks *how many* times the timer is started, and that it allows you to access the *most recent* elapsed time, not just the overall total.

There is also a mechanism that allows you to create a set of related timers and iterate through them for display or to serialize them to a log without the overhead of dictionary lookups during the code that is being timed.

## Version History
 - 1.0.4	2016		Public release
 - 1.0.5	2017-04-05	Downgraded to csproj, switch to .NET Standard 1.3 (should be compatible with .NET 4.6+ and .NET Core)
 - 1.0.6	2018-06-10	Upgraded dependency for reflection

## How to Use It

It could not be any simpler:

```C#
using RT;
...

var t = new PerformanceTimer("MyTimer");
t.Start();
// Do stuff...
t.Finish();

Console.WriteLine(t.Name + ": " + t.Count.ToString() + " calls in " + t.TotalDurationMS().ToString() + "ms.");
```

The `name` argument in the constructor is optional and can be anything you like. It isn't used by the library itself, it is just there for convenience when you display or log your timers.

If you have a group of timers, you can store them in a class derived from `RT.PerformanceTimers`, which allows you to use reflection to iterate through them:

```C#
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

Originally I used a `Dictionary<string, PerformanceTimer>` for my collections of timers, but for my use case, the dictionary hash lookups added far too much overhead to my code. At the same time, I didn't want to lose the semantics of being able to easily loop through the timers in the collection (in my case, usually to write them to a database log). Yes, reflection has some overhead as well, but it's only used to process the results, not for the timing operations themselves.

Incidentally, I used `Finish()` rather than `Stop()` simply because it makes Intellisense faster by two keystrokes. :)

In addition to using explicit Start/Finish calls, you can use the `Time(Action action)` method to directly time a specific call.

```C#
myTimer.Time( () => { 
	// Do stuff
});
```

I've released this code under an MIT license because I thought it might help someone else.

I have some other code (implemented as extension methods) to send the results of an `RT.PerformanceTimers` instance to JSON or a database table, but those bits are so specific to my own stack that I didn't include them here.

I have no grand plans for expanding this library, it serves my needs, but if you have any suggestions, please open an issue here on GitHub. Thanks!
