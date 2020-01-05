using System;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class GrandfatherClockMetadata {
    public string TriggeringPlayerName { get; set; }


    public GrandfatherClockMetadata(string triggeringPlayerName) {
      this.TriggeringPlayerName = triggeringPlayerName;
    }

    public GrandfatherClockMetadata() {}
  }
}
