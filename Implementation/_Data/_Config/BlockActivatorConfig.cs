using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class BlockActivatorConfig {
    public int MaxChangeableBlocks { get; set; }
    public int Cooldown { get; set; }


    public BlockActivatorConfig() {
      this.MaxChangeableBlocks = 100;
      this.Cooldown = 30;
    }

    public static BlockActivatorConfig FromXmlElement(XmlElement xmlData) {
      BlockActivatorConfig resultingBlockActivatorConfig = new BlockActivatorConfig();
      resultingBlockActivatorConfig.MaxChangeableBlocks = int.Parse(xmlData["MaxChangeableBlocks"].InnerText);
      resultingBlockActivatorConfig.Cooldown = int.Parse(xmlData["Cooldown"].InnerText);

      return resultingBlockActivatorConfig;
    }
  }
}
