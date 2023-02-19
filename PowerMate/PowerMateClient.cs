using System.ComponentModel;
using HidSharp;

namespace PowerMate;

/// <inheritdoc />
public class PowerMateClient: IPowerMateClient {

    private const int  PowerMateVendorId    = 0x077d;
    private const int  PowerMateProductId   = 0x0410;
    private const int  MessageLength        = 7;
    private const byte DefaultLedBrightness = 80;

    private readonly object _hidStreamLock = new();

    private DeviceList? _deviceList;
    private bool        _isConnected;
    private byte        _ledBrightness = DefaultLedBrightness;
    private int?        _ledPulseSpeed;
    private bool        _ledPulseDuringSleep;

    /// <inheritdoc />
    public event EventHandler<bool>? IsConnectedChanged;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public event EventHandler<PowerMateInput>? InputReceived;

    /// <inheritdoc />
    public SynchronizationContext EventSynchronizationContext { get; set; } = SynchronizationContext.Current ?? new SynchronizationContext();

    private CancellationTokenSource? _cancellationTokenSource;
    private HidStream?               _hidStream;

    /// <summary>
    /// <para>Constructs a new instance that communicates with a PowerMate device.</para>
    /// <para>Upon construction, the new instance will immediately attempt to connect to any PowerMate connected to your computer. If none are connected, it will wait and connect when one is plugged
    /// in. If a PowerMate disconnects, it will try to reconnect whenever one is plugged in again.</para>
    /// <para>If multiple PowerMate devices are present, it will pick one arbitrarily and connect to it.</para>
    /// <para>Once you have constructed an instance, you can subscribe to <see cref="InputReceived"/> events to be notified when the PowerMate knob is rotated, pressed, or released.</para>
    /// <para>You can also subscribe to <see cref="IsConnectedChanged"/> or <see cref="INotifyPropertyChanged.PropertyChanged"/> to be notified when it connects or disconnects from a PowerMate.</para>
    /// <para>Remember to dispose of this instance when you're done using it by calling <see cref="Dispose()"/>, or with a <see langword="using" /> statement or declaration.</para>
    /// </summary>
    public PowerMateClient(): this(DeviceList.Local) { }

    internal PowerMateClient(DeviceList deviceList) {
        _deviceList         =  deviceList;
        _deviceList.Changed += onDeviceListChanged;
        AttachToDevice();
    }

    /// <inheritdoc />
    public bool IsConnected {
        get => _isConnected;
        private set {
            if (value != _isConnected) {
                _isConnected = value;
                EventSynchronizationContext.Post(_ => {
                    IsConnectedChanged?.Invoke(this, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
                }, null);
            }
        }
    }

    private void onDeviceListChanged(object? sender, DeviceListChangedEventArgs e) {
        AttachToDevice();
    }

    private void AttachToDevice() {
        bool isNewStream = false;
        lock (_hidStreamLock) {
            if (_hidStream == null) {
                HidDevice? newDevice = _deviceList?.GetHidDeviceOrNull(PowerMateVendorId, PowerMateProductId);
                if (newDevice != null) {
                    _hidStream  = newDevice.Open();
                    isNewStream = true;
                }
            }
        }

        if (_hidStream != null && isNewStream) {
            _hidStream.Closed        += ReattachToDevice;
            _hidStream.ReadTimeout   =  Timeout.Infinite;
            _cancellationTokenSource =  new CancellationTokenSource();

            try {
                Task.Factory.StartNew(HidReadLoop, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            } catch (TaskCanceledException) { }

            IsConnected = true;

            SetFeature(PowerMateFeature.LedPulseDuringSleepEnabled, Convert.ToByte(LedPulseDuringSleep));
            SetFeature(PowerMateFeature.LedPulseAlwaysEnabled, Convert.ToByte(LedPulseSpeed != null));
            if (LedPulseSpeed != null) {
                SetFeature(PowerMateFeature.LedPulseSpeed, EncodePulseSpeed(LedPulseSpeed.Value));
            } else {
                SetFeature(PowerMateFeature.LedBrightness, LedBrightness);
            }
        }
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
            ReattachToDevice();
        }
    }

    private void ReattachToDevice(object? sender = null, EventArgs? e = null) {
        bool disconnected = false;
        lock (_hidStreamLock) {
            if (_hidStream != null) {
                _hidStream.Closed -= ReattachToDevice;
                _hidStream.Close();
                _hidStream.Dispose();
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

    /// <inheritdoc />
    public byte LedBrightness {
        get => _ledBrightness;
        set {
            if (IsConnected && LedPulseSpeed == null) {
                SetFeature(PowerMateFeature.LedBrightness, value);
            }

            _ledBrightness = value;
        }
    }

    /// <inheritdoc />
    public int? LedPulseSpeed {
        get => _ledPulseSpeed;
        set {
            int? newSpeed = value;
            if (newSpeed.HasValue) {
                newSpeed = Math.Max(0, Math.Min(24, newSpeed.Value));
            }

            if (IsConnected) {
                if (newSpeed.HasValue != _ledPulseSpeed.HasValue) {
                    SetFeature(PowerMateFeature.LedPulseAlwaysEnabled, Convert.ToByte(newSpeed.HasValue));
                    if (!newSpeed.HasValue) {
                        SetFeature(PowerMateFeature.LedBrightness, _ledBrightness);
                    }
                }

                if (newSpeed.HasValue) {
                    SetFeature(PowerMateFeature.LedPulseSpeed, EncodePulseSpeed(newSpeed.Value));
                }
            }

            _ledPulseSpeed = newSpeed;
        }
    }

    /// <inheritdoc />
    public bool LedPulseDuringSleep {
        get => _ledPulseDuringSleep;
        set {
            if (IsConnected) {
                SetFeature(PowerMateFeature.LedPulseDuringSleepEnabled, Convert.ToByte(value));
            }

            _ledPulseDuringSleep = value;
        }
    }

    /// <exception cref="ArgumentOutOfRangeException">if payload is more than 4 bytes long</exception>
    // ExceptionAdjustment: M:System.Array.CopyTo(System.Array,System.Int32) -T:System.ArrayTypeMismatchException
    // ExceptionAdjustment: M:System.Array.CopyTo(System.Array,System.Int32) -T:System.RankException
    private void SetFeature(PowerMateFeature feature, params byte[] payload) {
        if (payload.Length > 4) {
            throw new ArgumentOutOfRangeException(nameof(payload), $"Payload must be 4 or fewer bytes long, got {payload.Length}");
        }

        byte[] featureData = { 0x00, 0x41, 0x01, (byte) feature, 0x00, /* payload copied here */ 0x00, 0x00, 0x00, 0x00 };
        payload.CopyTo(featureData, 5);

        _hidStream?.SetFeature(featureData);
    }

    internal static byte[] EncodePulseSpeed(int pulseSpeed) {
        byte[] encoded = BitConverter.GetBytes((ushort) (pulseSpeed switch {
            < 0   => 0xe,
            <= 7  => (7 - pulseSpeed) * 2,
            8     => 0x100,
            <= 24 => (pulseSpeed - 8) * 2 + 0x200,
            > 24  => 0x220
        }));

        if (BitConverter.IsLittleEndian) {
            (encoded[0], encoded[1]) = (encoded[1], encoded[0]);
        }

        return encoded;
    }

    /// <summary>
    /// <para>Clean up managed and, optionally, unmanaged resources.</para>
    /// <para>When inheriting from <see cref="PowerMateClient"/>, you should override this method, dispose of your managed resources if <paramref name="disposing"/> is <see langword="true" />, then
    /// free your unmanaged resources regardless of the value of <paramref name="disposing"/>, and finally call this base <see cref="Dispose(bool)"/> implementation.</para>
    /// <para>For more information, see <see url="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose#implement-the-dispose-pattern-for-a-derived-class">Implement
    /// the dispose pattern for a derived class</see>.</para>
    /// </summary>
    /// <param name="disposing">Should be <see langword="false" /> when called from a finalizer, and <see langword="true" /> when called from the <see cref="Dispose()"/> method. In other words, it is
    /// <see langword="true" /> when deterministically called and <see langword="false" /> when non-deterministically called.</param>
    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            try {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            } catch (AggregateException) { }

            lock (_hidStreamLock) {
                if (_hidStream != null) {
                    _hidStream.Closed -= ReattachToDevice;
                    _hidStream.Close();
                    _hidStream.Dispose();
                    _hidStream = null;
                }
            }

            if (_deviceList != null) {
                _deviceList.Changed -= onDeviceListChanged;
                _deviceList         =  null;
            }
        }
    }

    /// <summary>
    /// <para>Disconnect from any connected PowerMate device and clean up managed resources.</para>
    /// <para><see cref="IsConnectedChanged"/> and <see cref="INotifyPropertyChanged.PropertyChanged"/> events will not be emitted if a PowerMate is disconnected during disposal.</para>
    /// <para>Subclasses of <see cref="PowerMateClient"/> should override <see cref="Dispose(bool)"/>.</para>
    /// <para>For more information, see <see url="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/unmanaged">Cleaning Up Unmanaged Resources</see> and
    /// <see url="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose">Implementing a Dispose Method</see>.</para>
    /// </summary>
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}