namespace PowerMate;

/// <summary>
/// Listen for events from a connected Griffin PowerMate.
/// </summary>
public interface IPowerMate: IDisposable {

    /// <summary>
    /// Fired when an event is received from the device
    /// </summary>
    event EventHandler<PowerMateEvent>? OnInput;

    /// <summary>
    /// <see cref="SynchronizationContext"/> on which to run event callbacks. Useful if your callbacks need to update a UI on the main thread.
    /// </summary>
    SynchronizationContext EventSynchronizationContext { get; set; }

}