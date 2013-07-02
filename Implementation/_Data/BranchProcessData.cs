using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using DPoint = System.Drawing.Point;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public struct BranchProcessData {
    #region [Property: BranchingTileLocation]
    private readonly DPoint branchingTileLocation;

    public DPoint BranchingTileLocation {
      get { return this.branchingTileLocation; }
    }
    #endregion

    #region [Property: FirstWireLocation]
    private readonly DPoint firstWireLocation;

    public DPoint FirstWireLocation {
      get { return this.firstWireLocation; }
    }
    #endregion

    #region [Property: LastWireLocation]
    private DPoint lastWireLocation;

    public DPoint LastWireLocation {
      get { return this.lastWireLocation; }
      set { this.lastWireLocation = value; }
    }
    #endregion

    #region [Property: Direction]
    private readonly Direction direction;

    public Direction Direction {
      get { return this.direction; }
    }
    #endregion

    #region [Property: Signal]
    private readonly SignalType signal;

    public SignalType Signal {
      get { return this.signal; }
    }
    #endregion


    #region [Method: Constructor]
    public BranchProcessData(DPoint branchingTileLocation, DPoint firstWireLocation, SignalType signal) {
      this.branchingTileLocation = branchingTileLocation;
      this.firstWireLocation = firstWireLocation;
      this.lastWireLocation = DPoint.Empty;
      this.signal = signal;
      this.direction = AdvancedCircuits.DirectionFromTileLocations(branchingTileLocation, firstWireLocation);
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
