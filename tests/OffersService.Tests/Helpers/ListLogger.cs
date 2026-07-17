using Microsoft.Extensions.Logging;

namespace OffersService.Tests.Helpers;

public sealed class ListLogger<T> : ILogger<T>
{
    public List<string> Messages { get; } = [];

    /// <summary>
    /// Returns a no-op logging scope for tests that do not need scoped log state.
    /// </summary>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return NullScope.Instance;
    }

    /// <summary>
    /// Enables every log level so tests can decide which messages to assert.
    /// </summary>
    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    /// <summary>
    /// Captures information-level log messages in memory for assertions.
    /// </summary>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (logLevel == LogLevel.Information)
        {
            Messages.Add(formatter(state, exception));
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        /// <summary>
        /// Completes the no-op scope without releasing any resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
