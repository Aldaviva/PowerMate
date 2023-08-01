using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using HidSharp;
using PowerMate;

namespace Tests;

[SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "explicit invocation ordering for tests")]
public class PowerMateClientOutputTest {

    private readonly HidDevice  _device     = A.Fake<HidDevice>();
    private readonly DeviceList _deviceList = A.Fake<DeviceList>();
    private readonly HidStream  _stream     = A.Fake<HidStream>();

    public PowerMateClientOutputTest() {
        A.CallTo(() => _device.VendorID).Returns(0x077d);
        A.CallTo(() => _device.ProductID).Returns(0x0410);

        A.CallTo(() => _deviceList.GetDevices(A<DeviceTypes>._)).Returns(new[] { _device });

        A.CallTo(_device).Where(call => call.Method.Name == "OpenDeviceAndRestrictAccess").WithReturnType<DeviceStream>().Returns(_stream);
        A.CallTo(() => _stream.ReadAsync(A<byte[]>._, An<int>._, An<int>._, A<CancellationToken>._)).ReturnsLazily(_ => new TaskCompletionSource<int>().Task);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(127)]
    [InlineData(255)]
    public void SetBrightness(byte brightness) {
        PowerMateClient client = new(_deviceList);
        client.LightBrightness = brightness;

        client.LightBrightness.Should().Be(brightness);
        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0x00, 0x41, 0x01, 0x01, 0x00, brightness, 0x00, 0x00, 0x00 }), 0, 9)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(-2, 0, 0x00, 0x0e)]
    [InlineData(-1, 0, 0x00, 0x0e)]
    [InlineData(0, 0, 0x00, 0x0e)]
    [InlineData(1, 1, 0x00, 0x0c)]
    [InlineData(2, 2, 0x00, 0x0a)]
    [InlineData(3, 3, 0x00, 0x08)]
    [InlineData(4, 4, 0x00, 0x06)]
    [InlineData(5, 5, 0x00, 0x04)]
    [InlineData(6, 6, 0x00, 0x02)]
    [InlineData(7, 7, 0x00, 0x00)]
    [InlineData(8, 8, 0x01, 0x00)]
    [InlineData(9, 9, 0x02, 0x02)]
    [InlineData(10, 10, 0x02, 0x04)]
    [InlineData(11, 11, 0x02, 0x06)]
    [InlineData(12, 12, 0x02, 0x08)]
    [InlineData(13, 13, 0x02, 0x0a)]
    [InlineData(14, 14, 0x02, 0x0c)]
    [InlineData(15, 15, 0x02, 0x0e)]
    [InlineData(16, 16, 0x02, 0x10)]
    [InlineData(17, 17, 0x02, 0x12)]
    [InlineData(18, 18, 0x02, 0x14)]
    [InlineData(19, 19, 0x02, 0x16)]
    [InlineData(20, 20, 0x02, 0x18)]
    [InlineData(21, 21, 0x02, 0x1a)]
    [InlineData(22, 22, 0x02, 0x1c)]
    [InlineData(23, 23, 0x02, 0x1e)]
    [InlineData(24, 24, 0x02, 0x20)]
    [InlineData(25, 24, 0x02, 0x20)]
    [InlineData(26, 24, 0x02, 0x20)]
    public void SetLightPulseSpeed(int input, int expectedSpeed, byte expectedLeftByte, byte expectedRightByte) {
        PowerMateClient client = new(_deviceList);
        client.LightAnimation  = LightAnimation.Pulsing;
        client.LightPulseSpeed = input;

        client.LightPulseSpeed.Should().Be(expectedSpeed);
        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0x00, 0x41, 0x01, 0x04, 0x00, expectedLeftByte, expectedRightByte, 0x00, 0x00 }), 0, 9))
            .MustHaveHappened();
    }

    [Fact]
    public void SetAnimationSolid() {
        PowerMateClient client = new(_deviceList);
        client.LightBrightness = 255;
        client.LightAnimation  = LightAnimation.Solid;

        client.LightAnimation.Should().Be(LightAnimation.Solid);
        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0x00, 0x41, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00 }), 0, 9)).MustHaveHappened();
        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0x00, 0x41, 0x01, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00 }), 0, 9)).MustHaveHappened();
        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0x00, 0x41, 0x01, 0x01, 0x00, 0xff, 0x00, 0x00, 0x00 }), 0, 9)).MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public void SetAnimationPulsing() {
        PowerMateClient client = new(_deviceList);
        client.LightPulseSpeed = 8;
        client.LightAnimation  = LightAnimation.Pulsing;

        client.LightAnimation.Should().Be(LightAnimation.Pulsing);
        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0x00, 0x41, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00 }), 0, 9)).MustHaveHappened();
        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0x00, 0x41, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00 }), 0, 9)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0x00, 0x41, 0x01, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00 }), 0, 9)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void SetAnimationPulsingDuringSleepOnly() {
        PowerMateClient client = new(_deviceList);
        client.LightPulseSpeed = 8;
        client.LightBrightness = 128;
        client.LightAnimation  = LightAnimation.SolidWhileAwakeAndPulsingDuringComputerStandby;

        client.LightAnimation.Should().Be(LightAnimation.SolidWhileAwakeAndPulsingDuringComputerStandby);
        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0x00, 0x41, 0x01, 0x02, 0x00, 0x01, 0x00, 0x00, 0x00 }), 0, 9)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0x00, 0x41, 0x01, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00 }), 0, 9)).MustHaveHappened();
        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0x00, 0x41, 0x01, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00 }), 0, 9)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0x00, 0x41, 0x01, 0x01, 0x00, 0x80, 0x00, 0x00, 0x00 }), 0, 9)).MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public void RetrySetFeatureOnceOnFailure() {
        PowerMateClient client   = new(_deviceList);
        byte[]          expected = { 0x00, 0x41, 0x01, 0x01, 0x00, 0xFF, 0x00, 0x00, 0x00 };
        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(expected), A<int>._, A<int>._)).Throws(new IOException("The operation completed successfully", new Win32Exception(0))).Once()
            .Then.DoesNothing();

        client.LightBrightness = 255;

        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(expected), A<int>._, A<int>._)).MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public void SetAllFeaturesIfStale() {
        PowerMateClient client = new(_deviceList);
        client.LightBrightness = 255;
        byte[] expected = { 0x00, 0x41, 0x01, 0x01, 0x00, 0xFF, 0x00, 0x00, 0x00 };
        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(expected), A<int>._, A<int>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _stream.GetFeature(A<byte[]>._, A<int>._, A<int>._)).Invokes((byte[] buffer, int offset, int count) => { Array.Fill(buffer, (byte) 0, offset, count); });

        Thread.Sleep(750);
        bool actual = client.SetAllFeaturesIfStale();

        actual.Should().BeTrue();
        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(expected), A<int>._, A<int>._)).MustHaveHappenedTwiceOrMore();
    }

}