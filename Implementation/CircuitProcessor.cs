using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using DPoint = System.Drawing.Point;

using TShockAPI;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class CircuitProcessor {
    #region [Constants]
    // The maximum amount of times a single component can be signaled in the same circuit execution until it "overheats".
    public const int PortDefiningComponentSignalMaximum = 5;
    #endregion

    #region [Property: Static Random]
    private static Random random;

    protected static Random Random {
      get {
        if (CircuitProcessor.random == null)
          CircuitProcessor.random = new Random();
        
        return CircuitProcessor.random;
      }
    }
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
    [ThreadStatic]
    private static List<RootBranchProcessData> queuedRootBranches;

    protected List<RootBranchProcessData> QueuedRootBranches {
      get {
        if (CircuitProcessor.queuedRootBranches == null)
          CircuitProcessor.queuedRootBranches = new List<RootBranchProcessData>(40);

        return CircuitProcessor.queuedRootBranches;
      }
    }
    #endregion

    #region [Property: PortDefiningComponentSignalCounter]
    [ThreadStatic]
    private static Dictionary<DPoint,int> portDefiningComponentSignalCounter;

    // Stores the amount of times a single port defining component was signaled during the current circuit execution.
    protected Dictionary<DPoint,int> PortDefiningComponentSignalCounter {
      get {
        if (CircuitProcessor.portDefiningComponentSignalCounter == null)
          CircuitProcessor.portDefiningComponentSignalCounter = new Dictionary<DPoint,int>();

        return CircuitProcessor.portDefiningComponentSignalCounter;
      }
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
    [ThreadStatic]
    private static Dictionary<DPoint,GateStateMetadata> temporaryGateStates;

    protected Dictionary<DPoint,GateStateMetadata> TemporaryGateStates {
      get {
        if (CircuitProcessor.temporaryGateStates == null)
          CircuitProcessor.temporaryGateStates = new Dictionary<DPoint,GateStateMetadata>();

        return CircuitProcessor.temporaryGateStates;
      }
    }
    #endregion

    #region [Properties: Result, IsAdvancedCircuit, TriggeringPlayer, IsTriggeredPassively, CircuitLength, IsCancellationPending]
    private readonly CircuitProcessingResult result;

    protected CircuitProcessingResult Result {
      get { return this.result; }
    }

    public bool IsAdvancedCircuit {
      get { return this.result.IsAdvancedCircuit; }
    }
    
    public TSPlayer TriggeringPlayer {
      get { return this.result.TriggeringPlayer; }
      set { this.result.TriggeringPlayer = value; }
    }

    public bool IsTriggeredPassively {
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
    private WireColor wireColor;
    

    #region [Methods: Constructors]
    public CircuitProcessor(PluginTrace pluginTrace, CircuitHandler circuitHandler, ObjectMeasureData senderMeasureData, WireColor forWireColor) {
      Contract.Requires<ArgumentNullException>(pluginTrace != null);
      Contract.Requires<ArgumentNullException>(circuitHandler != null);

      this.pluginTrace = pluginTrace;
      this.circuitHandler = circuitHandler;
      this.senderMeasureData = senderMeasureData;
      this.wireColor = forWireColor;

      // Is sender directly wired?
      bool isSenderWired;
      if (this.SenderMeasureData.BlockType == BlockType.DoorOpened) {
        isSenderWired = false;
        for (int y = this.SenderMeasureData.OriginTileLocation.Y; y < this.SenderMeasureData.OriginTileLocation.Y + this.SenderMeasureData.Size.Y; y++) {
          if (TerrariaUtils.Tiles[this.SenderMeasureData.OriginTileLocation.X, y].HasWire(forWireColor)) {
            isSenderWired = true;
            break;
          }
        }
      } else {
        isSenderWired = TerrariaUtils.Tiles.IsObjectWired(this.SenderMeasureData, forWireColor);
      }

      this.result = new CircuitProcessingResult {
        IsAdvancedCircuit = !isSenderWired,
        SenderLocation = this.SenderMeasureData.OriginTileLocation
      };
    }

    public CircuitProcessor(PluginTrace pluginTrace, CircuitHandler circuitHandler, DPoint senderLocation, WireColor forWireColor): this(
      pluginTrace, circuitHandler, CircuitProcessor.CircuitProcessorCtor_MeasureSender(senderLocation), forWireColor
    ) {
      Contract.Requires<ArgumentNullException>(pluginTrace != null);
      Contract.Requires<ArgumentNullException>(circuitHandler != null);
    }

    private static ObjectMeasureData CircuitProcessorCtor_MeasureSender(DPoint senderLocation) {
      Tile senderTile = TerrariaUtils.Tiles[senderLocation];
      if (!senderTile.active())
        throw new ArgumentException("No tile at the given sender location.", "senderLocation");

      return TerrariaUtils.Tiles.MeasureObject(senderLocation);
    }
    #endregion

    #region [Methods: ProcessCircuit, ProcessRootBranch, ProcessSubBranches, ProcessTile, PostProcessCircuit]
    public CircuitProcessingResult ProcessCircuit(TSPlayer player = null, SignalType? overrideSignal = null, bool switchSender = true, bool switchSenderLocalOnly = true) {
      if (this.wasExecuted)
        throw new InvalidOperationException("This Circuit Processor has already processed a circuit.");

      this.wasExecuted = true;
      this.SignaledInletPumps.Clear();
      this.SignaledOutletPumps.Clear();
      this.TilesToFrameOnPost.Clear();
      this.QueuedRootBranches.Clear();

      if (this.IsAdvancedCircuit) {
        this.PortDefiningComponentSignalCounter.Clear();
        this.TemporaryGateStates.Clear();
      }

      DateTime processingStartTime = DateTime.Now;
      BlockType senderBlockType = this.SenderMeasureData.BlockType;

      this.TriggeringPlayer = player;
      this.IsTriggeredPassively = (senderBlockType == BlockType.XSecondTimer || senderBlockType == BlockType.GrandfatherClock);

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
          if (senderBlockType == BlockType.DoorOpened || senderBlockType == BlockType.DoorClosed)
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
            senderBlockType == BlockType.XSecondTimer ||
            senderBlockType == BlockType.DoorOpened ||
            senderBlockType == BlockType.DoorClosed
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

            // Timers do not execute circuits when they are switched.
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
          if (!portTile.HasWire(this.wireColor))
            continue;

          DPoint portAdjacentTileLocation = AdvancedCircuits.GetPortAdjacentComponentTileLocation(
            this.SenderMeasureData, portLocation
          );

          SignalType portSignal = signal;
          if (this.IsAdvancedCircuit && portTile.active()) {
            if (portTile.type == (int)AdvancedCircuits.BlockType_NOTGate)
              portSignal = AdvancedCircuits.BoolToSignal(!AdvancedCircuits.SignalToBool(portSignal).Value);
            else if (portTile.type == (int)AdvancedCircuits.BlockType_XORGate)
              portSignal = SignalType.Off;
          }

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
          // expect the wiring of the sender to be 1.
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
      if (
        !startTile.HasWire(this.wireColor) || (
          this.IsAdvancedCircuit && 
          this.SenderMeasureData.BlockType != BlockType.PressurePlate &&
          startTile.type == (int)AdvancedCircuits.BlockType_InputPort
        )
      )
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
        while (currentTile.HasWire(this.wireColor) && !this.IsCancellationPending) {
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
          if (adjacentTile1.HasWire(this.wireColor))
            subBranches.Add(new BranchProcessData(currentTileLocation, adjacentTileLocation1, signal));
          else
            this.ProcessTile(rootBranch, adjacentTileLocation1, currentTileLocation, signal);

          if (adjacentTile2.HasWire(this.wireColor))
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
        while (currentTile.HasWire(this.wireColor) && !this.IsCancellationPending) {
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
          if (adjacentTile1.HasWire(this.wireColor)) {
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
          if (adjacentTile2.HasWire(this.wireColor)) {
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
      if (!tile.HasWire(this.wireColor) && tile.active()) {
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

        int componentSignalCounter;
        this.PortDefiningComponentSignalCounter.TryGetValue(acComponentMeasureData.OriginTileLocation, out componentSignalCounter);
        if (componentSignalCounter > CircuitProcessor.PortDefiningComponentSignalMaximum) {
          this.Result.CancellationReason = CircuitCancellationReason.SignaledSameComponentTooOften;
          this.Result.CancellationRelatedComponentType = acComponentMeasureData.BlockType;
          return;
        }

        if (
          AdvancedCircuits.IsOriginSenderComponent((BlockType)tile.type) &&
          rootBranch.SignaledComponentLocations.Contains(acComponentMeasureData.OriginTileLocation)
        )
          return;

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

      if (!tile.HasWire(this.wireColor))
        return;

      try {
        if (rootBranch.BlockActivator != null) {
          Tile blockActivatorTile = TerrariaUtils.Tiles[rootBranch.BlockActivatorLocation];
          if (tile.wall == blockActivatorTile.wall) {
            Tile tileAbove = TerrariaUtils.Tiles[tileLocation.OffsetEx(0, -1)];
            if (!tileAbove.active() || tileAbove.type != (int)BlockType.Chest) {
              if (
                signal == SignalType.Off && tile.active() && AdvancedCircuits.IsCustomActivatableBlock((BlockType)tile.type)
              ) {
                if (rootBranch.BlockActivator.RegisteredInactiveBlocks.Count > this.CircuitHandler.Config.BlockActivatorConfig.MaxChangeableBlocks) {
                  this.Result.WarnReason = CircuitWarnReason.BlockActivatorChangedTooManyBlocks;
                  return;
                }

                rootBranch.BlockActivator.RegisteredInactiveBlocks.Add(tileLocation, (BlockType)tile.type);

                tile.type = 0;
                tile.active(false);
                tile.frameX = -1;
                tile.frameY = -1;
                tile.frameNumber(0);
                this.TilesToFrameOnPost.Add(tileLocation);
              
                return;
              } else if (
                signal == SignalType.On && (rootBranch.BlockActivatorMode == BlockActivatorMode.ReplaceBlocks || !tile.active())
              ) {
                BlockType registeredBlockType;
                if (rootBranch.BlockActivator.RegisteredInactiveBlocks.TryGetValue(tileLocation, out registeredBlockType)) {
                  rootBranch.BlockActivator.RegisteredInactiveBlocks.Remove(tileLocation);

                  tile.type = (byte)registeredBlockType;
                  tile.active(true);
                  this.TilesToFrameOnPost.Add(tileLocation);

                  return;
                }
              }
            }
          }
        }

        if (!tile.active())
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
                transferWater = !pumpTile.lava();
              if (transferWater.Value && pumpTile.lava())
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
          if (totalLoss < 0)
            inputableLiquid += Math.Abs(totalLoss);
          
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
              if (pumpTile.liquid > 0 && transferWater.Value == pumpTile.lava())
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
            if (liquidToTransfer > 0) {
              int liquidToInput = liquidToTransfer;
              // Produce liquid?
              if (totalLoss < 0)
                liquidToInput += totalLoss;

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

                  WorldGen.SquareTileFrame(pumpTileLocation.X, pumpTileLocation.Y);
                }
              }
              if (totalLoss > 0)
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
                    pumpTile.lava(!transferWater.Value);
                    pumpTile.liquid += transferedLiquid;
                    liquidToTransfer = Math.Max(liquidToTransfer - transferedLiquid, 0);

                    WorldGen.SquareTileFrame(pumpTileLocation.X, pumpTileLocation.Y);
                  }
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
              return false;

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
          if (this.IsAdvancedCircuit)
            return false;

          this.OpenDoor(measureData, signal);
          return true;
        }
        case BlockType.InletPump:
        case BlockType.OutletPump: {
          if (signal == SignalType.Off)
            return false;

          PumpConfig pumpConfig;
          ComponentConfigProfile configProfile = AdvancedCircuits.ModifierCountToConfigProfile(
            AdvancedCircuits.CountComponentModifiers(measureData)
          );
          if (
            (
              this.CircuitHandler.Config.PumpConfigs.TryGetValue(configProfile, out pumpConfig) ||
              this.CircuitHandler.Config.PumpConfigs.TryGetValue(ComponentConfigProfile.Default, out pumpConfig)
            ) &&
              pumpConfig.Cooldown == 0 ||
              WorldGen.checkMech(originX, originY, pumpConfig.Cooldown
            )
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
            return false;

          DartTrapConfig trapConfig;
          ComponentConfigProfile configProfile = AdvancedCircuits.ModifierCountToConfigProfile(
            AdvancedCircuits.CountComponentModifiers(measureData)
          );
          if (
            (
              this.CircuitHandler.Config.DartTrapConfigs.TryGetValue(configProfile, out trapConfig) ||
              this.CircuitHandler.Config.DartTrapConfigs.TryGetValue(ComponentConfigProfile.Default, out trapConfig)
            ) &&
              trapConfig.Cooldown == 0 ||
              WorldGen.checkMech(originX, originY, trapConfig.Cooldown
            )
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
            Projectile projectile = Main.projectile[projectileIndex];
            projectile.SetDefaults(trapConfig.ProjectileType);
            Vector2 projectileSpawn = new Vector2(
              (originX * TerrariaUtils.TileSize + (trapConfig.ProjectileOffset * normalizedPolarOffset.X)), 
              (originY * TerrariaUtils.TileSize + (trapConfig.ProjectileOffset * normalizedPolarOffset.Y))
            );
            projectileSpawn = projectileSpawn.Add(new Vector2(
              TerrariaUtils.TileSize / 2 - projectile.width / 2, 
              TerrariaUtils.TileSize / 2 - projectile.height / 2
            ));
            projectile.position.X = projectileSpawn.X;
            projectile.position.Y = projectileSpawn.Y;
            projectile.owner = Main.myPlayer;
            projectile.velocity.X = (trapConfig.ProjectileSpeed * normalizedPolarOffset.X);
            projectile.velocity.Y = (trapConfig.ProjectileSpeed * normalizedPolarOffset.Y);
            projectile.damage = trapConfig.ProjectileDamage;
            projectile.knockBack = trapConfig.ProjectileKnockback;
            projectile.identity = projectileIndex;
            projectile.timeLeft = trapConfig.ProjectileLifeTime;
            projectile.wet = Collision.WetCollision(projectile.position, projectile.width, projectile.height);
            TSPlayer.All.SendData(PacketTypes.ProjectileNew, string.Empty, projectileIndex);
            
            this.Result.SignaledDartTraps++;
          }
          
          return true;
        }
        case BlockType.Explosives: {
          if (signal == SignalType.Off)
            return false;

          WorldGen.KillTile(originX, originY, false, false, true);
          TSPlayer.All.SendTileSquareEx(originX, originY, 1);
          Projectile.NewProjectile((originX * 16 + 8), (originY * 16 + 8), 0f, 0f, 108, 250, 10f, Main.myPlayer);
          
          return true;
        }
        case BlockType.Statue: {
          if (signal == SignalType.Off)
            return false;

          StatueStyle statueStyle = TerrariaUtils.Tiles.GetStatueStyle(TerrariaUtils.Tiles[measureData.OriginTileLocation]);
          StatueConfig statueConfig;
          if (
            this.CircuitHandler.Config.StatueConfigs.TryGetValue(statueStyle, out statueConfig) &&
            statueConfig.Actions.Count > 0 && (
              statueConfig.Cooldown == 0 ||
              WorldGen.checkMech(originX, originY, statueConfig.Cooldown)
            )
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

            if (statueConfig.PlayerCheckRange > 0) {
              bool isPlayerInRange = false;
              foreach (TSPlayer player in TShock.Players) {
                if (player == null || !player.Active)
                  continue;
                if (Math.Sqrt(Math.Pow(player.TileX - originX, 2) + Math.Pow(player.TileY - originY, 2)) <= statueConfig.PlayerCheckRange) {
                  isPlayerInRange = true;
                  break;
                }
              }
              if (!isPlayerInRange)
                return true;
            }

            if (statueConfig.ActionsProcessingMethod == ActionListProcessingMethod.ExecuteAll) {
              foreach (NullStatueAction action in statueConfig.Actions)
                this.ExecuteStatueAction(measureData.OriginTileLocation, action);
            } else {
              NullStatueAction randomAction = statueConfig.Actions[CircuitProcessor.Random.Next(0, statueConfig.Actions.Count)];
              this.ExecuteStatueAction(measureData.OriginTileLocation, randomAction);
            }

            this.Result.SignaledStatues++;
          }

          return true;
        }
        case BlockType.Sign: {
          if (
            !this.IsAdvancedCircuit || signal == SignalType.Off || this.TriggeringPlayer == TSPlayer.Server
          )
            return false;

          if (
            this.IsTriggeredPassively && !string.IsNullOrEmpty(this.CircuitHandler.Config.SignConfig.PassiveTriggerPermission) &&
            !this.TriggeringPlayer.Group.HasPermission(this.CircuitHandler.Config.SignConfig.PassiveTriggerPermission)
          ) {
            this.Result.WarnReason = CircuitWarnReason.InsufficientPermissionToSignalComponent;
            this.Result.WarnRelatedComponentType = BlockType.Sign;
            return false;
          }
          
          string signText = Main.sign[Sign.ReadSign(originX, originY)].text;
          if (signText == null)
            return false;

          if (
            this.CircuitHandler.PluginCooperationHandler.IsSignCommandsAvailable &&
            this.CircuitHandler.PluginCooperationHandler.SignCommands_CheckIsSignCommand(signText)
          ) {
            if (
              !string.IsNullOrEmpty(this.CircuitHandler.Config.SignConfig.TriggerSignCommandPermission) &&
              !this.TriggeringPlayer.Group.HasPermission(this.CircuitHandler.Config.SignConfig.TriggerSignCommandPermission)
            ) {
              this.Result.WarnReason = CircuitWarnReason.InsufficientPermissionToSignalComponent;
              this.Result.WarnRelatedComponentType = BlockType.Sign;
              return false;
            }

            this.CircuitHandler.PluginCooperationHandler.SignCommands_ExecuteSignCommand(
              this.TriggeringPlayer, measureData.OriginTileLocation, signText
            );
            return true;
          }

          if (!WorldGen.checkMech(originX, originY, 300))
            return true;

          string fullText = this.CircuitHandler.Config.SignConfig.ReadPrefix + signText;
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
            return false;

          WorldGen.KillTile(originX, originY, false, false, true);
          TSPlayer.All.SendTileSquareEx(originX, originY, 2);

          return true;
        }
      }

      return false;
    }

    private void OpenDoor(ObjectMeasureData measureData, SignalType signal) {
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
    }

    private void ExecuteStatueAction(DPoint statueLocation, NullStatueAction statueAction) {
      MoveNpcStatueAction moveNpcAction = (statueAction as MoveNpcStatueAction);
      SpawnNpcStatueAction spawnNpcAction = (statueAction as SpawnNpcStatueAction);
      SpawnItemStatueAction spawnItemAction = (statueAction as SpawnItemStatueAction);
      BuffPlayerStatueAction buffPlayerAction = (statueAction as BuffPlayerStatueAction);

      DPoint spawnLocation = new DPoint((statueLocation.X + 1) * 16, (statueLocation.Y + 1) * 16);
      if (moveNpcAction != null) {
        int npcIndex = -1;
        for (int i = 0; i < 200; i++) {
          NPC npc = Main.npc[i];
          if (!npc.active || npc.type != moveNpcAction.NpcType)
            continue;

          npcIndex = i;
          break;
        }

        if (npcIndex == -1) {
          if (!moveNpcAction.SpawnIfNotExistent)
            return;

          TerrariaUtils.Npcs.Spawn(moveNpcAction.NpcType, spawnLocation, noDrops: true);
        } else {
          TerrariaUtils.Npcs.Move(npcIndex, spawnLocation);
        }
      } else if (spawnNpcAction != null) {
        if (spawnNpcAction.CheckRange > 0 && spawnNpcAction.CheckAmount > 0) {
          int closeByNpcs = 0;
          foreach (NPC closeByNpc in TerrariaUtils.Npcs.EnumerateNPCsAroundPoint(spawnLocation, spawnNpcAction.CheckRange * TerrariaUtils.TileSize)) {
            if (closeByNpc.type == spawnNpcAction.NpcType)
              closeByNpcs++;
          }
          if (closeByNpcs >= spawnNpcAction.CheckAmount)
            return;
        }

        for (int i = 0; i < spawnNpcAction.Amount; i++)
          TerrariaUtils.Npcs.Spawn(spawnNpcAction.NpcType, spawnLocation, noDrops: true);
      } else if (spawnItemAction != null) {
        if (spawnItemAction.CheckRange > 0 && spawnItemAction.CheckAmount > 0) {
          int closeByItems = 0;
          foreach (Item closeByItem in TerrariaUtils.Items.EnumerateItemsAroundPoint(spawnLocation, spawnItemAction.CheckRange * TerrariaUtils.TileSize)) {
            if (closeByItem.type == (int)spawnItemAction.ItemType)
              closeByItems++;
          }
          if (closeByItems >= spawnItemAction.CheckAmount)
            return;
        }

        if (spawnItemAction.Amount <= 5) {
          for (int i = 0; i < spawnItemAction.Amount; i++)
            Item.NewItem(spawnLocation.X, spawnLocation.Y, 0, 0, (int)spawnItemAction.ItemType);
        } else {
          Item.NewItem(spawnLocation.X, spawnLocation.Y, 0, 0, (int)spawnItemAction.ItemType, spawnItemAction.Amount);
        }
      } else if (buffPlayerAction != null) {
        foreach (TSPlayer player in TShock.Players) {
          if (player == null || !player.Active)
            continue;

          if (Math.Sqrt(Math.Pow(player.TileX - statueLocation.X, 2) + Math.Pow(player.TileY - statueLocation.Y, 2)) <= buffPlayerAction.Radius)
            player.SetBuff(buffPlayerAction.BuffId, buffPlayerAction.BuffTime);
        }
      }
    }

    private bool SignalPortDefiningComponent(RootBranchProcessData rootBranch, ObjectMeasureData measureData, DPoint portLocation, bool signal) {
      if (!this.IsAdvancedCircuit)
        throw new InvalidOperationException("This is no advanced circuit.");

      Tile portTile = TerrariaUtils.Tiles[portLocation];
      Tile componentTile = TerrariaUtils.Tiles[measureData.OriginTileLocation];
      if (!portTile.HasWire(this.wireColor) || componentTile.HasWire(this.wireColor))
        return false;

      if (portTile.active() && portTile.type == (int)AdvancedCircuits.BlockType_NOTGate && measureData.BlockType != AdvancedCircuits.BlockType_NOTGate)
        signal = !signal;
      else if (portTile.active() && portTile.type == (int)AdvancedCircuits.BlockType_XORGate && measureData.BlockType != AdvancedCircuits.BlockType_XORGate)
        signal = false;

      List<DPoint> componentPorts = null;
      bool outputSignal = signal;
      DPoint componentLocation = measureData.OriginTileLocation;
      
      BlockActivatorMetadata blockActivatorToRegister = null;
      DPoint blockActivatorLocationToRegister = DPoint.Empty;
      BlockActivatorMode blockActivatorModeToRegister = BlockActivatorMode.Default;

      switch (measureData.BlockType) {
        case BlockType.DoorOpened:
        case BlockType.DoorClosed: {
          for (int y = measureData.OriginTileLocation.Y; y < measureData.OriginTileLocation.Y + measureData.Size.Y; y++)
            if (TerrariaUtils.Tiles[measureData.OriginTileLocation.X, y].HasWire(this.wireColor))
              return false;

          if (measureData.BlockType == BlockType.DoorOpened) {
            // Extra check needed if a port of the door is really hit. This is because doors define ports differently than
            // other components if they are opened.
            componentPorts = new List<DPoint>(AdvancedCircuits.EnumerateComponentPortLocations(measureData));
            if (!componentPorts.Contains(portLocation))
              return false;
          }

          this.OpenDoor(measureData, AdvancedCircuits.BoolToSignal(signal));
          break;
        }
        case BlockType.XSecondTimer: {
          if (!portTile.active() || portTile.type != (int)AdvancedCircuits.BlockType_InputPort)
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
          if (measureData.BlockType == BlockType.Lever && TerrariaUtils.Tiles.IsObjectWired(measureData))
            return false;

          bool isInputPort = (portTile.active() && portTile.type == (int)AdvancedCircuits.BlockType_InputPort);
          bool currentState = (TerrariaUtils.Tiles.ObjectHasActiveState(measureData));
          if (isInputPort) {
            if (currentState != signal)
              TerrariaUtils.Tiles.SetObjectState(measureData, signal);

            return true;
          } else {
            int modifiers = AdvancedCircuits.CountComponentModifiers(measureData);
            switch (modifiers) {
              case 0:
                if (currentState == signal)
                  return true;

                TerrariaUtils.Tiles.SetObjectState(measureData, signal);
                break;
              case 1:
                if (currentState != signal)
                  TerrariaUtils.Tiles.SetObjectState(measureData, signal);

                break;
              case 2:
                if (currentState != signal)
                  return true;

                break;
              case 3:
                if (currentState != signal)
                  return true;

                if (CircuitProcessor.Random.Next(0, 2) == 0)
                  outputSignal = !signal;

                break;
            }

            if (modifiers != 1) {
              blockActivatorToRegister = rootBranch.BlockActivator;
              blockActivatorLocationToRegister = rootBranch.BlockActivatorLocation;
              blockActivatorModeToRegister = rootBranch.BlockActivatorMode;
            }
          }

          break;
        }
        case AdvancedCircuits.BlockType_NOTGate: {
          if (!portTile.active() || portTile.type != (int)AdvancedCircuits.BlockType_InputPort)
            return false;

          outputSignal = !signal;

          blockActivatorToRegister = rootBranch.BlockActivator;
          blockActivatorLocationToRegister = rootBranch.BlockActivatorLocation;
          blockActivatorModeToRegister = rootBranch.BlockActivatorMode;

          break;
        }
        case AdvancedCircuits.BlockType_ANDGate:
        case AdvancedCircuits.BlockType_ORGate:
        case AdvancedCircuits.BlockType_XORGate: {
          if (!portTile.active() || portTile.type != (int)AdvancedCircuits.BlockType_InputPort)
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
            if (portTile.HasWire(this.wireColor) && portTile.active() && portTile.type == (int)AdvancedCircuits.BlockType_InputPort) {
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

          blockActivatorToRegister = rootBranch.BlockActivator;
          blockActivatorLocationToRegister = rootBranch.BlockActivatorLocation;
          blockActivatorModeToRegister = rootBranch.BlockActivatorMode;
          
          break;
        }
        case AdvancedCircuits.BlockType_Swapper: {
          if (!signal)
            return false;

          int modifiers = AdvancedCircuits.CountComponentModifiers(measureData);
          int swapperCounterValue;
          if (!this.CircuitHandler.WorldMetadata.Swappers.TryGetValue(componentLocation, out swapperCounterValue)) {
            this.CircuitHandler.WorldMetadata.Swappers.Add(componentLocation, 1);

            if (modifiers == 0)
              outputSignal = false;
            else
              return true;
          } else {
            swapperCounterValue++;

            bool sendSignal = true;
            int swapperCounterMax = modifiers + 1;
            if (swapperCounterValue == swapperCounterMax) {
              outputSignal = false;
            } else if (swapperCounterValue >= swapperCounterMax * 2) {
              outputSignal = true;
              swapperCounterValue = 0;
            } else {
              sendSignal = false;
            }

            this.CircuitHandler.WorldMetadata.Swappers[componentLocation] = swapperCounterValue;
            if (!sendSignal)
              return true;
          }

          blockActivatorToRegister = rootBranch.BlockActivator;
          blockActivatorLocationToRegister = rootBranch.BlockActivatorLocation;
          blockActivatorModeToRegister = rootBranch.BlockActivatorMode;

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
          blockActivatorModeToRegister = rootBranch.BlockActivatorMode;

          break;
        }
        case AdvancedCircuits.BlockType_BlockActivator: {
          if (!portTile.active() || portTile.type != (int)AdvancedCircuits.BlockType_InputPort)
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
          if (!portTile.active() || portTile.type != (int)AdvancedCircuits.BlockType_InputPort)
            return false;

          WirelessTransmitterConfig transmitterConfig;
          ComponentConfigProfile configProfile = AdvancedCircuits.ModifierCountToConfigProfile(
            AdvancedCircuits.CountComponentModifiers(measureData)
          );
          if (
            (
              this.CircuitHandler.Config.WirelessTransmitterConfigs.TryGetValue(configProfile, out transmitterConfig) ||
              this.CircuitHandler.Config.WirelessTransmitterConfigs.TryGetValue(ComponentConfigProfile.Default, out transmitterConfig)
            ) && (
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

            string sendingTransmitterOwner;
            if (!this.CircuitHandler.WorldMetadata.WirelessTransmitters.TryGetValue(componentLocation, out sendingTransmitterOwner))
              return false;

            bool isBroadcasting = (transmitterConfig.Network == 0);
            double sendingSquareRange = 0;
            if (transmitterConfig.Range > 0)
              sendingSquareRange = Math.Pow(transmitterConfig.Range + 1, 2);

            DPoint portOffset = new DPoint(portLocation.X - componentLocation.X, portLocation.Y - componentLocation.Y);
            foreach (KeyValuePair<DPoint,string> pair in this.CircuitHandler.WorldMetadata.WirelessTransmitters) {
              DPoint receivingTransmitterLocation = pair.Key;
              if (receivingTransmitterLocation == componentLocation)
                continue;

              string receivingTransmitterOwner = pair.Value;
              if (receivingTransmitterOwner != sendingTransmitterOwner)
                continue;

              Tile transmitterTile = TerrariaUtils.Tiles[receivingTransmitterLocation];
              if (!transmitterTile.active() || transmitterTile.type != (int)AdvancedCircuits.BlockType_WirelessTransmitter)
                continue;

              if (
                sendingSquareRange > 0 &&
                sendingSquareRange <= (
                  Math.Pow(receivingTransmitterLocation.X - componentLocation.X, 2) + 
                  Math.Pow(receivingTransmitterLocation.Y - componentLocation.Y, 2)
                )
              )
                continue;

              DPoint outputPortLocation = receivingTransmitterLocation.OffsetEx(portOffset.X, portOffset.Y);
              Tile outputPortTile = TerrariaUtils.Tiles[outputPortLocation];
              if (!outputPortTile.HasWire(this.wireColor) || (outputPortTile.active() && outputPortTile.type == (int)AdvancedCircuits.BlockType_InputPort))
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
              if (outputPortTile.active() && outputPortTile.type == (int)AdvancedCircuits.BlockType_NOTGate)
                portOutputSignal = !portOutputSignal;
              else if (outputPortTile.active() && outputPortTile.type == (int)AdvancedCircuits.BlockType_XORGate)
                portOutputSignal = false;

              this.QueuedRootBranches.Add(new RootBranchProcessData(receivingTransmitterLocation, outputPortLocation, AdvancedCircuits.BoolToSignal(portOutputSignal)) {
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
        // A component shouldn't send a signal through a port where it just received one.
        if (port == portLocation) {
          componentPorts.RemoveAt(i--);
          continue;
        }

        portTile = TerrariaUtils.Tiles[port];
        if (!portTile.HasWire(this.wireColor) || (portTile.active() && portTile.type == (int)AdvancedCircuits.BlockType_InputPort)) {
          componentPorts.RemoveAt(i--);
          continue;
        }
      }

      foreach (DPoint port in componentPorts) {
        portTile = TerrariaUtils.Tiles[port];
        bool portOutputSignal = outputSignal;
        if (
          portTile.active() && portTile.type == (int)AdvancedCircuits.BlockType_NOTGate && 
          measureData.BlockType != AdvancedCircuits.BlockType_NOTGate
        ) {
          portOutputSignal = !portOutputSignal;
        } else if (
          portTile.active() && portTile.type == (int)AdvancedCircuits.BlockType_XORGate && 
          measureData.BlockType != AdvancedCircuits.BlockType_XORGate
        ) {
          portOutputSignal = false;
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
