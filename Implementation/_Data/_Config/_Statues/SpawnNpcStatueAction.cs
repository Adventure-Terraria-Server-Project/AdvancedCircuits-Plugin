using System;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class SpawnNpcStatueAction: NullStatueAction {
    public int NpcType { get; set; }
    public int Amount { get; set; }
    public int CheckRange { get; set; }
    public int CheckAmount { get; set; }


    public static new SpawnNpcStatueAction FromXmlElement(XmlElement xmlData) {
      SpawnNpcStatueAction resultingAction = new SpawnNpcStatueAction();
      resultingAction.NpcType = int.Parse(xmlData["NpcId"].InnerText);
      resultingAction.Amount = int.Parse(xmlData["Amount"].InnerText);
      resultingAction.CheckRange = int.Parse(xmlData["CheckRange"].InnerText);
      resultingAction.CheckAmount = int.Parse(xmlData["CheckAmount"].InnerText);

      return resultingAction;
    }
  }
}
