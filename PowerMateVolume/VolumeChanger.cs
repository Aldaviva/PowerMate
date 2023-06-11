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
    /// <para>If the default audio output device is not currently muted, then mute it. Otherwise, unmute it.</para>
    /// </summary>
    void ToggleMute();

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

    public void IncreaseVolume(int increments = 1) {
        if (_audioOutputVolume is not null && increments != 0) {
            float newVolume = Math.Max(0, Math.Min(1, _audioOutputVolume.MasterVolumeLevelScalar + VolumeIncrement * increments));
            _audioOutputVolume.MasterVolumeLevelScalar = newVolume;
            // Console.WriteLine($"Set volume to {newVolume:P2}");
        }
    }

    public void ToggleMute() {
        if (_audioOutputVolume is not null) {
            _audioOutputVolume.IsMuted ^= true;
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