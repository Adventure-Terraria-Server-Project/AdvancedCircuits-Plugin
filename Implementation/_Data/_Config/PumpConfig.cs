using System;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class PumpConfig {
    public int TransferableWater { get; set; }
    public int TransferableLava { get; set; }
    public int TransferableHoney { get; set; }
    public int LossValue { get; set; }
    public int Cooldown { get; set; }
    public string TriggerPermission { get; set; }
    public string WirePermission { get; set; }


    public PumpConfig() {
      this.TransferableWater = 1020;
      this.TransferableLava = 1020;
      this.TransferableHoney = 1020;
      this.LossValue = 1;
      this.Cooldown = 10;
    }

    public static PumpConfig FromXmlElement(XmlElement xmlData) {
      PumpConfig resultingPumpConfig = new PumpConfig();
      resultingPumpConfig.TransferableWater = int.Parse(xmlData["TransferableWater"].InnerText);
      resultingPumpConfig.TransferableLava = int.Parse(xmlData["TransferableLava"].InnerText);
      resultingPumpConfig.TransferableHoney = int.Parse(xmlData["TransferableHoney"].InnerText);
      resultingPumpConfig.LossValue = int.Parse(xmlData["LossValue"].InnerText);
      resultingPumpConfig.Cooldown = int.Parse(xmlData["Cooldown"].InnerText);

      if (xmlData["TriggerPermission"] != null)
        resultingPumpConfig.TriggerPermission = xmlData["TriggerPermission"].InnerText;
      if (xmlData["WirePermission"] != null)
        resultingPumpConfig.WirePermission = xmlData["WirePermission"].InnerText;

      return resultingPumpConfig;
    }
  }
}
