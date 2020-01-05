using System;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class GateStateMetadata {
    public bool?[] PortStates { get; set; }


    public GateStateMetadata() {
      this.PortStates = new bool?[4];
    }
  }
}
