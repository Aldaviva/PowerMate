namespace PowerMate;

/// <summary>
/// Different modes for how the blue/cyan LED in the base of the PowerMate should animate. The default value is <see cref="Solid"/>. You can change this by setting <see cref="IPowerMateClient.LightAnimation"/>.
/// </summary>
public enum LightAnimation {

    /// <summary>
    /// <para>The PowerMate LED will shine at a constant brightness, controlled by <see cref="IPowerMateClient.LightBrightness"/>.</para>
    /// <para>When the computer to which the PowerMate is connected goes to sleep, the light will turn off.</para>
    /// </summary>
    Solid,

    /// <summary>
    /// <para>The light will get brighter and dimmer in a cyclical animation, the frequency of which is controlled by <see cref="IPowerMateClient.LightPulseSpeed"/>.</para>
    /// <para>When the computer to which the PowerMate is connected goes to sleep, the light will turn off.</para>
    /// </summary>
    Pulsing,

    /// <summary>
    /// <para>The PowerMate LED will shine at a constant brightness, controlled by <see cref="IPowerMateClient.LightBrightness"/>.</para>
    /// <para>When the computer to which the PowerMate is connected goes to sleep, the light will get brighter and dimmer in a cyclical animation, the frequency of which is controlled by <see cref="IPowerMateClient.LightPulseSpeed"/>.</para>
    /// </summary>
    SolidWhileAwakeAndPulsingDuringComputerStandby

}