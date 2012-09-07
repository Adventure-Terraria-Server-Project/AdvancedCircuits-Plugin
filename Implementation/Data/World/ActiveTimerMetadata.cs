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
  [XmlRoot("ActiveTimer")]
  public class ActiveTimerMetadata {
    #region [Property: FramesLeft]
    private int framesLeft;

    public int FramesLeft {
      get { return this.framesLeft; }
      set { this.framesLeft = value; }
    }
    #endregion


    #region [Method: Constructor]
    public ActiveTimerMetadata(int framesLeft) {
      this.framesLeft = framesLeft;
    }

    public ActiveTimerMetadata() {}
    #endregion
  }
}
