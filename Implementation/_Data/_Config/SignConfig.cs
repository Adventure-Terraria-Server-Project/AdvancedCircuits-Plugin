using System;
using System.Diagnostics.Contracts;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class SignConfig {
    public string ReadPrefix { get; set; }
    public string WirePermission { get; set; }
    public string PassiveTriggerPermission { get; set; }
    public string TriggerSignCommandPermission { get; set; }


    public SignConfig() {
      this.ReadPrefix = "Sign: ";
    }

    public static SignConfig FromXmlElement(XmlElement xmlData) {
      SignConfig resultingSignConfig = new SignConfig();
      resultingSignConfig.ReadPrefix = xmlData["ReadPrefix"].InnerText;

      if (xmlData["WirePermission"] != null)
        resultingSignConfig.WirePermission = xmlData["WirePermission"].InnerText;
      if (xmlData["PassiveTriggerPermission"] != null)
        resultingSignConfig.PassiveTriggerPermission = xmlData["PassiveTriggerPermission"].InnerText;
      if (xmlData["TriggerSignCommandPermission"] != null)
        resultingSignConfig.TriggerSignCommandPermission = xmlData["TriggerSignCommandPermission"].InnerText;

      return resultingSignConfig;
    }
  }
}