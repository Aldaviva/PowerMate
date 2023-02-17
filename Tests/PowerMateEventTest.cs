namespace Tests;

public class PowerMateEventTest {

    [Fact]
    public void Pressed() {
        PowerMateEvent actual = new(Convert.FromHexString("000100004F1000"));
        actual.IsPressed.Should().BeTrue();
        actual.IsRotationClockwise.Should().BeNull();
        actual.RotationDistance.Should().Be(0);
    }

    [Fact]
    public void Released() {
        PowerMateEvent actual = new(Convert.FromHexString("000000004F1000"));
        actual.IsPressed.Should().BeFalse();
        actual.IsRotationClockwise.Should().BeNull();
        actual.RotationDistance.Should().Be(0);
    }

    [Fact]
    public void RotatedClockwiseSlowlyUnpressed() {
        PowerMateEvent actual = new(Convert.FromHexString("000001004F1000"));
        actual.IsPressed.Should().BeFalse();
        actual.IsRotationClockwise.Should().BeTrue();
        actual.RotationDistance.Should().Be(1);
    }

    [Fact]
    public void RotatedCounterclockwiseSlowlyUnpressed() {
        PowerMateEvent actual = new(Convert.FromHexString("0000FF004F1000"));
        actual.IsPressed.Should().BeFalse();
        actual.IsRotationClockwise.Should().BeFalse();
        actual.RotationDistance.Should().Be(1);
    }

    [Fact]
    public void RotatedClockwiseQuicklyUnpressed() {
        PowerMateEvent actual = new(Convert.FromHexString("000004004F1000"));
        actual.IsPressed.Should().BeFalse();
        actual.IsRotationClockwise.Should().BeTrue();
        actual.RotationDistance.Should().Be(4);
    }

    [Fact]
    public void RotatedCounterclockwiseQuicklyUnpressed() {
        PowerMateEvent actual = new(Convert.FromHexString("0000FD004F1000"));
        actual.IsPressed.Should().BeFalse();
        actual.IsRotationClockwise.Should().BeFalse();
        actual.RotationDistance.Should().Be(3);
    }

    [Fact]
    public void RotatedClockwiseSlowlyPressed() {
        PowerMateEvent actual = new(Convert.FromHexString("000101004F1000"));
        actual.IsPressed.Should().BeTrue();
        actual.IsRotationClockwise.Should().BeTrue();
        actual.RotationDistance.Should().Be(1);
    }

    [Fact]
    public void RotatedCounterclockwiseSlowlyPressed() {
        PowerMateEvent actual = new(Convert.FromHexString("0001FF004F1000"));
        actual.IsPressed.Should().BeTrue();
        actual.IsRotationClockwise.Should().BeFalse();
        actual.RotationDistance.Should().Be(1);
    }

    [Fact]
    public void Equal() {
        PowerMateEvent a = new(Convert.FromHexString("000001004F1000"));
        PowerMateEvent b = new(Convert.FromHexString("000001004F1000"));

        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void NotEqual() {
        PowerMateEvent a = new(Convert.FromHexString("000001004F1000"));
        PowerMateEvent b = new(Convert.FromHexString("0000FF004F1000"));

        (a == b).Should().BeFalse();
        (a != b).Should().BeTrue();
        a.Should().NotBe(b);
        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

}