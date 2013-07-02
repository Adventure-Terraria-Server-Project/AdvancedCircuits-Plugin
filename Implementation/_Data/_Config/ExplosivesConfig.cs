using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class ExplosivesConfig {
    #region [Property: BlastAreaShape]
    private ExplosionAreaShape blastAreaShape;

    public ExplosionAreaShape BlastAreaShape {
      get { return this.blastAreaShape; }
      set { this.blastAreaShape = value; }
    }
    #endregion

    #region [Property: BlastAreaWidth]
    private int blastAreaWidth;

    public int BlastAreaWidth {
      get { return this.blastAreaWidth; }
      set { this.blastAreaWidth = value; }
    }
    #endregion

    #region [Property: BlastAreaHeight]
    private int blastAreaHeight;

    public int BlastAreaHeight {
      get { return this.blastAreaHeight; }
      set { this.blastAreaHeight = value; }
    }
    #endregion

    #region [Property: BlockDestroyMethod]
    private BlockDestroyMethod blockDestroyMethod;

    public BlockDestroyMethod BlockDestroyMethod {
      get { return this.blockDestroyMethod; }
      set { this.blockDestroyMethod = value; }
    }
    #endregion

    #region [Property: DropDestroyedBlocks]
    private bool dropDestroyedBlocks;

    public bool DropDestroyedBlocks {
      get { return this.dropDestroyedBlocks; }
      set { this.dropDestroyedBlocks = value; }
    }
    #endregion

    #region [Property: DropDestroyedWires]
    private bool dropDestroyedWires;

    public bool DropDestroyedWires {
      get { return this.dropDestroyedWires; }
      set { this.dropDestroyedWires = value; }
    }
    #endregion

    #region [Property: DestroyWalls]
    private bool destroyWalls;

    public bool DestroyWalls {
      get { return this.destroyWalls; }
      set { this.destroyWalls = value; }
    }
    #endregion

    #region [Property: DestroyWires]
    private bool destroyWires;

    public bool DestroyWires {
      get { return this.destroyWires; }
      set { this.destroyWires = value; }
    }
    #endregion

    #region [Property: RemoveLiquid]
    private bool removeLiquid;

    public bool RemoveLiquid {
      get { return this.removeLiquid; }
      set { this.removeLiquid = value; }
    }
    #endregion

    #region [Property: PlayerDamage]
    private int playerDamage;

    public int PlayerDamage {
      get { return this.playerDamage; }
      set { this.playerDamage = value; }
    }
    #endregion

    #region [Property: NpcDamage]
    private int npcDamage;

    public int NpcDamage {
      get { return this.npcDamage; }
      set { this.npcDamage = value; }
    }
    #endregion

    #region [Property: MobDamage]
    private int mobDamage;

    public int MobDamage {
      get { return this.mobDamage; }
      set { this.mobDamage = value; }
    }
    #endregion

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


    #region [Methods: Constructor, Static FromXmlElement]
    public ExplosivesConfig() {
      this.blastAreaShape = ExplosionAreaShape.Circle;
      this.blastAreaWidth = 20;
      this.blastAreaHeight = 20;
      this.blockDestroyMethod = BlockDestroyMethod.DestroyNonBombProofBlocks;
      this.dropDestroyedBlocks = true;
      this.dropDestroyedWires = false;
      this.destroyWalls = true;
      this.destroyWires = true;
      this.removeLiquid = false;
      this.playerDamage = 250;
      this.npcDamage = 125;
      this.mobDamage = 125;
    }

    public static ExplosivesConfig FromXmlElement(XmlElement xmlData) {
      ExplosivesConfig resultingExplosivesConfig = new ExplosivesConfig();
      resultingExplosivesConfig.blastAreaShape = (ExplosionAreaShape)Enum.Parse(typeof(ExplosionAreaShape), xmlData["BlastAreaShape"].InnerText);
      resultingExplosivesConfig.blastAreaWidth = int.Parse(xmlData["BlastAreaWidth"].InnerText);
      resultingExplosivesConfig.blastAreaHeight = int.Parse(xmlData["BlastAreaHeight"].InnerText);
      resultingExplosivesConfig.blockDestroyMethod = (BlockDestroyMethod)Enum.Parse(typeof(BlockDestroyMethod), xmlData["BlockDestroyMethod"].InnerText);
      resultingExplosivesConfig.dropDestroyedBlocks = bool.Parse(xmlData["DropDestroyedBlocks"].InnerText);
      resultingExplosivesConfig.dropDestroyedWires = bool.Parse(xmlData["DropDestroyedWires"].InnerText);
      resultingExplosivesConfig.destroyWalls = bool.Parse(xmlData["DestroyWalls"].InnerText);
      resultingExplosivesConfig.destroyWires = bool.Parse(xmlData["DestroyWires"].InnerText);
      resultingExplosivesConfig.removeLiquid = bool.Parse(xmlData["RemoveLiquid"].InnerText);
      resultingExplosivesConfig.playerDamage = int.Parse(xmlData["PlayerDamage"].InnerText);
      resultingExplosivesConfig.npcDamage = int.Parse(xmlData["NpcDamage"].InnerText);
      resultingExplosivesConfig.mobDamage = int.Parse(xmlData["MobDamage"].InnerText);

      if (xmlData["TriggerPermission"] != null)
        resultingExplosivesConfig.triggerPermission = xmlData["TriggerPermission"].InnerText;
      if (xmlData["WirePermission"] != null)
        resultingExplosivesConfig.wirePermission = xmlData["WirePermission"].InnerText;

      return resultingExplosivesConfig;
    }
    #endregion
  }
}
