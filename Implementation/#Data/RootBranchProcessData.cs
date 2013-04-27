// This file is provided unter the terms of the 
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// 
// Written by CoderCow

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using DPoint = System.Drawing.Point;

namespace Terraria.Plugins.Common.AdvancedCircuits {
  public struct RootBranchProcessData {
    #region [Property: SenderLocation]
    private DPoint senderLocation;

    public DPoint SenderLocation {
      get { return this.senderLocation; }
      set { this.senderLocation = value; }
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

    #region [Property: SignaledComponentLocations]
    private readonly List<DPoint> signaledComponentLocations;

    public List<DPoint> SignaledComponentLocations {
      get { return this.signaledComponentLocations; }
    }
    #endregion

    #region [Property: BlockActivatorLocation]
    private DPoint blockActivatorLocation;

    public DPoint BlockActivatorLocation {
      get { return this.blockActivatorLocation; }
      set { this.blockActivatorLocation = value; }
    }
    #endregion

    #region [Property: BlockActivator]
    private BlockActivatorMetadata blockActivator;

    public BlockActivatorMetadata BlockActivator {
      get { return this.blockActivator; }
      set { this.blockActivator = value; }
    }
    #endregion

    #region [Property: BlockActivatorDeactivatedBlockCounter]
    private int blockActivatorDeactivatedBlockCounter;

    public int BlockActivatorDeactivatedBlockCounter {
      get { return this.blockActivatorDeactivatedBlockCounter; }
      set { this.blockActivatorDeactivatedBlockCounter = value; }
    }
    #endregion


    #region [Method: Constructor]
    public RootBranchProcessData(DPoint senderLocation, DPoint firstWireLocation, SignalType signal) {
      this.senderLocation = senderLocation;
      this.firstWireLocation = firstWireLocation;
      this.lastWireLocation = firstWireLocation;
      this.direction = AdvancedCircuits.DirectionFromTileLocations(senderLocation, firstWireLocation);
      this.signal = signal;
      this.signaledComponentLocations = new List<DPoint>();
      this.blockActivator = null;
      this.blockActivatorLocation = DPoint.Empty;
      this.blockActivatorDeactivatedBlockCounter = 0;
    }
    #endregion

    #region [Method: ToBranchProcessData]
    public BranchProcessData ToBranchProcessData() {
      BranchProcessData branch = new BranchProcessData(this.SenderLocation, this.FirstWireLocation, this.Signal);
      branch.LastWireLocation = this.LastWireLocation;

      return branch;
    }
    #endregion
  }
}
