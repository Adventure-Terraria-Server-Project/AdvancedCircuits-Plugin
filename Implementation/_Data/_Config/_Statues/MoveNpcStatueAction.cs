using System;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class MoveNpcStatueAction: NullStatueAction {
    public int NpcType { get; set; }
    public bool SpawnIfNotExistent { get; set; }
    public int CheckRange { get; set; }


    public static new MoveNpcStatueAction FromXmlElement(XmlElement xmlData) {
      MoveNpcStatueAction resultingAction = new MoveNpcStatueAction();
      resultingAction.NpcType = int.Parse(xmlData["NpcId"].InnerText);
      resultingAction.SpawnIfNotExistent = bool.Parse(xmlData["SpawnIfNotExistent"].InnerText);
      resultingAction.CheckRange = int.Parse(xmlData["CheckRange"].InnerText);

      return resultingAction;
    }
  }
}
