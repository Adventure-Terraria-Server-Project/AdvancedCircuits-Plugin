using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using DPoint = System.Drawing.Point;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class BlockActivatorMetadata {
    #region [Property: IsActivated]
    private bool isActivated;

    public bool IsActivated {
      get { return this.isActivated; }
      set { this.isActivated = value; }
    }
    #endregion

    #region [Property: RegisteredInactiveTiles]
    private readonly Dictionary<DPoint,byte> registeredInactiveTiles;

    public Dictionary<DPoint,byte> RegisteredInactiveTiles {
      get { return this.registeredInactiveTiles; }
    }
    #endregion


    #region [Method: Constructor]
    public BlockActivatorMetadata() {
      this.registeredInactiveTiles = new Dictionary<DPoint,byte>();
    }
    #endregion
  }
}
