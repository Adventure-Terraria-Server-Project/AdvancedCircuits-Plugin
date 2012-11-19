// This file is provided unter the terms of the 
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// 
// Written by CoderCow

/* Star Cloak Projectiles:
for (int i = 0; i < 3; i++) {
  float x = this.position.X + (float)Main.rand.Next(-400, 400);
  float y = this.position.Y - (float)Main.rand.Next(500, 800);
  Vector2 vector = new Vector2(x, y);
  float num4 = this.position.X + (float)(this.width / 2) - vector.X;
  float num5 = this.position.Y + (float)(this.height / 2) - vector.Y;
  num4 += (float)Main.rand.Next(-100, 101);
  int num6 = 23;
  float num7 = (float)Math.Sqrt((double)(num4 * num4 + num5 * num5));
  num7 = (float)num6 / num7;
  num4 *= num7;
  num5 *= num7;
  int num8 = Projectile.NewProjectile(x, y, num4, num5, 92, 30, 5f, this.whoAmi);
  Main.projectile[num8].ai[1] = this.position.Y;
}

case Terraria.TileId_Sunflower: {
  if (signal == true) {
    DPoint projectileSpawn = new DPoint((originX * 16 + 16), (originY * 16 + 8));
    float projectileSpeedX = 5;
    float projectileSpeedY = 5;
    int projectileType = 10;
            
    Projectile.NewProjectile(
      projectileSpawn.X, projectileSpawn.Y, projectileSpeedX, projectileSpeedY, projectileType, 1, 
      1, Main.myPlayer
    );

    Projectile.NewProjectile(
      projectileSpawn.X, projectileSpawn.Y, -projectileSpeedX, projectileSpeedY, projectileType, 1, 
      1, Main.myPlayer
    );
  }

  if (stripData != null) {
    for (int tx = 0; tx < frameWidth; tx++) {
      for (int ty = 0; ty < frameHeight; ty++) {
        int absoluteX = originX + tx;
        int absoluteY = originY + ty;

        stripData.Value.IgnoredTiles.Add(new DPoint(absoluteX, absoluteY));
      }
    }
  }
  return true;
}
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using DPoint = System.Drawing.Point;

using TShockAPI;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class CircuitHandler {
    #region [Constants]
    private const int TimerUpdateFrameRate = 10;
    private const int ClockUpdateFrameRate = 60;
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


    #region [Method: Constructor]
    public CircuitHandler(Configuration config, WorldMetadata worldMetadata) {
      this.config = config;
      this.worldMetadata = worldMetadata;
      this.isDayTime = Main.dayTime;
      this.isDaylight = (Main.dayTime && Main.time >= 7200 && Main.time <= 46800);

      // Timers are always inactive when a map is loaded, so switch them into their active state if necessary.
      foreach (DPoint activeTimerLocation in this.WorldMetadata.ActiveTimers.Keys) {
        Terraria.SpriteMeasureData timerMeasureData = Terraria.MeasureSprite(activeTimerLocation);
        
        if(!Terraria.HasSpriteActiveFrame(timerMeasureData))
          Terraria.SetSpriteActiveFrame(timerMeasureData, true);
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

          Tile timerTile = Terraria.Tiles[timerLocation];
          if (timerMetadata == null || !timerTile.active || timerTile.type != Terraria.TileId_XSecondTimer) {
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
            if (!Terraria.Tiles[activeTimer.Key].wire)
              signalType = SignalType.On;
            else 
              signalType = SignalType.Swap;

            try {
              TSPlayer triggeringPlayer = null;
              if (activeTimer.Value.TriggeringPlayerName != null)
                triggeringPlayer = TShockEx.GetPlayerByName(activeTimer.Value.TriggeringPlayerName);
              if (triggeringPlayer == null)
                triggeringPlayer = TSPlayer.Server;

              CircuitProcessor processor = new CircuitProcessor(this.Config, this.WorldMetadata, activeTimer.Key);
              processor.ProcessCircuit(triggeringPlayer, signalType, false);
            } catch (Exception ex) {
              AdvancedCircuitsPlugin.Trace.WriteLineError(
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
            Tile clockTile = Terraria.Tiles[clockLocation];

            if (!clockTile.active || clockTile.type != Terraria.TileId_GrandfatherClock) {
              if (clocksToRemove == null)
                clocksToRemove = new List<DPoint>();

              clocksToRemove.Add(clock.Key);
              continue;
            }

            bool signal;
            switch (AdvancedCircuits.CountComponentModifiers(Terraria.MeasureSprite(clockLocation))) {
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

              CircuitProcessor processor = new CircuitProcessor(this.Config, this.WorldMetadata, clockLocation);
              processor.ProcessCircuit(triggeringPlayer, AdvancedCircuits.BoolToSignal(signal), false);
            } catch (Exception ex) {
              AdvancedCircuitsPlugin.Trace.WriteLineError(
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

    public bool HandleHitSwitch(TSPlayer player, int x, int y) {
      try {
        CircuitProcessor processor = new CircuitProcessor(this.Config, this.WorldMetadata, new DPoint(x, y));
        this.NotifyPlayer(processor.ProcessCircuit(player));
      } catch (Exception ex) {
        AdvancedCircuitsPlugin.Trace.WriteLineError(
          "HitSwitch for \"{0}\" at {1} failed. See inner exception for details.\n{2}", 
          Terraria.Tiles.GetBlockName(Terraria.Tiles[x, y].type), new DPoint(x, y), ex.ToString()
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
      }

      switch (result.CancellationReason) {
        case CircuitCancellationReason.ExceededMaxLength:
          player.SendErrorMessage("Error: Circuit execution was cancelled because it exceeded");
          player.SendErrorMessage(string.Format(
            "the maximum length of {0} wires, current length is {1}.", this.Config.MaxCircuitLength, result.CircuitLength
          ));
          break;
        case CircuitCancellationReason.SignaledSameComponentTooOften:
          if (result.CancellationRelatedComponentType == -1) {
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
