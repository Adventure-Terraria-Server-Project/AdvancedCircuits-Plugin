using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class BlockActivatorConfig {
    #region [Property: TriggerPermission]
    private string triggerPermission;

    public string TriggerPermission {
      get { return this.triggerPermission; }
      set { this.triggerPermission = value; }
    }
    #endregion

    #region [Property: MaxChangeableBlocks]
    private int maxChangeableBlocks;

    public int MaxChangeableBlocks {
      get { return this.maxChangeableBlocks; }
      set { this.maxChangeableBlocks = value; }
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
    public BlockActivatorConfig() {
      this.maxChangeableBlocks = 100;
      this.cooldown = 30;
    }

    public static BlockActivatorConfig FromXmlElement(XmlElement xmlData) {
      BlockActivatorConfig resultingBlockActivatorConfig = new BlockActivatorConfig();
      resultingBlockActivatorConfig.triggerPermission = xmlData["TriggerPermission"].InnerText;
      resultingBlockActivatorConfig.maxChangeableBlocks = int.Parse(xmlData["MaxChangeableBlocks"].InnerText);
      resultingBlockActivatorConfig.cooldown = int.Parse(xmlData["Cooldown"].InnerText);

      return resultingBlockActivatorConfig;
    }
    #endregion
  }
}
