using System;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class TrapConfig {
    public string WirePermission { get; set; }
    public string TriggerPermission { get; set; }
    public int ProjectileType { get; set; }
    public int ProjectileDamage { get; set; }
    public float ProjectileSpeed { get; set; }
    public float ProjectileAngle { get; set; }
    public float ProjectileAngleVariance { get; set; }
    public float ProjectileOffset { get; set; }
    public int ProjectileLifeTime { get; set; }
    public float ProjectileKnockback { get; set; }
    public int Cooldown { get; set; }


    public TrapConfig() {
      this.Cooldown = 180;
      this.ProjectileType = 98;
      this.ProjectileDamage = 20;
      this.ProjectileAngle = 45;
      this.ProjectileAngleVariance = 0;
      this.ProjectileOffset = 8;
      this.ProjectileSpeed = 12;
      this.ProjectileLifeTime = 80;
      this.ProjectileKnockback = 2f;
    }

    public static TrapConfig FromXmlElement(XmlElement xmlData) {
      TrapConfig resultingTrapConfig = new TrapConfig();
      resultingTrapConfig.ProjectileType = int.Parse(xmlData["ProjectileType"].InnerText);
      resultingTrapConfig.ProjectileDamage = int.Parse(xmlData["ProjectileDamage"].InnerText);
      resultingTrapConfig.ProjectileSpeed = float.Parse(xmlData["ProjectileSpeed"].InnerText);
      resultingTrapConfig.ProjectileAngle = float.Parse(xmlData["ProjectileAngle"].InnerText);
      resultingTrapConfig.ProjectileAngleVariance = float.Parse(xmlData["ProjectileAngleVariance"].InnerText);
      resultingTrapConfig.ProjectileOffset = float.Parse(xmlData["ProjectileOffset"].InnerText);
      resultingTrapConfig.ProjectileLifeTime = int.Parse(xmlData["ProjectileLifeTime"].InnerText);
      resultingTrapConfig.ProjectileKnockback = float.Parse(xmlData["ProjectileKnockback"].InnerText);
      resultingTrapConfig.Cooldown = int.Parse(xmlData["Cooldown"].InnerText);
      if (xmlData["WirePermission"] != null)
        resultingTrapConfig.WirePermission = xmlData["WirePermission"].InnerText;
      if (xmlData["TriggerPermission"] != null)
        resultingTrapConfig.TriggerPermission = xmlData["TriggerPermission"].InnerText;

      return resultingTrapConfig;
    }
  }
}
