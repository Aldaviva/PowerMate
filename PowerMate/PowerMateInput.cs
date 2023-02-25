namespace PowerMate;

/// <summary>
/// Event data describing how the PowerMate knob was rotated, pressed, or released. Emitted by <see cref="IPowerMateClient.InputReceived"/>.
/// </summary>
public readonly struct PowerMateInput {

    /// <summary>
    /// <see langword="true" /> if the knob is being held down, or <see langword="false" /> if it is up. Pressing and releasing the knob will generate two events, with <see langword="true" /> and
    /// <see langword="false" /> in order. Will also be <see langword="true" /> when the knob is rotated while being held down.
    /// </summary>
    public readonly bool IsPressed;

    /// <summary>
    /// <para>Which direction, if any, the knob was rotated when viewed from above (from the shiny knob side, not from the LED base side).</para>
    /// <para>If the knob was not rotated because it was only pressed or released straight up and down, this will be <see cref="PowerMate.RotationDirection.None"/>, and <see cref="RotationDistance"/>
    /// will be <c>0</c>.</para>
    /// </summary>
    public readonly RotationDirection RotationDirection;

    /// <summary>
    /// <para>How far, in arbitrary angular units, the knob was rotated since the last update.</para>
    /// <para>When you rotate the knob slowly, you will receive multiple events, each with this set to <c>1</c>. As you rotate it faster, updates are batched and this number increases to <c>2</c> or
    /// more. The highest value I have seen is <c>8</c>.</para>
    /// <para>This is always non-negative, regardless of the rotation direction; use <see cref="RotationDirection"/> to determine the direction.</para>
    /// <para>If the knob is pressed without being rotated, this will be <c>0</c> and <see cref="RotationDirection"/> will be <see cref="PowerMate.RotationDirection.None"/>.</para>
    /// </summary>
    public readonly uint RotationDistance = 0;

    /// <summary>
    /// Parse input from raw HID bytes.
    /// </summary>
    /// <param name="rawData">7-item byte array from the HID update</param>
    public PowerMateInput(IReadOnlyList<byte> rawData): this(rawData[1] == 0x01, GetRotationDirection(rawData), GetRotationDistance(rawData)) { }

    /// <summary>
    /// Construct a synthetic instance, useful for testing.
    /// </summary>
    /// <param name="isPressed"><see langword="true" /> if the knob is being held down, or <see langword="false" /> if it is up. Pressing and releasing the knob will generate two events, with
    /// <see langword="true" /> and <see langword="false" /> in order. Will also be <see langword="true" /> when the knob is rotated while being held down.</param>
    /// <param name="rotationDirection"><para>Which direction, if any, the knob was rotated when viewed from above (from the shiny knob side, not from the LED base side).</para><para>If the knob was
    /// not rotated because it was only pressed or released straight up and down, this will be <see cref="PowerMate.RotationDirection.None"/>, and <see cref="RotationDistance"/> will be <c>0</c>.
    /// </para></param>
    /// <param name="rotationDistance"><para>How far, in arbitrary angular units, the knob was rotated since the last update.</para><para>When you rotate the knob slowly, you will receive multiple
    /// events, each with this set to <c>1</c>. As you rotate it faster, updates are batched and this number increases to <c>2</c> or more. The highest value I have seen is <c>8</c>.</para><para>This
    /// is always non-negative, regardless of the rotation direction; use <see cref="RotationDirection"/> to determine the direction.</para><para>If the knob is pressed without being rotated, this
    /// will be <c>0</c> and <see cref="RotationDirection"/> will be <see cref="PowerMate.RotationDirection.None"/>.</para></param>
    public PowerMateInput(bool isPressed, RotationDirection rotationDirection, uint rotationDistance) {
        IsPressed         = isPressed;
        RotationDirection = rotationDirection;
        RotationDistance  = rotationDistance;
    }

    private static uint GetRotationDistance(IReadOnlyList<byte> rawData) => (uint) Math.Abs((int) (sbyte) rawData[2]);

    private static RotationDirection GetRotationDirection(IReadOnlyList<byte> rawData) => rawData[2] switch {
        > 0 and < 128 => RotationDirection.Clockwise,
        > 128         => RotationDirection.Counterclockwise,
        0 or _        => RotationDirection.None
    };

    /// <inheritdoc cref="object.Equals(object?)" />
    public bool Equals(PowerMateInput obj) => IsPressed == obj.IsPressed && RotationDirection == obj.RotationDirection && RotationDistance == obj.RotationDistance;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PowerMateInput other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() {
        unchecked {
            int hashCode = IsPressed.GetHashCode();
            hashCode = (hashCode * 397) ^ RotationDirection.GetHashCode();
            hashCode = (hashCode * 397) ^ (int) RotationDistance;
            return hashCode;
        }
    }

    /// <summary>
    /// Determines if the <paramref name="left"/> instance is equal to the <paramref name="right"/> instance.
    /// </summary>
    /// <param name="left">An instance to compare to <paramref name="right"/></param>
    /// <param name="right">An instance to compare to <paramref name="left"/></param>
    /// <returns><see langword="true" /> if <paramref name="left"/> and <paramref name="right"/> are equal, <see langword="false" /> otherwise.</returns>
    public static bool operator ==(PowerMateInput left, PowerMateInput right) => left.Equals(right);

    /// <summary>
    /// Determines if the <paramref name="left"/> instance is equal to the <paramref name="right"/> instance.
    /// </summary>
    /// <param name="left">An instance to compare to <paramref name="right"/></param>
    /// <param name="right">An instance to compare to <paramref name="left"/></param>
    /// <returns><see langword="false" /> if <paramref name="left"/> and <paramref name="right"/> are equal, <see langword="true" /> otherwise.</returns>
    public static bool operator !=(PowerMateInput left, PowerMateInput right) => !left.Equals(right);

    /// <summary>
    /// <para>Examples:</para>
    /// <para><c>Turning clockwise 1 increment while not pressed</c></para>
    /// <para><c>Turning counterclockwise 2 increments while pressed</c></para>
    /// <para><c>Not turning while pressed</c></para>
    /// <para><c>Not turning while not pressed</c></para>
    /// </summary>
    /// <returns>Textual description of this instance</returns>
    public override string ToString() =>
        RotationDirection switch {
            RotationDirection.Clockwise        => "Turning clockwise ",
            RotationDirection.Counterclockwise => "Turning counterclockwise ",
            _                                  => "Not turning "
        } +
        RotationDistance switch { 0 => string.Empty, 1 => "1 increment ", var d => $"{d:N0} increments " } +
        (IsPressed ? "while pressed" : "while not pressed");

}