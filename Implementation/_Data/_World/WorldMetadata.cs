using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using DPoint = System.Drawing.Point;

using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class WorldMetadata: IMetadataFile {
    protected const string CurrentVersion = "1.2";

    public string Version { get; private set; }
    public Dictionary<DPoint,GateStateMetadata> GateStates { get; private set; }
    public Dictionary<DPoint,ActiveTimerMetadata> ActiveTimers { get; private set; }
    public Dictionary<DPoint,GrandfatherClockMetadata> Clocks { get; private set; }
    public Dictionary<DPoint,int> Swappers { get; private set; }
    public Dictionary<DPoint,BlockActivatorMetadata> BlockActivators { get; private set; }
    public Dictionary<DPoint,string> WirelessTransmitters { get; private set; }


    public static WorldMetadata Read(string filePath) {
      WorldMetadata metadata;
      using (StreamReader fileReader = new StreamReader(filePath))
        metadata = JsonConvert.DeserializeObject<WorldMetadata>(fileReader.ReadToEnd());

      metadata.EnsureMetadata();
      return metadata;
    }

    public void Write(string filePath) {
      using (StreamWriter fileWriter = new StreamWriter(filePath)) {
        fileWriter.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
      }
    }

    public WorldMetadata() {
      this.EnsureMetadata();
    }

    public void EnsureMetadata() {
      this.Version = WorldMetadata.CurrentVersion;

      if (this.GateStates == null)
        this.GateStates = new Dictionary<DPoint,GateStateMetadata>();

      if (this.ActiveTimers == null)
        this.ActiveTimers = new Dictionary<DPoint,ActiveTimerMetadata>();

      if (this.Clocks == null)
        this.Clocks = new Dictionary<DPoint,GrandfatherClockMetadata>();

      if (this.Swappers == null)
        this.Swappers = new Dictionary<DPoint,int>();

      if (this.BlockActivators == null)
        this.BlockActivators = new Dictionary<DPoint,BlockActivatorMetadata>();

      if (this.WirelessTransmitters == null)
        this.WirelessTransmitters = new Dictionary<DPoint,string>();
    }
  }
}
