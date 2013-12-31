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
    public const string CurrentVersion = "1.3";

    public bool OverrideVanillaCircuits { get; set; }
    public bool AdvancedCircuitsEnabled { get; set; }
    public int MaxTrapsPerCircuit { get; set; }
    public int MaxStatuesPerCircuit { get; set; }
    public int MaxPumpsPerCircuit { get; set; }
    public int MaxCircuitLength { get; set; }
    public TimeSpan MaxTimerActivityTime { get; set; }
    public SignConfig SignConfig { get; set; }
    public BlockActivatorConfig BlockActivatorConfig { get; set; }
    public Dictionary<PaintColor,PumpConfig> PumpConfigs { get; set; }
    public Dictionary<TrapConfigKey,TrapConfig> TrapConfigs { get; set; }
    //public Dictionary<PaintColor,ExplosivesConfig> ExplosivesConfigs { get; set; }
    public Dictionary<PaintColor,WirelessTransmitterConfig> WirelessTransmitterConfigs { get; set; }
    public Dictionary<StatueStyle,StatueConfig> StatueConfigs { get; set; }


    public Configuration(): this(true) {}

    protected Configuration(bool fillDictionaries) {
      this.OverrideVanillaCircuits = true;
      this.AdvancedCircuitsEnabled = true;
      this.MaxTrapsPerCircuit = 10;
      this.MaxStatuesPerCircuit = 10;
      this.MaxPumpsPerCircuit = 4;
      this.MaxCircuitLength = 1200;
      this.MaxTimerActivityTime = TimeSpan.FromHours(24);

      this.SignConfig = new SignConfig();
      this.BlockActivatorConfig = new BlockActivatorConfig();

      this.PumpConfigs = new Dictionary<PaintColor,PumpConfig>();
      if (fillDictionaries)
        this.PumpConfigs.Add(PaintColor.None, new PumpConfig());

      this.TrapConfigs = new Dictionary<TrapConfigKey,TrapConfig>();
      if (fillDictionaries) { 
        this.TrapConfigs.Add(new TrapConfigKey(TrapStyle.DartTrap, PaintColor.None), new TrapConfig());
        this.TrapConfigs.Add(new TrapConfigKey(TrapStyle.SuperDartTrap, PaintColor.None), new TrapConfig());
        this.TrapConfigs.Add(new TrapConfigKey(TrapStyle.FlameTrap, PaintColor.None), new TrapConfig());
        this.TrapConfigs.Add(new TrapConfigKey(TrapStyle.SpikyBallTrap, PaintColor.None), new TrapConfig());
        this.TrapConfigs.Add(new TrapConfigKey(TrapStyle.SpearTrap, PaintColor.None), new TrapConfig());
      }

      this.WirelessTransmitterConfigs = new Dictionary<PaintColor,WirelessTransmitterConfig>();
      if (fillDictionaries)
        this.WirelessTransmitterConfigs.Add(PaintColor.None, new WirelessTransmitterConfig());

      /*this.ExplosivesConfigs = new Dictionary<PaintColor,ExplosivesConfig>();
      if (fillDictionaries)
        this.ExplosivesConfigs.Add(PaintColor.None, new ExplosivesConfig());*/

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
      using (XmlReader configReader = XmlReader.Create(filePath, configReaderSettings))
        document.Load(configReader);

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
      resultingConfig.MaxTrapsPerCircuit  = int.Parse(rootElement["MaxTrapsPerCircuit"].InnerText);
      resultingConfig.MaxStatuesPerCircuit    = int.Parse(rootElement["MaxStatuesPerCircuit"].InnerText);
      resultingConfig.MaxPumpsPerCircuit      = int.Parse(rootElement["MaxPumpsPerCircuit"].InnerText);
      resultingConfig.MaxCircuitLength        = int.Parse(rootElement["MaxCircuitLength"].InnerText);
      if (string.IsNullOrWhiteSpace(rootElement["MaxTimerActivityTime"].InnerText)) {
        resultingConfig.MaxTimerActivityTime = TimeSpan.Zero;
      } else {
        TimeSpan maxTimerActivityTime;
        if (TimeSpanEx.TryParseShort(rootElement["MaxTimerActivityTime"].InnerText, out maxTimerActivityTime))
          resultingConfig.MaxTimerActivityTime = maxTimerActivityTime;
      }
      resultingConfig.SignConfig              = SignConfig.FromXmlElement(rootElement["SignConfig"]);
      resultingConfig.BlockActivatorConfig    = BlockActivatorConfig.FromXmlElement(rootElement["BlockActivatorConfig"]);

      XmlElement pumpConfigsNode = rootElement["PumpConfigs"];
      foreach (XmlNode pumpConfigNode in pumpConfigsNode.ChildNodes) {
        XmlElement pumpConfigElement = (pumpConfigNode as XmlElement);
        if (pumpConfigElement == null)
          continue;

        PaintColor paintColor = (PaintColor)Enum.Parse(typeof(PaintColor), pumpConfigElement.Attributes["Paint"].Value);
        resultingConfig.PumpConfigs.Add(paintColor, PumpConfig.FromXmlElement(pumpConfigElement));
      }

      XmlElement trapConfigsNode = rootElement["TrapConfigs"];
      foreach (XmlNode trapConfigNode in trapConfigsNode.ChildNodes) {
        XmlElement trapConfigElement = (trapConfigNode as XmlElement);
        if (trapConfigElement == null)
          continue;
        
        TrapStyle trapStyle = (TrapStyle)Enum.Parse(typeof(TrapStyle), trapConfigElement.Attributes["TrapType"].Value);
        PaintColor paintColor = (PaintColor)Enum.Parse(typeof(PaintColor), trapConfigElement.Attributes["Paint"].Value);
        resultingConfig.TrapConfigs.Add(new TrapConfigKey(trapStyle, paintColor), TrapConfig.FromXmlElement(trapConfigElement));
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

        PaintColor paintColor = (PaintColor)Enum.Parse(typeof(PaintColor), wirelessTransmitterConfigElement.Attributes["Paint"].Value);
        resultingConfig.WirelessTransmitterConfigs.Add(paintColor, WirelessTransmitterConfig.FromXmlElement(wirelessTransmitterConfigElement));
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
