// This file is provided unter the terms of the 
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// 
// Written by CoderCow

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Xml;
using System.Xml.Serialization;
using DPoint = System.Drawing.Point;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class GateStatesDictionary: SerializableDictionary<DPoint,GateStateMetadata> {
    public GateStatesDictionary(): base("GateState", "Location") {}
  }
  public class ActiveTimersDictionary: SerializableDictionary<DPoint,ActiveTimerMetadata> {
    public ActiveTimersDictionary(): base("ActiveTimer", "Location") {}
  }

  [XmlRoot("AdvancedCircuitsMetadata")]
  public class WorldMetadata: IWorldMetadata {
    #region [Property: GateStates]
    private GateStatesDictionary gateStates;

    public GateStatesDictionary GateStates {
      get { return this.gateStates; }
      set { this.gateStates = value; }
    }
    #endregion

    #region [Property: ActiveTimers]
    private ActiveTimersDictionary activeTimers;

    public ActiveTimersDictionary ActiveTimers {
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
      using (XmlReader xmlReader = XmlReader.Create(filePath)) {
        return (WorldMetadata)WorldMetadata.GetSerializer().Deserialize(xmlReader);
      }
    }

    public void Write(string filePath) {
      XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
      xmlWriterSettings.Indent = true;
      using (XmlWriter xmlWriter = XmlWriter.Create(filePath, xmlWriterSettings)) {
        WorldMetadata.GetSerializer().Serialize(xmlWriter, this);
      }
    }

    private static XmlSerializer cachedSerializer;
    private static XmlSerializer GetSerializer() {
      if (WorldMetadata.cachedSerializer == null) {
        WorldMetadata.cachedSerializer = new XmlSerializer(
          typeof(WorldMetadata), new[] {
            typeof(GateStatesDictionary), typeof(ActiveTimersDictionary), typeof(GateStateMetadata), typeof(ActiveTimerMetadata),
            typeof(Collection<DPoint>)
          }
        );
      }

      return WorldMetadata.cachedSerializer;
    }
    #endregion

    #region [Method: Constructor]
    public WorldMetadata() {
      this.gateStates = new GateStatesDictionary();
      this.activeTimers = new ActiveTimersDictionary();
      this.clockLocations = new Collection<DPoint>();
      this.activeSwapperLocations = new Collection<DPoint>();
    }
    #endregion
  }
}
