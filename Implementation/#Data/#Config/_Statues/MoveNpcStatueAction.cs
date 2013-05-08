using System;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class MoveNpcStatueAction: NullStatueAction {
    #region [Property: NpcId]
    private int npcType;

    public int NpcType {
      get { return this.npcType; }
      set { this.npcType = value; }
    }
    #endregion

    #region [Property: SpawnIfNotExistent]
    private bool spawnIfNotExistent;

    public bool SpawnIfNotExistent {
      get { return this.spawnIfNotExistent; }
      set { this.spawnIfNotExistent = value; }
    }
    #endregion

    #region [Property: CheckRange]
    private int checkRange;

    public int CheckRange {
      get { return this.checkRange; }
      set { this.checkRange = value; }
    }
    #endregion


    #region [Method: Static FromXmlElement]
    public static new MoveNpcStatueAction FromXmlElement(XmlElement xmlData) {
      MoveNpcStatueAction resultingAction = new MoveNpcStatueAction();
      resultingAction.npcType = int.Parse(xmlData["NpcId"].InnerText);
      resultingAction.spawnIfNotExistent = bool.Parse(xmlData["SpawnIfNotExistent"].InnerText);
      resultingAction.checkRange = int.Parse(xmlData["CheckRange"].InnerText);

      return resultingAction;
    }
    #endregion
  }
}
