using System;
using System.Xml;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class SpawnItemStatueAction: NullStatueAction {
    public ItemType ItemType { get; set; }
    public int Amount { get; set; }
    public int CheckRange { get; set; }
    public int CheckAmount { get; set; }


    public static new SpawnItemStatueAction FromXmlElement(XmlElement xmlData) {
      SpawnItemStatueAction resultingAction = new SpawnItemStatueAction();
      resultingAction.ItemType = (ItemType)int.Parse(xmlData["ItemId"].InnerText);
      resultingAction.Amount = int.Parse(xmlData["Amount"].InnerText);
      resultingAction.CheckRange = int.Parse(xmlData["CheckRange"].InnerText);
      resultingAction.CheckAmount = int.Parse(xmlData["CheckAmount"].InnerText);

      return resultingAction;
    }
  }
}
