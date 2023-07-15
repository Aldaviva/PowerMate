using System.Diagnostics.Eventing.Reader;

namespace PowerMateVolume;

public interface IStandbyListener: IDisposable {

    event EventHandler StandingBy;
    event EventHandler Resumed;
    event EventHandler<Exception> FatalError;

}

public class EventLogStandbyListener: IStandbyListener {

    public event EventHandler? StandingBy;
    public event EventHandler? Resumed;
    public event EventHandler<Exception>? FatalError;

    private readonly EventLogWatcher _logWatcher;

    /// <exception cref="EventLogNotFoundException">if the given event log or file was not found</exception>
    /// <exception cref="UnauthorizedAccessException">if the log did not already exist and this program is not running elevated</exception>
    public EventLogStandbyListener() {
        _logWatcher = new EventLogWatcher(new EventLogQuery("System", PathType.LogName, "*[System[Provider/@Name=\"Microsoft-Windows-Kernel-Power\" and (EventID=42 or EventID=107)]]"));

        _logWatcher.EventRecordWritten += onEventRecord;

        try {
            _logWatcher.Enabled = true;
        } catch (EventLogNotFoundException) {
            _logWatcher.Dispose();
            throw;
        } catch (UnauthorizedAccessException) {
            _logWatcher.Dispose();
            throw;
        }
    }

    private void onEventRecord(object? sender, EventRecordWrittenEventArgs e) {
        if (e.EventException is { } exception) {
            FatalError?.Invoke(this, exception);
            Dispose();
        } else {
            using EventRecord? record = e.EventRecord;
            switch (record?.Id) {
                case 42:
                    StandingBy?.Invoke(this, EventArgs.Empty);
                    break;
                case 107:
                    Resumed?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }
    }

    public void Dispose() {
        _logWatcher.Dispose();
        GC.SuppressFinalize(this);
    }

}