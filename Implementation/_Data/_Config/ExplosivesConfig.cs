using System;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class ExplosivesConfig {
    public ExplosionAreaShape BlastAreaShape { get; set; }
    public int BlastAreaWidth { get; set; }
    public int BlastAreaHeight { get; set; }
    public BlockDestroyMethod BlockDestroyMethod { get; set; }
    public bool DropDestroyedBlocks { get; set; }
    public bool DropDestroyedWires { get; set; }
    public bool DestroyWalls { get; set; }
    public bool DestroyWires { get; set; }
    public bool RemoveLiquid { get; set; }
    public int PlayerDamage { get; set; }
    public int NpcDamage { get; set; }
    public int MobDamage { get; set; }
    public string WirePermission { get; set; }
    public string TriggerPermission { get; set; }


    public ExplosivesConfig() {
      this.BlastAreaShape = ExplosionAreaShape.Circle;
      this.BlastAreaWidth = 20;
      this.BlastAreaHeight = 20;
      this.BlockDestroyMethod = BlockDestroyMethod.DestroyNonBombProofBlocks;
      this.DropDestroyedBlocks = true;
      this.DropDestroyedWires = false;
      this.DestroyWalls = true;
      this.DestroyWires = true;
      this.RemoveLiquid = false;
      this.PlayerDamage = 250;
      this.NpcDamage = 125;
      this.MobDamage = 125;
    }

    public static ExplosivesConfig FromXmlElement(XmlElement xmlData) {
      ExplosivesConfig resultingExplosivesConfig = new ExplosivesConfig();
      resultingExplosivesConfig.BlastAreaShape = (ExplosionAreaShape)Enum.Parse(typeof(ExplosionAreaShape), xmlData["BlastAreaShape"].InnerText);
      resultingExplosivesConfig.BlastAreaWidth = int.Parse(xmlData["BlastAreaWidth"].InnerText);
      resultingExplosivesConfig.BlastAreaHeight = int.Parse(xmlData["BlastAreaHeight"].InnerText);
      resultingExplosivesConfig.BlockDestroyMethod = (BlockDestroyMethod)Enum.Parse(typeof(BlockDestroyMethod), xmlData["BlockDestroyMethod"].InnerText);
      resultingExplosivesConfig.DropDestroyedBlocks = bool.Parse(xmlData["DropDestroyedBlocks"].InnerText);
      resultingExplosivesConfig.DropDestroyedWires = bool.Parse(xmlData["DropDestroyedWires"].InnerText);
      resultingExplosivesConfig.DestroyWalls = bool.Parse(xmlData["DestroyWalls"].InnerText);
      resultingExplosivesConfig.DestroyWires = bool.Parse(xmlData["DestroyWires"].InnerText);
      resultingExplosivesConfig.RemoveLiquid = bool.Parse(xmlData["RemoveLiquid"].InnerText);
      resultingExplosivesConfig.PlayerDamage = int.Parse(xmlData["PlayerDamage"].InnerText);
      resultingExplosivesConfig.NpcDamage = int.Parse(xmlData["NpcDamage"].InnerText);
      resultingExplosivesConfig.MobDamage = int.Parse(xmlData["MobDamage"].InnerText);

      if (xmlData["TriggerPermission"] != null)
        resultingExplosivesConfig.TriggerPermission = xmlData["TriggerPermission"].InnerText;
      if (xmlData["WirePermission"] != null)
        resultingExplosivesConfig.WirePermission = xmlData["WirePermission"].InnerText;

      return resultingExplosivesConfig;
    }
  }
}
