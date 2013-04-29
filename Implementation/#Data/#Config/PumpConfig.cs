// This file is provided unter the terms of the 
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// 
// Written by CoderCow

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class PumpConfig {
    #region [Property: TransferableWater]
    private int transferableWater;

    public int TransferableWater {
      get { return this.transferableWater; }
      set { this.transferableWater = value; }
    }
    #endregion

    #region [Property: TransferableLava]
    private int transferableLava;

    public int TransferableLava {
      get { return this.transferableLava; }
      set { this.transferableLava = value; }
    }
    #endregion

    #region [Property: LossValue]
    private int lossValue;

    public int LossValue {
      get { return this.lossValue; }
      set { this.lossValue = value; }
    }
    #endregion

    #region [Property: Cooldown]
    private int cooldown;

    public int Cooldown {
      get { return this.cooldown; }
      set { this.cooldown = value; }
    }
    #endregion

    #region [Property: TriggerPermission]
    private string triggerPermission;

    public string TriggerPermission {
      get { return this.triggerPermission; }
      set { this.triggerPermission = value; }
    }
    #endregion

    #region [Property: WirePermission]
    private string wirePermission;

    public string WirePermission {
      get { return this.wirePermission; }
      set { this.wirePermission = value; }
    }
    #endregion


    #region [Methods: Constructor, Static FromXmlElement]
    public PumpConfig() {
      this.transferableWater = 1020;
      this.transferableLava = 1020;
      this.lossValue = 1;
      this.cooldown = 10;
    }

    public static PumpConfig FromXmlElement(XmlElement xmlData) {
      PumpConfig resultingPumpConfig = new PumpConfig();
      resultingPumpConfig.transferableWater = int.Parse(xmlData["TransferableWater"].InnerText);
      resultingPumpConfig.transferableLava = int.Parse(xmlData["TransferableLava"].InnerText);
      resultingPumpConfig.lossValue = int.Parse(xmlData["LossValue"].InnerText);
      resultingPumpConfig.cooldown = int.Parse(xmlData["Cooldown"].InnerText);

      if (xmlData["TriggerPermission"] != null)
        resultingPumpConfig.triggerPermission = xmlData["TriggerPermission"].InnerText;
      if (xmlData["WirePermission"] != null)
        resultingPumpConfig.wirePermission = xmlData["WirePermission"].InnerText;

      return resultingPumpConfig;
    }
    #endregion
  }
}
