using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using DPoint = System.Drawing.Point;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class BlockActivatorMetadata {
    public bool IsActivated { get; set; }
    public Dictionary<DPoint,int> RegisteredInactiveBlocks { get; private set; }


    public BlockActivatorMetadata() {
      this.RegisteredInactiveBlocks = new Dictionary<DPoint,int>();
    }
  }
}
