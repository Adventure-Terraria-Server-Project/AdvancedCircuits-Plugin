using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class ActiveTimerMetadata {
    #region [Property: TriggeringPlayerName]
    private string triggeringPlayerName;

    public string TriggeringPlayerName {
      get { return this.triggeringPlayerName; }
      set { this.triggeringPlayerName = value; }
    }
    #endregion

    #region [Property: FramesLeft]
    private int framesLeft;

    public int FramesLeft {
      get { return this.framesLeft; }
      set { this.framesLeft = value; }
    }
    #endregion


    #region [Method: Constructor]
    public ActiveTimerMetadata(int framesLeft, string triggeringPlayerName) {
      this.framesLeft = framesLeft;
      this.triggeringPlayerName = triggeringPlayerName;
    }

    public ActiveTimerMetadata() {}
    #endregion
  }
}
