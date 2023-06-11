using PowerMate;

namespace Tests;

public class PowerMateInputTest {

    [Fact]
    public void Pressed() {
        PowerMateInput actual = new(Convert.FromHexString("000100004F1000"));
        actual.IsPressed.Should().BeTrue();
        actual.RotationDirection.Should().Be(RotationDirection.None);
        actual.RotationDistance.Should().Be(0);
    }

    [Fact]
    public void Released() {
        PowerMateInput actual = new(Convert.FromHexString("000000004F1000"));
        actual.IsPressed.Should().BeFalse();
        actual.RotationDirection.Should().Be(RotationDirection.None);
        actual.RotationDistance.Should().Be(0);
    }

    [Fact]
    public void RotatedClockwiseSlowlyUnpressed() {
        PowerMateInput actual = new(Convert.FromHexString("000001004F1000"));
        actual.IsPressed.Should().BeFalse();
        actual.RotationDirection.Should().Be(RotationDirection.Clockwise);
        actual.RotationDistance.Should().Be(1);
    }

    [Fact]
    public void RotatedCounterclockwiseSlowlyUnpressed() {
        PowerMateInput actual = new(Convert.FromHexString("0000FF004F1000"));
        actual.IsPressed.Should().BeFalse();
        actual.RotationDirection.Should().Be(RotationDirection.Counterclockwise);
        actual.RotationDistance.Should().Be(1);
    }

    [Fact]
    public void RotatedClockwiseQuicklyUnpressed() {
        PowerMateInput actual = new(Convert.FromHexString("000004004F1000"));
        actual.IsPressed.Should().BeFalse();
        actual.RotationDirection.Should().Be(RotationDirection.Clockwise);
        actual.RotationDistance.Should().Be(4);
    }

    [Fact]
    public void RotatedCounterclockwiseQuicklyUnpressed() {
        PowerMateInput actual = new(Convert.FromHexString("0000FD004F1000"));
        actual.IsPressed.Should().BeFalse();
        actual.RotationDirection.Should().Be(RotationDirection.Counterclockwise);
        actual.RotationDistance.Should().Be(3);
    }

    [Fact]
    public void RotatedClockwiseSlowlyPressed() {
        PowerMateInput actual = new(Convert.FromHexString("000101004F1000"));
        actual.IsPressed.Should().BeTrue();
        actual.RotationDirection.Should().Be(RotationDirection.Clockwise);
        actual.RotationDistance.Should().Be(1);
    }

    [Fact]
    public void RotatedCounterclockwiseSlowlyPressed() {
        PowerMateInput actual = new(Convert.FromHexString("0001FF004F1000"));
        actual.IsPressed.Should().BeTrue();
        actual.RotationDirection.Should().Be(RotationDirection.Counterclockwise);
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
        new PowerMateInput(false, RotationDirection.Clockwise, 1).ToString().Should().Be("Turning clockwise 1 increment while not pressed");
        new PowerMateInput(true, RotationDirection.Counterclockwise, 2).ToString().Should().Be("Turning counterclockwise 2 increments while pressed");
        new PowerMateInput(true, RotationDirection.None, 0).ToString().Should().Be("Not turning while pressed");
        new PowerMateInput(false, RotationDirection.None, 0).ToString().Should().Be("Not turning while not pressed");
    }

    [Theory]
    [InlineData(0x01, 0x0e, 0)]
    [InlineData(0x01, 0x0c, 1)]
    [InlineData(0x01, 0x0a, 2)]
    [InlineData(0x01, 0x08, 3)]
    [InlineData(0x01, 0x06, 4)]
    [InlineData(0x01, 0x04, 5)]
    [InlineData(0x01, 0x02, 6)]
    [InlineData(0x01, 0x01, 7)]
    [InlineData(0x11, 0x01, 8)]
    [InlineData(0x21, 0x02, 9)]
    [InlineData(0x21, 0x04, 10)]
    [InlineData(0x21, 0x06, 11)]
    [InlineData(0x21, 0x08, 12)]
    [InlineData(0x21, 0x0a, 13)]
    [InlineData(0x21, 0x0c, 14)]
    [InlineData(0x21, 0x0e, 15)]
    [InlineData(0x21, 0x10, 16)]
    [InlineData(0x21, 0x12, 17)]
    [InlineData(0x21, 0x14, 18)]
    [InlineData(0x21, 0x16, 19)]
    [InlineData(0x21, 0x18, 20)]
    [InlineData(0x21, 0x1a, 21)]
    [InlineData(0x21, 0x1c, 22)]
    [InlineData(0x21, 0x1e, 23)]
    [InlineData(0x21, 0x20, 24)]
    public void DecodeAlwaysPulsingSpeed(byte inputByte5, byte inputByte6, int expected) {
        PowerMateInput actual = new(new byte[] { 0, 0, 1, 0, 73, inputByte5, inputByte6 });
        actual.ActualLightAnimation.Should().Be(LightAnimation.Pulsing);
        actual.ActualLightPulseSpeed.Should().Be(expected);
    }

    [Theory]
    [InlineData(0x04, 0x0e, 0)]
    [InlineData(0x04, 0x0c, 1)]
    [InlineData(0x04, 0x0a, 2)]
    [InlineData(0x04, 0x08, 3)]
    [InlineData(0x04, 0x06, 4)]
    [InlineData(0x04, 0x04, 5)]
    [InlineData(0x04, 0x02, 6)]
    [InlineData(0x04, 0x01, 7)]
    [InlineData(0x14, 0x01, 8)]
    [InlineData(0x24, 0x02, 9)]
    [InlineData(0x24, 0x04, 10)]
    [InlineData(0x24, 0x06, 11)]
    [InlineData(0x24, 0x08, 12)]
    [InlineData(0x24, 0x0a, 13)]
    [InlineData(0x24, 0x0c, 14)]
    [InlineData(0x24, 0x0e, 15)]
    [InlineData(0x24, 0x10, 16)]
    [InlineData(0x24, 0x12, 17)]
    [InlineData(0x24, 0x14, 18)]
    [InlineData(0x24, 0x16, 19)]
    [InlineData(0x24, 0x18, 20)]
    [InlineData(0x24, 0x1a, 21)]
    [InlineData(0x24, 0x1c, 22)]
    [InlineData(0x24, 0x1e, 23)]
    [InlineData(0x24, 0x20, 24)]
    public void DecodeStandbyPulsingSpeed(byte inputByte5, byte inputByte6, int expected) {
        PowerMateInput actual = new(new byte[] { 0, 0, 1, 0, 73, inputByte5, inputByte6 });
        actual.ActualLightAnimation.Should().Be(LightAnimation.SolidWhileAwakeAndPulsingDuringComputerStandby);
        actual.ActualLightPulseSpeed.Should().Be(expected);
    }

    [Theory]
    [InlineData(0x0, 0x20, 0)]
    [InlineData(0x7f, 0x20, 127)]
    [InlineData(0xff, 0x20, 255)]
    public void DecodeBrightness(byte inputByte4, byte inputByte5, byte expected) {
        PowerMateInput actual = new(new byte[] { 0, 0, 1, 0, inputByte4, inputByte5, 0x0a });
        actual.ActualLightAnimation.Should().Be(LightAnimation.Solid);
        actual.ActualLightBrightness.Should().Be(expected);
    }

}