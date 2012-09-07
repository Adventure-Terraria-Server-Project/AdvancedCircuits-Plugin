// This file is provided unter the terms of the 
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// 
// Written by CoderCow

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Xml;
using System.Xml.Serialization;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class StatueConfigsDictionary: SerializableDictionary<StatueType,StatueConfig> {
    public StatueConfigsDictionary(): base("StatueConfigItem", "StatueType") {} 
  }

  [XmlRoot("AdvancedCircuitsConfiguration")]
  public class Configuration {
    #region [Property: OverrideVanillaCircuits]
    private bool overrideVanillaCircuits;

    public bool OverrideVanillaCircuits {
      get { return this.overrideVanillaCircuits; }
      set { this.overrideVanillaCircuits = value; }
    }
    #endregion

    #region [Property: AdvancedCircuitsEnabled]
    private bool advancedCircuitsEnabled;

    public bool AdvancedCircuitsEnabled {
      get { return this.advancedCircuitsEnabled; }
      set { this.advancedCircuitsEnabled = value; }
    }
    #endregion

    #region [Property: MaxDartTrapsPerCircuit]
    private int maxDartTrapsPerCircuit;

    public int MaxDartTrapsPerCircuit {
      get { return this.maxDartTrapsPerCircuit; }
      set { this.maxDartTrapsPerCircuit = value; }
    }
    #endregion

    #region [Property: MaxStatuesPerCircuit]
    private int maxStatuesPerCircuit;

    public int MaxStatuesPerCircuit {
      get { return this.maxStatuesPerCircuit; }
      set { this.maxStatuesPerCircuit = value; }
    }
    #endregion

    #region [Property: MaxPumpsPerCircuit]
    private int maxPumpsPerCircuit;

    public int MaxPumpsPerCircuit {
      get { return this.maxPumpsPerCircuit; }
      set { this.maxPumpsPerCircuit = value; }
    }
    #endregion

    #region [Property: MaxCircuitLength]
    private int maxCircuitLength;

    public int MaxCircuitLength {
      get { return this.maxCircuitLength; }
      set { this.maxCircuitLength = value; }
    }
    #endregion

    #region [Property: DartTrapConfig]
    private DartTrapConfig dartTrapConfig;

    public DartTrapConfig DartTrapConfig {
      get { return this.dartTrapConfig; }
      set { this.dartTrapConfig = value; }
    }
    #endregion

    #region [Property: StatueConfigs]
    private StatueConfigsDictionary statueConfigs;

    public StatueConfigsDictionary StatueConfigs {
      get { return this.statueConfigs; }
      set { this.statueConfigs = value; }
    }
    #endregion


    #region [Methods: Static Read]
    public static Configuration Read(string filePath) {
      XmlSerializer configSerializer = new XmlSerializer(typeof(Configuration));
      using (XmlReader xmlReader = XmlReader.Create(filePath)) {
        return (Configuration)configSerializer.Deserialize(xmlReader);
      }
    }
    #endregion

    #region [Methods: Constructor]
    public Configuration() {
      this.overrideVanillaCircuits = false;
      this.advancedCircuitsEnabled = true;
      this.maxDartTrapsPerCircuit = 5;
      this.maxStatuesPerCircuit = 10;
      this.maxPumpsPerCircuit = 4;
      this.maxCircuitLength = 500;

      this.dartTrapConfig = new DartTrapConfig();
      this.statueConfigs = new StatueConfigsDictionary();
    }
    #endregion
  }
}
