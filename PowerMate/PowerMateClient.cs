using HidSharp;

namespace PowerMate;

/// <inheritdoc />
public class PowerMateClient: IPowerMateClient {

    private const int PowerMateVendorId  = 0x077d;
    private const int PowerMateProductId = 0x0410;
    private const int MessageLength      = 7;

    private readonly DeviceList _deviceList;
    private readonly object     _hidStreamLock = new();

    private bool _isConnected;

    public bool IsConnected {
        get => _isConnected;
        private set {
            if (value != _isConnected) {
                _isConnected = value;
                EventSynchronizationContext.Post(_ => IsConnectedChanged?.Invoke(this, value), null);
            }
        }
    }

    public event EventHandler<bool>? IsConnectedChanged;

    /// <inheritdoc />
    public event EventHandler<PowerMateInput>? InputReceived;

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
        // Console.WriteLine("Device list changed, reattaching...");
        AttachToDevice();
    }

    private void AttachToDevice() {
        bool isNewStream = false;
        lock (_hidStreamLock) {
            if (_hidStream == null) {
                HidDevice? newDevice = _deviceList.GetHidDeviceOrNull(PowerMateVendorId, PowerMateProductId);
                if (newDevice != null) {
                    // Console.WriteLine($"Attach to device {newDevice.GetFriendlyName() ?? "null"}");
                    _hidStream  = newDevice.Open();
                    isNewStream = true;
                }
            }
        }

        if (_hidStream != null && isNewStream) {
            // Console.WriteLine("Registered _hidStream.Closed event handler");
            _hidStream.Closed        += ReattachToDevice;
            _hidStream.ReadTimeout   =  Timeout.Infinite;
            _cancellationTokenSource =  new CancellationTokenSource();

            try {
                Task.Factory.StartNew(HidReadLoop, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            } catch (TaskCanceledException) { }

            IsConnected = true;
        }

        // Console.WriteLine("Done attaching to device");
    }

    private async Task HidReadLoop() {
        CancellationToken cancellationToken = _cancellationTokenSource!.Token;

        try {
            byte[] readBuffer = new byte[MessageLength];
            while (!cancellationToken.IsCancellationRequested) {
                int readBytes = await _hidStream!.ReadAsync(readBuffer, 0, readBuffer.Length, cancellationToken);
                if (readBuffer.Length == readBytes) {
                    PowerMateInput powerMateInput = new(readBuffer);
                    EventSynchronizationContext.Post(_ => { InputReceived?.Invoke(this, powerMateInput); }, null);
                }
            }
        } catch (IOException) {
            // Console.WriteLine("PowerMate disconnected, reconnecting...");
            ReattachToDevice();
        }
    }

    private void ReattachToDevice(object? sender = null, EventArgs? e = null) {
        bool disconnected = false;
        lock (_hidStreamLock) {
            // Console.WriteLine("Reattaching to device");
            if (_hidStream != null) {
                _hidStream.Closed -= ReattachToDevice;
                _hidStream.Close();
                _hidStream.Dispose();
                // Console.WriteLine("ReattachToDevice: setting _hidstream to null");
                _hidStream   = null;
                disconnected = true;
            }
        }

        if (disconnected) {
            IsConnected = false;
        }

        try {
            _cancellationTokenSource?.Cancel();
        } catch (AggregateException) { }

        AttachToDevice();
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            // Console.WriteLine("Dispose() called");
            try {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
            } catch (AggregateException) { }

            lock (_hidStreamLock) {
                if (_hidStream != null) {
                    // Console.WriteLine("Dispose: unregistering _hidStream.Closed event handler");
                    _hidStream.Closed -= ReattachToDevice;
                    // Console.WriteLine("Dispose: closing _hidstream");
                    _hidStream.Close();
                    // Console.WriteLine("Dispose: disposing _hidstream");
                    _hidStream.Dispose();
                    _hidStream = null;
                }
            }

            _deviceList.Changed -= onDeviceListChanged;
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}