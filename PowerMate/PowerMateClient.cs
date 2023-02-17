using System.Diagnostics;
using HidSharp;

namespace PowerMate;

/// <inheritdoc />
public class PowerMateClient: IPowerMateClient {

    private const int PowerMateVendorId  = 0x077d;
    private const int PowerMateProductId = 0x0410;
    private const int MessageLength      = 7;

    private readonly DeviceList _deviceList;

    /// <inheritdoc />
    public event EventHandler<PowerMateEvent>? OnInput;

    /// <inheritdoc />
    public SynchronizationContext EventSynchronizationContext { get; set; } = SynchronizationContext.Current ?? new SynchronizationContext();

    private CancellationTokenSource? _cancellationTokenSource;
    private HidStream?               _hidStream;

    public PowerMateClient(): this(DeviceList.Local) { }

    internal PowerMateClient(DeviceList deviceList) {
        _deviceList         =  deviceList;
        _deviceList.Changed += onDeviceListChanged;
        AttachToDevice();
    }

    private void onDeviceListChanged(object? sender, DeviceListChangedEventArgs e) {
        if (_hidStream == null) {
            AttachToDevice();
        }
    }

    private void AttachToDevice() => AttachToDevice(_deviceList.GetHidDeviceOrNull(PowerMateVendorId, PowerMateProductId));

    private void AttachToDevice(HidDevice? device) {
        _hidStream?.Dispose();
        _hidStream = device?.Open();

        if (_hidStream != null) {
            Trace.WriteLine("PowerMate connected");
            _hidStream.Closed        += ReattachToDevice;
            _hidStream.ReadTimeout   =  Timeout.Infinite;
            _cancellationTokenSource =  new CancellationTokenSource();

            try {
                Task.Factory.StartNew(HidReadLoop, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            } catch (TaskCanceledException) { }
        }
    }

    private async Task HidReadLoop() {
        CancellationToken cancellationToken = _cancellationTokenSource!.Token;

        try {
            while (!cancellationToken.IsCancellationRequested) {
                byte[] readBuffer = new byte[MessageLength];
                int    readBytes  = await _hidStream!.ReadAsync(readBuffer, 0, readBuffer.Length, cancellationToken);
                if (readBuffer.Length == readBytes) {
                    Console.WriteLine($"Received HID bytes: {string.Join(string.Empty, readBuffer.Select(b => $"{b:X2}"))}");
                    PowerMateEvent powerMateEvent = new(readBuffer);
                    EventSynchronizationContext.Post(_ => { OnInput?.Invoke(this, powerMateEvent); }, null);
                }
            }
        } catch (IOException) {
            Trace.WriteLine("PowerMate disconnected");
            ReattachToDevice();
        }
    }

    private void ReattachToDevice(object? sender = null, EventArgs? e = null) {
        if (_hidStream != null) {
            _hidStream.Closed -= ReattachToDevice;
            _hidStream.Close();
            _hidStream.Dispose();
            _hidStream = null;
        }

        try {
            _cancellationTokenSource?.Cancel();
        } catch (AggregateException) { }

        AttachToDevice();
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            try {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
            } catch (AggregateException) { }

            if (_hidStream != null) {
                _hidStream.Closed -= ReattachToDevice;
                _hidStream.Close();
                _hidStream.Dispose();
                _hidStream = null;
            }

            _deviceList.Changed -= onDeviceListChanged;
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}