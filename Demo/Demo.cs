using PowerMate;

using IPowerMateClient powerMate = new PowerMateClient();

powerMate.IsConnectedChanged += (_, isConnected) => Console.WriteLine(isConnected ? "Connected to PowerMate" : "Disconnected from PowerMate, attempting reconnection");

powerMate.InputReceived += (_, input) => Console.WriteLine($"Received event from PowerMate: {input}");

CancellationTokenSource cancellationTokenSource = new();
Console.CancelKeyPress += (_, eventArgs) => {
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

Console.WriteLine(powerMate.IsConnected ? "Listening for PowerMate events" : "Waiting for PowerMate to be connected");
cancellationTokenSource.Token.WaitHandle.WaitOne();