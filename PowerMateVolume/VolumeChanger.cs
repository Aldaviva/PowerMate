using CSCore.CoreAudioAPI;

namespace PowerMateVolume;

/// <summary>
/// Change the volume of the default Windows output device.
/// </summary>
public interface IVolumeChanger: IDisposable {

    /// <summary>
    /// <para>How much the volume should change for each increment or decrement.</para>
    /// <para>Defaults to <c>0.01</c>, or 1 percentage point.</para>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException" accessor="set">if the value is outside of the range <c>(0, 1]</c></exception>
    float VolumeIncrement { get; set; }

    /// <summary>
    /// <para>Increase or decrease the volume by 1 or more increments of size <see cref="VolumeIncrement"/>.</para>
    /// <para>To decrease the volume, pass a negative <paramref name="increments"/>.</para>
    /// </summary>
    /// <param name="increments"></param>
    void IncreaseVolume(int increments = 1);

    /// <summary>
    /// Get or set the default audio output device's volume.
    /// </summary>
    /// <value>The absolute volume level, in the range <c>[0, 1]</c>. Will be clipped if it's set to a value outside that range.</value>
    float Volume { get; set; }

    /// <summary>
    /// <para>If the default audio output device is not currently muted, then mute it. Otherwise, unmute it.</para>
    /// </summary>
    void ToggleMute();

    /// <summary>
    /// Mute or unmute the default audio output device, or get whether or not it's currently muted.
    /// </summary>
    /// <value><see langword="true"/> if the device is or should be muted, or <see langword="false"/> if it is or should be unmuted.</value>
    bool Muted { get; set; }

}

public class VolumeChanger: IVolumeChanger {

    private readonly MMDeviceEnumerator _mmDeviceEnumerator = new();

    private MMDevice?            _audioOutputEndpoint;
    private AudioEndpointVolume? _audioOutputVolume;

    private float _volumeIncrement = 0.01f;

    /// <inheritdoc />
    public float VolumeIncrement {
        get => _volumeIncrement;
        set {
            if (value is not (> 0 and <= 1)) {
                throw new ArgumentOutOfRangeException(nameof(value), "must be in the range (0, 1]");
            }

            _volumeIncrement = value;
        }
    }

    public VolumeChanger() {
        _mmDeviceEnumerator.DefaultDeviceChanged += onDefaultDeviceChanged;
        AttachToDevice();
    }

    private void AttachToDevice(MMDevice? newAudioEndpoint = null) {
        DetachFromCurrentDevice();
        try {
            _audioOutputEndpoint = newAudioEndpoint ?? _mmDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        } catch (CoreAudioAPIException) {
            // program started when there were no audio output devices connected
            // leave _defaultAudioEndpoint null and wait for onDefaultDeviceChanged to update it when a device is connected
            return;
        }

        _audioOutputVolume = AudioEndpointVolume.FromDevice(_audioOutputEndpoint);
    }

    private void DetachFromCurrentDevice() {
        _audioOutputVolume?.Dispose();
        _audioOutputVolume = null;
        _audioOutputEndpoint?.Dispose();
        _audioOutputEndpoint = null;
    }

    /// <inheritdoc />
    public void IncreaseVolume(int increments = 1) {
        if (_audioOutputVolume is not null && increments != 0) {
            Volume = _audioOutputVolume.MasterVolumeLevelScalar + VolumeIncrement * increments;
            // Console.WriteLine($"Set volume to {newVolume:P2}");
        }
    }

    /// <inheritdoc />
    public float Volume {
        get => _audioOutputVolume?.MasterVolumeLevelScalar ?? 0;
        set {
            float newVolume = Math.Max(0, Math.Min(1, value));
            if (_audioOutputVolume != null) {
                _audioOutputVolume.MasterVolumeLevelScalar = newVolume;
            }
        }
    }

    /// <inheritdoc />
    public void ToggleMute() {
        if (_audioOutputVolume is not null) {
            Muted = !Muted;
        }
    }

    /// <inheritdoc />
    public bool Muted {
        get => _audioOutputVolume?.IsMuted ?? true;
        set {
            if (_audioOutputVolume is not null) {
                _audioOutputVolume.IsMuted = value;
            }
        }
    }

    private void onDefaultDeviceChanged(object? sender, DefaultDeviceChangedEventArgs eventArgs) {
        if (eventArgs is { DataFlow: DataFlow.Render or DataFlow.All, Role: Role.Multimedia }) {
            eventArgs.TryGetDevice(out MMDevice? newDefaultAudioEndpoint);

            if (newDefaultAudioEndpoint is not null) {
                AttachToDevice(newDefaultAudioEndpoint);
            }
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            DetachFromCurrentDevice();
            _mmDeviceEnumerator.DefaultDeviceChanged -= onDefaultDeviceChanged;
            _mmDeviceEnumerator.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}