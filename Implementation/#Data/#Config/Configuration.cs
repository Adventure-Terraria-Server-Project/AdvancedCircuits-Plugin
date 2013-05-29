using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class Configuration {
    #region [Constants]
    public const string CurrentVersion = "1.2";
    #endregion

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

    #region [Property: BoulderWirePermission]
    private string boulderWirePermission;

    public string BoulderWirePermission {
      get { return this.boulderWirePermission; }
      set { this.boulderWirePermission = value; }
    }
    #endregion

    #region [Property: SignConfig]
    private SignConfig signConfig;

    public SignConfig SignConfig {
      get { return this.signConfig; }
      set { this.signConfig = value; }
    }
    #endregion

    #region [Property: BlockActivatorConfig]
    private BlockActivatorConfig blockActivatorConfig;

    public BlockActivatorConfig BlockActivatorConfig {
      get { return this.blockActivatorConfig; }
      set { this.blockActivatorConfig = value; }
    }
    #endregion

    #region [Property: PumpConfigs]
    private Dictionary<ComponentConfigProfile,PumpConfig> pumpConfigs;

    public Dictionary<ComponentConfigProfile,PumpConfig> PumpConfigs {
      get { return this.pumpConfigs; }
      set { this.pumpConfigs = value; }
    }
    #endregion

    #region [Property: DartTrapConfigs]
    private Dictionary<ComponentConfigProfile,DartTrapConfig> dartTrapConfigs;

    public Dictionary<ComponentConfigProfile,DartTrapConfig> DartTrapConfigs {
      get { return this.dartTrapConfigs; }
      set { this.dartTrapConfigs = value; }
    }
    #endregion

    #region [Property: ExplosivesConfigs]
    private Dictionary<ComponentConfigProfile,ExplosivesConfig> explosivesConfigs;

    public Dictionary<ComponentConfigProfile,ExplosivesConfig> ExplosivesConfigs {
      get { return this.explosivesConfigs; }
      set { this.explosivesConfigs = value; }
    }
    #endregion

    #region [Property: WirelessTransmitterConfigs]
    private Dictionary<ComponentConfigProfile, WirelessTransmitterConfig> wirelessTransmitterConfigs;

    public Dictionary<ComponentConfigProfile,WirelessTransmitterConfig> WirelessTransmitterConfigs {
      get { return this.wirelessTransmitterConfigs; }
      set { this.wirelessTransmitterConfigs = value; }
    }
    #endregion

    #region [Property: StatueConfigs]
    private Dictionary<StatueStyle,StatueConfig> statueConfigs;

    public Dictionary<StatueStyle,StatueConfig> StatueConfigs {
      get { return this.statueConfigs; }
      set { this.statueConfigs = value; }
    }
    #endregion


    #region [Methods: Constructor, Static Read]
    public Configuration(): this(true) {}

    protected Configuration(bool fillDictionaries) {
      this.overrideVanillaCircuits = false;
      this.advancedCircuitsEnabled = true;
      this.maxDartTrapsPerCircuit = 10;
      this.maxStatuesPerCircuit = 10;
      this.maxPumpsPerCircuit = 4;
      this.maxCircuitLength = 400;

      this.signConfig = new SignConfig();
      this.blockActivatorConfig = new BlockActivatorConfig();

      this.pumpConfigs = new Dictionary<ComponentConfigProfile,PumpConfig>();
      if (fillDictionaries)
        this.pumpConfigs.Add(ComponentConfigProfile.Default, new PumpConfig());

      this.dartTrapConfigs = new Dictionary<ComponentConfigProfile,DartTrapConfig>();
      if (fillDictionaries)
        this.dartTrapConfigs.Add(ComponentConfigProfile.Default, new DartTrapConfig());

      this.wirelessTransmitterConfigs = new Dictionary<ComponentConfigProfile,WirelessTransmitterConfig>();
      if (fillDictionaries)
        this.wirelessTransmitterConfigs.Add(ComponentConfigProfile.Default, new WirelessTransmitterConfig());

      this.explosivesConfigs = new Dictionary<ComponentConfigProfile,ExplosivesConfig>();
      if (fillDictionaries)
        this.explosivesConfigs.Add(ComponentConfigProfile.Default, new ExplosivesConfig());

      this.statueConfigs = new Dictionary<StatueStyle,StatueConfig>();
    }

    public static Configuration Read(string filePath) {
      XmlReaderSettings configReaderSettings = new XmlReaderSettings {
        ValidationType = ValidationType.Schema,
        ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints | XmlSchemaValidationFlags.ReportValidationWarnings
      };
      
      string configSchemaPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".xsd");
      configReaderSettings.Schemas.Add(null, configSchemaPath);

      XmlDocument document = new XmlDocument();
      using (XmlReader configReader = XmlReader.Create(filePath, configReaderSettings)) {
        document.Load(configReader);
      }

      // Before validating using the schema, first check if the configuration file's version matches with the supported version.
      XmlElement rootElement = document.DocumentElement;
      string fileVersionRaw;
      if (rootElement.HasAttribute("Version"))
        fileVersionRaw = rootElement.GetAttribute("Version");
      else
        fileVersionRaw = "1.0";
      
      if (fileVersionRaw != Configuration.CurrentVersion) {
        throw new FormatException(string.Format(
          "The configuration file is either outdated or too new. Expected version was: {0}. File version is: {1}", 
          Configuration.CurrentVersion, fileVersionRaw
        ));
      }

      Configuration resultingConfig = new Configuration(false);
      resultingConfig.overrideVanillaCircuits = BoolEx.ParseEx(rootElement["OverrideVanillaCircuits"].InnerXml);
      resultingConfig.advancedCircuitsEnabled = BoolEx.ParseEx(rootElement["AdvancedCircuitsEnabled"].InnerText);
      resultingConfig.maxDartTrapsPerCircuit  = int.Parse(rootElement["MaxDartTrapsPerCircuit"].InnerText);
      resultingConfig.maxStatuesPerCircuit    = int.Parse(rootElement["MaxStatuesPerCircuit"].InnerText);
      resultingConfig.maxPumpsPerCircuit      = int.Parse(rootElement["MaxPumpsPerCircuit"].InnerText);
      resultingConfig.maxCircuitLength        = int.Parse(rootElement["MaxCircuitLength"].InnerText);
      resultingConfig.boulderWirePermission   = rootElement["BoulderWirePermission"].InnerText;
      resultingConfig.signConfig              = SignConfig.FromXmlElement(rootElement["SignConfig"]);
      resultingConfig.blockActivatorConfig    = BlockActivatorConfig.FromXmlElement(rootElement["BlockActivatorConfig"]);

      XmlElement pumpConfigsNode = rootElement["PumpConfigs"];
      foreach (XmlNode pumpConfigNode in pumpConfigsNode.ChildNodes) {
        XmlElement pumpConfigElement = (pumpConfigNode as XmlElement);
        if (pumpConfigElement == null)
          continue;

        ComponentConfigProfile componentConfigProfile = (ComponentConfigProfile)Enum.Parse(typeof(ComponentConfigProfile), pumpConfigElement.Attributes["Profile"].Value);
        resultingConfig.pumpConfigs.Add(componentConfigProfile, PumpConfig.FromXmlElement(pumpConfigElement));
      }

      XmlElement dartTrapConfigsNode = rootElement["DartTrapConfigs"];
      foreach (XmlNode dartTrapConfigNode in dartTrapConfigsNode.ChildNodes) {
        XmlElement dartTrapConfigElement = (dartTrapConfigNode as XmlElement);
        if (dartTrapConfigElement == null)
          continue;

        ComponentConfigProfile componentConfigProfile = (ComponentConfigProfile)Enum.Parse(typeof(ComponentConfigProfile), dartTrapConfigElement.Attributes["Profile"].Value);
        resultingConfig.dartTrapConfigs.Add(componentConfigProfile, DartTrapConfig.FromXmlElement(dartTrapConfigElement));
      }

      /*XmlElement explosivesConfigsNode = rootElement["ExplosivesConfigs"];
      foreach (XmlNode explosivesConfigNode in explosivesConfigsNode.ChildNodes) {
        XmlElement explosivesConfigElement = (explosivesConfigNode as XmlElement);
        if (explosivesConfigElement == null)
          continue;

        ComponentConfigProfile componentConfigProfile = (ComponentConfigProfile)Enum.Parse(typeof(ComponentConfigProfile), explosivesConfigElement.Attributes["Profile"].Value);
        resultingConfig.explosivesConfigs.Add(componentConfigProfile, ExplosivesConfig.FromXmlElement(explosivesConfigElement));
      }*/

      XmlElement wirelessTransmitterConfigsNode = rootElement["WirelessTransmitterConfigs"];
      foreach (XmlNode wirelessTransmitterConfigNode in wirelessTransmitterConfigsNode.ChildNodes) {
        XmlElement wirelessTransmitterConfigElement = (wirelessTransmitterConfigNode as XmlElement);
        if (wirelessTransmitterConfigElement == null)
          continue;

        ComponentConfigProfile componentConfigProfile = (ComponentConfigProfile)Enum.Parse(typeof(ComponentConfigProfile), wirelessTransmitterConfigElement.Attributes["Profile"].Value);
        resultingConfig.wirelessTransmitterConfigs.Add(componentConfigProfile, WirelessTransmitterConfig.FromXmlElement(wirelessTransmitterConfigElement));
      }

      XmlElement statueConfigsNode = rootElement["StatueConfigs"];
      foreach (XmlNode statueConfigNode in statueConfigsNode.ChildNodes) {
        XmlElement statueConfigElement = (statueConfigNode as XmlElement);
        if (statueConfigElement == null)
          continue;

        StatueStyle statueStyle = (StatueStyle)Enum.Parse(typeof(StatueStyle), statueConfigElement.Attributes["StatueType"].Value);
        resultingConfig.statueConfigs.Add(statueStyle, StatueConfig.FromXmlElement(statueConfigElement));
      }

      return resultingConfig;
    }
    #endregion
  }
}
