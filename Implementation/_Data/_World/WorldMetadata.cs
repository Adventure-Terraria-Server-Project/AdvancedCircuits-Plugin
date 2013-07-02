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
    private string version;

    public string Version {
      get { return this.version; }
    }
    #endregion

    #region [Property: GateStates]
    private Dictionary<DPoint,GateStateMetadata> gateStates;

    public Dictionary<DPoint,GateStateMetadata> GateStates {
      get { return this.gateStates; }
    }
    #endregion

    #region [Property: ActiveTimers]
    private Dictionary<DPoint,ActiveTimerMetadata> activeTimers;

    public Dictionary<DPoint,ActiveTimerMetadata> ActiveTimers {
      get { return this.activeTimers; }
    }
    #endregion

    #region [Property: Clocks]
    private Dictionary<DPoint,GrandfatherClockMetadata> clocks;

    public Dictionary<DPoint,GrandfatherClockMetadata> Clocks {
      get { return this.clocks; }
    }
    #endregion

    #region [Property: Swappers]
    private Dictionary<DPoint,int> swappers;

    public Dictionary<DPoint,int> Swappers {
      get { return this.swappers; }
    }
    #endregion

    #region [Property: BlockActivators]
    private Dictionary<DPoint, BlockActivatorMetadata> blockActivators;

    public Dictionary<DPoint,BlockActivatorMetadata> BlockActivators {
      get { return this.blockActivators; }
    }
    #endregion

    #region [Property: WirelessTransmitters]
    private Dictionary<DPoint,string> wirelessTransmitters;

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

    #region [Methods: Constructor, EnsureMetadata]
    public WorldMetadata() {
      this.EnsureMetadata();
    }

    public void EnsureMetadata() {
      this.version = WorldMetadata.CurrentVersion;

      if (this.gateStates == null)
        this.gateStates = new Dictionary<DPoint,GateStateMetadata>();

      if (this.activeTimers == null)
        this.activeTimers = new Dictionary<DPoint,ActiveTimerMetadata>();

      if (this.clocks == null)
        this.clocks = new Dictionary<DPoint,GrandfatherClockMetadata>();

      if (this.swappers == null)
        this.swappers = new Dictionary<DPoint,int>();

      if (this.blockActivators == null)
        this.blockActivators = new Dictionary<DPoint,BlockActivatorMetadata>();

      if (this.wirelessTransmitters == null)
        this.wirelessTransmitters = new Dictionary<DPoint,string>();
    }
    #endregion
  }
}
