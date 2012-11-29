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
using DPoint = System.Drawing.Point;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class CircuitProcessor {
    #region [Constants]
    // The maximum amount of times a single component can be signaled in the same circuit execution until it "overheats".
    public const int PortDefiningComponentSignalMaximum = 5;
    #endregion

    #region [Property: CircuitHandler]
    private readonly CircuitHandler circuitHandler;

    public CircuitHandler CircuitHandler {
      get { return this.circuitHandler; }
    }
    #endregion

    #region [Property: SenderMeasureData]
    private readonly Terraria.SpriteMeasureData senderMeasureData;

    protected Terraria.SpriteMeasureData SenderMeasureData {
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

    #region [Property: SignaledPumps]
    [ThreadStatic]
    private static List<DPoint> signaledPumps;

    protected List<DPoint> SignaledPumps {
      get {
        if (CircuitProcessor.signaledPumps == null)
          CircuitProcessor.signaledPumps = new List<DPoint>();

        return CircuitProcessor.signaledPumps;
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
    public CircuitProcessor(CircuitHandler circuitHandler, DPoint senderLocation) {
      Tile senderTile = Terraria.Tiles[senderLocation];
      if (!senderTile.active)
        throw new ArgumentException("No tile at the given sender location.", "senderLocation");

      this.circuitHandler = circuitHandler;
      this.senderMeasureData = Terraria.MeasureSprite(senderLocation);
      this.queuedRootBranches = new List<RootBranchProcessData>(20);
      this.result = new CircuitProcessResult {
        IsAdvancedCircuit = !Terraria.IsSpriteWired(this.SenderMeasureData),
        SenderLocation = this.SenderMeasureData.OriginTileLocation
      };

      if (this.IsAdvancedCircuit)
        this.portDefiningComponentSignalCounter = new Dictionary<DPoint,int>(20);
    }
    #endregion

    #region [Methods: ProcessCircuit, ProcessRootBranch, ProcessSubBranches, ProcessTile]
    public CircuitProcessResult ProcessCircuit(
      TSPlayer player = null, SignalType? overrideSignal = null, bool switchSender = true, bool switchSenderLocalOnly = true
    ) {
      if (this.wasExecuted)
        throw new InvalidOperationException("This Circuit Processor has already processed a circuit.");

      this.wasExecuted = true;

      DateTime processingStartTime = DateTime.Now;
      int senderType = this.SenderMeasureData.SpriteType;

      this.TriggeringPlayer = player;
      this.TriggeredPassively = (senderType == Terraria.TileId_XSecondTimer || senderType == Terraria.TileId_GrandfatherClock);

      SignalType signal = SignalType.Swap;
      try {
        if (this.IsAdvancedCircuit) {
          if (!this.CircuitHandler.Config.AdvancedCircuitsEnabled)
            return this.Result;
          
          if (overrideSignal == null) {
            switch (senderType) {
              case Terraria.TileId_PressurePlate:
                // Red sends "0", all the others send "1".
                signal = AdvancedCircuits.BoolToSignal(Terraria.Tiles[this.SenderMeasureData.OriginTileLocation].frameY > 0);

                break;
              case Terraria.TileId_Lever:
              case Terraria.TileId_Switch:
              case Terraria.TileId_XSecondTimer:
                signal = AdvancedCircuits.BoolToSignal(!Terraria.HasSpriteActiveFrame(this.SenderMeasureData));
                break;

              default:
                signal = SignalType.On;
                break;
            }
          }
        } else {
          // Grandfather Clock is an Advanced Circuit component and thus wont work in Vanilla Circuits.
          if (senderType == Terraria.TileId_GrandfatherClock)
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
            senderType == Terraria.TileId_Switch || 
            senderType == Terraria.TileId_Lever || 
            senderType == Terraria.TileId_XSecondTimer
          )
        ) {
          bool newSenderState;
          if (signal == SignalType.Swap)
            newSenderState = !Terraria.HasSpriteActiveFrame(this.SenderMeasureData);
          else 
            newSenderState = AdvancedCircuits.SignalToBool(signal).Value;

          if (Terraria.HasSpriteActiveFrame(this.SenderMeasureData) != newSenderState)
            Terraria.SetSpriteActiveFrame(this.SenderMeasureData, newSenderState, !switchSenderLocalOnly);

          if (senderType == Terraria.TileId_XSecondTimer) {
            this.RegisterUnregisterTimer(this.SenderMeasureData, newSenderState);

            // Timers do not execute circuits as they are switched.
            return this.Result;
          }
        }

        if (this.IsAdvancedCircuit) {
          AdvancedCircuitsPlugin.Trace.WriteLineVerbose(
            "Started processing Advanced Circuit at {0} with signal {1}.", 
            this.SenderMeasureData.OriginTileLocation, signal.ToString()
          );
        } else {
          AdvancedCircuitsPlugin.Trace.WriteLineVerbose(
            "Started processing Vanilla Circuit at {0} with signal {1}.", 
            this.SenderMeasureData.OriginTileLocation, signal.ToString()
          );
        }

        foreach (DPoint portLocation in AdvancedCircuits.EnumerateComponentPortLocations(this.SenderMeasureData)) {
          Tile portTile = Terraria.Tiles[portLocation];
          if (!portTile.wire)
            continue;

          DPoint portAdjacentTileLocation = AdvancedCircuits.GetPortAdjacentComponentTileLocation(
            this.SenderMeasureData, portLocation
          );

          SignalType portSignal = signal;
          if (this.IsAdvancedCircuit && portTile.active && portTile.type == AdvancedCircuits.TileId_NOTGate)
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

        this.PostProcessing();
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
        Terraria.Tiles.GetBlockName(Terraria.Tiles[this.Result.SenderLocation].type), 
        this.Result.SenderLocation, this.Result.IsAdvancedCircuit, this.Result.ProcessedBranchCount
      );

      if (this.Result.WarnReason != CircuitWarnReason.None)
        resultString.AppendLine("  Warning: " + this.Result.WarnReason);

      if (this.Result.CancellationReason != CircuitCancellationReason.None) {
        string relatedComponentName;
        if (this.Result.CancellationRelatedComponentType == -1)
          relatedComponentName = "None";
        else
          relatedComponentName = Terraria.Tiles.GetBlockName(this.Result.CancellationRelatedComponentType);

        resultString.AppendFormat(
          "  Error: {0}\n  Error Related Component: {1}\n", this.Result.CancellationReason, relatedComponentName
        );
      }

      AdvancedCircuitsPlugin.Trace.WriteLineVerbose(resultString.ToString());
      #endif

      return this.Result;
    }

    protected void PostProcessing() {
      // Transfer Liquid
      if (this.SignaledPumps.Count > 2) {
        int inletPumpCount = 0;
        List<DPoint> outletPumps = new List<DPoint>();
        foreach (DPoint pumpLocation in this.SignaledPumps) {
          if (Terraria.Tiles[pumpLocation].type == Terraria.TileId_InletPump)
            inletPumpCount++;
          else
            outletPumps.Add(pumpLocation);
        }

        if (inletPumpCount > 0 && outletPumps.Count > 0) {
          foreach (DPoint pumpLocation in this.SignaledPumps) {
            if (Terraria.Tiles[pumpLocation].type != Terraria.TileId_InletPump)
              continue;

            Terraria.SpriteMeasureData measureData = Terraria.MeasureSprite(pumpLocation);
            int modifiers = AdvancedCircuits.CountComponentModifiers(measureData);
            ComponentConfigProfile configProfile = AdvancedCircuits.ModifierCountToConfigProfile(modifiers);
            PumpConfig pumpConfig;
            if (!this.CircuitHandler.Config.PumpConfigs.TryGetValue(configProfile, out pumpConfig))
              pumpConfig = this.CircuitHandler.Config.PumpConfigs[ComponentConfigProfile.Default];

            bool isWater = true;
            foreach (Tile pumpTile in Terraria.EnumerateSpriteTiles(measureData)) {
              if (pumpTile.liquid > 0 && pumpTile.lava) {
                isWater = false;
                break;
              }
            }

            int maxLiquidAmount;
            if (isWater)
              maxLiquidAmount = pumpConfig.TransferableWater;
            else
              maxLiquidAmount = pumpConfig.TransferableLava;

            int takenLiquidAmount = 0;
            for (int x = measureData.OriginTileLocation.X; x < measureData.OriginTileLocation.X + 2; x++) {
              for (int y = measureData.OriginTileLocation.Y + 1; y >= measureData.OriginTileLocation.Y; y++) {
                Tile pumpTile = Terraria.Tiles[x, y];
                if (pumpTile.liquid <= 0 || pumpTile.lava == isWater)
                  continue;
              
                byte liquidToTake = pumpTile.liquid;
                if (liquidToTake + takenLiquidAmount > maxLiquidAmount)
                  liquidToTake = (byte)(maxLiquidAmount - takenLiquidAmount);

                takenLiquidAmount += liquidToTake;
                pumpTile.liquid -= liquidToTake;
              }
            }
            takenLiquidAmount += pumpConfig.LossValue;

            if (takenLiquidAmount > 0) {
              foreach (DPoint outletPumpLocation in outletPumps) {
                
                
                for (int x = outletPumpLocation.X; x < outletPumpLocation.X + 2; x++) {
                  for (int y = outletPumpLocation.Y + 1; y >= outletPumpLocation.Y; y++) {
                    Tile pumpTile = Terraria.Tiles[x, y];
                    WorldGen.xferWater();
                  }
                }
              }
            }
          }
        }
      }
    }

    protected void ProcessRootBranch(RootBranchProcessData rootBranch) {
      if (this.IsCancellationPending)
        return;

      Tile startTile = Terraria.Tiles[rootBranch.FirstWireLocation];
      if (!startTile.wire || (this.IsAdvancedCircuit && startTile.type == AdvancedCircuits.TileId_InputPort))
        return;

      SignalType signal = rootBranch.Signal;
      Direction direction = rootBranch.Direction;
      result.ProcessedBranchCount++;
      List<BranchProcessData> subBranches = new List<BranchProcessData>();
      // "Move" straight through the branch and register the initial adjacent sub-branches.
      {
        DPoint previousTileLocation = DPoint.Empty;
        DPoint currentTileLocation = rootBranch.FirstWireLocation;
        Tile currentTile = Terraria.Tiles[currentTileLocation];
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

          Tile adjacentTile1 = Terraria.Tiles[adjacentTileLocation1];
          Tile adjacentTile2 = Terraria.Tiles[adjacentTileLocation2];
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
          currentTile = Terraria.Tiles[currentTileLocation];
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
        Tile currentTile = Terraria.Tiles[currentTileLocation];
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

          Tile adjacentTile1 = Terraria.Tiles[adjacentTileLocation1];
          Tile adjacentTile2 = Terraria.Tiles[adjacentTileLocation2];
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
          currentTile = Terraria.Tiles[currentTileLocation];
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

      Tile tile = Terraria.Tiles[tileLocation];

      // If the tile has no wire it might be a AC-Component and thus the adjacent tile would be its port.
      if (!tile.wire && tile.active) {
        if (!this.IsAdvancedCircuit || adjacentTileLocation == DPoint.Empty)
          return;
        if (!AdvancedCircuits.IsPortDefiningComponentBlock(tile.type))
          return;
        if (signal == SignalType.Swap)
          throw new ArgumentException("A Port can not receive a Swap signal.", "signal");

        Terraria.SpriteMeasureData acComponentMeasureData = Terraria.MeasureSprite(tileLocation);
        // The origin sender can only signal itself if it is a timer.
        if (
          acComponentMeasureData.OriginTileLocation == this.SenderMeasureData.OriginTileLocation &&
          tile.type != Terraria.TileId_XSecondTimer
        ) {
          return;
        }

        if (rootBranch.SignaledComponentLocations.Contains(acComponentMeasureData.OriginTileLocation))
          return;

        int componentSignalCounter;
        this.PortDefiningComponentSignalCounter.TryGetValue(acComponentMeasureData.OriginTileLocation, out componentSignalCounter);
        if (componentSignalCounter > CircuitProcessor.PortDefiningComponentSignalMaximum) {
          this.Result.CancellationReason = CircuitCancellationReason.SignaledSameComponentTooOften;
          this.Result.CancellationRelatedComponentType = acComponentMeasureData.SpriteType;
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
          Tile blockActivatorTile = Terraria.Tiles[rootBranch.BlockActivatorLocation];
          if (tile.wall == blockActivatorTile.wall) {
            if (
              signal == SignalType.Off && tile.active && AdvancedCircuits.IsCustomActivatableBlock(tile.type)
            ) {
              if (rootBranch.BlockActivator.RegisteredInactiveTiles.Count > this.CircuitHandler.Config.BlockActivatorConfig.MaxChangeableBlocks) {
                this.Result.WarnReason = CircuitWarnReason.BlockActivatorChangedTooManyBlocks;
                return;
              }

              rootBranch.BlockActivator.RegisteredInactiveTiles.Add(tileLocation, tile.type);
              Terraria.Tiles.RemoveBlock(tileLocation, true);
              
              return;
            } else if (signal == SignalType.On && !tile.active) {
              byte registeredTileId;
              if (rootBranch.BlockActivator.RegisteredInactiveTiles.TryGetValue(tileLocation, out registeredTileId)) {
                rootBranch.BlockActivator.RegisteredInactiveTiles.Remove(tileLocation);
                Terraria.Tiles.SetBlock(tileLocation, registeredTileId);

                return;
              }
            }
          }
        }

        if (!tile.active)
          return;

        Terraria.SpriteMeasureData componentMeasureData = Terraria.MeasureSprite(tileLocation);
        if (rootBranch.SignaledComponentLocations.Contains(componentMeasureData.OriginTileLocation))
          return;

        // The origin sender can never signal itself if wired directly.
        if (componentMeasureData.OriginTileLocation == this.SenderMeasureData.OriginTileLocation)
          return;

        // Switches and Levers can not be signaled if they are wired directly.
        if (tile.type == Terraria.TileId_Switch || tile.type == Terraria.TileId_Lever)
          return;

        if (this.SignalComponent(ref componentMeasureData, signal)) {
          rootBranch.SignaledComponentLocations.Add(componentMeasureData.OriginTileLocation);
          this.Result.SignaledComponentsCounter++;
        }
      } finally {
        this.CircuitLength++;
        
        if (this.CircuitLength >= this.CircuitHandler.Config.MaxCircuitLength) {
          this.Result.CancellationReason = CircuitCancellationReason.ExceededMaxLength;
          AdvancedCircuitsPlugin.Trace.WriteLineInfo(
            "Processing of the circuit at {0} was cancelled because the signal reached the maximum transfer length of {1} wires.",
            this.SenderMeasureData.OriginTileLocation, this.CircuitHandler.Config.MaxCircuitLength
          );
        }
      }
    }
    #endregion

    #region [Methods: SignalComponent, SignalPortDefiningComponent]
    protected bool SignalComponent(ref Terraria.SpriteMeasureData measureData, SignalType signal, bool localOnly = false) {
      int originX = measureData.OriginTileLocation.X;
      int originY = measureData.OriginTileLocation.Y;

      switch (measureData.SpriteType) {
        case Terraria.TileId_Torch:
        case Terraria.TileId_XMasLight:
        case Terraria.TileId_Candle:
        case Terraria.TileId_ChainLantern:
        case Terraria.TileId_ChineseLantern:
        case Terraria.TileId_Candelabra:
        case Terraria.TileId_DiscoBall:
        case Terraria.TileId_TikiTorch:
        case Terraria.TileId_CopperChandelier:
        case Terraria.TileId_SilverChandelier:
        case Terraria.TileId_GoldChandelier:
        case Terraria.TileId_LampPost:
        case Terraria.TileId_MusicBox:
        case Terraria.TileId_XSecondTimer: {
          bool currentState = Terraria.HasSpriteActiveFrame(measureData);
          bool newState;
          if (signal == SignalType.Swap)
            newState = !currentState;
          else
            newState = AdvancedCircuits.SignalToBool(signal).Value;

          if (measureData.SpriteType == Terraria.TileId_XSecondTimer) {
            // Directly wired Timers in an Advanced Circuit are not meant to be switched.
            if (this.IsAdvancedCircuit)
              return true;

            if (newState != currentState)
              this.RegisterUnregisterTimer(measureData, newState);
          }

          if (newState != currentState)
            Terraria.SetSpriteActiveFrame(measureData, newState, !localOnly);

          return true;
        }
        case Terraria.TileId_ActiveStone:
        case Terraria.TileId_InactiveStone: {
          bool currentState = (measureData.SpriteType == Terraria.TileId_ActiveStone);
          bool newState;
          if (signal == SignalType.Swap)
            newState = !currentState;
          else
            newState = AdvancedCircuits.SignalToBool(signal).Value;

          if (newState != currentState) {
            byte newTileType;
            if (newState)
              newTileType = Terraria.TileId_ActiveStone;
            else
              newTileType = Terraria.TileId_InactiveStone;

            Terraria.Tiles[measureData.OriginTileLocation].type = newTileType;
            WorldGen.SquareTileFrame(originX, originY);
            TSPlayer.All.SendTileSquareEx(originX, originY, 1);
          }
          
          return true;
        }
        case Terraria.TileId_DoorClosed:
        case Terraria.TileId_DoorOpened: {
          bool currentState = (measureData.SpriteType == Terraria.TileId_DoorOpened);
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
                measureData = Terraria.MeasureSprite(measureData.OriginTileLocation);
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
        case Terraria.TileId_InletPump:
        case Terraria.TileId_OutletPump: {
          if (signal == SignalType.Off)
            return true;

          if (this.Result.SignaledPumps > this.CircuitHandler.Config.MaxPumpsPerCircuit) {
            this.Result.WarnReason = CircuitWarnReason.SignalesTooManyPumps;
            return true;
          }

          this.SignaledPumps.Add(new DPoint(originX, originY));
          this.Result.SignaledPumps++;

          return true;
        }
        case Terraria.TileId_DartTrap: {
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
              this.Result.WarnRelatedComponentType = Terraria.TileId_DartTrap;
              return true;
            }


            bool isPointingLeft = (Terraria.Tiles[originX, originY].frameX == 0);
            DPoint projectileSpawn = new DPoint((originX * 16), (originY * 16 + 9));
            float projectileSpeed;
              
            if (isPointingLeft) {
              projectileSpawn.X -= trapConfig.ProjectileOffset;
              projectileSpeed = -trapConfig.ProjectileSpeed;
            } else {
              projectileSpawn.X += Terraria.TileSize + trapConfig.ProjectileOffset;
              projectileSpeed = trapConfig.ProjectileSpeed;
            }
            
            int projectileDamage = trapConfig.ProjectileDamage;
            int projectileType = trapConfig.ProjectileType;
            const float ProjectileKnockBack = 2f;
            int projectileIndex = Projectile.NewProjectile(
              projectileSpawn.X, projectileSpawn.Y, projectileSpeed, 0f, projectileType, projectileDamage, 
              ProjectileKnockBack, Main.myPlayer
            );
            Main.projectile[projectileIndex].timeLeft = trapConfig.ProjectileLifeTime;

            this.Result.SignaledDartTraps++;
          }
          
          return true;
        }
        case Terraria.TileId_Explosives: {
          if (signal == SignalType.Off)
            return true;

          WorldGen.KillTile(originX, originY, false, false, true);
          TSPlayer.All.SendTileSquareEx(originX, originY, 1);
          Projectile.NewProjectile((originX * 16 + 8), (originY * 16 + 8), 0f, 0f, 108, 250, 10f, Main.myPlayer);

          return true;
        }
        case Terraria.TileId_Statue: {
          if (signal == SignalType.Off)
            return true;

          StatueType statueType = Terraria.GetStatueType(measureData.OriginTileLocation);
          StatueConfig statueConfig;
          if (
            this.CircuitHandler.Config.StatueConfigs.TryGetValue(statueType, out statueConfig) && statueConfig != null &&
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
              this.Result.WarnRelatedComponentType = Terraria.TileId_Statue;
              return true;
            }

            int spawnX = (originX + 1) * 16;
            int spawnY = (originY + 1) * 16;

            switch (statueConfig.ActionType) {
              case StatueActionType.MoveNPC:
                // Param1 = NPC group
                // Param2 = Specific NPC id
                  
                List<int> npcIndexes = null;
                switch (statueConfig.ActionParam) {
                  // Random
                  case 0:
                    npcIndexes = Terraria.GetFriendlyNPCIndexes();
                    break;
                  // Female
                  case 1:
                    npcIndexes = Terraria.GetFriendlyFemaleNPCIndexes();
                    break;
                  // Male
                  case 2:
                    npcIndexes = Terraria.GetFriendlyMaleNPCIndexes();
                    break;
                  // Specific
                  case 3:
                    npcIndexes = Terraria.GetSpecificNPCIndexes(new List<int> { statueConfig.ActionParam2 });
                    break;
                }

                if (npcIndexes != null && npcIndexes.Count > 0) {
                  Random rnd = new Random();
                  int pickedIndex = npcIndexes[rnd.Next(npcIndexes.Count)];
                  Main.npc[pickedIndex].position.X = (spawnX - (Main.npc[pickedIndex].width / 2));
                  Main.npc[pickedIndex].position.Y = (spawnY - (Main.npc[pickedIndex].height - 1));

                  NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, string.Empty, pickedIndex);
                }
                
                break;
              case StatueActionType.SpawnMob:
                // Param1 = Mob type
                // Param2 = Max mobs in range
                // Param3 = Range to check
                if (Terraria.CountNPCsInTileRange(spawnX, spawnY, statueConfig.ActionParam, statueConfig.ActionParam3) < statueConfig.ActionParam2) {
                  int mobIndex = NPC.NewNPC(spawnX, spawnY, statueConfig.ActionParam);
                  // Ensure that the spawned mob drops no money.
                  Main.npc[mobIndex].value = 0.0f;
                  Main.npc[mobIndex].npcSlots = 0.0f;
                }

                break;
              case StatueActionType.SpawnItem:
                // Param1 = Item type
                // Param2 = Max mobs in range
                // Param3 = Range to check
                if (Terraria.CountItemsInTileRange(spawnX, spawnY, statueConfig.ActionParam, statueConfig.ActionParam3) < statueConfig.ActionParam2)
                  Item.NewItem(spawnX, spawnY, 0, 0, statueConfig.ActionParam);
                  
                break;
              case StatueActionType.SpawnBoss:

                break;
            }

            this.Result.SignaledStatues++;
          }

          return true;
        }
        case Terraria.TileId_Sign: {
          if (
            !this.IsAdvancedCircuit || signal == SignalType.Off || this.TriggeringPlayer == TSPlayer.Server || 
            this.TriggeredPassively
          )
            return true;

          if (!WorldGen.checkMech(originX, originY, 300))
            return true;

          string signText = null;
          if (!this.CircuitHandler.PluginCooperationHandler.IsInfiniteSignsAvailable)
            signText = Main.sign[Sign.ReadSign(originX, originY)].text;
          else
            signText = this.CircuitHandler.PluginCooperationHandler.InfiniteSigns_GetSignText(new DPoint(originX, originY));

          if (signText == null)
            return true;

          string fullText = "Sign: " + signText;
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
        case Terraria.TileId_Boulder: {
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
      RootBranchProcessData rootBranch, Terraria.SpriteMeasureData measureData, DPoint portLocation, bool signal
    ) {
      if (!this.IsAdvancedCircuit)
        throw new InvalidOperationException("This is no advanced circuit.");

      Tile portTile = Terraria.Tiles[portLocation];
      Tile componentTile = Terraria.Tiles[measureData.OriginTileLocation];
      if (!portTile.wire || componentTile.wire)
        return false;
      if (
        portTile.active && portTile.type == AdvancedCircuits.TileId_NOTGate && 
        measureData.SpriteType != AdvancedCircuits.TileId_NOTGate
      ) {
        signal = !signal;
      }

      List<DPoint> componentPorts = null;
      bool outputSignal = signal;
      DPoint componentLocation = measureData.OriginTileLocation;
      
      BlockActivatorMetadata blockActivatorToRegister = null;
      DPoint blockActivatorLocationToRegister = DPoint.Empty;
      switch (measureData.SpriteType) {
        case Terraria.TileId_XSecondTimer: {
          bool currentState = (Terraria.HasSpriteActiveFrame(measureData));
          if (currentState != signal)
            Terraria.SetSpriteActiveFrame(measureData, signal);

          if (currentState != signal)
            this.RegisterUnregisterTimer(measureData, signal);
          else if (signal)
            this.ResetTimer(measureData);

          return true;
        }
        case Terraria.TileId_Switch:
        case Terraria.TileId_Lever: {
          bool currentState = (Terraria.HasSpriteActiveFrame(measureData));
          int modifiers = AdvancedCircuits.CountComponentModifiers(measureData);

          if (currentState != signal)
            Terraria.SetSpriteActiveFrame(measureData, signal);
          else if (modifiers == 0)
            return true;

          break;
        }
        case AdvancedCircuits.TileId_NOTGate: {
          if (!portTile.active || portTile.type != AdvancedCircuits.TileId_InputPort)
            return false;

          outputSignal = !signal;
          break;
        }
        case AdvancedCircuits.TileId_ANDGate:
        case AdvancedCircuits.TileId_ORGate:
        case AdvancedCircuits.TileId_XORGate: {
          if (!portTile.active || portTile.type != AdvancedCircuits.TileId_InputPort)
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
            portTile = Terraria.Tiles[port];
            if (portTile.wire && portTile.active && portTile.type == AdvancedCircuits.TileId_InputPort) {
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

          switch (componentTile.type) {
            case AdvancedCircuits.TileId_ANDGate:
              outputSignal = (inputPorts == signaledPorts);
              break;
            case AdvancedCircuits.TileId_ORGate:
              outputSignal = (signaledPorts > 0);
              break;
            case AdvancedCircuits.TileId_XORGate:
              outputSignal = (signaledPorts != 0 && signaledPorts < inputPorts);
              break;
          }
          
          break;
        }
        case AdvancedCircuits.TileId_Swapper: {
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
        case AdvancedCircuits.TileId_CrossoverBridge:
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

          break;
        case AdvancedCircuits.TileId_BlockActivator:
          if (!portTile.active || portTile.type != AdvancedCircuits.TileId_InputPort)
            return false;
          if (
            this.CircuitHandler.Config.BlockActivatorConfig.Cooldown > 0 && 
            !WorldGen.checkMech(componentLocation.X, componentLocation.Y, this.CircuitHandler.Config.BlockActivatorConfig.Cooldown
          ))
            return false;

          if (
            this.CircuitHandler.Config.BlockActivatorConfig.TriggerPermission != null && 
            this.TriggeringPlayer != TSPlayer.Server
          ) {
            if (!this.TriggeringPlayer.Group.HasPermission(this.CircuitHandler.Config.BlockActivatorConfig.TriggerPermission)) {
              this.Result.WarnReason = CircuitWarnReason.InsufficientPermissionToSignalComponent;
              this.Result.WarnRelatedComponentType = AdvancedCircuits.TileId_BlockActivator;
              return true;
            }
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

              blockActivator.RegisteredInactiveTiles.Clear();
            }
          }

          blockActivator.IsActivated = signal;
          if (signal && blockActivator.RegisteredInactiveTiles.Count == 0)
            return true;

          blockActivatorToRegister = blockActivator;
          blockActivatorLocationToRegister = componentLocation;

          break;
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

        portTile = Terraria.Tiles[port];
        if (!portTile.wire || (portTile.active && portTile.type == AdvancedCircuits.TileId_InputPort)) {
          componentPorts.RemoveAt(i--);
          continue;
        }
      }

      foreach (DPoint port in componentPorts) {
        portTile = Terraria.Tiles[port];
        bool portOutputSignal = outputSignal;
        if (
          portTile.active && portTile.type == AdvancedCircuits.TileId_NOTGate && 
          measureData.SpriteType != AdvancedCircuits.TileId_NOTGate
        ) {
          portOutputSignal = !portOutputSignal;
        }

        RootBranchProcessData newRootBranch = new RootBranchProcessData(
          componentLocation, port, AdvancedCircuits.BoolToSignal(portOutputSignal)
        );
        if (blockActivatorToRegister != null) {
          newRootBranch.BlockActivator = blockActivatorToRegister;
          newRootBranch.BlockActivatorLocation = blockActivatorLocationToRegister;
        }

        this.QueuedRootBranches.Add(newRootBranch);
      }

      return true;
    }
    #endregion

    #region [Methods: RegisterUnregisterTimer, ResetTimer]
    private void RegisterUnregisterTimer(Terraria.SpriteMeasureData measureData, bool register) {
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

    private void ResetTimer(Terraria.SpriteMeasureData measureData) {
      ActiveTimerMetadata activeTimer;
      if (this.CircuitHandler.WorldMetadata.ActiveTimers.TryGetValue(measureData.OriginTileLocation, out activeTimer))
        activeTimer.FramesLeft = AdvancedCircuits.MeasureTimerFrameTime(measureData.OriginTileLocation);
    }
    #endregion
  }
}
