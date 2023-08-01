using HidSharp;
using PowerMate;

namespace Tests;

public class PowerMateClientInputTest {

    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(4);

    private readonly HidDevice  _device     = A.Fake<HidDevice>();
    private readonly DeviceList _deviceList = A.Fake<DeviceList>();
    private readonly HidStream  _stream     = A.Fake<HidStream>();

    public PowerMateClientInputTest() {
        A.CallTo(() => _device.VendorID).Returns(0x077d);
        A.CallTo(() => _device.ProductID).Returns(0x0410);

        A.CallTo(() => _deviceList.GetDevices(A<DeviceTypes>._)).Returns(new[] { _device });

        A.CallTo(_device).Where(call => call.Method.Name == "OpenDeviceAndRestrictAccess").WithReturnType<DeviceStream>().Returns(_stream);
        A.CallTo(() => _stream.ReadAsync(A<byte[]>._, An<int>._, An<int>._, A<CancellationToken>._)).ReturnsLazily(call => {
            byte[] buffer        = (byte[]) call.Arguments[0]!;
            int    offset        = (int) call.Arguments[1]!;
            int    count         = (int) call.Arguments[2]!;
            byte[] fakeHidBytes  = Convert.FromHexString("000100004F1000");
            int    occupiedCount = Math.Min(count, fakeHidBytes.Length);
            Array.Copy(fakeHidBytes, 0, buffer, offset, occupiedCount);
            return Task.FromResult(occupiedCount);
        });
    }

    [Fact]
    public void Constructor() {
        new PowerMateClient().Dispose();
    }

    [Fact]
    public void Pressed() {
        PowerMateClient      client       = new(_deviceList);
        ManualResetEventSlim eventArrived = new();
        PowerMateInput?      actualEvent  = null;
        client.InputReceived += (_, @event) => {
            actualEvent = @event;
            eventArrived.Set();
        };
        eventArrived.Wait(TestTimeout);
        actualEvent.HasValue.Should().BeTrue();
        actualEvent!.Value.IsPressed.Should().BeTrue();
        actualEvent!.Value.RotationDirection.Should().Be(RotationDirection.None);
        actualEvent!.Value.RotationDistance.Should().Be(0);
    }

    [Fact]
    public void ResetAllFeaturesOnStaleRead() {
        PowerMateClient client = new(_deviceList) {
            LightBrightness = 255
        };
        byte[] expected = { 0x00, 0x41, 0x01, 0x01, 0x00, 0xFF, 0x00, 0x00, 0x00 };
        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(expected), A<int>._, A<int>._)).MustHaveHappenedOnceExactly();

        Thread.Sleep(750);

        A.CallTo(() => _stream.SetFeature(A<byte[]>.That.IsSameSequenceAs(expected), A<int>._, A<int>._)).MustHaveHappenedTwiceOrMore();
    }

}