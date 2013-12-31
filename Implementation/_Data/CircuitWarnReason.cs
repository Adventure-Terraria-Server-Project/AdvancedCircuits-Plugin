using System;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public enum CircuitWarnReason {
    None,
    SignalesTooManyPumps,
    SignalesTooManyTraps,
    SignalesTooManyStatues,
    InsufficientPermissionToSignalComponent,
    BlockActivatorChangedTooManyBlocks
  }
}
