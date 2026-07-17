using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace OffersService.Tests.Helpers;

public class DbCommandCounterInterceptor : DbCommandInterceptor
{
    private readonly Lock _sync = new();
    private readonly List<string> _commandTexts = [];
    private int _executedCount;

    public int ExecutedCount => Volatile.Read(ref _executedCount);

    public IReadOnlyList<string> CommandTexts
    {
        get
        {
            lock (_sync)
            {
                return _commandTexts.ToList();
            }
        }
    }

    /// <summary>
    /// Clears the captured command count and command text history.
    /// </summary>
    public void Reset()
    {
        Interlocked.Exchange(ref _executedCount, 0);
        lock (_sync)
        {
            _commandTexts.Clear();
        }
    }

    /// <summary>
    /// Records synchronous reader commands before EF Core executes them.
    /// </summary>
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        Increment(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    /// <summary>
    /// Records asynchronous reader commands before EF Core executes them.
    /// </summary>
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Increment(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <summary>
    /// Records synchronous non-query commands before EF Core executes them.
    /// </summary>
    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        Increment(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    /// <summary>
    /// Records asynchronous non-query commands before EF Core executes them.
    /// </summary>
    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Increment(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <summary>
    /// Records synchronous scalar commands before EF Core executes them.
    /// </summary>
    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        Increment(command);
        return base.ScalarExecuting(command, eventData, result);
    }

    /// <summary>
    /// Records asynchronous scalar commands before EF Core executes them.
    /// </summary>
    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        Increment(command);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <summary>
    /// Adds one command execution and stores its SQL text.
    /// </summary>
    private void Increment(DbCommand command)
    {
        Interlocked.Increment(ref _executedCount);
        lock (_sync)
        {
            _commandTexts.Add(command.CommandText);
        }
    }
}
