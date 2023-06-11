using System.ComponentModel;
using System.Runtime.CompilerServices;
using HidClient;
using HidSharp;

namespace PowerMate;

/// <inheritdoc cref="IPowerMateClient" />
public class PowerMateClient: AbstractHidClient, IPowerMateClient {

    private const byte DefaultLightBrightness = 80;

    /// <inheritdoc />
    protected override int VendorId { get; } = 0x077d;

    /// <inheritdoc />
    protected override int ProductId { get; } = 0x0410;

    private byte           _lightBrightness = DefaultLightBrightness;
    private LightAnimation _lightAnimation  = LightAnimation.Solid;
    private int            _lightPulseSpeed = 12;
    private DateTime?      _mostRecentFeatureSetTime;

    /// <inheritdoc />
    public event EventHandler<PowerMateInput>? InputReceived;

    /// <inheritdoc />
    public PowerMateClient() { }

    /// <inheritdoc />
    public PowerMateClient(DeviceList deviceList): base(deviceList) { }

    /// <inheritdoc />
    protected override void OnConnect() {
        LightAnimation = LightAnimation; //resend all pulsing and brightness values to device
    }

    /// <inheritdoc />
    protected override void OnHidRead(byte[] readBuffer) {
        // Console.WriteLine($"Read HID bytes {string.Join(" ", readBuffer.Select(b => $"{b:x2}"))}");
        PowerMateInput input = new(readBuffer);
        EventSynchronizationContext.Post(_ => { InputReceived?.Invoke(this, input); }, null);

        if ((LightAnimation != input.ActualLightAnimation
                || (LightAnimation != LightAnimation.Pulsing && LightBrightness != input.ActualLightBrightness)
                || (LightAnimation == LightAnimation.Pulsing && LightPulseSpeed != input.ActualLightPulseSpeed))
            && _mostRecentFeatureSetTime is not null && DateTime.Now - _mostRecentFeatureSetTime > TimeSpan.FromMilliseconds(500)) {
            Console.WriteLine("Resetting features...");
            LightAnimation = LightAnimation;
        }
    }

    /// <inheritdoc />
    public byte LightBrightness {
        get => _lightBrightness;
        set {
            if (IsConnected && LightAnimation != LightAnimation.Pulsing) {
                SetFeature(PowerMateFeature.LightBrightness, value);
            }

            if (_lightBrightness != value) {
                _lightBrightness = value;
                TriggerPropertyChangedEvent();
            }
        }
    }

    /// <inheritdoc />
    public LightAnimation LightAnimation {
        get => _lightAnimation;
        set {
            if (IsConnected) {
                SetFeature(PowerMateFeature.LightPulseDuringSleepEnabled, Convert.ToByte(value == LightAnimation.SolidWhileAwakeAndPulsingDuringComputerStandby));
                SetFeature(PowerMateFeature.LightPulseAlwaysEnabled, Convert.ToByte(value == LightAnimation.Pulsing));

                if (value != LightAnimation.Solid) {
                    SetFeature(PowerMateFeature.LightPulseSpeed, EncodePulseSpeed(LightPulseSpeed));
                }

                if (value != LightAnimation.Pulsing) {
                    SetFeature(PowerMateFeature.LightBrightness, LightBrightness);
                }
            }

            if (_lightAnimation != value) {
                _lightAnimation = value;
                TriggerPropertyChangedEvent();
            }
        }
    }

    /// <inheritdoc />
    public int LightPulseSpeed {
        get => _lightPulseSpeed;
        set {
            value = Math.Max(0, Math.Min(24, value));

            if (IsConnected && LightAnimation != LightAnimation.Solid) {
                SetFeature(PowerMateFeature.LightPulseSpeed, EncodePulseSpeed(value));
            }

            if (LightPulseSpeed != value) {
                _lightPulseSpeed = value;
                TriggerPropertyChangedEvent();
            }
        }
    }

    // ExceptionAdjustment: M:System.Array.Copy(System.Array,System.Int32,System.Array,System.Int32,System.Int32) -T:System.RankException
    // ExceptionAdjustment: M:System.Array.Copy(System.Array,System.Int32,System.Array,System.Int32,System.Int32) -T:System.ArrayTypeMismatchException
    private void SetFeature(PowerMateFeature feature, params byte[] payload) {
        _mostRecentFeatureSetTime = DateTime.Now;
        byte[] featureData = { 0x00, 0x41, 0x01, (byte) feature, 0x00, /* payload copied here */ 0x00, 0x00, 0x00, 0x00 };
        Array.Copy(payload, 0, featureData, 5, Math.Min(payload.Length, 4));

        try {
            DeviceStream?.SetFeature(featureData);
        } catch (IOException e) {
            if (e.InnerException is Win32Exception { NativeErrorCode: 0 }) {
                // retry once with no delay if we get a "The operation completed successfully" error
                DeviceStream?.SetFeature(featureData);
            }
        }
    }

    /// <param name="pulseSpeed">in the range [0, 24]</param>
    /// <returns>two big-endian bytes to send to the PowerMate to set its pulsing speed</returns>
    private static byte[] EncodePulseSpeed(int pulseSpeed) {
        byte[] encoded = BitConverter.GetBytes((ushort) (pulseSpeed switch {
            < 8 => Math.Min(0x00e, (7 - pulseSpeed) * 2),
            8   => 0x100,
            > 8 => Math.Min(0x220, (pulseSpeed - 8) * 2 + 0x200)
        }));

        if (BitConverter.IsLittleEndian) {
            (encoded[0], encoded[1]) = (encoded[1], encoded[0]);
        }

        // Console.WriteLine($"Encoded pulse speed {pulseSpeed} to {encoded[0]:x2} {encoded[1]:x2}");
        return encoded;
    }

    private void TriggerPropertyChangedEvent([CallerMemberName] string propertyName = "") {
        EventSynchronizationContext.Post(_ => OnPropertyChanged(propertyName), null);
    }

}