namespace PowerMate;

public readonly struct PowerMateInput {

    public readonly bool  IsPressed;
    public readonly bool? IsRotationClockwise;
    public readonly uint  RotationDistance = 0;

    public PowerMateInput(IReadOnlyList<byte> rawData): this(rawData[1] == 0x01, GetRotationDirection(rawData), GetRotationDistance(rawData)) { }

    /// <summary>
    /// For unit testing
    /// </summary>
    /// <param name="isPressed"></param>
    /// <param name="isRotationClockwise"></param>
    /// <param name="rotationDistance"></param>
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

    public bool Equals(PowerMateInput other) {
        return IsPressed == other.IsPressed && IsRotationClockwise == other.IsRotationClockwise && RotationDistance == other.RotationDistance;
    }

    public override bool Equals(object? obj) {
        return obj is PowerMateInput other && Equals(other);
    }

    public override int GetHashCode() {
        unchecked {
            int hashCode = IsPressed.GetHashCode();
            hashCode = (hashCode * 397) ^ IsRotationClockwise.GetHashCode();
            hashCode = (hashCode * 397) ^ (int) RotationDistance;
            return hashCode;
        }
    }

    public static bool operator ==(PowerMateInput left, PowerMateInput right) {
        return left.Equals(right);
    }

    public static bool operator !=(PowerMateInput left, PowerMateInput right) {
        return !left.Equals(right);
    }

    public override string ToString() {
        return IsRotationClockwise switch { true => "Turning clockwise ", false => "Turning counterclockwise ", _ => "Not turning " } +
            RotationDistance switch { 0          => string.Empty, 1             => "1 increment ", var d          => $"{d:N0} increments " } +
            (IsPressed ? "while pressed" : "while not pressed");
    }

}