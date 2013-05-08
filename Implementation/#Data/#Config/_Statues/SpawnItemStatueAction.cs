using System;
using System.Xml;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class SpawnItemStatueAction: NullStatueAction {
    #region [Property: ItemId]
    private ItemType itemType;

    public ItemType ItemType {
      get { return this.itemType; }
      set { this.itemType = value; }
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
    public static new SpawnItemStatueAction FromXmlElement(XmlElement xmlData) {
      SpawnItemStatueAction resultingAction = new SpawnItemStatueAction();
      resultingAction.itemType = (ItemType)int.Parse(xmlData["ItemId"].InnerText);
      resultingAction.amount = int.Parse(xmlData["Amount"].InnerText);
      resultingAction.checkRange = int.Parse(xmlData["CheckRange"].InnerText);
      resultingAction.checkAmount = int.Parse(xmlData["CheckAmount"].InnerText);

      return resultingAction;
    }
    #endregion
  }
}
