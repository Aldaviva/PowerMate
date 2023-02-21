namespace PowerMate;

/// <summary>
/// Different modes for how the blue/cyan LED in the base of the PowerMate should animate. The default value is <see cref="Solid"/>. You can change this by setting <see cref="IPowerMateClient.LightAnimation"/>.
/// </summary>
public enum LightAnimation {

    /// <summary>
    /// The light will shine at a constant brightness, controlled by <see cref="IPowerMateClient.LightBrightness"/>.
    /// </summary>
    Solid,

    /// <summary>
    /// The light will get brighter and dimmer in a cyclical animation, the frequency of which is controlled by <see cref="IPowerMateClient.LightPulseSpeed"/>.
    /// </summary>
    Pulsing,

    /// <summary>
    /// While the computer to which the PowerMate is attached is awake, use <see cref="Solid"/>, but while it is asleep, use <see cref="Pulsing"/>.
    /// </summary>
    SolidWhileAwakeAndPulsingDuringComputerStandby

}