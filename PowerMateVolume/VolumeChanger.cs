using CSCore.CoreAudioAPI;

namespace PowerMateVolume;

/// <summary>
/// Change the volume of the default Windows output device.
/// </summary>
public interface VolumeChanger: IDisposable {

    /// <summary>
    /// <para>How much the volume should change for each increment or decrement.</para>
    /// <para>Defaults to <c>0.01</c>, or 1 percentage point.</para>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException" accessor="set">if the value is outside of the range <c>(0, 1]</c></exception>
    float volumeIncrement { get; set; }

    /// <summary>
    /// <para>Increase or decrease the volume by 1 or more increments of size <see cref="volumeIncrement"/>.</para>
    /// <para>To decrease the volume, pass a negative <paramref name="increments"/>.</para>
    /// </summary>
    /// <param name="increments"></param>
    void increaseVolume(int increments = 1);

    /// <summary>
    /// <para>If the default audio output device is not currently muted, then mute it. Otherwise, unmute it.</para>
    /// </summary>
    void toggleMute();

}

public class VolumeChangerImpl: VolumeChanger {

    private readonly MMDeviceEnumerator mmDeviceEnumerator = new();

    private MMDevice?            defaultAudioEndpoint;
    private AudioEndpointVolume? audioEndpointVolume;

    private float _volumeIncrement = 0.01f;

    /// <inheritdoc />
    public float volumeIncrement {
        get => _volumeIncrement;
        set {
            if (value is not (> 0 and <= 1)) {
                throw new ArgumentOutOfRangeException(nameof(value), "must be in the range (0, 1]");
            }

            _volumeIncrement = value;
        }
    }

    public VolumeChangerImpl() {
        mmDeviceEnumerator.DefaultDeviceChanged += onDefaultDeviceChanged;
        attachToDefaultDevice();
    }

    private void attachToDefaultDevice(MMDevice? newDefaultAudioEndpoint = null) {
        detachFromDefaultDevice();
        defaultAudioEndpoint = newDefaultAudioEndpoint ?? mmDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        audioEndpointVolume  = AudioEndpointVolume.FromDevice(defaultAudioEndpoint);
    }

    private void detachFromDefaultDevice() {
        audioEndpointVolume?.Dispose();
        audioEndpointVolume = null;
        defaultAudioEndpoint?.Dispose();
        defaultAudioEndpoint = null;
    }

    public void increaseVolume(int increments = 1) {
        if (audioEndpointVolume is not null && increments != 0) {
            float newVolume = Math.Max(0, Math.Min(1, audioEndpointVolume.MasterVolumeLevelScalar + volumeIncrement * increments));
            audioEndpointVolume.MasterVolumeLevelScalar = newVolume;
            Console.WriteLine($"Set volume to {newVolume:P2}");
        }
    }

    public void toggleMute() {
        if (audioEndpointVolume is not null) {
            audioEndpointVolume.IsMuted ^= true;
        }
    }

    private void onDefaultDeviceChanged(object? sender, DefaultDeviceChangedEventArgs eventArgs) {
        if (eventArgs is { DataFlow: DataFlow.Render or DataFlow.All, Role: Role.Multimedia }) {
            eventArgs.TryGetDevice(out MMDevice? newDefaultAudioEndpoint);

            if (newDefaultAudioEndpoint is not null) {
                attachToDefaultDevice(newDefaultAudioEndpoint);
            }
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            detachFromDefaultDevice();
            mmDeviceEnumerator.DefaultDeviceChanged -= onDefaultDeviceChanged;
            mmDeviceEnumerator.Dispose();
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}