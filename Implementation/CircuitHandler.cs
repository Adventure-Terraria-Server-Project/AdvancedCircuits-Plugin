// This file is provided unter the terms of the 
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// 
// Written by CoderCow

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using DPoint = System.Drawing.Point;

using TShockAPI;

namespace Terraria.Plugins.Common.AdvancedCircuits {
  public class CircuitHandler {
    #region [Constants]
    private const int TimerUpdateFrameRate = 10;
    private const int ClockUpdateFrameRate = 60;
    #endregion

    #region [Property: PluginTrace]
    private readonly PluginTrace pluginTrace;

    protected PluginTrace PluginTrace {
      get { return this.pluginTrace; }
    }
    #endregion

    #region [Property: Config]
    private readonly Configuration config;

    public Configuration Config {
      get { return this.config; }
    }
    #endregion

    #region [Property: WorldMetadata]
    private readonly WorldMetadata worldMetadata;

    public WorldMetadata WorldMetadata {
      get { return this.worldMetadata; }
    }
    #endregion

    #region [Property: PluginCooperationHandler]
    private readonly PluginCooperationHandler pluginCooperationHandler;

    public PluginCooperationHandler PluginCooperationHandler {
      get { return this.pluginCooperationHandler; }
    }
    #endregion


    #region [Method: Constructor]
    public CircuitHandler(
      PluginTrace pluginTrace,
      Configuration config, WorldMetadata worldMetadata, PluginCooperationHandler pluginCooperationHandler
    ) {
      Contract.Requires<ArgumentNullException>(pluginTrace != null);
      Contract.Requires<ArgumentNullException>(config != null);
      Contract.Requires<ArgumentNullException>(worldMetadata != null);
      Contract.Requires<ArgumentNullException>(pluginCooperationHandler != null);

      this.pluginTrace = pluginTrace;
      this.config = config;
      this.worldMetadata = worldMetadata;
      this.pluginCooperationHandler = pluginCooperationHandler;
      this.isDayTime = Main.dayTime;
      this.isDaylight = (Main.dayTime && Main.time >= 7200 && Main.time <= 46800);

      // Timers are always inactive when a map is loaded, so switch them into their active state.
      foreach (DPoint activeTimerLocation in this.WorldMetadata.ActiveTimers.Keys) {
        ObjectMeasureData timerMeasureData = TerrariaUtils.Tiles.MeasureObject(activeTimerLocation);
        
        if(!TerrariaUtils.Tiles.ObjectHasActiveState(timerMeasureData))
          TerrariaUtils.Tiles.SetObjectState(timerMeasureData, true);
      }
    }
    #endregion

    #region [Methods: HandleGameUpdate, HandleHitSwitch]
    private int frameCounter;
    private bool isDayTime;
    private bool isDaylight = true;

    public void HandleGameUpdate() {
      this.frameCounter++;

      if (this.frameCounter % CircuitHandler.TimerUpdateFrameRate == 0) {
        List<DPoint> timersToDelete = null;
        List<KeyValuePair<DPoint,ActiveTimerMetadata>> timersToProcess = null;
        foreach (KeyValuePair<DPoint,ActiveTimerMetadata> activeTimer in this.WorldMetadata.ActiveTimers) {
          DPoint timerLocation = activeTimer.Key;
          ActiveTimerMetadata timerMetadata = activeTimer.Value;

          Tile timerTile = TerrariaUtils.Tiles[timerLocation];
          if (timerMetadata == null || !timerTile.active || timerTile.type != (int)BlockType.XSecondTimer) {
            if (timersToDelete == null)
              timersToDelete = new List<DPoint>();

            timersToDelete.Add(timerLocation);
            continue;
          }

          if (timerMetadata.FramesLeft <= 0) {
            if (timersToProcess == null)
              timersToProcess = new List<KeyValuePair<DPoint,ActiveTimerMetadata>>();

            timersToProcess.Add(activeTimer);
            timerMetadata.FramesLeft = AdvancedCircuits.MeasureTimerFrameTime(timerLocation);

            continue;
          }
            
          timerMetadata.FramesLeft -= CircuitHandler.TimerUpdateFrameRate;
        }

        if (timersToProcess != null) {
          foreach (KeyValuePair<DPoint,ActiveTimerMetadata> activeTimer in timersToProcess) {
            SignalType signalType;
            // Is Advanced Circuit?
            if (!TerrariaUtils.Tiles[activeTimer.Key].wire)
              signalType = SignalType.On;
            else 
              signalType = SignalType.Swap;

            try {
              TSPlayer triggeringPlayer = null;
              if (activeTimer.Value.TriggeringPlayerName != null)
                triggeringPlayer = TShockEx.GetPlayerByName(activeTimer.Value.TriggeringPlayerName);
              if (triggeringPlayer == null)
                triggeringPlayer = TSPlayer.Server;

              CircuitProcessor processor = new CircuitProcessor(this.PluginTrace, this, activeTimer.Key);
              processor.ProcessCircuit(triggeringPlayer, signalType, false);
            } catch (Exception ex) {
              this.PluginTrace.WriteLineError(
                "Circuit processing for a Timer at {0} failed. See inner exception for details.\n{1}", 
                activeTimer.Key, ex.ToString()
              );
            }
          }
        }
        
        if (timersToDelete != null) {
          foreach (DPoint timerLocation in timersToDelete)
            this.WorldMetadata.ActiveTimers.Remove(timerLocation);
        }
      }

      if (this.frameCounter % CircuitHandler.ClockUpdateFrameRate == 0) {
        bool isDaylight = (Main.dayTime && Main.time >= 7200 && Main.time <= 46800);
        bool dayTimeChanged = (Main.dayTime != this.isDayTime);
        bool daylightChanged = (this.isDaylight != isDaylight);

        if (dayTimeChanged || daylightChanged) {
          List<DPoint> clocksToRemove = null;

          foreach (KeyValuePair<DPoint,GrandfatherClockMetadata> clock in this.WorldMetadata.Clocks) {
            DPoint clockLocation = clock.Key;
            Tile clockTile = TerrariaUtils.Tiles[clockLocation];

            if (!clockTile.active || clockTile.type != (int)BlockType.GrandfatherClock) {
              if (clocksToRemove == null)
                clocksToRemove = new List<DPoint>();

              clocksToRemove.Add(clock.Key);
              continue;
            }

            bool signal;
            switch (AdvancedCircuits.CountComponentModifiers(TerrariaUtils.Tiles.MeasureObject(clockLocation))) {
              case 1:
                if (!daylightChanged)
                  continue;

                signal = !isDaylight;
                break;
              case 2:
                if (!dayTimeChanged)
                  continue;

                signal = !Main.dayTime && Main.bloodMoon;
                break;
              default:
                if (!dayTimeChanged)
                  continue;

                signal = !Main.dayTime;
                break;
            }

            try {
              TSPlayer triggeringPlayer = null;
              if (clock.Value.TriggeringPlayerName != null)
                triggeringPlayer = TShockEx.GetPlayerByName(clock.Value.TriggeringPlayerName);
              if (triggeringPlayer == null)
                triggeringPlayer = TSPlayer.Server;

              CircuitProcessor processor = new CircuitProcessor(this.PluginTrace, this, clockLocation);
              processor.ProcessCircuit(triggeringPlayer, AdvancedCircuits.BoolToSignal(signal), false);
            } catch (Exception ex) {
              this.PluginTrace.WriteLineError(
                "Circuit processing for a Grandfather Clock at {0} failed. See inner exception for details.\n{1}", 
                clockLocation, ex.ToString()
              );
            }
          }

          if (clocksToRemove != null) {
            foreach (DPoint clockLocation in clocksToRemove)
              this.WorldMetadata.Clocks.Remove(clockLocation);
          }

          if (dayTimeChanged)
            this.isDayTime = Main.dayTime;
          if (daylightChanged)
            this.isDaylight = isDaylight;
        }

        if (this.frameCounter >= 100000)
          this.frameCounter = 0;
      }
    }

    public bool HandleHitSwitch(TSPlayer player, DPoint tileLocation) {
      try {
        CircuitProcessor processor = new CircuitProcessor(this.PluginTrace, this, tileLocation);
        this.NotifyPlayer(processor.ProcessCircuit(player));
      } catch (Exception ex) {
        this.PluginTrace.WriteLineError(
          "HitSwitch for \"{0}\" at {1} failed. See inner exception for details.\n{2}", 
          TerrariaUtils.Tiles.GetBlockTypeName((BlockType)TerrariaUtils.Tiles[tileLocation].type), tileLocation, ex.ToString()
        );
      }

      return true;
    }
    #endregion

    #region [Method: NotifyPlayer]
    protected void NotifyPlayer(CircuitProcessResult result) {
      TSPlayer player = result.TriggeringPlayer;
      if (player == TSPlayer.Server)
        return;

      switch (result.WarnReason) {
        case CircuitWarnReason.SignalesTooManyPumps:
          player.SendWarningMessage(string.Format(
            "Warning: This circuit tried to signal {0} Pumps, though the allowed maximum is {1}.",
            result.SignaledPumps, this.Config.MaxPumpsPerCircuit
          ));
          break;
        case CircuitWarnReason.SignalesTooManyDartTraps:
          player.SendWarningMessage(string.Format(
            "Warning: This circuit tried to signal {0} Dart Traps, though the allowed maximum is {1}.",
            result.SignaledDartTraps, this.Config.MaxDartTrapsPerCircuit
          ));
          break;
        case CircuitWarnReason.SignalesTooManyStatues:
          player.SendWarningMessage(string.Format(
            "Warning: This circuit tried to signal {0} Statues, though the allowed maximum is {1}.",
            result.SignaledStatues, this.Config.MaxStatuesPerCircuit
          ));
          break;
        case CircuitWarnReason.InsufficientPermissionToSignalComponent:
          player.SendWarningMessage("Warning: You don't have the required permission to signal");
          player.SendWarningMessage(string.Format(
            "the component \"{0}\".", AdvancedCircuits.GetComponentName(result.WarnRelatedComponentType)
          ));
          break;
        case CircuitWarnReason.BlockActivatorChangedTooManyBlocks:
          player.SendWarningMessage("Warning: A \"Block Activator\" component tried to change more");
          player.SendWarningMessage(string.Format(
            "blocks than the allowed maximum of {0} blocks.", this.Config.BlockActivatorConfig.MaxChangeableBlocks
          ));
          break;
      }

      switch (result.CancellationReason) {
        case CircuitCancellationReason.ExceededMaxLength:
          player.SendErrorMessage("Error: Circuit execution was cancelled because it exceeded");
          player.SendErrorMessage(string.Format("the maximum length of {0} wires.", this.Config.MaxCircuitLength));
          break;
        case CircuitCancellationReason.SignaledSameComponentTooOften:
          if (result.CancellationRelatedComponentType == BlockType.Invalid) {
            player.SendErrorMessage("Error: Circuit execution was cancelled because a component");
            player.SendErrorMessage("was signaled too often.");
          } else {
            player.SendErrorMessage("Error: Circuit execution was cancelled because the component");
            player.SendErrorMessage(string.Format(
              "\"{0}\" was signaled too often. Check up your circuit for loops.", AdvancedCircuits.GetComponentName(result.CancellationRelatedComponentType)
            ));
          }
          
          break;
      }

      #if Debug || Testrun
      player.SendInfoMessage(string.Format(
        "Length: {0}, Branches: {1}, Comps: {2}, PD-Comps: {3}, Time: {4}ms", 
        result.CircuitLength, result.ProcessedBranchCount, result.SignaledComponentsCounter, 
        result.SignaledPortDefiningComponentsCounter, result.ProcessingTime.TotalMilliseconds
      ));
      #endif
    }
    #endregion
  }
}
