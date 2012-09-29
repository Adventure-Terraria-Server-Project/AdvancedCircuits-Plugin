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

using TShockAPI;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class CircuitStripData {
    #region [Property: IsAdvancedCircuit]
    private readonly bool isAdvancedCircuit;

    public bool IsAdvancedCircuit {
      get { return this.isAdvancedCircuit; }
    }
    #endregion

    #region [Property: IsCancelled]
    private bool isCancelled;

    public bool IsCancelled {
      get { return this.isCancelled; }
      set { this.isCancelled = value; }
    }
    #endregion

    #region [Property: FirstWireLocation]
    private readonly DPoint firstWireLocation;

    public DPoint FirstWireLocation {
      get { return this.firstWireLocation; }
    }
    #endregion

    #region [Property: SendingPlayer]
    private readonly TSPlayer sendingPlayer;

    public TSPlayer SendingPlayer {
      get { return this.sendingPlayer; }
    }
    #endregion

    #region [Property: SenderLocation]
    private readonly DPoint senderLocation;

    public DPoint SenderLocation {
      get { return this.senderLocation; }
    }
    #endregion

    #region [Properties: ProcessedWires, LastWireLocation]
    private readonly List<DPoint> processedWires;

    public List<DPoint> ProcessedWires {
      get { return this.processedWires; }
    }

    public DPoint LastWireLocation {
      get {
        if (this.processedWires.Count == 0)
          return new DPoint(-1, -1);

        return this.processedWires[this.processedWires.Count - 1];
      }
    }
    #endregion

    #region [Property: IgnoredTiles]
    private readonly List<DPoint> ignoredTiles;

    public List<DPoint> IgnoredTiles {
      get { return this.ignoredTiles; }
    }
    #endregion

    #region [Property: SignaledComponentsCounter]
    private int signaledComponentsCounter;

    public int SignaledComponentsCounter {
      get { return this.signaledComponentsCounter; }
      set { this.signaledComponentsCounter = value; }
    }
    #endregion

    #region [Property: SignaledACComponentsCounter]
    private int signaledAcComponentsCounter;

    public int SignaledACComponentsCounter {
      get { return this.signaledAcComponentsCounter; }
      set { this.signaledAcComponentsCounter = value; }
    }
    #endregion

    #region [Property: SignaledDartTraps]
    private int signaledDartTraps;

    public int SignaledDartTraps {
      get { return this.signaledDartTraps; }
      set { this.signaledDartTraps = value; }
    }
    #endregion

    #region [Property: SignaledStatues]
    private int signaledStatues;

    public int SignaledStatues {
      get { return this.signaledStatues; }
      set { this.signaledStatues = value; }
    }
    #endregion

    #region [Property: SignaledPumps]
    private int signaledPumps;

    public int SignaledPumps {
      get { return this.signaledPumps; }
      set { this.signaledPumps = value; }
    }
    #endregion


    #region [Method: Constructor]
    public CircuitStripData(
      TSPlayer sendingPlayer, DPoint senderLocation, DPoint firstWireLocation, bool isAdvancedCircuit,
      IEnumerable<DPoint> ignoredTiles = null
    ) {
      this.sendingPlayer = sendingPlayer;
      this.firstWireLocation = firstWireLocation;
      this.isAdvancedCircuit = isAdvancedCircuit;

      this.senderLocation = senderLocation;
      this.processedWires = new List<DPoint>(500);

      this.ignoredTiles = new List<DPoint>(30);
      if (ignoredTiles != null)
        this.ignoredTiles.AddRange(ignoredTiles);
    }
    #endregion
  }
}
