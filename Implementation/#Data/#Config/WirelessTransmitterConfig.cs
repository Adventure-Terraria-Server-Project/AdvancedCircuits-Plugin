using System;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class WirelessTransmitterConfig {
    #region [Property: WirePermission]
    private string wirePermission;

    public string WirePermission {
      get { return this.wirePermission; }
      set { this.wirePermission = value; }
    }
    #endregion

    #region [Property: TriggerPermission]
    private string triggerPermission;

    public string TriggerPermission {
      get { return this.triggerPermission; }
      set { this.triggerPermission = value; }
    }
    #endregion

    #region [Property: Range]
    private int range;

    public int Range {
      get { return this.range; }
      set { this.range = value; }
    }
    #endregion

    #region [Property: Network]
    private int network;

    public int Network {
      get { return this.network; }
      set { this.network = value; }
    }
    #endregion

    #region [Property: Cooldown]
    private int cooldown;

    public int Cooldown {
      get { return this.cooldown; }
      set { this.cooldown = value; }
    }
    #endregion


    #region [Methods: Constructor, Static FromXmlElement]
    public WirelessTransmitterConfig() {
      this.cooldown = 10;
      this.range = 140;
    }

    public static WirelessTransmitterConfig FromXmlElement(XmlElement xmlData) {
      WirelessTransmitterConfig resultingConfig = new WirelessTransmitterConfig();
      resultingConfig.network = int.Parse(xmlData["Network"].InnerText);
      resultingConfig.range = int.Parse(xmlData["Range"].InnerText);
      resultingConfig.cooldown = int.Parse(xmlData["Cooldown"].InnerText);
      if (xmlData["WirePermission"] != null)
        resultingConfig.wirePermission = xmlData["WirePermission"].InnerText;
      if (xmlData["TriggerPermission"] != null)
        resultingConfig.triggerPermission = xmlData["TriggerPermission"].InnerText;

      return resultingConfig;
    }
    #endregion
  }
}
