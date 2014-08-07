using System;
using System.Collections.Generic;
using System.Xml;

using TShockAPI;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class BuffPlayerStatueAction: NullStatueAction {
    public int BuffId { get; set; }
    public int BuffTime { get; set; }
    public int Radius { get; set; }


    public static new BuffPlayerStatueAction FromXmlElement(XmlElement xmlData) {
      BuffPlayerStatueAction resultingAction = new BuffPlayerStatueAction();

      string buffName = xmlData["BuffId"].InnerText;
      int buffId;
      if (!int.TryParse(buffName, out buffId)) {
        List<int> buffs = TShock.Utils.GetBuffByName(buffName);
        if (buffs.Count == 0)
          throw new ArgumentException(string.Format("\"{0}\" is not a valid buff name.", buffName));
        if (buffs.Count > 1)
          throw new ArgumentException(string.Format("\"{0}\" matches more than one buff.", buffName));

        buffId = buffs[0];
      }

      resultingAction.BuffId = buffId;
      resultingAction.BuffTime = int.Parse(xmlData["BuffTime"].InnerText);
      resultingAction.Radius = int.Parse(xmlData["Radius"].InnerText);
      return resultingAction;
    }
  }
}
