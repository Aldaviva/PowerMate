namespace PowerMate;

/// <summary>
/// Event data describing how the PowerMate knob was rotated, pressed, or released.
/// </summary>
public readonly struct PowerMateInput {

    /// <summary>
    /// <see langword="true" /> if the knob is being held down, or <see langword="false" /> if it is up. Pressing and releasing the knob will generate two events, with <see langword="true" /> and
    /// <see langword="false" /> in order. Will also be <see langword="true" /> when the knob is rotated while being held down.
    /// </summary>
    public readonly bool IsPressed;

    /// <summary>
    /// <see langword="true" /> if the knob is being rotated clockwise when viewed from above, <see langword="false" /> if it is being rotated counterclockwise, or <see langword="null" /> if it is not
    /// being rotated.
    /// </summary>
    public readonly bool? IsRotationClockwise;

    /// <summary>
    /// <para>How far, in arbitrary angular units, the knob was rotated since the last update.</para>
    /// <para>When you rotate the knob slowly, you will receive multiple events, each with this set to <c>1</c>. As you rotate it faster, updates are batched and this number increases to <c>2</c> or
    /// more. The highest value I have seen is <c>8</c>.</para>
    /// <para>This is always non-negative, regardless of the rotation direction; use <c>IsRotationClockwise</c> to determine the direction.</para>
    /// <para>If the knob is pressed without being rotated, this is <c>0</c>.</para>
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
    /// <see langword="true" /> and
    /// <see langword="false" /> in order. Will also be <see langword="true" /> when the knob is rotated while being held down.</param>
    /// <param name="isRotationClockwise"><see langword="true" /> if the knob is being rotated clockwise when viewed from above, <see langword="false" /> if it is being rotated counterclockwise, or
    /// <see langword="null" /> if it is not
    /// being rotated.</param>
    /// <param name="rotationDistance"><para>How far, in arbitrary angular units, the knob was rotated since the last update.</para>
    /// <para>When you rotate the knob slowly, you will receive multiple events, each with this set to <c>1</c>. As you rotate it faster, updates are batched and this number increases to <c>2</c> or
    /// more. The highest value I have seen is <c>8</c>.</para>
    /// <para>This is always non-negative, regardless of the rotation direction; use <c>IsRotationClockwise</c> to determine the direction.</para>
    /// <para>If the knob is pressed without being rotated, this is <c>0</c>.</para></param>
    public PowerMateInput(bool isPressed, bool? isRotationClockwise, uint rotationDistance) {
        IsPressed           = isPressed;
        IsRotationClockwise = isRotationClockwise;
        RotationDistance    = rotationDistance;
    }

    private static uint GetRotationDistance(IReadOnlyList<byte> rawData) => (uint) Math.Abs((int) (sbyte) rawData[2]);

    private static bool? GetRotationDirection(IReadOnlyList<byte> rawData) => rawData[2] switch {
        > 0 and < 128 => true,
        > 128         => false,
        0 or _        => null
    };

    /// <inheritdoc cref="object.Equals(object?)" />
    public bool Equals(PowerMateInput obj) {
        return IsPressed == obj.IsPressed && IsRotationClockwise == obj.IsRotationClockwise && RotationDistance == obj.RotationDistance;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        return obj is PowerMateInput other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode() {
        unchecked {
            int hashCode = IsPressed.GetHashCode();
            hashCode = (hashCode * 397) ^ IsRotationClockwise.GetHashCode();
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
    public static bool operator ==(PowerMateInput left, PowerMateInput right) {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines if the <paramref name="left"/> instance is equal to the <paramref name="right"/> instance.
    /// </summary>
    /// <param name="left">An instance to compare to <paramref name="right"/></param>
    /// <param name="right">An instance to compare to <paramref name="left"/></param>
    /// <returns><see langword="false" /> if <paramref name="left"/> and <paramref name="right"/> are equal, <see langword="true" /> otherwise.</returns>
    public static bool operator !=(PowerMateInput left, PowerMateInput right) {
        return !left.Equals(right);
    }

    /// <summary>
    /// <para>Examples:</para>
    /// <para><c>Turning clockwise 1 increment while not pressed</c></para>
    /// <para><c>Turning counterclockwise 2 increments while pressed</c></para>
    /// <para><c>Not turning while pressed</c></para>
    /// <para><c>Not turning while not pressed</c></para>
    /// </summary>
    /// <returns>Textual description of this instance</returns>
    public override string ToString() {
        return IsRotationClockwise switch { true => "Turning clockwise ", false => "Turning counterclockwise ", _ => "Not turning " } +
            RotationDistance switch { 0          => string.Empty, 1             => "1 increment ", var d          => $"{d:N0} increments " } +
            (IsPressed ? "while pressed" : "while not pressed");
    }

}