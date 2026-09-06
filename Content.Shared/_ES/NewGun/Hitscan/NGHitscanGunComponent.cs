using Robust.Shared.GameStates;

/// <summary>
/// This gun shoots hitscans.
/// The hitscans are initially shot on the client and predicted, then the result is sent to the server who checks it then accepts it.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NGHitscanGunComponent : Component;