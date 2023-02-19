using HidSharp;

namespace Tests;

public class PowerMateClientTest {

    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(4);

    private readonly HidDevice  _device     = A.Fake<HidDevice>();
    private readonly DeviceList _deviceList = A.Fake<DeviceList>();
    private readonly HidStream  _stream     = A.Fake<HidStream>();

    public PowerMateClientTest() {
        A.CallTo(() => _device.VendorID).Returns(0x077d);
        A.CallTo(() => _device.ProductID).Returns(0x0410);

        A.CallTo(() => _deviceList.GetDevices(A<DeviceTypes>._)).Returns(new[] { _device });

        A.CallTo(_device).Where(call => call.Method.Name == "OpenDeviceAndRestrictAccess").WithReturnType<DeviceStream>().Returns(_stream);
        A.CallTo(() => _stream.ReadAsync(A<byte[]>._, An<int>._, An<int>._, A<CancellationToken>._)).ReturnsLazily(call => {
            byte[] buffer       = (byte[]) call.Arguments[0]!;
            int    offset       = (int) call.Arguments[1]!;
            int    count        = (int) call.Arguments[2]!;
            byte[] fakeHidBytes = Convert.FromHexString("000100004F1000");
            Array.Copy(fakeHidBytes, 0, buffer, offset, count);
            return Task.FromResult(Math.Min(count, fakeHidBytes.Length));
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
        actualEvent!.Value.IsRotationClockwise.Should().BeNull();
        actualEvent!.Value.RotationDistance.Should().Be(0);
    }

    [Fact]
    public void LateAttach() {
        A.CallTo(() => _deviceList.GetDevices(A<DeviceTypes>._)).ReturnsNextFromSequence(
            Enumerable.Empty<HidDevice>(),
            new[] { _device });

        bool?           connectedEventArg = null;
        PowerMateClient client            = new(_deviceList);
        client.IsConnected.Should().BeFalse();
        PowerMateInput?      actualEvent        = null;
        ManualResetEventSlim inputReceived      = new();
        ManualResetEventSlim isConnectedChanged = new();
        client.InputReceived += (_, @event) => {
            actualEvent = @event;
            inputReceived.Set();
        };
        client.IsConnectedChanged += (_, b) => {
            connectedEventArg = b;
            isConnectedChanged.Set();
        };

        _deviceList.RaiseChanged();

        inputReceived.Wait(TestTimeout);
        isConnectedChanged.Wait(TestTimeout);

        client.IsConnected.Should().BeTrue();
        connectedEventArg.HasValue.Should().BeTrue();
        connectedEventArg!.Value.Should().BeTrue();
        actualEvent.HasValue.Should().BeTrue();
        actualEvent!.Value.IsPressed.Should().BeTrue();
        actualEvent!.Value.IsRotationClockwise.Should().BeNull();
        actualEvent!.Value.RotationDistance.Should().Be(0);
    }

    [Fact]
    public void Reconnect() {
        A.CallTo(() => _stream.ReadAsync(A<byte[]>._, An<int>._, An<int>._, A<CancellationToken>._))
            .ThrowsAsync(new IOException("fake disconnected")).Once().Then
            .ReturnsLazily(call => {
                byte[] buffer       = (byte[]) call.Arguments[0]!;
                int    offset       = (int) call.Arguments[1]!;
                int    count        = (int) call.Arguments[2]!;
                byte[] fakeHidBytes = Convert.FromHexString("000100004F1000");
                Array.Copy(fakeHidBytes, 0, buffer, offset, count);
                return Task.FromResult(Math.Min(count, fakeHidBytes.Length));
            });

        ManualResetEventSlim eventArrived = new();
        PowerMateClient      client       = new(_deviceList);
        PowerMateInput?      actualEvent  = null;
        client.InputReceived += (_, @event) => {
            actualEvent = @event;
            eventArrived.Set();
        };

        _deviceList.RaiseChanged();

        eventArrived.Wait(TestTimeout);
        actualEvent.HasValue.Should().BeTrue();
        actualEvent!.Value.IsPressed.Should().BeTrue();
        actualEvent!.Value.IsRotationClockwise.Should().BeNull();
        actualEvent!.Value.RotationDistance.Should().Be(0);
    }

    [Fact]
    public void SynchronizationContext() {
        ManualResetEventSlim   eventArrived           = new();
        SynchronizationContext synchronizationContext = A.Fake<SynchronizationContext>();
        A.CallTo(() => synchronizationContext.Post(A<SendOrPostCallback>._, An<object?>._)).Invokes(() => eventArrived.Set());

        PowerMateClient client = new(_deviceList) { EventSynchronizationContext = synchronizationContext };
        eventArrived.Wait(TestTimeout);

        A.CallTo(() => synchronizationContext.Post(A<SendOrPostCallback>._, An<object?>._)).MustHaveHappenedOnceOrMore();
    }

    [Fact]
    public void Dispose() {
        PowerMateClient client = new(_deviceList);
        client.Dispose();
    }

    [Fact]
    public void DisposeIdempotent() {
        PowerMateClient client = new(_deviceList);
        client.Dispose();
        client.Dispose();
    }

    [Theory]
    [InlineData(-2, 0x00, 0x0e)]
    [InlineData(-1, 0x00, 0x0e)]
    [InlineData(0, 0x00, 0x0e)]
    [InlineData(1, 0x00, 0x0c)]
    [InlineData(2, 0x00, 0x0a)]
    [InlineData(3, 0x00, 0x08)]
    [InlineData(4, 0x00, 0x06)]
    [InlineData(5, 0x00, 0x04)]
    [InlineData(6, 0x00, 0x02)]
    [InlineData(7, 0x00, 0x00)]
    [InlineData(8, 0x01, 0x00)]
    [InlineData(9, 0x02, 0x02)]
    [InlineData(10, 0x02, 0x04)]
    [InlineData(11, 0x02, 0x06)]
    [InlineData(12, 0x02, 0x08)]
    [InlineData(13, 0x02, 0x0a)]
    [InlineData(14, 0x02, 0x0c)]
    [InlineData(15, 0x02, 0x0e)]
    [InlineData(16, 0x02, 0x10)]
    [InlineData(17, 0x02, 0x12)]
    [InlineData(18, 0x02, 0x14)]
    [InlineData(19, 0x02, 0x16)]
    [InlineData(20, 0x02, 0x18)]
    [InlineData(21, 0x02, 0x1a)]
    [InlineData(22, 0x02, 0x1c)]
    [InlineData(23, 0x02, 0x1e)]
    [InlineData(24, 0x02, 0x20)]
    [InlineData(25, 0x02, 0x20)]
    [InlineData(26, 0x02, 0x20)]
    public void EncodePulseSpeed(int input, byte expectedLeftByte, byte expectedRightByte) {
        byte[] actual = PowerMateClient.EncodePulseSpeed(input);
        actual.Should().HaveCount(2);
        if (!BitConverter.IsLittleEndian) {
            (expectedLeftByte, expectedRightByte) = (expectedRightByte, expectedLeftByte);
        }

        actual[0].Should().Be(expectedLeftByte);
        actual[1].Should().Be(expectedRightByte);
    }

}