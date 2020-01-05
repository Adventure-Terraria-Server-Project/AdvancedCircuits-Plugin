using System;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class SignConfig {
    public string ReadPrefix { get; set; }


    public SignConfig() {
      this.ReadPrefix = "Sign: ";
    }

    public static SignConfig FromXmlElement(XmlElement xmlData) {
      SignConfig resultingSignConfig = new SignConfig();
      resultingSignConfig.ReadPrefix = xmlData["ReadPrefix"].InnerText;

      return resultingSignConfig;
    }
  }
}