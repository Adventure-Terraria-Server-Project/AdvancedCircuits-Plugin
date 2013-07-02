using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class GrandfatherClockMetadata {
    #region [Property: TriggeringPlayerName]
    private string triggeringPlayerName;

    public string TriggeringPlayerName {
      get { return this.triggeringPlayerName; }
      set { this.triggeringPlayerName = value; }
    }
    #endregion


    #region [Method: Constructor]
    public GrandfatherClockMetadata(string triggeringPlayerName) {
      this.triggeringPlayerName = triggeringPlayerName;
    }

    public GrandfatherClockMetadata() {}
    #endregion
  }
}
