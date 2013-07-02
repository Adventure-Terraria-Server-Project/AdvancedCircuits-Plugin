using System;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public enum CircuitCancellationReason {
    None,
    ExceededMaxLength,
    SignaledSameComponentTooOften
  }
}
