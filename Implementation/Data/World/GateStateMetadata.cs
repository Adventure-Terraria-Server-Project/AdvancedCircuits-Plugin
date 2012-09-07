// This file is provided unter the terms of the 
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// 
// Written by CoderCow

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Xml.Serialization;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  [XmlRoot("GateState")]
  public class GateStateMetadata {
    #region [Property: PortStates]
    private bool?[] portStates;

    [XmlArray("PortStates")]
    [XmlArrayItem("PortState")]
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
