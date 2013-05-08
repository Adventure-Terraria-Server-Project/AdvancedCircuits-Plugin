using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class GateStateMetadata {
    #region [Property: PortStates]
    private bool?[] portStates;

    public bool?[] PortStates {
      get { return this.portStates; }
      set { this.portStates = value; }
    }
    #endregion


    #region [Method: Constructor]
    public GateStateMetadata() {
      this.portStates = new bool?[4];
    }
    #endregion
  }
}
