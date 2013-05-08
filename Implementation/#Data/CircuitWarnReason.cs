using System;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public enum CircuitWarnReason {
    None,
    SignalesTooManyPumps,
    SignalesTooManyDartTraps,
    SignalesTooManyStatues,
    InsufficientPermissionToSignalComponent,
    BlockActivatorChangedTooManyBlocks
  }
}
