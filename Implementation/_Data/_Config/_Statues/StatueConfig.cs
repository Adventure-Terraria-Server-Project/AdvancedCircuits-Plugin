using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class StatueConfig {
    public int PlayerCheckRange { get; set; }
    public int Cooldown { get; set; }
    public string WirePermission { get; set; }
    public string TriggerPermission { get; set; }
    public Collection<NullStatueAction> Actions { get; set; }
    public ActionListProcessingMethod ActionsProcessingMethod { get; set; }


    public StatueConfig() {
      this.PlayerCheckRange = 0;
      this.Cooldown = 30;
      this.Actions = new Collection<NullStatueAction>();
      this.ActionsProcessingMethod = ActionListProcessingMethod.ExecuteAll;
    }

    public static StatueConfig FromXmlElement(XmlElement xmlData) {
      StatueConfig resultingStatueConfig = new StatueConfig();
      if (xmlData["PlayerCheckRange"] != null)
        resultingStatueConfig.PlayerCheckRange = int.Parse(xmlData["PlayerCheckRange"].InnerText);
      resultingStatueConfig.Cooldown = int.Parse(xmlData["Cooldown"].InnerText);
      if (xmlData["TriggerPermission"] != null)
        resultingStatueConfig.TriggerPermission = xmlData["TriggerPermission"].InnerText;
      if (xmlData["WirePermission"] != null)
        resultingStatueConfig.WirePermission = xmlData["WirePermission"].InnerText;

      XmlElement actionListNode = xmlData["Actions"];
      resultingStatueConfig.ActionsProcessingMethod = (ActionListProcessingMethod)Enum.Parse(
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
          case "BuffPlayerAction":
            resultingStatueConfig.Actions.Add(BuffPlayerStatueAction.FromXmlElement(actionNode));
            break;
        }
      }

      return resultingStatueConfig;
    }
  }
}
