// This file is provided unter the terms of the 
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// 
// Written by CoderCow

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Xml;

namespace Terraria.Plugins.Common.AdvancedCircuits {
  public class StatueConfig {
    #region [Property: ActionType]
    private StatueActionType actionType;

    public StatueActionType ActionType {
      get { return this.actionType; }
      set { this.actionType = value; }
    }
    #endregion

    #region [Property: ActionParam]
    private int actionParam;

    public int ActionParam {
      get { return this.actionParam; }
      set { this.actionParam = value; }
    }
    #endregion

    #region [Property: ActionParam2]
    private int actionParam2;

    public int ActionParam2 {
      get { return this.actionParam2; }
      set { this.actionParam2 = value; }
    }
    #endregion

    #region [Property: ActionParam3]
    private int actionParam3;

    public int ActionParam3 {
      get { return this.actionParam3; }
      set { this.actionParam3 = value; }
    }
    #endregion

    #region [Property: ActionParam4]
    private int actionParam4;

    public int ActionParam4 {
      get { return this.actionParam4; }
      set { this.actionParam4 = value; }
    }
    #endregion

    #region [Property: Cooldown]
    private int cooldown;

    public int Cooldown {
      get { return this.cooldown; }
      set { this.cooldown = value; }
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
    public StatueConfig() {
      this.cooldown = 30;
      this.actionParam3 = 4;
      this.actionParam4 = 25;
    }

    public static StatueConfig FromXmlElement(XmlElement xmlData) {
      StatueConfig resultingStatueConfig = new StatueConfig();
      resultingStatueConfig.actionType = (StatueActionType)Enum.Parse(typeof(StatueActionType), xmlData["ActionType"].InnerText);
      resultingStatueConfig.actionParam = int.Parse(xmlData["ActionParam"].InnerText);
      if (xmlData["ActionParam2"] != null)
        resultingStatueConfig.actionParam2 = int.Parse(xmlData["ActionParam2"].InnerText);
      if (xmlData["ActionParam3"] != null)
        resultingStatueConfig.actionParam3 = int.Parse(xmlData["ActionParam3"].InnerText);
      if (xmlData["ActionParam4"] != null)
        resultingStatueConfig.actionParam4 = int.Parse(xmlData["ActionParam4"].InnerText);
      resultingStatueConfig.cooldown = int.Parse(xmlData["Cooldown"].InnerText);
      if (xmlData["TriggerPermission"] != null)
        resultingStatueConfig.triggerPermission = xmlData["TriggerPermission"].InnerText;
      if (xmlData["WirePermission"] != null)
        resultingStatueConfig.wirePermission = xmlData["WirePermission"].InnerText;

      return resultingStatueConfig;
    }
    #endregion
  }
}
