﻿using System;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class ActiveTimerMetadata {
    public string TriggeringPlayerName { get; set; }
    public int FramesLeft { get; set; }
    public DateTime TimeOfRegistration { get; set; }


    public ActiveTimerMetadata(int framesLeft, string triggeringPlayerName) {
      this.FramesLeft = framesLeft;
      this.TriggeringPlayerName = triggeringPlayerName;
      this.TimeOfRegistration = DateTime.UtcNow;
    }

    public ActiveTimerMetadata() {}
  }
}
