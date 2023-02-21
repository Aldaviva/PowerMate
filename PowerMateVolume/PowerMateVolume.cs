﻿using PowerMate;
using PowerMateVolume;

if (!float.TryParse(Environment.GetCommandLineArgs().ElementAtOrDefault(1), out float volumeIncrement)) {
    volumeIncrement = 0.01f;
}

using IPowerMateClient powerMateClient = new PowerMateClient();
using VolumeChanger    volumeChanger   = new VolumeChangerImpl { volumeIncrement = volumeIncrement };

powerMateClient.LightBrightness = 0;

CancellationTokenSource cancellationTokenSource = new();
Console.CancelKeyPress += (_, eventArgs) => {
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

powerMateClient.InputReceived += (_, powerMateEvent) => {
    switch (powerMateEvent) {
        case { IsPressed: true, RotationDirection: RotationDirection.None }:
            volumeChanger.toggleMute();
            break;
        case { IsPressed: false, RotationDirection: RotationDirection.Clockwise }:
            volumeChanger.increaseVolume((int) powerMateEvent.RotationDistance);
            break;
        case { IsPressed: false, RotationDirection: RotationDirection.Counterclockwise }:
            volumeChanger.increaseVolume(-1 * (int) powerMateEvent.RotationDistance);
            break;
        default:
            break;
    }
};

Console.WriteLine("Listening for PowerMate events");
cancellationTokenSource.Token.WaitHandle.WaitOne();