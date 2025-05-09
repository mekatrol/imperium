﻿namespace Imperium.Server.Options;

public abstract class BackgroundServiceOptions
{
    /// <summary>
    /// The maximum number of consecutive exceptions in the background service
    /// main service loop before it will just exit the service loop (and not run again till a restart).
    /// NOTE: this is only a count of the exception that are propagated to the service loop itself.
    ///       exceptions that are handled within loop method calls are not counted.
    /// </summary>
    public int MaxConsecutiveExceptions { get; set; } = 10;

    /// <summary>
    /// The number of milliseconds to sleep in the loop after an exception occurs.
    /// </summary>
    public int LoopExceptionSleep { get; set; } = 10000;

    /// <summary>
    /// The number of milliseconds to sleep after each iteration of the loop
    /// </summary>
    public int LoopIterationSleep { get; set; } = 500;
}

public class TimerBackgroundServiceOptions : BackgroundServiceOptions
{
    public static string SectionName { get; } = "Timer";
}

public class DeviceControllerBackgroundServiceOptions : BackgroundServiceOptions
{
    public static string SectionName { get; } = "DeviceController";
}

public class FlowExecutorBackgroundServiceOptions : BackgroundServiceOptions
{
    public static string SectionName { get; } = "FlowExecutor";
}
