using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class ActiveTimerMetadata {
    public string TriggeringPlayerName { get; set; }
    public int FramesLeft { get; set; }


    public ActiveTimerMetadata(int framesLeft, string triggeringPlayerName) {
      this.FramesLeft = framesLeft;
      this.TriggeringPlayerName = triggeringPlayerName;
    }

    public ActiveTimerMetadata() {}
  }
}
