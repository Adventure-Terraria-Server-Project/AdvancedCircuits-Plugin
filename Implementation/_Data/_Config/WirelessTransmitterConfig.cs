using System;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class WirelessTransmitterConfig {
    public string WirePermission { get; set; }
    public string TriggerPermission { get; set; }
    public int Range { get; set; }
    public int Network { get; set; }
    public int Cooldown { get; set; }


    public WirelessTransmitterConfig() {
      this.Cooldown = 10;
      this.Range = 140;
    }

    public static WirelessTransmitterConfig FromXmlElement(XmlElement xmlData) {
      WirelessTransmitterConfig resultingConfig = new WirelessTransmitterConfig();
      resultingConfig.Network = int.Parse(xmlData["Network"].InnerText);
      resultingConfig.Range = int.Parse(xmlData["Range"].InnerText);
      resultingConfig.Cooldown = int.Parse(xmlData["Cooldown"].InnerText);

      if (xmlData["WirePermission"] != null)
        resultingConfig.WirePermission = xmlData["WirePermission"].InnerText;
      if (xmlData["TriggerPermission"] != null)
        resultingConfig.TriggerPermission = xmlData["TriggerPermission"].InnerText;

      return resultingConfig;
    }
  }
}
