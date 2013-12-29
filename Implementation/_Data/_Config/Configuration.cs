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
    public const string CurrentVersion = "1.2";

    public bool OverrideVanillaCircuits { get; set; }
    public bool AdvancedCircuitsEnabled { get; set; }
    public int MaxDartTrapsPerCircuit { get; set; }
    public int MaxStatuesPerCircuit { get; set; }
    public int MaxPumpsPerCircuit { get; set; }
    public int MaxCircuitLength { get; set; }
    public string BoulderWirePermission { get; set; }
    public SignConfig SignConfig { get; set; }
    public BlockActivatorConfig BlockActivatorConfig { get; set; }
    public Dictionary<ComponentConfigProfile,PumpConfig> PumpConfigs { get; set; }
    public Dictionary<ComponentConfigProfile,DartTrapConfig> DartTrapConfigs { get; set; }
    public Dictionary<ComponentConfigProfile,ExplosivesConfig> ExplosivesConfigs { get; set; }
    public Dictionary<ComponentConfigProfile,WirelessTransmitterConfig> WirelessTransmitterConfigs { get; set; }
    public Dictionary<StatueStyle,StatueConfig> StatueConfigs { get; set; }


    public Configuration(): this(true) {}

    protected Configuration(bool fillDictionaries) {
      this.OverrideVanillaCircuits = false;
      this.AdvancedCircuitsEnabled = true;
      this.MaxDartTrapsPerCircuit = 10;
      this.MaxStatuesPerCircuit = 10;
      this.MaxPumpsPerCircuit = 4;
      this.MaxCircuitLength = 400;

      this.SignConfig = new SignConfig();
      this.BlockActivatorConfig = new BlockActivatorConfig();

      this.PumpConfigs = new Dictionary<ComponentConfigProfile,PumpConfig>();
      if (fillDictionaries)
        this.PumpConfigs.Add(ComponentConfigProfile.Default, new PumpConfig());

      this.DartTrapConfigs = new Dictionary<ComponentConfigProfile,DartTrapConfig>();
      if (fillDictionaries)
        this.DartTrapConfigs.Add(ComponentConfigProfile.Default, new DartTrapConfig());

      this.WirelessTransmitterConfigs = new Dictionary<ComponentConfigProfile,WirelessTransmitterConfig>();
      if (fillDictionaries)
        this.WirelessTransmitterConfigs.Add(ComponentConfigProfile.Default, new WirelessTransmitterConfig());

      this.ExplosivesConfigs = new Dictionary<ComponentConfigProfile,ExplosivesConfig>();
      if (fillDictionaries)
        this.ExplosivesConfigs.Add(ComponentConfigProfile.Default, new ExplosivesConfig());

      this.StatueConfigs = new Dictionary<StatueStyle,StatueConfig>();
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
      resultingConfig.OverrideVanillaCircuits = BoolEx.ParseEx(rootElement["OverrideVanillaCircuits"].InnerXml);
      resultingConfig.AdvancedCircuitsEnabled = BoolEx.ParseEx(rootElement["AdvancedCircuitsEnabled"].InnerText);
      resultingConfig.MaxDartTrapsPerCircuit  = int.Parse(rootElement["MaxDartTrapsPerCircuit"].InnerText);
      resultingConfig.MaxStatuesPerCircuit    = int.Parse(rootElement["MaxStatuesPerCircuit"].InnerText);
      resultingConfig.MaxPumpsPerCircuit      = int.Parse(rootElement["MaxPumpsPerCircuit"].InnerText);
      resultingConfig.MaxCircuitLength        = int.Parse(rootElement["MaxCircuitLength"].InnerText);
      if (rootElement["BoulderWirePermission"] != null)
        resultingConfig.BoulderWirePermission = rootElement["BoulderWirePermission"].InnerText;
      resultingConfig.SignConfig              = SignConfig.FromXmlElement(rootElement["SignConfig"]);
      resultingConfig.BlockActivatorConfig    = BlockActivatorConfig.FromXmlElement(rootElement["BlockActivatorConfig"]);

      XmlElement pumpConfigsNode = rootElement["PumpConfigs"];
      foreach (XmlNode pumpConfigNode in pumpConfigsNode.ChildNodes) {
        XmlElement pumpConfigElement = (pumpConfigNode as XmlElement);
        if (pumpConfigElement == null)
          continue;

        ComponentConfigProfile componentConfigProfile = (ComponentConfigProfile)Enum.Parse(typeof(ComponentConfigProfile), pumpConfigElement.Attributes["Profile"].Value);
        resultingConfig.PumpConfigs.Add(componentConfigProfile, PumpConfig.FromXmlElement(pumpConfigElement));
      }

      XmlElement dartTrapConfigsNode = rootElement["DartTrapConfigs"];
      foreach (XmlNode dartTrapConfigNode in dartTrapConfigsNode.ChildNodes) {
        XmlElement dartTrapConfigElement = (dartTrapConfigNode as XmlElement);
        if (dartTrapConfigElement == null)
          continue;

        ComponentConfigProfile componentConfigProfile = (ComponentConfigProfile)Enum.Parse(typeof(ComponentConfigProfile), dartTrapConfigElement.Attributes["Profile"].Value);
        resultingConfig.DartTrapConfigs.Add(componentConfigProfile, DartTrapConfig.FromXmlElement(dartTrapConfigElement));
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
        resultingConfig.WirelessTransmitterConfigs.Add(componentConfigProfile, WirelessTransmitterConfig.FromXmlElement(wirelessTransmitterConfigElement));
      }

      XmlElement statueConfigsNode = rootElement["StatueConfigs"];
      foreach (XmlNode statueConfigNode in statueConfigsNode.ChildNodes) {
        XmlElement statueConfigElement = (statueConfigNode as XmlElement);
        if (statueConfigElement == null)
          continue;

        StatueStyle statueStyle = (StatueStyle)Enum.Parse(typeof(StatueStyle), statueConfigElement.Attributes["StatueType"].Value);
        resultingConfig.StatueConfigs.Add(statueStyle, StatueConfig.FromXmlElement(statueConfigElement));
      }

      return resultingConfig;
    }
  }
}
