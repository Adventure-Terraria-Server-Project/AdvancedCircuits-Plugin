using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using DPoint = System.Drawing.Point;

namespace Terraria.Plugins.Common.AdvancedCircuits {
  public class BlockActivatorMetadata {
    #region [Property: IsActivated]
    private bool isActivated;

    public bool IsActivated {
      get { return this.isActivated; }
      set { this.isActivated = value; }
    }
    #endregion

    #region [Property: RegisteredInactiveBlocks]
    private readonly Dictionary<DPoint,BlockType> registeredInactiveBlocks;

    public Dictionary<DPoint,BlockType> RegisteredInactiveBlocks {
      get { return this.registeredInactiveBlocks; }
    }
    #endregion


    #region [Method: Constructor]
    public BlockActivatorMetadata() {
      this.registeredInactiveBlocks = new Dictionary<DPoint,BlockType>();
    }
    #endregion
  }
}
