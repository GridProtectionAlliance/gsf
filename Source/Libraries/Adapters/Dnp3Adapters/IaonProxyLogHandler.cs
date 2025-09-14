using System;
using System.Collections.Generic;
using System.Text;
using Automatak.DNP3.Interface;
using GSF.Diagnostics;
using GSF.TimeSeries.Adapters;

#nullable enable

namespace DNP3Adapters;

/// <summary>
/// Defines the interface for a DNP3 adapter that can be used to process exceptions and status messages.
/// </summary>
public interface IDnp3Adapter : IAdapter
{
    /// <summary>
    /// Raises the <see cref="IAdapter.ProcessException"/> event.
    /// </summary>
    /// <param name="level">The <see cref="MessageLevel"/> to assign to this message</param>
    /// <param name="exception">Processing <see cref="Exception"/>.</param>
    void OnProcessException(MessageLevel level, Exception exception);

    /// <summary>
    /// Raises the <see cref="IAdapter.StatusMessage"/> event and sends this data to the <see cref="Logger"/>.
    /// </summary>
    /// <param name="level">The <see cref="MessageLevel"/> to assign to this message</param>
    /// <param name="status">New status message.</param>
    void OnStatusMessage(MessageLevel level, string status);
}

internal class IaonProxyLogHandler<T> : ILogHandler where T : IDnp3Adapter
{
    /// <summary>
    /// Gets the static adapters that are currently registered with this proxy.
    /// </summary>
    public List<IDnp3Adapter> Adapters { get; } = [];

    /// <summary>
    /// Gets the static status proxy that is used to process exceptions and status messages.
    /// </summary>
    public IDnp3Adapter? StatusProxy { get; private set; }

    /// <summary>
    /// Registers the specified adapter as an available adapter for this proxy.
    /// </summary>
    /// <param name="adapter">Adapter instance to register.</param>
    public void RegisterAdapter(T adapter)
    {
        lock (Adapters)
        {
            // Add adapter to list of available adapters 
            Adapters.Add(adapter);

            // If no adapter has been designated as the status proxy, assign this one
            StatusProxy ??= adapter;
        }
    }

    /// <summary>
    /// Unregisters the specified adapter from the list of available adapters.
    /// </summary>
    /// <param name="adapter">Adapter instance to unregister.</param>
    public void UnregisterAdapter(T adapter)
    {
        lock (Adapters)
        {
            // Remove this adapter from the available list
            Adapters.Remove(adapter);

            // See if we are disposing the status proxy instance
            if (ReferenceEquals(StatusProxy, adapter))
            {
                // Attempt to find a new status proxy
                StatusProxy = Adapters.Count > 0 ? Adapters[0] : null;
            }
        }
    }

    /// <summary>
    /// Handler for log entries.
    /// </summary>
    /// <param name="entry"><see cref="LogEntry"/> to handle.</param>
    public void Log(LogEntry entry)
    {
        // We avoid race conditions by always making sure access to status proxy is locked - this only
        // contends with adapter initialization and disposal so contention will not be the normal case
        lock (Adapters)
        {
            if (StatusProxy is null || StatusProxy.IsDisposed)
                return;

            if ((entry.filter.Flags & LogFilters.ERROR) > 0)
            {
                // Expose errors through exception processor
                InvalidOperationException exception = new(FormatLogEntry(entry));
                StatusProxy.OnProcessException(MessageLevel.Error, exception);
            }
            else
            {
                // For other messages, we just expose as a normal status
                string message = FormatLogEntry(entry);

                if ((entry.filter.Flags & LogFilters.WARNING) > 0)
                    StatusProxy.OnStatusMessage(MessageLevel.Warning, message);
                else if ((entry.filter.Flags & LogFilters.DEBUG) > 0)
                    StatusProxy.OnStatusMessage(MessageLevel.Debug, message);
                else
                    StatusProxy.OnStatusMessage(MessageLevel.Info, message);
            }
        }
    }

    private static string FormatLogEntry(LogEntry entry)
    {
        StringBuilder entryText = new();

        entryText.Append(entry.message);
        entryText.Append($" ({LogFilters.GetFilterString(entry.filter.Flags)})");

        if (!string.IsNullOrWhiteSpace(entry.location))
            entryText.Append($" @ {entry.location}");

        return entryText.ToString();
    }
}
