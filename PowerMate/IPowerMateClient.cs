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

    /// <summary>
    /// <para>Get or set how bright the blue/cyan LED in the base of the PowerMate is, between <c>0</c> (off) and <c>255</c> (brightest), inclusive. When the device is first plugged in, it defaults to <c>80</c>.</para>
    /// <para>This property does not reflect brightness changes by other programs.</para>
    /// <para>It is safe to set this property even if a device is not connected. If you do, the brightness value will be saved until the device reconnects, when your value will be automatically reapplied to the device.</para>
    /// <para>Changes to this property will not take effect while the LED is pulsing (i.e. while <see cref="LedPulseSpeed"/> is non-<see langword="null"/>). If you try to set a brightness while it's pulsing, the LED will continue pulsing until you set <see cref="LedPulseSpeed"/> to <see langword="null"/>, at which point the brightness value you tried to set earlier will take effect.</para>
    /// <para>Warning: changing this property very frequently can result in HID errors, so consider throttling your writes.</para>
    /// </summary>
    byte LedBrightness { get; set; }

    /// <summary>
    /// <para>Get or set how fast, if at all, the blue/cyan LED in the base of the PowerMate is flashing, between <c>0</c> (slowest, about 0.03443 Hz) and <c>24</c> (fastest, about 15.63 Hz), inclusive. You can also set it to <see langword="null"/> to disable pulsing, in which case it will shine at the constant <see cref="LedBrightness"/> level. Values you set are clamped to the range <c>[0, 24]</c>.</para>
    /// <para>This property does not reflect pulsing changes by other programs.</para>
    /// <para>It is safe to set this property even if a device is not connected. If you do, the pulsing speed value will be saved until the device reconnects, when your value will be automatically reapplied to the device.</para>
    /// </summary>
    int? LedPulseSpeed { get; set; }

    /// <summary>
    /// <para><see langword="true"/> if the blue/cyan LED in the base of the PowerMate should pulse when it is attached to a computer which is in a standby power state, or <see langword="false"/> if the LED should turn off instead. Defaults to <see langword="false"/>.</para>
    /// <para>When set to <see langword="true"/>, the pulse speed is the same as that set by <see cref="LedPulseSpeed"/>. To change the sleep pulse speed but not pulse while awake, set <see cref="LedPulseDuringSleep"/> to <see langword="true"/>, then set the desired sleeping pulse speed with <see cref="LedPulseSpeed"/>, adn finally set <see cref="LedPulseSpeed"/> to <see langword="null"/>. This will store the pulse speed to be used during sleep in the device, and also keep a constant brightness while the computer is awake.</para>
    /// </summary>
    bool LedPulseDuringSleep { get; set; }

}