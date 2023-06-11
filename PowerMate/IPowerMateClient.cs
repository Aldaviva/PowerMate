using System.ComponentModel;
using HidClient;

namespace PowerMate;

/// <summary>
/// <para>Listen for events and control the light on a connected Griffin PowerMate USB device.</para>
/// <para>To get started, construct a new instance of <see cref="PowerMateClient"/>.</para>
/// <para>Once you have constructed an instance, you can subscribe to <see cref="InputReceived"/> events to be notified when the PowerMate knob is rotated, pressed, or released.</para>
/// <para>You can also subscribe to <see cref="IHidClient.IsConnectedChanged"/> or <see cref="INotifyPropertyChanged.PropertyChanged"/> to be notified when it connects or disconnects from a PowerMate.</para>
/// <para>The light is controlled by the <see cref="LightBrightness"/>, <see cref="LightAnimation"/>, and <see cref="LightPulseSpeed"/> properties.</para>
/// <para>Remember to dispose of this instance when you're done using it by calling <see cref="IDisposable.Dispose"/>, or with a <see langword="using" /> statement or declaration.</para>
/// </summary>
public interface IPowerMateClient: IHidClient {

    /// <summary>
    /// Fired whenever the PowerMate knob is rotated, pressed, or released.
    /// </summary>
    event EventHandler<PowerMateInput> InputReceived;

    /// <summary>
    /// <para>Get or set how bright the blue/cyan LED in the base of the PowerMate is, between <c>0</c> (off) and <c>255</c> (brightest), inclusive. When the device is first plugged in, it defaults to
    /// <c>80</c>.</para>
    /// <para>This property does not reflect brightness changes made to the device by other programs running on your computer.</para>
    /// <para>It is safe to set this property even if a device is not connected. If you do, the brightness value will be saved until the device reconnects, when your value will be automatically
    /// reapplied to the device.</para>
    /// <para>Changes to this property will not take effect while the LED is pulsing (i.e. while <see cref="LightAnimation"/> is <see cref="PowerMate.LightAnimation.Pulsing"/>). If you try to set a brightness while it's pulsing, the LED
    /// will continue pulsing until you set <see cref="LightAnimation"/> to <see cref="PowerMate.LightAnimation.Solid"/> or <see cref="PowerMate.LightAnimation.SolidWhileAwakeAndPulsingDuringComputerStandby"/>, at which point the brightness value you tried to set earlier will take effect.</para>
    /// <para>Warning: changing this property very frequently can result in HID errors, so consider throttling your writes.</para>
    /// </summary>
    byte LightBrightness { get; set; }

    /// <summary>
    /// <para>Enables or disables a pulsing animation of the blue/cyan LED in the base of the PowerMate.</para>
    /// <para>By default, this is <see cref="PowerMate.LightAnimation.Solid"/>, and the light shines at a constant brightness controlled by <see cref="LightBrightness"/>.</para>
    /// <para>To enable the pulsing animation, set this to <see cref="PowerMate.LightAnimation.Pulsing"/>, and set <see cref="LightPulseSpeed"/> to control the frequency.</para>
    /// <para>By setting this to <see cref="PowerMate.LightAnimation.SolidWhileAwakeAndPulsingDuringComputerStandby"/>, you can set the light to shine at a constant brightness while the computer which the device is attached to is awake, but change to a pulsing animation when the computer goes to sleep, instead of turning off when the computer goes to sleep.</para>
    /// <para>This property does not reflect animation changes made to the device by other programs running on your computer.</para>
    /// <para>It is safe to set this property even if a device is not connected. If you do, the animation will be saved until the device reconnects, when your value will be automatically
    /// reapplied to the device.</para>
    /// </summary>
    LightAnimation LightAnimation { get; set; }

    /// <summary>
    /// <para>Get or set how fast, if at all, the blue/cyan LED in the base of the PowerMate is flashing, between <c>0</c> (slowest, about 0.03443 Hz, a 29.04 sec period) and <c>24</c> (fastest, about
    /// 15.63 Hz, or a 64 ms period), inclusive. The default value is <c>12</c>. Values you set are clamped to the range [0, 24].</para>
    /// <par>To disable pulsing, set <see cref="LightAnimation"/> to <see cref="PowerMate.LightAnimation.Solid"/>, in which case the light will shine at the constant
    /// <see cref="LightBrightness"/> level.</par>
    /// <para>Changes to this property will not take effect while the LED is solid (i.e. while <see cref="LightAnimation"/> is <see cref="PowerMate.LightAnimation.Solid"/> or <see cref="PowerMate.LightAnimation.SolidWhileAwakeAndPulsingDuringComputerStandby"/>). If you try to set a pulse speed while it's solid, the LED will continue solidly shining until you set <see cref="LightAnimation"/> to <see cref="PowerMate.LightAnimation.Solid"/> or <see cref="PowerMate.LightAnimation.SolidWhileAwakeAndPulsingDuringComputerStandby"/>, at which point the pulse speed you tried to set earlier will take effect.</para>
    /// <para>This property does not reflect pulsing speed changes made to the device by other programs running on your computer.</para>
    /// <para>It is safe to set this property even if a device is not connected. If you do, the pulsing speed value will be saved until the device reconnects, when your value will be automatically
    /// reapplied to the device.</para>
    /// </summary>
    int LightPulseSpeed { get; set; }

}