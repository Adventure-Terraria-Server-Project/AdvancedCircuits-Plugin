// This file is provided unter the terms of the 
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// 
// Written by CoderCow

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Xml.Serialization;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  [XmlRoot("DartTrapConfig")]
  public class DartTrapConfig {
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
    private int projectileSpeed;

    public int ProjectileSpeed {
      get { return this.projectileSpeed; }
      set { this.projectileSpeed = value; }
    }
    #endregion

    #region [Property: ProjectileOffset]
    private int projectileOffset;

    public int ProjectileOffset {
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

    #region [Property: Cooldown]
    private int cooldown;

    public int Cooldown {
      get { return this.cooldown; }
      set { this.cooldown = value; }
    }
    #endregion


    #region [Methods: Constructor]
    public DartTrapConfig() {
      this.cooldown = 180;
      this.projectileOffset = 8; //2;
      this.projectileSpeed = 12;
      this.projectileType = 98;
      this.projectileLifeTime = 80;
    }
    #endregion
  }
}
