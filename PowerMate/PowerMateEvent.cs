namespace PowerMate;

public readonly struct PowerMateEvent {

    public readonly bool  IsPressed;
    public readonly bool? IsRotationClockwise;
    public readonly uint  RotationDistance = 0;

    public PowerMateEvent(IReadOnlyList<byte> rawData): this(rawData[1] == 0x01, GetRotationDirection(rawData), GetRotationDistance(rawData)) { }

    /// <summary>
    /// For unit testing
    /// </summary>
    /// <param name="isPressed"></param>
    /// <param name="isRotationClockwise"></param>
    /// <param name="rotationDistance"></param>
    public PowerMateEvent(bool isPressed, bool? isRotationClockwise, uint rotationDistance) {
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

    public bool Equals(PowerMateEvent other) {
        return IsPressed == other.IsPressed && IsRotationClockwise == other.IsRotationClockwise && RotationDistance == other.RotationDistance;
    }

    public override bool Equals(object? obj) {
        return obj is PowerMateEvent other && Equals(other);
    }

    public override int GetHashCode() {
        unchecked {
            int hashCode = IsPressed.GetHashCode();
            hashCode = (hashCode * 397) ^ IsRotationClockwise.GetHashCode();
            hashCode = (hashCode * 397) ^ (int) RotationDistance;
            return hashCode;
        }
    }

    public static bool operator ==(PowerMateEvent left, PowerMateEvent right) {
        return left.Equals(right);
    }

    public static bool operator !=(PowerMateEvent left, PowerMateEvent right) {
        return !left.Equals(right);
    }

}