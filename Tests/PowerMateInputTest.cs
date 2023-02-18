namespace Tests;

public class PowerMateInputTest {

    [Fact]
    public void Pressed() {
        PowerMateInput actual = new(Convert.FromHexString("000100004F1000"));
        actual.IsPressed.Should().BeTrue();
        actual.IsRotationClockwise.Should().BeNull();
        actual.RotationDistance.Should().Be(0);
    }

    [Fact]
    public void Released() {
        PowerMateInput actual = new(Convert.FromHexString("000000004F1000"));
        actual.IsPressed.Should().BeFalse();
        actual.IsRotationClockwise.Should().BeNull();
        actual.RotationDistance.Should().Be(0);
    }

    [Fact]
    public void RotatedClockwiseSlowlyUnpressed() {
        PowerMateInput actual = new(Convert.FromHexString("000001004F1000"));
        actual.IsPressed.Should().BeFalse();
        actual.IsRotationClockwise.Should().BeTrue();
        actual.RotationDistance.Should().Be(1);
    }

    [Fact]
    public void RotatedCounterclockwiseSlowlyUnpressed() {
        PowerMateInput actual = new(Convert.FromHexString("0000FF004F1000"));
        actual.IsPressed.Should().BeFalse();
        actual.IsRotationClockwise.Should().BeFalse();
        actual.RotationDistance.Should().Be(1);
    }

    [Fact]
    public void RotatedClockwiseQuicklyUnpressed() {
        PowerMateInput actual = new(Convert.FromHexString("000004004F1000"));
        actual.IsPressed.Should().BeFalse();
        actual.IsRotationClockwise.Should().BeTrue();
        actual.RotationDistance.Should().Be(4);
    }

    [Fact]
    public void RotatedCounterclockwiseQuicklyUnpressed() {
        PowerMateInput actual = new(Convert.FromHexString("0000FD004F1000"));
        actual.IsPressed.Should().BeFalse();
        actual.IsRotationClockwise.Should().BeFalse();
        actual.RotationDistance.Should().Be(3);
    }

    [Fact]
    public void RotatedClockwiseSlowlyPressed() {
        PowerMateInput actual = new(Convert.FromHexString("000101004F1000"));
        actual.IsPressed.Should().BeTrue();
        actual.IsRotationClockwise.Should().BeTrue();
        actual.RotationDistance.Should().Be(1);
    }

    [Fact]
    public void RotatedCounterclockwiseSlowlyPressed() {
        PowerMateInput actual = new(Convert.FromHexString("0001FF004F1000"));
        actual.IsPressed.Should().BeTrue();
        actual.IsRotationClockwise.Should().BeFalse();
        actual.RotationDistance.Should().Be(1);
    }

    [Fact]
    public void Equal() {
        PowerMateInput a = new(Convert.FromHexString("000001004F1000"));
        PowerMateInput b = new(Convert.FromHexString("000001004F1000"));

        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void NotEqual() {
        PowerMateInput a = new(Convert.FromHexString("000001004F1000"));
        PowerMateInput b = new(Convert.FromHexString("0000FF004F1000"));

        (a == b).Should().BeFalse();
        (a != b).Should().BeTrue();
        a.Should().NotBe(b);
        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact]
    public void Formatting() {
        new PowerMateInput(false, true, 1).ToString().Should().Be("Turning clockwise 1 increment while not pressed");
        new PowerMateInput(true, false, 2).ToString().Should().Be("Turning counterclockwise 2 increments while pressed");
        new PowerMateInput(true, null, 0).ToString().Should().Be("Not turning while pressed");
        new PowerMateInput(false, null, 0).ToString().Should().Be("Not turning while not pressed");
    }

}