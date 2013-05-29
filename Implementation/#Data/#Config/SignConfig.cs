using System;
using System.Diagnostics.Contracts;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class SignConfig {
    #region [Property: ReadPrefix]
    private string readPrefix;

    public string ReadPrefix {
      get { return this.readPrefix; }
      set { this.readPrefix = value; }
    }
    #endregion

    #region [Property: WirePermission]
    private string wirePermission;

    public string WirePermission {
      get { return this.wirePermission; }
      set { this.wirePermission = value; }
    }
    #endregion

    #region [Property: PassiveTriggerPermission]
    private string passiveTriggerPermission;

    public string PassiveTriggerPermission {
      get { return this.passiveTriggerPermission; }
      set { this.passiveTriggerPermission = value; }
    }
    #endregion

    #region [Property: TriggerSignCommandPermission]
    private string triggerSignCommandPermission;

    public string TriggerSignCommandPermission {
      get { return this.triggerSignCommandPermission; }
      set { this.triggerSignCommandPermission = value; }
    }
    #endregion


    #region [Methods: Constructor, Static FromXmlElement]
    public SignConfig() {
      this.readPrefix = "Sign: ";
    }

    public static SignConfig FromXmlElement(XmlElement xmlData) {
      SignConfig resultingSignConfig = new SignConfig();
      resultingSignConfig.readPrefix = xmlData["ReadPrefix"].InnerText;
      resultingSignConfig.wirePermission = xmlData["WirePermission"].InnerText;
      resultingSignConfig.passiveTriggerPermission = xmlData["PassiveTriggerPermission"].InnerText;
      resultingSignConfig.triggerSignCommandPermission = xmlData["TriggerSignCommandPermission"].InnerText;

      return resultingSignConfig;
    }
    #endregion
  }
}