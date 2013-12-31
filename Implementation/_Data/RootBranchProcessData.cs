using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using DPoint = System.Drawing.Point;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class RootBranchProcessData {
    public WireColor WireColor { get; set; }
    public DPoint SenderLocation { get; set; }
    public DPoint FirstWireLocation { get; set; }
    public DPoint LastWireLocation { get; set; }
    public Direction Direction { get; private set; }
    public SignalType Signal { get; private set; }
    public List<DPoint> SignaledComponentLocations { get; private set; }
    public DPoint BlockActivatorLocation { get; set; }
    public BlockActivatorMetadata BlockActivator { get; set; }
    public int BlockActivatorDeactivatedBlockCounter { get; set; }
    public BlockActivatorMode BlockActivatorMode { get; set; }
    public DPoint TeleporterLocation { get; set; }


    public RootBranchProcessData(DPoint senderLocation, DPoint firstWireLocation, SignalType signal, WireColor wireColor) {
      this.SenderLocation = senderLocation;
      this.FirstWireLocation = firstWireLocation;
      this.LastWireLocation = firstWireLocation;
      this.WireColor = wireColor;
      this.Direction = AdvancedCircuits.DirectionFromTileLocations(senderLocation, firstWireLocation);
      this.Signal = signal;
      this.SignaledComponentLocations = new List<DPoint>();
      this.BlockActivator = null;
      this.BlockActivatorLocation = DPoint.Empty;
      this.BlockActivatorDeactivatedBlockCounter = 0;
      this.BlockActivatorMode = BlockActivatorMode.Default;
    }

    public BranchProcessData ToBranchProcessData() {
      BranchProcessData branch = new BranchProcessData(this.SenderLocation, this.FirstWireLocation, this.Signal);
      branch.LastWireLocation = this.LastWireLocation;

      return branch;
    }
  }
}
