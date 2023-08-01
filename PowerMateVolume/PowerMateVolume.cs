using PowerMate;
using PowerMateVolume;

// ReSharper disable AccessToDisposedClosure - disposal happens at program shutdown, so access can't happen after that

if (!float.TryParse(Environment.GetCommandLineArgs().ElementAtOrDefault(1), out float volumeIncrement)) {
    volumeIncrement = 0.01f;
}

using IPowerMateClient powerMate     = new PowerMateClient();
using IVolumeChanger   volumeChanger = new VolumeChanger { VolumeIncrement = volumeIncrement };

powerMate.LightBrightness = 0;

CancellationTokenSource cancellationTokenSource = new();
Console.CancelKeyPress += (_, eventArgs) => {
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

powerMate.InputReceived += (_, powerMateEvent) => {
    switch (powerMateEvent) {
        case { IsPressed: true, RotationDirection: RotationDirection.None }:
            volumeChanger.ToggleMute();
            break;
        case { IsPressed: false, RotationDirection: RotationDirection.Clockwise }:
            volumeChanger.IncreaseVolume((int) powerMateEvent.RotationDistance);
            break;
        case { IsPressed: false, RotationDirection: RotationDirection.Counterclockwise }:
            volumeChanger.IncreaseVolume(-1 * (int) powerMateEvent.RotationDistance);
            break;
        default:
            break;
    }
};

using IStandbyListener standbyListener = new EventLogStandbyListener();
standbyListener.FatalError += (_, exception) =>
    MessageBox.Show("Event log subscription is broken, continuing without resume detection: " + exception, "PowerMateVolume", MessageBoxButtons.OK, MessageBoxIcon.Error);
standbyListener.Resumed += (_, _) => powerMate.SetAllFeaturesIfStale();

Console.WriteLine("Listening for PowerMate events");
cancellationTokenSource.Token.WaitHandle.WaitOne();