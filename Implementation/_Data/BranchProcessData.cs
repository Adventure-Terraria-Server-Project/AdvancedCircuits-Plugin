using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using DPoint = System.Drawing.Point;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public struct BranchProcessData {
    public DPoint BranchingTileLocation { get; private set; }
    public DPoint FirstWireLocation { get; private set; }
    public DPoint LastWireLocation { get; set; }
    public Direction Direction { get; private set; }
    public SignalType Signal { get; private set; }


    #region [Method: Constructor]
    public BranchProcessData(DPoint branchingTileLocation, DPoint firstWireLocation, SignalType signal): this() {
      this.BranchingTileLocation = branchingTileLocation;
      this.FirstWireLocation = firstWireLocation;
      this.LastWireLocation = DPoint.Empty;
      this.Signal = signal;
      this.Direction = AdvancedCircuits.DirectionFromTileLocations(branchingTileLocation, firstWireLocation);
    }
    #endregion

    #region [Method: IsTileInBetween]
    public bool IsTileInBetween(DPoint tileLocation) {
      switch (this.Direction) {
        case Direction.Left:
          return (
            (this.FirstWireLocation.Y == tileLocation.Y) &&
            (tileLocation.X >= this.LastWireLocation.X && tileLocation.X <= this.FirstWireLocation.X)
          );
        case Direction.Right:
          return (
            (this.FirstWireLocation.Y == tileLocation.Y) &&
            (tileLocation.X >= this.FirstWireLocation.X && tileLocation.X <= this.LastWireLocation.X)
          );
        case Direction.Up:
          return (
            (this.FirstWireLocation.X == tileLocation.X) &&
            (tileLocation.Y >= this.LastWireLocation.Y && tileLocation.Y <= this.FirstWireLocation.Y)
          );
        case Direction.Down:
          return (
            (this.FirstWireLocation.X == tileLocation.X) &&
            (tileLocation.Y >= this.FirstWireLocation.Y && tileLocation.Y <= this.LastWireLocation.Y)
          );
        case Direction.Unknown:
          return (this.FirstWireLocation == tileLocation);
        default:
          throw new InvalidOperationException();
      }
    }
    #endregion

    #region [Method: ToString]
    public override string ToString() {
      return string.Format("First Wire: {0}, Direction: {1}", this.FirstWireLocation, this.Direction);
    }
    #endregion
  }
}
