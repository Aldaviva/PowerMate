using HidSharp;

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
        actualEvent!.Value.RotationDirection.Should().Be(RotationDirection.None);
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
        actualEvent!.Value.RotationDirection.Should().Be(RotationDirection.None);
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
        actualEvent!.Value.RotationDirection.Should().Be(RotationDirection.None);
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

}