using System.ComponentModel;

namespace PowerMate;

/// <summary>
/// <para>Listen for events from a connected Griffin PowerMate.</para>
/// <para>To get started, construct a new instance of <see cref="PowerMateClient"/>.</para>
/// <para>Once you have constructed an instance, you can subscribe to <see cref="InputReceived"/> events to be notified when the PowerMate knob is rotated, pressed, or released.</para>
/// <para>You can also subscribe to <see cref="IsConnectedChanged"/> or <see cref="INotifyPropertyChanged.PropertyChanged"/> to be notified when it connects or disconnects from a PowerMate.</para>
/// <para>Remember to dispose of this instance when you're done using it by calling <see cref="IDisposable.Dispose"/>, or with a <see langword="using" /> statement or declaration.</para>
/// </summary>
public interface IPowerMateClient: IDisposable, INotifyPropertyChanged {

    /// <summary>
    /// <para><see langword="true" /> if the client is currently connected to a PowerMate device, or <see langword="false" /> if it is disconnected, possibly because there is no PowerMate device
    /// plugged into the computer.</para>
    /// <para><see cref="PowerMateClient"/> will automatically try to connect to a PowerMate device when you construct a new instance, so you don't have to call any additional methods in order to make 
    /// it start connecting.</para>
    /// <para>If a PowerMate device is plugged in, <see cref="IsConnected"/> will already be <see langword="true" /> by the time the <see cref="PowerMateClient"/> constructor returns.</para>
    /// <para>To receive notifications when this property changes, you can subscribe to the <see cref="IsConnectedChanged"/> or <see cref="INotifyPropertyChanged.PropertyChanged"/> events.</para>
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Fired whenever the PowerMate knob is rotated, pressed, or released.
    /// </summary>
    event EventHandler<PowerMateInput>? InputReceived;

    /// <summary>
    /// <para>Fired whenever the connection state of the PowerMate changes. Not fired when constructing or disposing the <see cref="PowerMateClient"/> instance.</para>
    /// <para>The event argument contains the new value of <see cref="IsConnected"/>.</para>
    /// <para>This value can also be accessed at any time by reading the <see cref="IsConnected"/> property.</para>
    /// <para>If you want to use data binding which expects <see cref="INotifyPropertyChanged.PropertyChanged"/> events, <see cref="IPowerMateClient"/> also implements
    /// <see cref="INotifyPropertyChanged"/>, so you can use that event instead.</para>
    /// </summary>
    event EventHandler<bool>? IsConnectedChanged;

    /// <summary>
    /// <see cref="SynchronizationContext"/> on which to run event callbacks. Useful if your delegates need to update a user interface on the main thread. Callbacks run on the current thread by
    /// default.
    /// </summary>
    SynchronizationContext EventSynchronizationContext { get; set; }

}