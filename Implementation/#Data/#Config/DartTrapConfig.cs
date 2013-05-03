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
  public class DartTrapConfig {
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

    #region [Property: ProjectileType]
    private int projectileType;

    public int ProjectileType {
      get { return this.projectileType; }
      set { this.projectileType = value; }
    }
    #endregion

    #region [Property: ProjectileDamage]
    private int projectileDamage;

    public int ProjectileDamage {
      get { return this.projectileDamage; }
      set { this.projectileDamage = value; }
    }
    #endregion

    #region [Property: ProjectileSpeed]
    private float projectileSpeed;

    public float ProjectileSpeed {
      get { return this.projectileSpeed; }
      set { this.projectileSpeed = value; }
    }
    #endregion

    #region [Property: ProjectileAngle]
    private float projectileAngle;

    public float ProjectileAngle {
      get { return this.projectileAngle; }
      set { this.projectileAngle = value; }
    }
    #endregion

    #region [Property: ProjectileOffset]
    private float projectileOffset;

    public float ProjectileOffset {
      get { return this.projectileOffset; }
      set { this.projectileOffset = value; }
    }
    #endregion

    #region [Property: ProjectileLifeTime]
    private int projectileLifeTime;

    public int ProjectileLifeTime {
      get { return this.projectileLifeTime; }
      set { this.projectileLifeTime = value; }
    }
    #endregion

    #region [Property: ProjectileKnockback]
    private float projectileKnockback;

    public float ProjectileKnockback {
      get { return this.projectileKnockback; }
      set { this.projectileKnockback = value; }
    }
    #endregion

    #region [Property: Cooldown]
    private int cooldown;

    public int Cooldown {
      get { return this.cooldown; }
      set { this.cooldown = value; }
    }
    #endregion


    #region [Methods: Constructor, Static FromXmlElement]
    public DartTrapConfig() {
      this.cooldown = 180;
      this.projectileType = 98;
      this.projectileDamage = 20;
      this.projectileAngle = 45;
      this.projectileOffset = 8;
      this.projectileSpeed = 12;
      this.projectileLifeTime = 80;
      this.projectileKnockback = 2f;
    }

    public static DartTrapConfig FromXmlElement(XmlElement xmlData) {
      DartTrapConfig resultingDartTrapConfig = new DartTrapConfig();
      resultingDartTrapConfig.projectileType = int.Parse(xmlData["ProjectileType"].InnerText);
      resultingDartTrapConfig.projectileDamage = int.Parse(xmlData["ProjectileDamage"].InnerText);
      resultingDartTrapConfig.projectileSpeed = float.Parse(xmlData["ProjectileSpeed"].InnerText);
      resultingDartTrapConfig.projectileAngle = float.Parse(xmlData["ProjectileAngle"].InnerText);
      resultingDartTrapConfig.projectileOffset = float.Parse(xmlData["ProjectileOffset"].InnerText);
      resultingDartTrapConfig.projectileLifeTime = int.Parse(xmlData["ProjectileLifeTime"].InnerText);
      resultingDartTrapConfig.projectileKnockback = float.Parse(xmlData["ProjectileKnockback"].InnerText);
      resultingDartTrapConfig.cooldown = int.Parse(xmlData["Cooldown"].InnerText);
      if (xmlData["WirePermission"] != null)
        resultingDartTrapConfig.wirePermission = xmlData["WirePermission"].InnerText;
      if (xmlData["TriggerPermission"] != null)
        resultingDartTrapConfig.triggerPermission = xmlData["TriggerPermission"].InnerText;

      return resultingDartTrapConfig;
    }
    #endregion
  }
}
