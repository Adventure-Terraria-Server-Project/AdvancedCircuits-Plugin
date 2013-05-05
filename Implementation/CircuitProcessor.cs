// This file is provided unter the terms of the 
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// 
// Written by CoderCow

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using TShockAPI;
using Terraria.Plugins.Common;
using DPoint = System.Drawing.Point;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class CircuitProcessor {
    #region [Constants]
    // The maximum amount of times a single component can be signaled in the same circuit execution until it "overheats".
    public const int PortDefiningComponentSignalMaximum = 5;
    #endregion

    #region [Property: PluginTrace]
    private readonly PluginTrace pluginTrace;

    protected PluginTrace PluginTrace {
      get { return this.pluginTrace; }
    }
    #endregion

    #region [Property: CircuitHandler]
    private readonly CircuitHandler circuitHandler;

    public CircuitHandler CircuitHandler {
      get { return this.circuitHandler; }
    }
    #endregion

    #region [Property: SenderMeasureData]
    private readonly ObjectMeasureData senderMeasureData;

    protected ObjectMeasureData SenderMeasureData {
      get { return this.senderMeasureData; }
    }
    #endregion

    #region [Property: QueuedRootBranches]
    private readonly List<RootBranchProcessData> queuedRootBranches;

    protected List<RootBranchProcessData> QueuedRootBranches {
      get { return this.queuedRootBranches; }
    }
    #endregion

    #region [Property: PortDefiningComponentSignalCounter]
    private readonly Dictionary<DPoint,int> portDefiningComponentSignalCounter;

    // Stores the amount of times a single port defining component was signaled during the current circuit execution.
    protected Dictionary<DPoint,int> PortDefiningComponentSignalCounter {
      get { return this.portDefiningComponentSignalCounter; }
    }
    #endregion

    #region [Properties: SignaledInletPumps, SignaledOutletPumps]
    [ThreadStatic]
    private static List<DPoint> signaledInletPumps;

    [ThreadStatic]
    private static List<DPoint> signaledOutletPumps;

    protected List<DPoint> SignaledInletPumps {
      get {
        if (CircuitProcessor.signaledInletPumps == null)
          CircuitProcessor.signaledInletPumps = new List<DPoint>();

        return CircuitProcessor.signaledInletPumps;
      }
    }

    protected List<DPoint> SignaledOutletPumps {
      get {
        if (CircuitProcessor.signaledOutletPumps == null)
          CircuitProcessor.signaledOutletPumps = new List<DPoint>();

        return CircuitProcessor.signaledOutletPumps;
      }
    }
    #endregion

    #region [Property: TilesToFrameOnPost]
    [ThreadStatic]
    private static List<DPoint> tilesToFrameOnPost;

    public List<DPoint> TilesToFrameOnPost {
      get {
        if (CircuitProcessor.tilesToFrameOnPost == null)
          CircuitProcessor.tilesToFrameOnPost = new List<DPoint>();

        return CircuitProcessor.tilesToFrameOnPost;
      }
    }
    #endregion

    #region [Property: TemporaryGateStates]
    private Dictionary<DPoint, GateStateMetadata> temporaryGateStates;

    protected Dictionary<DPoint,GateStateMetadata> TemporaryGateStates {
      get { return this.temporaryGateStates; }
    }
    #endregion

    #region [Properties: Result, IsAdvancedCircuit, TriggeringPlayer, TriggeredPassively, CircuitLength, IsCancellationPending]
    private readonly CircuitProcessResult result;

    protected CircuitProcessResult Result {
      get { return this.result; }
    }

    public bool IsAdvancedCircuit {
      get { return this.result.IsAdvancedCircuit; }
    }
    
    public TSPlayer TriggeringPlayer {
      get { return this.result.TriggeringPlayer; }
      set { this.result.TriggeringPlayer = value; }
    }

    public bool TriggeredPassively {
      get { return this.result.TriggeredPassively; }
      set { this.result.TriggeredPassively = value; }
    }

    public int CircuitLength { 
      get { return this.result.CircuitLength; }
      set { this.result.CircuitLength = value; }
    }

    public bool IsCancellationPending {
      get { return (this.result.CancellationReason != CircuitCancellationReason.None); }
    }
    #endregion

    private bool wasExecuted;


    #region [Method: Constructor]
    public CircuitProcessor(PluginTrace pluginTrace, CircuitHandler circuitHandler, DPoint senderLocation) {
      Contract.Requires<ArgumentNullException>(pluginTrace != null);
      Contract.Requires<ArgumentNullException>(circuitHandler != null);

      this.pluginTrace = pluginTrace;

      Tile senderTile = TerrariaUtils.Tiles[senderLocation];
      if (!senderTile.active)
        throw new ArgumentException("No tile at the given sender location.", "senderLocation");

      this.circuitHandler = circuitHandler;
      this.senderMeasureData = TerrariaUtils.Tiles.MeasureObject(senderLocation);
      this.queuedRootBranches = new List<RootBranchProcessData>(20);
      this.result = new CircuitProcessResult {
        IsAdvancedCircuit = !TerrariaUtils.Tiles.IsObjectWired(this.SenderMeasureData),
        SenderLocation = this.SenderMeasureData.OriginTileLocation
      };

      if (this.IsAdvancedCircuit)
        this.portDefiningComponentSignalCounter = new Dictionary<DPoint,int>(20);
    }
    #endregion

    #region [Methods: ProcessCircuit, ProcessRootBranch, ProcessSubBranches, ProcessTile, PostProcessCircuit]
    public CircuitProcessResult ProcessCircuit(
      TSPlayer player = null, SignalType? overrideSignal = null, bool switchSender = true, bool switchSenderLocalOnly = true
    ) {
      if (this.wasExecuted)
        throw new InvalidOperationException("This Circuit Processor has already processed a circuit.");

      this.wasExecuted = true;
      this.SignaledInletPumps.Clear();
      this.SignaledOutletPumps.Clear();
      this.TilesToFrameOnPost.Clear();

      DateTime processingStartTime = DateTime.Now;
      BlockType senderBlockType = this.SenderMeasureData.BlockType;

      this.TriggeringPlayer = player;
      this.TriggeredPassively = (senderBlockType == BlockType.XSecondTimer || senderBlockType == BlockType.GrandfatherClock);

      SignalType signal = SignalType.Swap;
      try {
        if (this.IsAdvancedCircuit) {
          if (!this.CircuitHandler.Config.AdvancedCircuitsEnabled)
            return this.Result;
          
          if (overrideSignal == null) {
            switch (senderBlockType) {
              case BlockType.PressurePlate:
                // Red sends "0", all the others send "1".
                signal = AdvancedCircuits.BoolToSignal(TerrariaUtils.Tiles[this.SenderMeasureData.OriginTileLocation].frameY > 0);

                break;
              case BlockType.Lever:
              case BlockType.Switch:
              case BlockType.XSecondTimer:
                signal = AdvancedCircuits.BoolToSignal(!TerrariaUtils.Tiles.ObjectHasActiveState(this.SenderMeasureData));
                break;

              default:
                signal = SignalType.On;
                break;
            }
          }
        } else {
          // Grandfather Clock is an Advanced Circuit component and thus wont work in Vanilla Circuits.
          if (senderBlockType == BlockType.GrandfatherClock)
            return this.Result;

          if (!this.CircuitHandler.Config.OverrideVanillaCircuits) {
            WorldGen.hitSwitch(this.SenderMeasureData.OriginTileLocation.X, this.SenderMeasureData.OriginTileLocation.Y);
            return this.Result;
          }

          signal = SignalType.Swap;
        }

        if (overrideSignal != null)
          signal = overrideSignal.Value;
        
        if (
          switchSender && (
            senderBlockType == BlockType.Switch || 
            senderBlockType == BlockType.Lever || 
            senderBlockType == BlockType.XSecondTimer
          )
        ) {
          bool newSenderState;
          if (signal == SignalType.Swap)
            newSenderState = !TerrariaUtils.Tiles.ObjectHasActiveState(this.SenderMeasureData);
          else 
            newSenderState = AdvancedCircuits.SignalToBool(signal).Value;

          if (TerrariaUtils.Tiles.ObjectHasActiveState(this.SenderMeasureData) != newSenderState)
            TerrariaUtils.Tiles.SetObjectState(this.SenderMeasureData, newSenderState, !switchSenderLocalOnly);

          if (senderBlockType == BlockType.XSecondTimer) {
            this.RegisterUnregisterTimer(this.SenderMeasureData, newSenderState);

            // Timers do not execute circuits as they are switched.
            return this.Result;
          }
        }

        if (this.IsAdvancedCircuit) {
          this.PluginTrace.WriteLineVerbose(
            "Started processing Advanced Circuit at {0} with signal {1}.", 
            this.SenderMeasureData.OriginTileLocation, signal.ToString()
          );
        } else {
          this.PluginTrace.WriteLineVerbose(
            "Started processing Vanilla Circuit at {0} with signal {1}.", 
            this.SenderMeasureData.OriginTileLocation, signal.ToString()
          );
        }

        foreach (DPoint portLocation in AdvancedCircuits.EnumerateComponentPortLocations(this.SenderMeasureData)) {
          Tile portTile = TerrariaUtils.Tiles[portLocation];
          if (!portTile.wire)
            continue;

          DPoint portAdjacentTileLocation = AdvancedCircuits.GetPortAdjacentComponentTileLocation(
            this.SenderMeasureData, portLocation
          );

          SignalType portSignal = signal;
          if (this.IsAdvancedCircuit && portTile.active && portTile.type == (int)AdvancedCircuits.BlockType_NOTGate)
            portSignal = AdvancedCircuits.BoolToSignal(!AdvancedCircuits.SignalToBool(portSignal).Value);

          this.QueuedRootBranches.Add(new RootBranchProcessData(portAdjacentTileLocation, portLocation, portSignal));
        }

        if (this.IsAdvancedCircuit) {
          // Main Branch Processing Loop
          while (this.QueuedRootBranches.Count > 0) {
            RootBranchProcessData currentBranch = this.QueuedRootBranches[0];

            this.ProcessRootBranch(currentBranch);
            this.QueuedRootBranches.RemoveAt(0);
          }
        } else {
          // We know that the sender must have at least one of its tiles wired, we are too lazy to count them though, so we just
          // expect the wiring of the sender as 1.
          this.CircuitLength++;

          RootBranchProcessData dummyRoot = new RootBranchProcessData(
            this.SenderMeasureData.OriginTileLocation, this.SenderMeasureData.OriginTileLocation, signal
          );
          List<BranchProcessData> subBranches = new List<BranchProcessData>(this.QueuedRootBranches.Capacity);
          foreach (RootBranchProcessData rootBranch in this.QueuedRootBranches)
            subBranches.Add(rootBranch.ToBranchProcessData());

          this.ProcessSubBranches(dummyRoot, subBranches);
        }

        this.PostProcessCircuit();
      } catch (Exception ex) {
        throw new InvalidOperationException("Processing circuit failed. See inner exception for details.", ex);
      } finally {
        this.Result.ProcessingTime = DateTime.Now - processingStartTime;
        this.Result.OriginSignal = signal;
      }

      #if Verbose
      StringBuilder resultString = new StringBuilder();
      resultString.AppendFormat(
        "\nEnded processing circuit:\n  Sender: {0} {1}\n  Advanced Circuit: {2}\n  Processed Branches: {3}\n",
        TerrariaUtils.Tiles.GetBlockTypeName((BlockType)TerrariaUtils.Tiles[this.Result.SenderLocation].type), 
        this.Result.SenderLocation, this.Result.IsAdvancedCircuit, this.Result.ProcessedBranchCount
      );

      if (this.Result.WarnReason != CircuitWarnReason.None)
        resultString.AppendLine("  Warning: " + this.Result.WarnReason);

      if (this.Result.CancellationReason != CircuitCancellationReason.None) {
        string relatedComponentName;
        if (this.Result.CancellationRelatedComponentType == BlockType.Invalid)
          relatedComponentName = "None";
        else
          relatedComponentName = TerrariaUtils.Tiles.GetBlockTypeName(this.Result.CancellationRelatedComponentType);

        resultString.AppendFormat(
          "  Error: {0}\n  Error Related Component: {1}\n", this.Result.CancellationReason, relatedComponentName
        );
      }

      this.PluginTrace.WriteLineVerbose(resultString.ToString());
      #endif

      return this.Result;
    }

    protected void ProcessRootBranch(RootBranchProcessData rootBranch) {
      if (this.IsCancellationPending)
        return;

      Tile startTile = TerrariaUtils.Tiles[rootBranch.FirstWireLocation];
      if (!startTile.wire || (this.IsAdvancedCircuit && startTile.type == (int)AdvancedCircuits.BlockType_InputPort))
        return;

      SignalType signal = rootBranch.Signal;
      Direction direction = rootBranch.Direction;
      result.ProcessedBranchCount++;
      List<BranchProcessData> subBranches = new List<BranchProcessData>();
      // "Move" straight through the branch and register the initial adjacent sub-branches.
      {
        DPoint previousTileLocation = DPoint.Empty;
        DPoint currentTileLocation = rootBranch.FirstWireLocation;
        Tile currentTile = TerrariaUtils.Tiles[currentTileLocation];
        while (currentTile.wire && !this.IsCancellationPending) {
          this.ProcessTile(rootBranch, currentTileLocation, previousTileLocation, signal);

          DPoint adjacentTileLocation1;
          DPoint adjacentTileLocation2;
          if (direction == Direction.Left || direction == Direction.Right) {
            adjacentTileLocation1 = new DPoint(currentTileLocation.X, currentTileLocation.Y - 1);
            adjacentTileLocation2 = new DPoint(currentTileLocation.X, currentTileLocation.Y + 1);
          } else {
            adjacentTileLocation1 = new DPoint(currentTileLocation.X + 1, currentTileLocation.Y);
            adjacentTileLocation2 = new DPoint(currentTileLocation.X - 1, currentTileLocation.Y);
          }

          Tile adjacentTile1 = TerrariaUtils.Tiles[adjacentTileLocation1];
          Tile adjacentTile2 = TerrariaUtils.Tiles[adjacentTileLocation2];
          if (adjacentTile1.wire)
            subBranches.Add(new BranchProcessData(currentTileLocation, adjacentTileLocation1, signal));
          else
            this.ProcessTile(rootBranch, adjacentTileLocation1, currentTileLocation, signal);

          if (adjacentTile2.wire)
            subBranches.Add(new BranchProcessData(currentTileLocation, adjacentTileLocation2, signal));
          else
            this.ProcessTile(rootBranch, adjacentTileLocation2, currentTileLocation, signal);

          // Next tile
          previousTileLocation = currentTileLocation;
          currentTileLocation = currentTileLocation.OffsetTowards(direction);
          currentTile = TerrariaUtils.Tiles[currentTileLocation];
        }

        // The tile above the "peak" of the branch may also contain a Port Defining Component.
        if (this.IsAdvancedCircuit)
          this.ProcessTile(rootBranch, currentTileLocation, previousTileLocation, signal);

        rootBranch.LastWireLocation = previousTileLocation;
      }
      
      if (this.IsCancellationPending)
        return;

      this.ProcessSubBranches(rootBranch, subBranches);
    }

    private void ProcessSubBranches(RootBranchProcessData rootBranch, IEnumerable<BranchProcessData> subBranches) {
      SignalType signal = rootBranch.Signal;
      List<BranchProcessData> queuedSubBranches = new List<BranchProcessData>(subBranches);

      // Process all Sub-Branches and their Sub-Branches
      List<BranchProcessData> processedSubBranches = new List<BranchProcessData>();
      processedSubBranches.Add(rootBranch.ToBranchProcessData());

      while (queuedSubBranches.Count > 0) {
        int currentBranchIndex = queuedSubBranches.Count - 1;
        BranchProcessData currentBranch = queuedSubBranches[currentBranchIndex];
        result.ProcessedBranchCount++;

        DPoint previousTileLocation = currentBranch.BranchingTileLocation;
        DPoint currentTileLocation = currentBranch.FirstWireLocation;
        Tile currentTile = TerrariaUtils.Tiles[currentTileLocation];
        while (currentTile.wire && !this.IsCancellationPending) {
          bool alreadyProcessed = false;
          for (int i = 0; i < processedSubBranches.Count; i++) {
            if (processedSubBranches[i].IsTileInBetween(currentTileLocation)) {
              alreadyProcessed = true;
              break;
            }
          }
          if (!alreadyProcessed)
            this.ProcessTile(rootBranch, currentTileLocation, previousTileLocation, signal);

          DPoint adjacentTileLocation1;
          DPoint adjacentTileLocation2;
          if (currentBranch.Direction == Direction.Left || currentBranch.Direction == Direction.Right) {
            adjacentTileLocation1 = new DPoint(currentTileLocation.X, currentTileLocation.Y - 1);
            adjacentTileLocation2 = new DPoint(currentTileLocation.X, currentTileLocation.Y + 1);
          } else {
            adjacentTileLocation1 = new DPoint(currentTileLocation.X + 1, currentTileLocation.Y);
            adjacentTileLocation2 = new DPoint(currentTileLocation.X - 1, currentTileLocation.Y);
          }

          Tile adjacentTile1 = TerrariaUtils.Tiles[adjacentTileLocation1];
          Tile adjacentTile2 = TerrariaUtils.Tiles[adjacentTileLocation2];
          if (adjacentTile1.wire) {
            alreadyProcessed = false;
            for (int i = 0; i < processedSubBranches.Count; i++) {
              if (processedSubBranches[i].IsTileInBetween(adjacentTileLocation1)) {
                alreadyProcessed = true;
                break;
              }
            }

            if (!alreadyProcessed) {
              bool alreadyQueued = false;
              for (int i = 0; i < queuedSubBranches.Count; i++) {
                if (
                  queuedSubBranches[i].FirstWireLocation == adjacentTileLocation1 ||
                  queuedSubBranches[i].LastWireLocation == adjacentTileLocation1
                ) {
                  alreadyQueued = true;
                  break;
                }
              }

              if (!alreadyQueued)
                queuedSubBranches.Add(new BranchProcessData(currentTileLocation, adjacentTileLocation1, signal));
            }
          } else {
            this.ProcessTile(rootBranch, adjacentTileLocation1, currentTileLocation, signal);
          }
          if (adjacentTile2.wire) {
            alreadyProcessed = false;
            for (int i = 0; i < processedSubBranches.Count; i++) {
              if (processedSubBranches[i].IsTileInBetween(adjacentTileLocation2)) {
                alreadyProcessed = true;
                break;
              }
            }

            if (!alreadyProcessed) {
              bool alreadyQueued = false;
              for (int i = 0; i < queuedSubBranches.Count; i++) {
                if (
                  queuedSubBranches[i].FirstWireLocation == adjacentTileLocation2 ||
                  queuedSubBranches[i].LastWireLocation == adjacentTileLocation2
                ) {
                  alreadyQueued = true;
                  break;
                }
              }

              if (!alreadyQueued)
                queuedSubBranches.Add(new BranchProcessData(currentTileLocation, adjacentTileLocation2, signal));
            }
          } else {
            this.ProcessTile(rootBranch, adjacentTileLocation2, currentTileLocation, signal);
          }

          // Next tile
          previousTileLocation = currentTileLocation;
          currentTileLocation = currentTileLocation.OffsetTowards(currentBranch.Direction);
          currentTile = TerrariaUtils.Tiles[currentTileLocation];
        }

        // The tile above the "peak" of the branch may also contain a Port Defining Component.
        if (this.IsAdvancedCircuit)
          this.ProcessTile(rootBranch, currentTileLocation, previousTileLocation, signal);

        currentBranch.LastWireLocation = previousTileLocation;
        processedSubBranches.Add(currentBranch);

        queuedSubBranches.RemoveAt(currentBranchIndex);
      }
    }

    protected virtual void ProcessTile(
      RootBranchProcessData rootBranch, DPoint tileLocation, DPoint adjacentTileLocation, SignalType signal
    ) {
      if (this.IsCancellationPending)
        return;

      Tile tile = TerrariaUtils.Tiles[tileLocation];

      // If the tile has no wire it might be a AC-Component and thus the adjacent tile would be its port.
      if (!tile.wire && tile.active) {
        if (!this.IsAdvancedCircuit || adjacentTileLocation == DPoint.Empty)
          return;
        if (!AdvancedCircuits.IsPortDefiningComponentBlock((BlockType)tile.type))
          return;
        if (signal == SignalType.Swap)
          throw new ArgumentException("A Port can not receive a Swap signal.", "signal");

        ObjectMeasureData acComponentMeasureData = TerrariaUtils.Tiles.MeasureObject(tileLocation);
        // The origin sender can only signal itself if it is a timer.
        if (
          acComponentMeasureData.OriginTileLocation == this.SenderMeasureData.OriginTileLocation &&
          tile.type != (int)BlockType.XSecondTimer
        ) {
          return;
        }

        if (rootBranch.SignaledComponentLocations.Contains(acComponentMeasureData.OriginTileLocation))
          return;

        int componentSignalCounter;
        this.PortDefiningComponentSignalCounter.TryGetValue(acComponentMeasureData.OriginTileLocation, out componentSignalCounter);
        if (componentSignalCounter > CircuitProcessor.PortDefiningComponentSignalMaximum) {
          this.Result.CancellationReason = CircuitCancellationReason.SignaledSameComponentTooOften;
          this.Result.CancellationRelatedComponentType = acComponentMeasureData.BlockType;
          return;
        }

        if (this.SignalPortDefiningComponent(
          rootBranch, acComponentMeasureData, adjacentTileLocation, AdvancedCircuits.SignalToBool(signal).Value
        )) {
          rootBranch.SignaledComponentLocations.Add(acComponentMeasureData.OriginTileLocation);

          if (componentSignalCounter == default(int))
            this.PortDefiningComponentSignalCounter.Add(acComponentMeasureData.OriginTileLocation, 1);
          else 
            this.PortDefiningComponentSignalCounter[acComponentMeasureData.OriginTileLocation] = componentSignalCounter + 1;

          this.Result.SignaledPortDefiningComponentsCounter++;
        }

        return;
      }

      if (!tile.wire)
        return;

      try {
        if (rootBranch.BlockActivator != null) {
          Tile blockActivatorTile = TerrariaUtils.Tiles[rootBranch.BlockActivatorLocation];
          if (tile.wall == blockActivatorTile.wall) {
            Tile tileAbove = TerrariaUtils.Tiles[tileLocation.OffsetEx(0, -1)];
            if (!tileAbove.active || tileAbove.type != (int)BlockType.Chest) {
              if (
                signal == SignalType.Off && tile.active && AdvancedCircuits.IsCustomActivatableBlock((BlockType)tile.type)
              ) {
                if (rootBranch.BlockActivator.RegisteredInactiveBlocks.Count > this.CircuitHandler.Config.BlockActivatorConfig.MaxChangeableBlocks) {
                  this.Result.WarnReason = CircuitWarnReason.BlockActivatorChangedTooManyBlocks;
                  return;
                }

                rootBranch.BlockActivator.RegisteredInactiveBlocks.Add(tileLocation, (BlockType)tile.type);

                tile.type = 0;
                tile.active = false;
                tile.frameX = -1;
                tile.frameY = -1;
                tile.frameNumber = 0;
                this.TilesToFrameOnPost.Add(tileLocation);
              
                return;
              } else if (
                signal == SignalType.On && (rootBranch.BlockActivatorMode == BlockActivatorMode.ReplaceBlocks || !tile.active)
              ) {
                BlockType registeredBlockType;
                if (rootBranch.BlockActivator.RegisteredInactiveBlocks.TryGetValue(tileLocation, out registeredBlockType)) {
                  rootBranch.BlockActivator.RegisteredInactiveBlocks.Remove(tileLocation);

                  tile.type = (byte)registeredBlockType;
                  tile.active = true;
                  this.TilesToFrameOnPost.Add(tileLocation);

                  return;
                }
              }
            }
          }
        }

        if (!tile.active)
          return;

        ObjectMeasureData componentMeasureData = TerrariaUtils.Tiles.MeasureObject(tileLocation);
        if (rootBranch.SignaledComponentLocations.Contains(componentMeasureData.OriginTileLocation))
          return;

        // The origin sender can never signal itself if wired directly.
        if (componentMeasureData.OriginTileLocation == this.SenderMeasureData.OriginTileLocation)
          return;

        // Switches and Levers can not be signaled if they are wired directly.
        if (tile.type == (int)BlockType.Switch || tile.type == (int)BlockType.Lever)
          return;

        if (this.SignalComponent(ref componentMeasureData, signal)) {
          rootBranch.SignaledComponentLocations.Add(componentMeasureData.OriginTileLocation);
          this.Result.SignaledComponentsCounter++;
        }
      } finally {
        this.CircuitLength++;
        
        if (this.CircuitLength >= this.CircuitHandler.Config.MaxCircuitLength) {
          this.Result.CancellationReason = CircuitCancellationReason.ExceededMaxLength;
          this.PluginTrace.WriteLineInfo(
            "Processing of the circuit at {0} was cancelled because the signal reached the maximum transfer length of {1} wires.",
            this.SenderMeasureData.OriginTileLocation, this.CircuitHandler.Config.MaxCircuitLength
          );
        }
      }
    }

    protected void PostProcessCircuit() {
      foreach (DPoint tileToFrameLocation in this.TilesToFrameOnPost) {
        WorldGen.SquareTileFrame(tileToFrameLocation.X, tileToFrameLocation.Y);
        TSPlayer.All.SendTileSquare(tileToFrameLocation.X, tileToFrameLocation.Y, 1);
      }

      // Transfer Liquid
      if (this.SignaledInletPumps.Count > 0 && this.SignaledOutletPumps.Count > 0) {
        // The first liquid kind we encounter will be only liquid to be transfered.
        bool? transferWater = null;

        // Measure the inputable liquid.
        int inputableLiquid = 0;
        int totalLoss = 0;
        for (int i = 0; i < this.SignaledInletPumps.Count; i++) {
          DPoint inletPumpLocation = this.SignaledInletPumps[i];
          ObjectMeasureData measureData = TerrariaUtils.Tiles.MeasureObject(inletPumpLocation);
          int modifiers = AdvancedCircuits.CountComponentModifiers(measureData);
          ComponentConfigProfile configProfile = AdvancedCircuits.ModifierCountToConfigProfile(modifiers);
          PumpConfig pumpConfig;
          if (!this.CircuitHandler.Config.PumpConfigs.TryGetValue(configProfile, out pumpConfig))
            pumpConfig = this.CircuitHandler.Config.PumpConfigs[ComponentConfigProfile.Default];

          bool hasLiquid = false;
          foreach (Tile pumpTile in TerrariaUtils.Tiles.EnumerateObjectTiles(measureData)) {
            if (pumpTile.liquid > 0) {
              if (!transferWater.HasValue)
                transferWater = !pumpTile.lava;
              if (transferWater.Value && pumpTile.lava)
                continue;

              hasLiquid = true;
              inputableLiquid += pumpTile.liquid;
            }
          }
          if (!hasLiquid) {
            this.SignaledInletPumps.RemoveAt(i--);
            continue;
          }

          Contract.Assert(transferWater.HasValue);

          int maxTransferableLiquid;
          if (transferWater.Value) 
            maxTransferableLiquid = pumpConfig.TransferableWater;
          else
            maxTransferableLiquid = pumpConfig.TransferableLava;

          totalLoss += pumpConfig.LossValue;
          inputableLiquid = Math.Min(inputableLiquid, maxTransferableLiquid);
        }
        if (this.SignaledInletPumps.Count > 0) {
          Contract.Assert(transferWater.HasValue);

          // Measure the outputable liquid.
          int outputableLiquid = 0;
          for (int i = 0; i < this.SignaledOutletPumps.Count; i++) {
            DPoint outletPumpLocation = this.SignaledOutletPumps[i];
            ObjectMeasureData measureData = TerrariaUtils.Tiles.MeasureObject(outletPumpLocation);

            bool canOutput = false;
            foreach (Tile pumpTile in TerrariaUtils.Tiles.EnumerateObjectTiles(measureData)) {
              if (pumpTile.liquid > 0 && transferWater.Value == pumpTile.lava)
                continue;
              if (pumpTile.liquid == 255)
                continue;

              canOutput = true;
              outputableLiquid += 255 - pumpTile.liquid;
            }

            if (!canOutput)
              this.SignaledOutletPumps.RemoveAt(i--);
          }

          if (this.SignaledOutletPumps.Count > 0) {
            // Take the input liquid.
            int liquidToTransfer = Math.Min(inputableLiquid, outputableLiquid);
            int liquidToInput = liquidToTransfer;
            for (int i = 0; i < this.SignaledInletPumps.Count; i++) {
              DPoint inletPumpLocation = this.SignaledInletPumps[i];
              ObjectMeasureData measureData = TerrariaUtils.Tiles.MeasureObject(inletPumpLocation);

              foreach (DPoint pumpTileLocation in TerrariaUtils.Tiles.EnumerateObjectTileLocations(measureData)) {
                Tile pumpTile = TerrariaUtils.Tiles[pumpTileLocation];
                if (pumpTile.liquid <= liquidToInput) {
                  liquidToInput -= pumpTile.liquid;
                  pumpTile.liquid = 0;
                } else {
                  pumpTile.liquid -= (byte)liquidToInput;
                  liquidToInput = 0;
                }

                WorldGen.SquareTileFrame(pumpTileLocation.X, pumpTileLocation.Y, true);
              }
            }
            liquidToTransfer -= totalLoss;

            // Output the liquid.
            for (int i = 0; i < this.SignaledOutletPumps.Count; i++) {
              DPoint outletPumpLocation = this.SignaledOutletPumps[i];

              for (int y = 1; y >= 0; y--) {
                for (int x = 0; x < 2; x++) {
                  DPoint pumpTileLocation = new DPoint(outletPumpLocation.X + x, outletPumpLocation.Y + y);
                  Tile pumpTile = TerrariaUtils.Tiles[pumpTileLocation];
                  if (pumpTile.liquid == 255)
                    continue;

                  byte transferedLiquid = (byte)Math.Min(liquidToTransfer, 255 - pumpTile.liquid);
                  pumpTile.lava = !transferWater.Value;
                  pumpTile.liquid += transferedLiquid;
                  liquidToTransfer = Math.Max(liquidToTransfer - transferedLiquid, 0);

                  WorldGen.SquareTileFrame(pumpTileLocation.X, pumpTileLocation.Y, true);
                }
              }
            }
          }
        }
      }
    }
    #endregion

    #region [Methods: SignalComponent, SignalPortDefiningComponent]
    protected bool SignalComponent(ref ObjectMeasureData measureData, SignalType signal, bool localOnly = false) {
      int originX = measureData.OriginTileLocation.X;
      int originY = measureData.OriginTileLocation.Y;

      switch (measureData.BlockType) {
        case BlockType.Torch:
        case BlockType.XMasLight:
        case BlockType.Candle:
        case BlockType.ChainLantern:
        case BlockType.ChineseLantern:
        case BlockType.Candelabra:
        case BlockType.DiscoBall:
        case BlockType.TikiTorch:
        case BlockType.CopperChandelier:
        case BlockType.SilverChandelier:
        case BlockType.GoldChandelier:
        case BlockType.LampPost:
        case BlockType.MusicBox:
        case BlockType.XSecondTimer: {
          bool currentState = TerrariaUtils.Tiles.ObjectHasActiveState(measureData);
          bool newState;
          if (signal == SignalType.Swap)
            newState = !currentState;
          else
            newState = AdvancedCircuits.SignalToBool(signal).Value;

          if (measureData.BlockType == BlockType.XSecondTimer) {
            // Directly wired Timers in an Advanced Circuit are not meant to be switched.
            if (this.IsAdvancedCircuit)
              return true;

            if (newState != currentState)
              this.RegisterUnregisterTimer(measureData, newState);
          }

          if (newState != currentState)
            TerrariaUtils.Tiles.SetObjectState(measureData, newState, !localOnly);

          return true;
        }
        case BlockType.ActiveStone:
        case BlockType.InactiveStone: {
          bool currentState = (measureData.BlockType == BlockType.ActiveStone);
          bool newState;
          if (signal == SignalType.Swap)
            newState = !currentState;
          else
            newState = AdvancedCircuits.SignalToBool(signal).Value;

          if (newState != currentState) {
            BlockType newBlockType;
            if (newState)
              newBlockType = BlockType.ActiveStone;
            else
              newBlockType = BlockType.InactiveStone;

            TerrariaUtils.Tiles[measureData.OriginTileLocation].type = (byte)newBlockType;
            WorldGen.SquareTileFrame(originX, originY);
            TSPlayer.All.SendTileSquareEx(originX, originY, 1);
          }
          
          return true;
        }
        case BlockType.DoorClosed:
        case BlockType.DoorOpened: {
          bool currentState = (measureData.BlockType == BlockType.DoorOpened);
          bool newState;
          if (signal == SignalType.Swap)
            newState = !currentState;
          else
            newState = AdvancedCircuits.SignalToBool(signal).Value;

          if (newState != currentState) {
            int doorX = measureData.OriginTileLocation.X;
            int doorY = measureData.OriginTileLocation.Y;

            if (newState) {
              // A door will always try to open to the opposite site of the sender's location that triggered the circuit first.
              int direction = 1;
              if (doorX < this.SenderMeasureData.OriginTileLocation.X)
                direction = -1;

              // If opening it towards one direction doesn't work, try the other.
              currentState = WorldGen.OpenDoor(doorX, doorY, direction);
              if (!currentState) {
                direction *= -1;
                currentState = WorldGen.OpenDoor(doorX, doorY, direction);
              }

              if (currentState) {
                TSPlayer.All.SendData(PacketTypes.DoorUse, string.Empty, 0, doorX, doorY, direction);
                // Because the door changed its sprite, we have to re-measure it now.
                measureData = TerrariaUtils.Tiles.MeasureObject(measureData.OriginTileLocation);
              }
            } else {
              if (WorldGen.CloseDoor(doorX, doorY, true))
                TSPlayer.All.SendData(PacketTypes.DoorUse, string.Empty, 1, doorX, doorY);
            }

            WorldGen.numWire = 0;
            WorldGen.numNoWire = 0;
          }

          return true;
        }
        case BlockType.InletPump:
        case BlockType.OutletPump: {
          if (signal == SignalType.Off)
            return true;

          PumpConfig pumpConfig;
          ComponentConfigProfile configProfile = AdvancedCircuits.ModifierCountToConfigProfile(
            AdvancedCircuits.CountComponentModifiers(measureData)
          );
          if (
            (this.CircuitHandler.Config.PumpConfigs.TryGetValue(configProfile, out pumpConfig) ||
            (this.CircuitHandler.Config.PumpConfigs.TryGetValue(ComponentConfigProfile.Default, out pumpConfig))) &&
            WorldGen.checkMech(originX, originY, pumpConfig.Cooldown)
          ) {
            if (this.Result.SignaledPumps > this.CircuitHandler.Config.MaxPumpsPerCircuit) {
              this.Result.WarnReason = CircuitWarnReason.SignalesTooManyPumps;
              return true;
            }
            if (
              pumpConfig.TriggerPermission != null && 
              this.TriggeringPlayer != TSPlayer.Server && 
              !this.TriggeringPlayer.Group.HasPermission(pumpConfig.TriggerPermission)
            ) {
              this.Result.WarnReason = CircuitWarnReason.InsufficientPermissionToSignalComponent;
              this.Result.WarnRelatedComponentType = measureData.BlockType;
              return true;
            }

            if (measureData.BlockType == BlockType.InletPump)
              this.SignaledInletPumps.Add(new DPoint(originX, originY));
            else
              this.SignaledOutletPumps.Add(new DPoint(originX, originY));

            this.Result.SignaledPumps++;
          }

          return true;
        }
        case BlockType.DartTrap: {
          if (signal == SignalType.Off)
            return true;

          DartTrapConfig trapConfig;
          ComponentConfigProfile configProfile = AdvancedCircuits.ModifierCountToConfigProfile(
            AdvancedCircuits.CountComponentModifiers(measureData)
          );
          if (
            (this.CircuitHandler.Config.DartTrapConfigs.TryGetValue(configProfile, out trapConfig) ||
            (this.CircuitHandler.Config.DartTrapConfigs.TryGetValue(ComponentConfigProfile.Default, out trapConfig))) &&
            WorldGen.checkMech(originX, originY, trapConfig.Cooldown)
          ) {
            if (this.Result.SignaledDartTraps > this.CircuitHandler.Config.MaxDartTrapsPerCircuit) {
              this.Result.WarnReason = CircuitWarnReason.SignalesTooManyDartTraps;
              return true;
            }
            if (
              trapConfig.TriggerPermission != null && 
              this.TriggeringPlayer != TSPlayer.Server && 
              !this.TriggeringPlayer.Group.HasPermission(trapConfig.TriggerPermission)
            ) {
              this.Result.WarnReason = CircuitWarnReason.InsufficientPermissionToSignalComponent;
              this.Result.WarnRelatedComponentType = BlockType.DartTrap;
              return true;
            }

            int projectileIndex = 1000;
            for (int i = 0; i < 1000; ++i) {
              if (!Main.projectile[i].active) {
                projectileIndex = i;
                break;
              }
            }
            if (projectileIndex == 1000)
              return true;

            bool isFacingLeft = (TerrariaUtils.Tiles[originX, originY].frameX == 0);
            float projectileAngle = trapConfig.ProjectileAngle;
            if (isFacingLeft)
              projectileAngle += 180f;

            Vector2 normalizedPolarOffset = new Vector2(
              (float)Math.Cos(Math.PI * projectileAngle / 180f),
              (float)Math.Sin(Math.PI * projectileAngle / 180f)
            );
            Main.projectile[projectileIndex].SetDefaults(trapConfig.ProjectileType);
            Vector2 projectileSpawn = new Vector2(
              (originX * TerrariaUtils.TileSize + (trapConfig.ProjectileOffset * normalizedPolarOffset.X)), 
              (originY * TerrariaUtils.TileSize + (trapConfig.ProjectileOffset * normalizedPolarOffset.Y))
            );
            projectileSpawn = projectileSpawn.Add(new Vector2(
              TerrariaUtils.TileSize / 2 - Main.projectile[projectileIndex].width / 2, 
              TerrariaUtils.TileSize / 2 - Main.projectile[projectileIndex].height / 2
            ));
            Main.projectile[projectileIndex].position.X = projectileSpawn.X;
            Main.projectile[projectileIndex].position.Y = projectileSpawn.Y;
            Main.projectile[projectileIndex].owner = Main.myPlayer;
            Main.projectile[projectileIndex].velocity.X = (trapConfig.ProjectileSpeed * normalizedPolarOffset.X);
            Main.projectile[projectileIndex].velocity.Y = (trapConfig.ProjectileSpeed * normalizedPolarOffset.Y);
            Main.projectile[projectileIndex].damage = trapConfig.ProjectileDamage;
            Main.projectile[projectileIndex].knockBack = trapConfig.ProjectileKnockback;
            Main.projectile[projectileIndex].identity = projectileIndex;
            Main.projectile[projectileIndex].timeLeft = trapConfig.ProjectileLifeTime;
            Main.projectile[projectileIndex].wet = Collision.WetCollision(
              Main.projectile[projectileIndex].position, Main.projectile[projectileIndex].width, Main.projectile[projectileIndex].height
            );
            TSPlayer.All.SendData(PacketTypes.ProjectileNew, string.Empty, projectileIndex);
            
            this.Result.SignaledDartTraps++;

            /*int projectileIndex = Projectile.NewProjectile(
              projectileSpawn.X, projectileSpawn.Y, projectileSpeed, 0f, projectileType, projectileDamage, 
              ProjectileKnockBack, Main.myPlayer
            );*/

            this.Result.SignaledDartTraps++;
          }
          
          return true;
        }
        case BlockType.Explosives: {
          if (signal == SignalType.Off)
            return true;

          WorldGen.KillTile(originX, originY, false, false, true);
          TSPlayer.All.SendTileSquareEx(originX, originY, 1);
          Projectile.NewProjectile((originX * 16 + 8), (originY * 16 + 8), 0f, 0f, 108, 250, 10f, Main.myPlayer);
          
          return true;
        }
        case BlockType.Statue: {
          if (signal == SignalType.Off)
            return true;

          StatueStyle statueStyle = TerrariaUtils.Tiles.GetStatueStyle(TerrariaUtils.Tiles[measureData.OriginTileLocation]);
          StatueConfig statueConfig;
          if (
            this.CircuitHandler.Config.StatueConfigs.TryGetValue(statueStyle, out statueConfig) && statueConfig != null &&
            WorldGen.checkMech(originX, originY, statueConfig.Cooldown)
          ) {
            if (this.Result.SignaledStatues > this.CircuitHandler.Config.MaxStatuesPerCircuit) {
              this.Result.WarnReason = CircuitWarnReason.SignalesTooManyStatues;
              return true;
            }
            if (
              statueConfig.TriggerPermission != null && 
              this.TriggeringPlayer != TSPlayer.Server && 
              !this.TriggeringPlayer.Group.HasPermission(statueConfig.TriggerPermission)
            ) {
              this.Result.WarnReason = CircuitWarnReason.InsufficientPermissionToSignalComponent;
              this.Result.WarnRelatedComponentType = BlockType.Statue;
              return true;
            }

            DPoint spawnLocation = new DPoint((originX + 1) * 16, (originY + 1) * 16);
            switch (statueConfig.ActionType) {
              case StatueActionType.MoveNPC:
                // Param1 = NPC group
                // Param2 = Specific NPC id
                  
                List<int> npcIndexes = null;
                switch (statueConfig.ActionParam) {
                  // Random
                  case 0:
                    npcIndexes = new List<int>(TerrariaUtils.Npcs.EnumerateFriendlyNPCIndexes());
                    break;
                  // Female
                  case 1:
                    npcIndexes = new List<int>(TerrariaUtils.Npcs.EnumerateFriendlyFemaleNPCIndexes());
                    break;
                  // Male
                  case 2:
                    npcIndexes = new List<int>(TerrariaUtils.Npcs.EnumerateFriendlyMaleNPCIndexes());
                    break;
                  // Specific
                  case 3:
                    npcIndexes = new List<int>(TerrariaUtils.Npcs.EnumerateSpecificNPCIndexes(
                      new List<int> { statueConfig.ActionParam2 }
                    ));
                    break;
                }

                if (npcIndexes != null && npcIndexes.Count > 0) {
                  Random rnd = new Random();
                  int pickedIndex = npcIndexes[rnd.Next(npcIndexes.Count)];
                  Main.npc[pickedIndex].position.X = (spawnLocation.X - (Main.npc[pickedIndex].width / 2));
                  Main.npc[pickedIndex].position.Y = (spawnLocation.Y - (Main.npc[pickedIndex].height - 1));

                  NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, string.Empty, pickedIndex);
                }
                
                break;
              case StatueActionType.SpawnMob:
                // Param1 = Mob type
                // Param2 = Max mobs in range
                // Param3 = Range to check
                /*if (TerrariaUtils.Npcs.EnumerateNPCsInRange(spawnLocation, statueConfig.ActionParam3) < statueConfig.ActionParam2) {
                  int mobIndex = NPC.NewNPC(spawnLocation.X, spawnLocation.Y, statueConfig.ActionParam);
                  // Ensure that the spawned mob drops no money.
                  Main.npc[mobIndex].value = 0.0f;
                  Main.npc[mobIndex].npcSlots = 0.0f;
                }*/

                break;
              case StatueActionType.SpawnItem:
                // Param1 = Item type
                // Param2 = Max mobs in range
                // Param3 = Range to check
                /*if (TerrariaUtils.Items.EnumerateItemsAroundPoint(spawnLocation, statueConfig.ActionParam, statueConfig.ActionParam3) < statueConfig.ActionParam2)
                  Item.NewItem(spawnLocation.X, spawnLocation.Y, 0, 0, statueConfig.ActionParam);*/
                  
                break;
              case StatueActionType.SpawnBoss:

                break;
            }

            this.Result.SignaledStatues++;
          }

          return true;
        }
        case BlockType.Sign: {
          if (
            !this.IsAdvancedCircuit || signal == SignalType.Off || this.TriggeringPlayer == TSPlayer.Server || 
            this.TriggeredPassively
          )
            return true;

          string signText = Main.sign[Sign.ReadSign(originX, originY)].text;
          if (signText == null)
            return true;

          if (
            this.CircuitHandler.PluginCooperationHandler.IsSignCommandsAvailable &&
            this.CircuitHandler.PluginCooperationHandler.SignCommands_CheckIsSignCommand(signText)
          ) {
            this.CircuitHandler.PluginCooperationHandler.SignCommands_ExecuteSignCommand(
              this.TriggeringPlayer, measureData.OriginTileLocation, signText
            );
            return true;
          }

          if (!WorldGen.checkMech(originX, originY, 300))
            return true;

          string fullText = this.CircuitHandler.Config.SignPrefix + signText;
          int lineStartIndex = 0;
          int lineLength = 0;
          for (int i = 0; i < fullText.Length; i++) {
            if (lineLength == 60 || fullText[i] == '\n' || (i == fullText.Length - 1 && lineLength > 0)) {
              if (fullText[i] == '\n') {
                if (lineLength > 0)
                  this.TriggeringPlayer.SendInfoMessage(fullText.Substring(lineStartIndex, i - lineStartIndex));
                else 
                  this.TriggeringPlayer.SendInfoMessage(string.Empty);

                lineStartIndex = i + 1;
              } else if (i == fullText.Length - 1) {
                this.TriggeringPlayer.SendInfoMessage(fullText.Substring(lineStartIndex, i - lineStartIndex + 1));
                lineStartIndex = i;
              } else {
                this.TriggeringPlayer.SendInfoMessage(fullText.Substring(lineStartIndex, i - lineStartIndex));
                lineStartIndex = i;
              }
              
              lineLength = 0;
              continue;
            }

            lineLength++;
          }

          return true;
        }
        case BlockType.Boulder: {
          if (!this.IsAdvancedCircuit || signal == SignalType.Off)
            return true;

          WorldGen.KillTile(originX, originY, false, false, true);
          TSPlayer.All.SendTileSquareEx(originX, originY, 2);

          return true;
        }
      }

      return false;
    }

    private bool SignalPortDefiningComponent(
      RootBranchProcessData rootBranch, ObjectMeasureData measureData, DPoint portLocation, bool signal
    ) {
      if (!this.IsAdvancedCircuit)
        throw new InvalidOperationException("This is no advanced circuit.");

      Tile portTile = TerrariaUtils.Tiles[portLocation];
      Tile componentTile = TerrariaUtils.Tiles[measureData.OriginTileLocation];
      if (!portTile.wire || componentTile.wire)
        return false;
      if (
        portTile.active && portTile.type == (int)AdvancedCircuits.BlockType_NOTGate && 
        measureData.BlockType != AdvancedCircuits.BlockType_NOTGate
      ) {
        signal = !signal;
      }

      List<DPoint> componentPorts = null;
      bool outputSignal = signal;
      DPoint componentLocation = measureData.OriginTileLocation;
      
      BlockActivatorMetadata blockActivatorToRegister = null;
      DPoint blockActivatorLocationToRegister = DPoint.Empty;
      BlockActivatorMode blockActivatorModeToRegister = BlockActivatorMode.Default;
      switch (measureData.BlockType) {
        case BlockType.XSecondTimer: {
          if (!portTile.active || portTile.type != (int)AdvancedCircuits.BlockType_InputPort)
            return false;

          bool currentState = (TerrariaUtils.Tiles.ObjectHasActiveState(measureData));
          if (currentState != signal)
            TerrariaUtils.Tiles.SetObjectState(measureData, signal);

          if (currentState != signal)
            this.RegisterUnregisterTimer(measureData, signal);
          else if (signal)
            this.ResetTimer(measureData);

          return true;
        }
        case BlockType.Switch:
        case BlockType.Lever: {
          bool currentState = (TerrariaUtils.Tiles.ObjectHasActiveState(measureData));
          int modifiers = AdvancedCircuits.CountComponentModifiers(measureData);

          if (currentState != signal)
            TerrariaUtils.Tiles.SetObjectState(measureData, signal);
          else if (modifiers == 0)
            return true;

          break;
        }
        case AdvancedCircuits.BlockType_NOTGate: {
          if (!portTile.active || portTile.type != (int)AdvancedCircuits.BlockType_InputPort)
            return false;

          outputSignal = !signal;
          break;
        }
        case AdvancedCircuits.BlockType_ANDGate:
        case AdvancedCircuits.BlockType_ORGate:
        case AdvancedCircuits.BlockType_XORGate: {
          if (!portTile.active || portTile.type != (int)AdvancedCircuits.BlockType_InputPort)
            return false;

          int modifiers = AdvancedCircuits.CountComponentModifiers(measureData);
          GateStateMetadata metadata;
          switch (modifiers) {
            default:
              if (!this.CircuitHandler.WorldMetadata.GateStates.TryGetValue(componentLocation, out metadata)) {
                metadata = new GateStateMetadata();
                this.CircuitHandler.WorldMetadata.GateStates.Add(componentLocation, metadata);
              }
              break;
            case 1:
              if (this.temporaryGateStates == null)
                this.temporaryGateStates = new Dictionary<DPoint,GateStateMetadata>();

              if (!temporaryGateStates.TryGetValue(componentLocation, out metadata)) {
                metadata = new GateStateMetadata();
                temporaryGateStates.Add(componentLocation, metadata);
              }
              break;
          }
          
          componentPorts = new List<DPoint>(AdvancedCircuits.EnumerateComponentPortLocations(measureData));
          int inputPorts = 0;
          int signaledPorts = 0;
          bool isInvalid = false;
          for (int i = 0; i < componentPorts.Count; i++) {
            DPoint port = componentPorts[i];
            portTile = TerrariaUtils.Tiles[port];
            if (portTile.wire && portTile.active && portTile.type == (int)AdvancedCircuits.BlockType_InputPort) {
              inputPorts++;

              if (port == portLocation)
                metadata.PortStates[i] = outputSignal;

              if (metadata.PortStates[i] == null)
                isInvalid = true;
            }

            if (metadata.PortStates[i] != null && metadata.PortStates[i].Value)
              signaledPorts++;
          }

          // Gates will not operate as long as the input port states are not clear.
          if (isInvalid)
            return false;

          switch ((BlockType)componentTile.type) {
            case AdvancedCircuits.BlockType_ANDGate:
              outputSignal = (inputPorts == signaledPorts);
              break;
            case AdvancedCircuits.BlockType_ORGate:
              outputSignal = (signaledPorts > 0);
              break;
            case AdvancedCircuits.BlockType_XORGate:
              outputSignal = (signaledPorts != 0 && signaledPorts < inputPorts);
              break;
          }
          
          break;
        }
        case AdvancedCircuits.BlockType_Swapper: {
          if (!signal)
            return false;

          int activeSwapperIndex = this.CircuitHandler.WorldMetadata.ActiveSwapperLocations.IndexOf(componentLocation);
          if (activeSwapperIndex == -1) {
            this.CircuitHandler.WorldMetadata.ActiveSwapperLocations.Add(componentLocation);
            outputSignal = false;
          } else {
            this.CircuitHandler.WorldMetadata.ActiveSwapperLocations.RemoveAt(activeSwapperIndex);
            outputSignal = true;
          }

          break;
        }
        case AdvancedCircuits.BlockType_CrossoverBridge: {
          switch (AdvancedCircuits.DirectionFromTileLocations(componentLocation, portLocation)) {
            case Direction.Left:
              componentPorts = new List<DPoint> { new DPoint(componentLocation.X + 1, componentLocation.Y) };
              break;
            case Direction.Up:
              componentPorts = new List<DPoint> { new DPoint(componentLocation.X, componentLocation.Y + 1) };
              break;
            case Direction.Right:
              componentPorts = new List<DPoint> { new DPoint(componentLocation.X - 1, componentLocation.Y) };
              break;
            case Direction.Down:
              componentPorts = new List<DPoint> { new DPoint(componentLocation.X, componentLocation.Y - 1) };
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }

          blockActivatorToRegister = rootBranch.BlockActivator;
          blockActivatorLocationToRegister = rootBranch.BlockActivatorLocation;

          break;
        }
        case AdvancedCircuits.BlockType_BlockActivator: {
          if (!portTile.active || portTile.type != (int)AdvancedCircuits.BlockType_InputPort)
            return false;
          if (
            this.CircuitHandler.Config.BlockActivatorConfig.Cooldown > 0 && 
            !WorldGen.checkMech(componentLocation.X, componentLocation.Y, this.CircuitHandler.Config.BlockActivatorConfig.Cooldown
          ))
            return false;

          if (
            this.CircuitHandler.Config.BlockActivatorConfig.TriggerPermission != null && 
            this.TriggeringPlayer != TSPlayer.Server &&
            !this.TriggeringPlayer.Group.HasPermission(this.CircuitHandler.Config.BlockActivatorConfig.TriggerPermission)
          ) {
            this.Result.WarnReason = CircuitWarnReason.InsufficientPermissionToSignalComponent;
            this.Result.WarnRelatedComponentType = AdvancedCircuits.BlockType_BlockActivator;
            return true;
          }

          BlockActivatorMetadata blockActivator;
          this.CircuitHandler.WorldMetadata.BlockActivators.TryGetValue(componentLocation, out blockActivator);

          if (signal) {
            if (blockActivator == null || blockActivator.IsActivated)
              return true;
          } else {
            if (blockActivator == null) {
              blockActivator = new BlockActivatorMetadata();
              this.CircuitHandler.WorldMetadata.BlockActivators.Add(componentLocation, blockActivator);
            } else {
              // Will do nothing if already deactivated.
              if (!blockActivator.IsActivated)
                return true;

              blockActivator.RegisteredInactiveBlocks.Clear();
            }
          }

          blockActivator.IsActivated = signal;
          if (signal && blockActivator.RegisteredInactiveBlocks.Count == 0)
            return true;

          blockActivatorToRegister = blockActivator;
          blockActivatorLocationToRegister = componentLocation;
          if (AdvancedCircuits.CountComponentModifiers(measureData) == 1)
            blockActivatorModeToRegister = BlockActivatorMode.ReplaceBlocks;

          break;
        }
        case AdvancedCircuits.BlockType_WirelessTransmitter: {
          if (!portTile.active || portTile.type != (int)AdvancedCircuits.BlockType_InputPort)
            return false;

          WirelessTransmitterConfig transmitterConfig;
          ComponentConfigProfile configProfile = AdvancedCircuits.ModifierCountToConfigProfile(
            AdvancedCircuits.CountComponentModifiers(measureData)
          );
          if (
            (this.CircuitHandler.Config.WirelessTransmitterConfigs.TryGetValue(configProfile, out transmitterConfig) ||
            (this.CircuitHandler.Config.WirelessTransmitterConfigs.TryGetValue(ComponentConfigProfile.Default, out transmitterConfig))) && (
              transmitterConfig.Cooldown == 0 ||
              WorldGen.checkMech(componentLocation.X, componentLocation.Y, transmitterConfig.Cooldown)
            )
          ) {
            if (
              transmitterConfig.TriggerPermission != null && 
              this.TriggeringPlayer != TSPlayer.Server && 
              !this.TriggeringPlayer.Group.HasPermission(transmitterConfig.TriggerPermission)
            ) {
              this.Result.WarnReason = CircuitWarnReason.InsufficientPermissionToSignalComponent;
              this.Result.WarnRelatedComponentType = AdvancedCircuits.BlockType_WirelessTransmitter;
              return true;
            }

            bool isBroadcasting = (transmitterConfig.Network == 0);
            double squareRange = Math.Pow(transmitterConfig.Range + 1, 2);
            DPoint portOffset = new DPoint(portLocation.X - componentLocation.X, portLocation.Y - componentLocation.Y);
            foreach (KeyValuePair<DPoint,string> pair in this.CircuitHandler.WorldMetadata.WirelessTransmitters) {
              DPoint receivingTransmitterLocation = pair.Key;
              if (receivingTransmitterLocation == componentLocation)
                continue;

              string owningPlayerName = pair.Value;
              if (owningPlayerName != this.TriggeringPlayer.Name)
                continue;

              Tile transmitterTile = TerrariaUtils.Tiles[receivingTransmitterLocation];
              if (!transmitterTile.active || transmitterTile.type != (int)AdvancedCircuits.BlockType_WirelessTransmitter)
                continue;

              if (
                squareRange <= (
                  Math.Pow(receivingTransmitterLocation.X - componentLocation.X, 2) + 
                  Math.Pow(receivingTransmitterLocation.Y - componentLocation.Y, 2)
                )
              )
                continue;

              DPoint outputPortLocation = receivingTransmitterLocation.OffsetEx(portOffset.X, portOffset.Y);
              Tile outputPortTile = TerrariaUtils.Tiles[outputPortLocation];
              if (!outputPortTile.wire || (outputPortTile.active && outputPortTile.type == (int)AdvancedCircuits.BlockType_InputPort))
                continue;

              if (!isBroadcasting) {
                ComponentConfigProfile receiverConfigProfile = AdvancedCircuits.ModifierCountToConfigProfile(
                  AdvancedCircuits.CountComponentModifiers(receivingTransmitterLocation, new DPoint(1, 1))
                );
                WirelessTransmitterConfig receiverConfig;
                if (!this.CircuitHandler.Config.WirelessTransmitterConfigs.TryGetValue(receiverConfigProfile, out receiverConfig))
                  receiverConfig = this.CircuitHandler.Config.WirelessTransmitterConfigs[ComponentConfigProfile.Default];

                if (receiverConfig.Network != 0 && receiverConfig.Network != transmitterConfig.Network)
                  continue;
              }

              bool portOutputSignal = outputSignal;
              if (outputPortTile.active && outputPortTile.type == (int)AdvancedCircuits.BlockType_NOTGate)
                portOutputSignal = !portOutputSignal;

              this.QueuedRootBranches.Add(new RootBranchProcessData(
                receivingTransmitterLocation, outputPortLocation, AdvancedCircuits.BoolToSignal(portOutputSignal)
              ) {
                BlockActivator = rootBranch.BlockActivator,
                BlockActivatorLocation = rootBranch.BlockActivatorLocation,
                BlockActivatorMode = rootBranch.BlockActivatorMode
              });
            }

            return true;
          }

          break;
        }
        default:
          return false;
      }

      if (componentPorts == null)
        componentPorts = new List<DPoint>(AdvancedCircuits.EnumerateComponentPortLocations(measureData));

      for (int i = 0; i < componentPorts.Count; i++) {
        DPoint port = componentPorts[i];
        if (port == portLocation) {
          componentPorts.RemoveAt(i--);
          continue;
        }

        portTile = TerrariaUtils.Tiles[port];
        if (!portTile.wire || (portTile.active && portTile.type == (int)AdvancedCircuits.BlockType_InputPort)) {
          componentPorts.RemoveAt(i--);
          continue;
        }
      }

      foreach (DPoint port in componentPorts) {
        portTile = TerrariaUtils.Tiles[port];
        bool portOutputSignal = outputSignal;
        if (
          portTile.active && portTile.type == (int)AdvancedCircuits.BlockType_NOTGate && 
          measureData.BlockType != AdvancedCircuits.BlockType_NOTGate
        ) {
          portOutputSignal = !portOutputSignal;
        }

        RootBranchProcessData newRootBranch = new RootBranchProcessData(
          componentLocation, port, AdvancedCircuits.BoolToSignal(portOutputSignal)
        );
        if (blockActivatorToRegister != null) {
          newRootBranch.BlockActivator = blockActivatorToRegister;
          newRootBranch.BlockActivatorLocation = blockActivatorLocationToRegister;
          newRootBranch.BlockActivatorMode = blockActivatorModeToRegister;
        }

        this.QueuedRootBranches.Add(newRootBranch);
      }

      return true;
    }
    #endregion

    #region [Methods: RegisterUnregisterTimer, ResetTimer]
    private void RegisterUnregisterTimer(ObjectMeasureData measureData, bool register) {
      bool alreadyRegistered = this.CircuitHandler.WorldMetadata.ActiveTimers.ContainsKey(measureData.OriginTileLocation);
      if (register) {
        if (!alreadyRegistered) {
          this.CircuitHandler.WorldMetadata.ActiveTimers.Add(
            measureData.OriginTileLocation, 
            new ActiveTimerMetadata(
              AdvancedCircuits.MeasureTimerFrameTime(measureData.OriginTileLocation), this.TriggeringPlayer.Name
            )
          );
        }
      } else if (alreadyRegistered) {
        this.CircuitHandler.WorldMetadata.ActiveTimers.Remove(measureData.OriginTileLocation);
      }
    }

    private void ResetTimer(ObjectMeasureData measureData) {
      ActiveTimerMetadata activeTimer;
      if (this.CircuitHandler.WorldMetadata.ActiveTimers.TryGetValue(measureData.OriginTileLocation, out activeTimer))
        activeTimer.FramesLeft = AdvancedCircuits.MeasureTimerFrameTime(measureData.OriginTileLocation);
    }
    #endregion
  }
}
