using System;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class SpawnNpcStatueAction: NullStatueAction {
    #region [Property: NpcId]
    private int npcType;

    public int NpcType {
      get { return this.npcType; }
      set { this.npcType = value; }
    }
    #endregion

    #region [Property: Amount]
    private int amount;

    public int Amount {
      get { return this.amount; }
      set { this.amount = value; }
    }
    #endregion

    #region [Property: CheckRange]
    private int checkRange;

    public int CheckRange {
      get { return this.checkRange; }
      set { this.checkRange = value; }
    }
    #endregion

    #region [Property: CheckAmount]
    private int checkAmount;

    public int CheckAmount {
      get { return this.checkAmount; }
      set { this.checkAmount = value; }
    }
    #endregion


    #region [Method: Static FromXmlElement]
    public static new SpawnNpcStatueAction FromXmlElement(XmlElement xmlData) {
      SpawnNpcStatueAction resultingAction = new SpawnNpcStatueAction();
      resultingAction.npcType = int.Parse(xmlData["NpcId"].InnerText);
      resultingAction.amount = int.Parse(xmlData["Amount"].InnerText);
      resultingAction.checkRange = int.Parse(xmlData["CheckRange"].InnerText);
      resultingAction.checkAmount = int.Parse(xmlData["CheckAmount"].InnerText);

      return resultingAction;
    }
    #endregion
  }
}
