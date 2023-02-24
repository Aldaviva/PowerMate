using Microsoft.Win32;
using PowerMate;
using PowerMateVolume;

// ReSharper disable AccessToDisposedClosure - disposal happens at program shutdown, so access won't happen after that

if (!float.TryParse(Environment.GetCommandLineArgs().ElementAtOrDefault(1), out float volumeIncrement)) {
    volumeIncrement = 0.01f;
}

using IPowerMateClient powerMateClient = new PowerMateClient();
using IVolumeChanger   volumeChanger   = new VolumeChanger { VolumeIncrement = volumeIncrement };

powerMateClient.LightBrightness = 0;

CancellationTokenSource cancellationTokenSource = new();
Console.CancelKeyPress += (_, eventArgs) => {
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

powerMateClient.InputReceived += (_, powerMateEvent) => {
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

SystemEvents.PowerModeChanged += (_, args) => {
    if (args.Mode == PowerModes.Resume) {
        // #1: On Jarnsaxa, waking up from sleep resets the PowerMate's light settings, so set them all again
        powerMateClient.LightAnimation = powerMateClient.LightAnimation;
    }
};

Console.WriteLine("Listening for PowerMate events");
cancellationTokenSource.Token.WaitHandle.WaitOne();