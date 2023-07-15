using PowerMate;

using IPowerMateClient powerMate = new PowerMateClient();

powerMate.LightBrightness = 128;
// powerMate.LightPulseSpeed = 3;
// powerMate.LightAnimation  = LightAnimation.Pulsing;
Console.WriteLine($"Set brightness to {powerMate.LightBrightness}.");

powerMate.IsConnectedChanged += (_, isConnected) => Console.WriteLine(isConnected ? "Connected to PowerMate" : "Disconnected from PowerMate, attempting reconnection");

// powerMate.InputReceived += (_, input) => Console.WriteLine($"Received event from PowerMate: {input}");

// using Timer               parameterChangeTimer = new(_ => { Console.WriteLine(DateTime.Now); }, null, 0, 10000);
// using Timer timer2 = new(10000) { AutoReset = true, Enabled = true };
// timer2.Elapsed += (_, _) => Console.WriteLine(DateTime.Now);

Console.WriteLine(powerMate.IsConnected ? "Listening for PowerMate events" : "Waiting for PowerMate to be connected");
using Timer setAllFeaturesTimer = new(_ => {
    try {
        bool didSetFeatures = powerMate.SetAllFeaturesIfStale();
        Console.WriteLine($"Features {(didSetFeatures ? "were" : "were not")} reset automatically");
    } catch (Exception e) {
        Console.WriteLine("Exception while setting features: " + e);
    }
}, null, 0, 3000);

CancellationTokenSource cancellationTokenSource = new();
Console.CancelKeyPress += (_, eventArgs) => {
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};
cancellationTokenSource.Token.WaitHandle.WaitOne();