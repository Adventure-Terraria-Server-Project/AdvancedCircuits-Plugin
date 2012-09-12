// This file is provided unter the terms of the 
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// 
// Written by CoderCow

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using DPoint = System.Drawing.Point;

using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class WorldMetadata: IWorldMetadata {
    #region [Property: GateStates]
    private Dictionary<DPoint,GateStateMetadata> gateStates;

    public Dictionary<DPoint,GateStateMetadata> GateStates {
      get { return this.gateStates; }
      set { this.gateStates = value; }
    }
    #endregion

    #region [Property: ActiveTimers]
    private Dictionary<DPoint,ActiveTimerMetadata> activeTimers;

    public Dictionary<DPoint,ActiveTimerMetadata> ActiveTimers {
      get { return this.activeTimers; }
      set { this.activeTimers = value; }
    }
    #endregion

    #region [Property: ClockLocations]
    private Collection<DPoint> clockLocations;

    public Collection<DPoint> ClockLocations {
      get { return this.clockLocations; }
      set { this.clockLocations = value; }
    }
    #endregion

    #region [Property: ActiveSwapperLocations]
    private Collection<DPoint> activeSwapperLocations;

    public Collection<DPoint> ActiveSwapperLocations {
      get { return this.activeSwapperLocations; }
      set { this.activeSwapperLocations = value; }
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
      this.gateStates = new Dictionary<DPoint,GateStateMetadata>();
      this.activeTimers = new Dictionary<DPoint,ActiveTimerMetadata>();
      this.clockLocations = new Collection<DPoint>();
      this.activeSwapperLocations = new Collection<DPoint>();
    }
    #endregion
  }
}
