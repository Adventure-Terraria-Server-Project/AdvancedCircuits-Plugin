using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class DartTrapConfig {
    public string WirePermission { get; set; }
    public string TriggerPermission { get; set; }
    public int ProjectileType { get; set; }
    public int ProjectileDamage { get; set; }
    public float ProjectileSpeed { get; set; }
    public float ProjectileAngle { get; set; }
    public float ProjectileOffset { get; set; }
    public int ProjectileLifeTime { get; set; }
    public float ProjectileKnockback { get; set; }
    public int Cooldown { get; set; }


    public DartTrapConfig() {
      this.Cooldown = 180;
      this.ProjectileType = 98;
      this.ProjectileDamage = 20;
      this.ProjectileAngle = 45;
      this.ProjectileOffset = 8;
      this.ProjectileSpeed = 12;
      this.ProjectileLifeTime = 80;
      this.ProjectileKnockback = 2f;
    }

    public static DartTrapConfig FromXmlElement(XmlElement xmlData) {
      DartTrapConfig resultingDartTrapConfig = new DartTrapConfig();
      resultingDartTrapConfig.ProjectileType = int.Parse(xmlData["ProjectileType"].InnerText);
      resultingDartTrapConfig.ProjectileDamage = int.Parse(xmlData["ProjectileDamage"].InnerText);
      resultingDartTrapConfig.ProjectileSpeed = float.Parse(xmlData["ProjectileSpeed"].InnerText);
      resultingDartTrapConfig.ProjectileAngle = float.Parse(xmlData["ProjectileAngle"].InnerText);
      resultingDartTrapConfig.ProjectileOffset = float.Parse(xmlData["ProjectileOffset"].InnerText);
      resultingDartTrapConfig.ProjectileLifeTime = int.Parse(xmlData["ProjectileLifeTime"].InnerText);
      resultingDartTrapConfig.ProjectileKnockback = float.Parse(xmlData["ProjectileKnockback"].InnerText);
      resultingDartTrapConfig.Cooldown = int.Parse(xmlData["Cooldown"].InnerText);
      if (xmlData["WirePermission"] != null)
        resultingDartTrapConfig.WirePermission = xmlData["WirePermission"].InnerText;
      if (xmlData["TriggerPermission"] != null)
        resultingDartTrapConfig.TriggerPermission = xmlData["TriggerPermission"].InnerText;

      return resultingDartTrapConfig;
    }
  }
}
