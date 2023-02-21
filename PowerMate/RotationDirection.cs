namespace PowerMate;

/// <summary>
/// Which direction, if any, the knob on the PowerMate was rotated, as viewed from above (from the shiny knob side, not from the LED base side).
/// </summary>
public enum RotationDirection {

    /// <summary>
    /// The knob was not rotated, it was only pressed or released straight up and down. <see cref="PowerMateInput.RotationDistance"/> will be <c>0</c>.
    /// </summary>
    None,

    /// <summary>
    /// The knob was rotated clockwise when viewed from above. The positive distance it was rotated is available in <see cref="PowerMateInput.RotationDistance"/>.
    /// </summary>
    Clockwise,

    /// <summary>
    /// The knob was rotated counterclockwise when viewed from above. The positive distance it was rotated is available in <see cref="PowerMateInput.RotationDistance"/>.
    /// </summary>
    Counterclockwise

}