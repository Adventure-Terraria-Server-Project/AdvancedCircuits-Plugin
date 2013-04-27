// This file is provided unter the terms of the 
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// 
// Written by CoderCow

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Terraria.Plugins.Common.AdvancedCircuits {
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
