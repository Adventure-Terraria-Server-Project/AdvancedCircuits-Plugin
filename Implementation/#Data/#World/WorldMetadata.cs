using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using DPoint = System.Drawing.Point;

using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class WorldMetadata: IMetadataFile {
    #region [Constants]
    protected const string CurrentVersion = "1.2";
    #endregion

    #region [Property: Version]
    private readonly string version;

    public string Version {
      get { return this.version; }
    }
    #endregion

    #region [Property: GateStates]
    private readonly Dictionary<DPoint,GateStateMetadata> gateStates;

    public Dictionary<DPoint,GateStateMetadata> GateStates {
      get { return this.gateStates; }
    }
    #endregion

    #region [Property: ActiveTimers]
    private readonly Dictionary<DPoint,ActiveTimerMetadata> activeTimers;

    public Dictionary<DPoint,ActiveTimerMetadata> ActiveTimers {
      get { return this.activeTimers; }
    }
    #endregion

    #region [Property: Clocks]
    private readonly Dictionary<DPoint,GrandfatherClockMetadata> clocks;

    public Dictionary<DPoint,GrandfatherClockMetadata> Clocks {
      get { return this.clocks; }
    }
    #endregion

    #region [Property: ActiveSwapperLocations]
    private readonly Collection<DPoint> activeSwapperLocations;

    public Collection<DPoint> ActiveSwapperLocations {
      get { return this.activeSwapperLocations; }
    }
    #endregion

    #region [Property: BlockActivators]
    private readonly Dictionary<DPoint, BlockActivatorMetadata> blockActivators;

    public Dictionary<DPoint,BlockActivatorMetadata> BlockActivators {
      get { return this.blockActivators; }
    }
    #endregion

    #region [Property: WirelessTransmitters]
    private readonly Dictionary<DPoint,string> wirelessTransmitters;

    public Dictionary<DPoint,string> WirelessTransmitters {
      get { return this.wirelessTransmitters; }
    }
    #endregion


    #region [Methods: Static Read, Write]
    public static WorldMetadata Read(string filePath) {
      using (StreamReader fileReader = new StreamReader(filePath)) {
        return JsonConvert.DeserializeObject<WorldMetadata>(fileReader.ReadToEnd());
      }
    }

    public void Write(string filePath) {
      using (StreamWriter fileWriter = new StreamWriter(filePath)) {
        fileWriter.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
      }
    }
    #endregion

    #region [Method: Constructor]
    public WorldMetadata() {
      this.version = WorldMetadata.CurrentVersion;
      this.gateStates = new Dictionary<DPoint,GateStateMetadata>();
      this.activeTimers = new Dictionary<DPoint,ActiveTimerMetadata>();
      this.clocks = new Dictionary<DPoint,GrandfatherClockMetadata>();
      this.activeSwapperLocations = new Collection<DPoint>();
      this.blockActivators = new Dictionary<DPoint,BlockActivatorMetadata>();
      this.wirelessTransmitters = new Dictionary<DPoint,string>();
    }
    #endregion
  }
}
