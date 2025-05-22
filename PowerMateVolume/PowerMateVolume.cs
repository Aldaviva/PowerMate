using PowerMate;
using PowerMateVolume;
using RuntimeUpgrade.Notifier;
using RuntimeUpgrade.Notifier.Data;
using Unfucked.Windows.Power;

// ReSharper disable AccessToDisposedClosure - disposal happens at program shutdown, so access can't happen after that

if (!float.TryParse(Environment.GetCommandLineArgs().ElementAtOrDefault(1), out float volumeIncrement)) {
    volumeIncrement = 0.01f;
}

using IPowerMateClient powerMate     = new PowerMateClient();
using IVolumeChanger   volumeChanger = new VolumeChanger { VolumeIncrement = volumeIncrement };

powerMate.LightBrightness = 0;

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
standbyListener.Resumed += (_, _) => {
    try {
        powerMate.SetAllFeaturesIfStale();
    } catch (IOException) {
        Thread.Sleep(2000);
        try {
            powerMate.SetAllFeaturesIfStale();
        } catch (IOException) {
            // device is probably in a bad state, but there's nothing we can do about it except wait for the user to unplug it
        }
    }
};

CancellationTokenSource exitTokenSource = new();
Console.CancelKeyPress += (_, eventArgs) => {
    eventArgs.Cancel = true;
    exitTokenSource.Cancel();
};

using IRuntimeUpgradeNotifier runtimeUpgradeNotifier = new RuntimeUpgradeNotifier {
    RestartStrategy = RestartStrategy.AutoRestartProcess,
    ExitStrategy    = new CancellationTokenExit(exitTokenSource)
};

Console.WriteLine("Listening for PowerMate events");
exitTokenSource.Token.WaitHandle.WaitOne();