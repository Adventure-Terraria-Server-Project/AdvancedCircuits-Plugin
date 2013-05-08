using System;
using System.Collections.Generic;
using System.Xml;

using TShockAPI;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class BuffPlayerStatueAction: NullStatueAction {
    #region [Property: BuffId]
    private int buffId;

    public int BuffId {
      get { return this.buffId; }
      set { this.buffId = value; }
    }
    #endregion

    #region [Property: BuffTime]
    private int buffTime;

    public int BuffTime {
      get { return this.buffTime; }
      set { this.buffTime = value; }
    }
    #endregion

    #region [Property: Radius]
    private int radius;

    public int Radius {
      get { return this.radius; }
      set { this.radius = value; }
    }
    #endregion


    #region [Method: Static FromXmlElement]
    public static new BuffPlayerStatueAction FromXmlElement(XmlElement xmlData) {
      BuffPlayerStatueAction resultingAction = new BuffPlayerStatueAction();

      string buffName = xmlData["BuffId"].InnerText;
      if (!int.TryParse(buffName, out resultingAction.buffId)) {
        List<int> buffs = TShock.Utils.GetBuffByName(buffName);
        if (buffs.Count == 0)
          throw new ArgumentException(string.Format("\"{0}\" is not a valid buff name.", buffName));
        else if (buffs.Count > 1)
          throw new ArgumentException(string.Format("\"{0}\" matches more than one buff.", buffName));
        else
          resultingAction.buffId = buffs[0];
      }

      resultingAction.buffTime = int.Parse(xmlData["BuffTime"].InnerText);
      resultingAction.radius = int.Parse(xmlData["Radius"].InnerText);

      return resultingAction;
    }
    #endregion
  }
}
