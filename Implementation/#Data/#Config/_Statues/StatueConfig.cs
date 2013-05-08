using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class StatueConfig {
    #region [Property: PlayerCheckRange]
    private int playerCheckRange;

    public int PlayerCheckRange {
      get { return this.playerCheckRange; }
      set { this.playerCheckRange = value; }
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

    #region [Property: Actions]
    private Collection<NullStatueAction> actions;

    public Collection<NullStatueAction> Actions {
      get { return this.actions; }
      set { this.actions = value; }
    }
    #endregion

    #region [Property: ActionsProcessingMethod]
    private ActionListProcessingMethod actionsProcessingMethod;

    public ActionListProcessingMethod ActionsProcessingMethod {
      get { return this.actionsProcessingMethod; }
      set { this.actionsProcessingMethod = value; }
    }
    #endregion


    #region [Methods: Constructor, Static FromXmlElement]
    public StatueConfig() {
      this.playerCheckRange = 0;
      this.cooldown = 30;
      this.actions = new Collection<NullStatueAction>();
      this.actionsProcessingMethod = ActionListProcessingMethod.ExecuteAll;
    }

    public static StatueConfig FromXmlElement(XmlElement xmlData) {
      StatueConfig resultingStatueConfig = new StatueConfig();
      if (xmlData["PlayerCheckRange"] != null)
        resultingStatueConfig.playerCheckRange = int.Parse(xmlData["PlayerCheckRange"].InnerText);
      resultingStatueConfig.cooldown = int.Parse(xmlData["Cooldown"].InnerText);
      if (xmlData["TriggerPermission"] != null)
        resultingStatueConfig.triggerPermission = xmlData["TriggerPermission"].InnerText;
      if (xmlData["WirePermission"] != null)
        resultingStatueConfig.wirePermission = xmlData["WirePermission"].InnerText;

      XmlElement actionListNode = xmlData["Actions"];
      resultingStatueConfig.actionsProcessingMethod = (ActionListProcessingMethod)Enum.Parse(
        typeof(ActionListProcessingMethod), actionListNode.Attributes["ProcessingMethod"].Value
      );
      foreach (XmlElement actionNode in actionListNode.ChildNodes) {
        switch (actionNode.LocalName) {
          case "NullAction":
            resultingStatueConfig.Actions.Add(NullStatueAction.FromXmlElement(actionNode));
            break;
          case "MoveNpcAction":
            resultingStatueConfig.Actions.Add(MoveNpcStatueAction.FromXmlElement(actionNode));
            break;
          case "SpawnNpcAction":
            resultingStatueConfig.Actions.Add(SpawnNpcStatueAction.FromXmlElement(actionNode));
            break;
          case "SpawnItemAction":
            resultingStatueConfig.Actions.Add(SpawnItemStatueAction.FromXmlElement(actionNode));
            break;
        }
      }

      return resultingStatueConfig;
    }
    #endregion
  }
}
