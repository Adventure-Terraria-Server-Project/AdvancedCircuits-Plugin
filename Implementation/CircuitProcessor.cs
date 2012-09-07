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

using Hooks;
using TShockAPI;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class CircuitProcessor {
    #region [Constants]
    private const int TileId_ORGate = Terraria.TileId_CopperOre;
    private const int TileId_ANDGate = Terraria.TileId_SilverOre;
    private const int TileId_XORGate = Terraria.TileId_GoldOre;
    private const int TileId_NOTGate = Terraria.TileId_Obsidian;
    private const int TileId_Swapper = Terraria.TileId_IronOre;
    private const int TileId_CrossoverBridge = Terraria.TileId_Spike;
    private const int TileId_InputPort = Terraria.TileId_Glass;
    private const int TileId_Modifier = Terraria.TileId_CobaltOre;

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
    public CircuitProcessor(Configuration config, WorldMetadata worldMetadata) {
      this.config = config;
      this.worldMetadata = worldMetadata;
      this.isDayTime = Main.dayTime;
    }
    #endregion

    #region [Method: HandleGameUpdate, HandleHitSwitch]
    private int frameCounter;
    private bool isDayTime;

    public void HandleGameUpdate() {
      this.frameCounter++;

      if (this.frameCounter % CircuitProcessor.TimerUpdateFrameRate != 0)
        return;

      // Safe looping a dictionary requires quite a bit of performance, so we try to do it only when necessary.
      bool safeLoopRequired = false;
      foreach (KeyValuePair<DPoint,ActiveTimerMetadata> activeTimer in this.WorldMetadata.ActiveTimers) {
        if (activeTimer.Value.FramesLeft <= 0)
          safeLoopRequired = true;

        activeTimer.Value.FramesLeft -= CircuitProcessor.TimerUpdateFrameRate;
      }
      
      if (safeLoopRequired) {
        List<DPoint> activeTimerLocations = new List<DPoint>(this.WorldMetadata.ActiveTimers.Keys);
        foreach (DPoint activeTimerLocation in activeTimerLocations) {
          ActiveTimerMetadata activeActiveTimer;
          if (!this.WorldMetadata.ActiveTimers.TryGetValue(activeTimerLocation, out activeActiveTimer))
            continue;

          if (activeActiveTimer.FramesLeft <= 0) {
            int x = activeTimerLocation.X;
            int y = activeTimerLocation.Y;
            if (!Main.tile[x, y].active || Main.tile[x, y].type != Terraria.TileId_XSecondTimer) {
              this.WorldMetadata.ActiveTimers.Remove(activeTimerLocation);
              continue;
            }
          
            this.HitSwitch(TSPlayer.Server, new DPoint(x, y), true);
            activeActiveTimer.FramesLeft = this.MeasureTimersExpirationTime(x, y);
          }
        }
      }

      if (this.frameCounter % CircuitProcessor.ClockUpdateFrameRate != 0)
        return;

      if (Main.dayTime != this.isDayTime) {
        for (int i = 0; i < this.WorldMetadata.ClockLocations.Count; i++) {
          DPoint clockLocation = this.WorldMetadata.ClockLocations[i];
          if (Main.tile[clockLocation.X, clockLocation.Y].type != Terraria.TileId_GrandfatherClock) {
            this.WorldMetadata.ClockLocations.RemoveAt(i);
            i--;
          }

          this.HitSwitch(TSPlayer.Server, clockLocation, true, !Main.dayTime);
        }

        this.isDayTime = Main.dayTime;
      }

      if (this.frameCounter == 100000)
        this.frameCounter = 0;
    }

    public bool HandleHitSwitch(TSPlayer player, int x, int y) {
      this.HitSwitch(player, new DPoint(x, y));

      return true;
    }

    public void HitSwitch(
      TSPlayer player, DPoint tileLocation, bool stripOnly = false, bool? overridenSignal = null
    ) {
      Tile hitTile = Main.tile[tileLocation.X, tileLocation.Y];

      bool signal = false;
      if (overridenSignal != null)
        signal = overridenSignal.Value;

      bool isAdvancedCircuit;
      Terraria.SpriteMeasureData? measureData = null;
      if (hitTile.type == Terraria.TileId_Lever) {
        if (!Terraria.MeasureSprite(tileLocation, out measureData) || measureData == null)
          return;

        isAdvancedCircuit = true;
        for (int x = 0; x < measureData.Value.Size.X; x++) {
          for (int y = 0; y < measureData.Value.Size.Y; y++) {
            if (Main.tile[measureData.Value.OriginTileLocation.X + x, measureData.Value.OriginTileLocation.Y + y].wire)
              isAdvancedCircuit = false;
          }
        }
      } else {
        isAdvancedCircuit = !hitTile.wire;
      }

      if (isAdvancedCircuit) {
        if (!this.Config.AdvancedCircuitsEnabled)
          return;
        
        switch (hitTile.type) {
          case Terraria.TileId_PressurePlate:
            // Red sends "0", all the others send "1".
            signal = (hitTile.frameY > 0);
            break;
          case Terraria.TileId_GrandfatherClock:
            if (!Terraria.MeasureSprite(tileLocation, out measureData) || measureData == null)
              return;

            break;
          case Terraria.TileId_Lever:
          case Terraria.TileId_Switch:
          case Terraria.TileId_XSecondTimer:
            if (measureData == null && (!Terraria.MeasureSprite(tileLocation, out measureData) || measureData == null))
              return;

            if (!stripOnly) {
              signal = !measureData.Value.HasActiveFrame;
              
              // Turn the lever / switch / timer
              this.SignalizeSprite(tileLocation, signal);
            } else {
              signal = measureData.Value.HasActiveFrame;
            }

            break;
          default:
            return;
        }
      } else { 
        if (this.Config.OverrideVanillaCircuits) {
          if (hitTile.type == Terraria.TileId_XSecondTimer && !stripOnly) {
            if (measureData == null && (!Terraria.MeasureSprite(tileLocation, out measureData) || measureData == null))
              return;

            this.SignalizeSprite(tileLocation, !measureData.Value.HasActiveFrame);
          }
        } else {
          WorldGen.hitSwitch(tileLocation.X, tileLocation.Y);
          return;
        }
      }

      if (hitTile.type == Terraria.TileId_XSecondTimer && !stripOnly)
        return;

      if (!isAdvancedCircuit) {
        if (hitTile.type == Terraria.TileId_Lever) {
          Debug.Assert(measureData != null);

          DPoint origin = measureData.Value.OriginTileLocation;
          DPoint size = measureData.Value.Size;
          List<DPoint> ignoredTiles = new List<DPoint>();
          bool wireFound = false;
          for (int x = 0; x < size.X && !wireFound; x++) {
            for (int y = 0; y < size.Y; y++) {
              DPoint innerTileLocation = new DPoint(origin.X + x, origin.Y + y);
              ignoredTiles.Add(innerTileLocation);
              if (Main.tile[origin.X + x, origin.Y + y].wire) {
                this.StripCircuit(false, new CircuitStripData(player, innerTileLocation, innerTileLocation, false));
                wireFound = true;
                break;
              }
            }
          }
        } else {
          this.StripCircuit(false, new CircuitStripData(player, tileLocation, tileLocation, false));
        }
      } else {
        switch (hitTile.type) {
          case Terraria.TileId_Lever:
          case Terraria.TileId_GrandfatherClock: {
            Debug.Assert(measureData != null);

            DPoint origin = measureData.Value.OriginTileLocation;
            DPoint size = measureData.Value.Size;
            List<DPoint> ignoredTiles = new List<DPoint>();
            List<DPoint> portLocations = new List<DPoint>();
            for (int x = 0; x < size.X; x++) {
              for (int y = 0; y < size.Y; y++) {
                ignoredTiles.Add(new DPoint(origin.X + x, origin.Y + y));

                // Got to do that for one single loop through y only.
                if (x == 0) {
                  portLocations.Add(new DPoint(origin.X - 1, origin.Y + y));
                  portLocations.Add(new DPoint(origin.X + size.X, origin.Y + y));
                }
              }

              portLocations.Add(new DPoint(origin.X + x, origin.Y - 1));
              portLocations.Add(new DPoint(origin.X + x, origin.Y + size.Y));
            }

            for (int i = 0; i < portLocations.Count; i++) {
              this.StripCircuit(
                signal, new CircuitStripData(player, tileLocation, portLocations[i], true, ignoredTiles)
              );
            }

            break;
          }
          // Expect 1x1 size of all the other senders.
          default: {
            DPoint[] portLocations = new[] {
              new DPoint(tileLocation.X - 1, tileLocation.Y),
              new DPoint(tileLocation.X, tileLocation.Y - 1),
              new DPoint(tileLocation.X + 1, tileLocation.Y),
              new DPoint(tileLocation.X, tileLocation.Y + 1)
            };

            List<DPoint> ignoredTiles = new List<DPoint>();
            // Allow timers to switch themselfes in the same circuit.
            if (hitTile.type != Terraria.TileId_XSecondTimer)
              ignoredTiles.Add(new DPoint(tileLocation.X, tileLocation.Y));

            for (int i = 0; i < portLocations.Length; i++) {
              this.StripCircuit(
                signal, new CircuitStripData(player, tileLocation, portLocations[i], true, ignoredTiles)
              );
            }

            break;
          }
        }
      }

      if (WorldGen.numInPump <= 0 || WorldGen.numOutPump <= 0)
        return;
      WorldGen.xferWater();
    }
    #endregion

    #region [Methods: StripCircuit, StripProcessTile]
    public void StripCircuit(bool signal, CircuitStripData stripData) {
      Tile tile = Main.tile[stripData.FirstWireLocation.X, stripData.FirstWireLocation.Y];
      if (!tile.wire)
        return;
      // Input Ports are not meant to send signals.
      if (stripData.IsAdvancedCircuit && tile.active && tile.type == CircuitProcessor.TileId_InputPort)
        return;

      #if Verbose
      if (stripData.IsAdvancedCircuit) {
        AdvancedCircuitsPlugin.Trace.WriteLineVerbose(
          "Started stripping Advanced Circuit at {0} with signal {1}.", stripData.SenderLocation, signal.ToString()
        );
      } else {
        AdvancedCircuitsPlugin.Trace.WriteLineVerbose(
          "Started stripping Vanilla Circuit at {0} with signal {1}.", stripData.SenderLocation, signal.ToString()
        );
      }
      #endif

      WorldGen.numWire = 0;
      WorldGen.numNoWire = 0;
      WorldGen.numInPump = 0;
      WorldGen.numOutPump = 0;

      try {
        this.StripProcessTile(stripData.FirstWireLocation, signal, stripData);
      } catch (Exception ex) {
        AdvancedCircuitsPlugin.Trace.WriteLineError(
          "Failed on stripping circuit at [{0}]. Exception details: {1}", stripData.FirstWireLocation, ex
        );
      }

      AdvancedCircuitsPlugin.Trace.WriteLineVerbose(
        "Ended stripping circuit. {0} wires signaled. {1} sprites signaled. {2} components signaled.", 
        stripData.ProcessedWires.Count, stripData.SignaledSpritesCounter, stripData.SignaledComponentsCounter
      );
    }

    protected virtual void StripProcessTile(DPoint tileLocation, bool signal, CircuitStripData stripData) {
      if (tileLocation == stripData.LastWireLocation || stripData.IsCancelled)
        return;

      Tile tile = Main.tile[tileLocation.X, tileLocation.Y];
      // If the tile has no wire it might be a component and thus the last wire would be its port.
      if (!tile.wire) {
        if (!stripData.IsAdvancedCircuit)
          return;
        if (stripData.IgnoredTiles.Contains(tileLocation))
          return;

        if (this.SignalizeComponent(stripData.LastWireLocation, tileLocation, signal, stripData))
          stripData.SignaledComponentsCounter++;

        return;
      }
      if (stripData.ProcessedWires.Count == this.Config.MaxCircuitLength) {
        AdvancedCircuitsPlugin.Trace.WriteLineInfo(
          "Stripping the Advanced Circuit at [{0}] was cancelled because the signal reached the maximum transfer length of {1} wires.",
          stripData.SenderLocation, this.Config.MaxCircuitLength
        );
        stripData.IsCancelled = true;
        
        return;
      }
      if (stripData.ProcessedWires.Contains(tileLocation))
        return;
      
      if (tile.active && !stripData.IgnoredTiles.Contains(tileLocation)) {
        if (this.SignalizeSprite(tileLocation, signal, stripData))
          stripData.SignaledSpritesCounter++;
      }

      stripData.ProcessedWires.Add(tileLocation);

      this.StripProcessTile(new DPoint(tileLocation.X - 1, tileLocation.Y), signal, stripData);
      this.StripProcessTile(new DPoint(tileLocation.X, tileLocation.Y - 1), signal, stripData);
      this.StripProcessTile(new DPoint(tileLocation.X + 1, tileLocation.Y), signal, stripData);
      this.StripProcessTile(new DPoint(tileLocation.X, tileLocation.Y + 1), signal, stripData);
    }
    #endregion

    #region [Methods: SignalizeSprite, SignalizeComponent]
    /// <summary>
    ///   Signalizes a directly wired (portless) component.
    /// </summary>
    /// <param name="anyTileLocation">
    ///   The location of a arbitary tile within the component.
    /// </param>
    /// <param name="signal">
    ///   The signal to send.
    /// </param>
    /// <param name="stripData">
    ///   The <see cref="CircuitStripData" /> describing the current circuit's state, set to <c>null</c> to indicate
    ///   that a sprite should be simply signaled as it were in an advanced circuit.
    /// </param>
    /// <returns>
    ///   The size of the unit. Null if the tile at the given location is no terraria unit at all.
    /// </returns>
    private bool SignalizeSprite(DPoint anyTileLocation, bool signal, CircuitStripData stripData = null) {
      int x = anyTileLocation.X;
      int y = anyTileLocation.Y;
      Tile tile = Main.tile[anyTileLocation.X, anyTileLocation.Y];

      Terraria.SpriteMeasureData? measureData;
      if (!Terraria.MeasureSprite(anyTileLocation, out measureData) || measureData == null)
        return false;

      int originX = measureData.Value.OriginTileLocation.X;
      int originY = measureData.Value.OriginTileLocation.Y;
      int spriteWidth = measureData.Value.Size.X;
      int spriteHeight = measureData.Value.Size.Y;
      DPoint spriteTextureTileSize = measureData.Value.TextureTileSize;
      bool isAdvancedCircuit = (stripData == null || stripData.IsAdvancedCircuit);

      switch (tile.type) {
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
        case Terraria.TileId_Switch:
        case Terraria.TileId_Lever: {
          bool doChange = true;
          if (
            tile.type == Terraria.TileId_Switch || 
            tile.type == Terraria.TileId_Lever
          ) {
            // Directly wired Switches / Levers should not be toggled.
            if (!isAdvancedCircuit) {
              doChange = false;
            } else {
              bool directlyWired = false;
              for (int tx = 0; tx < spriteWidth && !directlyWired; tx++) {
                for (int ty = 0; ty < spriteHeight; ty++) {
                  if (Main.tile[originX + tx, originY + ty].wire) {
                    directlyWired = true;
                    break;
                  }
                }
              }

              doChange = !directlyWired;
            }
          }

          if (doChange) {
            if (!isAdvancedCircuit)
              signal = !measureData.Value.HasActiveFrame;
          } else {
            signal = measureData.Value.HasActiveFrame;
          }

          this.SetSpriteActiveFrame(measureData.Value, signal, stripData);

          return true;
        }
        case Terraria.TileId_XSecondTimer:
          // A sending timer shall not toggle itself. (In an Advanced Circuit this would work through a port though.)
          //if (stripData == null || anyTileLocation != stripData.SenderLocation) {
            if (!isAdvancedCircuit)
              signal = !measureData.Value.HasActiveFrame;

            this.SetSpriteActiveFrame(measureData.Value, signal, stripData);

            if (tile.type == Terraria.TileId_XSecondTimer) {
              // If a timer receives true while it is already activated, reset its frame time.
              if (isAdvancedCircuit && signal == measureData.Value.HasActiveFrame) {
                if (signal)
                  this.WorldMetadata.ActiveTimers[anyTileLocation].FramesLeft = this.MeasureTimersExpirationTime(x, y);
              
                return true;
              }

              if (signal)
                this.WorldMetadata.ActiveTimers.Add(anyTileLocation, new ActiveTimerMetadata(this.MeasureTimersExpirationTime(x, y)));
              else
                this.WorldMetadata.ActiveTimers.Remove(anyTileLocation);
            }
          //} else if (stripData != null) {
//            stripData.IgnoredTiles.Add(anyTileLocation);
          //}

          return true;
        case Terraria.TileId_ActiveStone:
        case Terraria.TileId_InactiveStone: {
          bool isActive = (tile.type == Terraria.TileId_ActiveStone);
          if (!isAdvancedCircuit)
            signal = !isActive;

          if (isActive != signal) {
            byte newTileType;
            if (signal)
              newTileType = Terraria.TileId_ActiveStone;
            else
              newTileType = Terraria.TileId_InactiveStone;

            Main.tile[originX, originY].type = newTileType;
            WorldGen.SquareTileFrame(originX, originY);
            TSPlayer.All.SendTileSquareEx(originX, originY, 1);
          }

          if (stripData != null)
            stripData.IgnoredTiles.Add(anyTileLocation);
          
          return true;
        }
        case Terraria.TileId_DoorClosed:
        case Terraria.TileId_DoorOpened: {
          bool isOpen = (tile.type == Terraria.TileId_DoorOpened);
          if (!isAdvancedCircuit)
            signal = !isOpen;

          if (signal != isOpen) {
            // TODO: This code sucks
            if (signal) {
              int direction = 1;
              if (stripData != null && x < stripData.SenderLocation.X)
                direction = -1;

              if (WorldGen.OpenDoor(x, y, direction) || WorldGen.OpenDoor(x, y, --direction)) {
                spriteWidth = 2;
                TSPlayer.All.SendData(PacketTypes.DoorUse, string.Empty, 0, x, y, direction);

                // Re-Measure the door's tiles.
                tile = Main.tile[originX, originY];
                int frameIndexX = tile.frameX / spriteTextureTileSize.X;
                if (frameIndexX >= spriteWidth)
                  frameIndexX -= spriteWidth;

                originX -= frameIndexX;
                originY = originY - (tile.frameY / spriteTextureTileSize.Y);
              }
            } else {
              if (WorldGen.CloseDoor(x, y, true))
                TSPlayer.All.SendData(PacketTypes.DoorUse, string.Empty, 1, x, y);
            }

            WorldGen.numWire = 0;
            WorldGen.numNoWire = 0;
          }

          for (int tx = 0; tx < spriteWidth; tx++) {
            for (int ty = 0; ty < spriteHeight; ty++) {
              int absoluteX = originX + tx;
              int absoluteY = originY + ty;

              if (stripData != null)
                stripData.IgnoredTiles.Add(new DPoint(absoluteX, absoluteY));
            }
          }

          return true;
        }
        case Terraria.TileId_MusicBox: {
          if (!isAdvancedCircuit)
            signal = !measureData.Value.HasActiveFrame;

          if (signal != measureData.Value.HasActiveFrame)
            WorldGen.SwitchMB(x, y);

          if (stripData != null) {
            stripData.IgnoredTiles.Add(new DPoint(originX, originY));
            stripData.IgnoredTiles.Add(new DPoint(originX + 1, originY));
            stripData.IgnoredTiles.Add(new DPoint(originX + 1, originY + 1));
            stripData.IgnoredTiles.Add(new DPoint(originX, originY + 1));
          }

          return true;
        }
        case Terraria.TileId_InletPump:
        case Terraria.TileId_OutletPump: {
          int pumpCounter;
          if (tile.type == Terraria.TileId_InletPump)
            pumpCounter = WorldGen.numInPump;
          else
            pumpCounter = WorldGen.numOutPump;

          for (int tx = 0; tx < spriteWidth; tx++) {
            for (int ty = 0; ty < spriteHeight; ty++) {
              int absoluteX = originX + tx;
              int absoluteY = originY + ty;

              if (!isAdvancedCircuit || signal) {
                if (stripData == null || stripData.SignaledPumps < this.Config.MaxPumpsPerCircuit) {
                  if (tile.type == Terraria.TileId_InletPump) {
                    WorldGen.inPumpX[pumpCounter] = absoluteX;
                    WorldGen.inPumpY[pumpCounter] = absoluteY;
                  } else {
                    WorldGen.outPumpX[pumpCounter] = absoluteX;
                    WorldGen.outPumpY[pumpCounter] = absoluteY;
                  }
                  pumpCounter++;
                }
              }

              if (stripData != null)
                stripData.IgnoredTiles.Add(new DPoint(absoluteX, absoluteY));
            }
          }

          if (tile.type == Terraria.TileId_InletPump)
            WorldGen.numInPump = pumpCounter;
          else
            WorldGen.numOutPump = pumpCounter;

          if (!isAdvancedCircuit || signal)
            stripData.SignaledPumps++;

          return true;
        }
        case Terraria.TileId_DartTrap: {
          DartTrapConfig config = this.Config.DartTrapConfig;

          if ((!isAdvancedCircuit || signal) && WorldGen.checkMech(x, y, config.Cooldown)) {
            if (stripData == null || stripData.SignaledDartTraps < this.Config.MaxDartTrapsPerCircuit) {
              bool isPointingLeft = (Main.tile[x, y].frameX == 0);
              DPoint projectileSpawn = new DPoint((x * 16), (y * 16 + 9));
              float projectileSpeed;
              
              if (isPointingLeft) {
                projectileSpawn.X -= config.ProjectileOffset;
                projectileSpeed = -config.ProjectileSpeed;
              } else {
                projectileSpawn.X += Terraria.TileSize + config.ProjectileOffset;
                projectileSpeed = config.ProjectileSpeed;
              }
            
              int projectileDamage = config.ProjectileDamage;
              int projectileType = config.ProjectileType;
              const float ProjectileKnockBack = 2f;
              int projectileIndex = Projectile.NewProjectile(
                projectileSpawn.X, projectileSpawn.Y, projectileSpeed, 0f, projectileType, projectileDamage, 
                ProjectileKnockBack, Main.myPlayer
              );
              Main.projectile[projectileIndex].timeLeft = config.ProjectileLifeTime;

              stripData.SignaledDartTraps++;
            }
          }

          if (stripData != null)
            stripData.IgnoredTiles.Add(new DPoint(x, y));

          return true;
        }
        case Terraria.TileId_Explosives: {
          if (!isAdvancedCircuit || signal) {
            WorldGen.KillTile(originX, originY, false, false, true);
            TSPlayer.All.SendTileSquareEx(originX, originY, 1);
            Projectile.NewProjectile((originX * 16 + 8), (originY * 16 + 8), 0f, 0f, 108, 250, 10f, Main.myPlayer);
          }

          if (stripData != null)
            stripData.IgnoredTiles.Add(new DPoint(originX, originY));

          return true;
        }
        case Terraria.TileId_Statue: {
          if (!isAdvancedCircuit || signal) {
            StatueType statueType = (StatueType)(tile.frameX / (spriteTextureTileSize.X * 2));

            StatueConfig statueConfig;
            if (
              this.Config.StatueConfigs.TryGetValue(statueType, out statueConfig) && statueConfig != null &&
              WorldGen.checkMech(originX, originY, statueConfig.Cooldown)
            ) {
              if (stripData == null || stripData.SignaledStatues < this.Config.MaxStatuesPerCircuit) {
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
                      int pickedIndex = rnd.Next(npcIndexes.Count);
                      Main.npc[pickedIndex].position.X = (spawnX - (Main.npc[pickedIndex].width / 2));
                      Main.npc[pickedIndex].position.Y = (spawnY - (Main.npc[pickedIndex].height - 1));

                      NetMessage.SendData((int)PacketTypes.NpcUpdate, -1, -1, string.Empty, pickedIndex);
                    }

                    break;
                  case StatueActionType.SpawnMob:
                    // Param1 = Mob type
                    // Param2 = Max mobs in range
                    // Param3 = Range to check
                    if (Terraria.CountNPCsInRange(spawnX, spawnY, statueConfig.ActionParam, statueConfig.ActionParam3) < statueConfig.ActionParam2) {
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
                    if (Terraria.CountItemsInRange(spawnX, spawnY, statueConfig.ActionParam, statueConfig.ActionParam3) < statueConfig.ActionParam2)
                      Item.NewItem(spawnX, spawnY, 0, 0, statueConfig.ActionParam);
                  
                    break;
                  case StatueActionType.SpawnBoss:

                    break;
                }
              }

              stripData.SignaledStatues++;
            }
          }

          if (stripData != null) {
            for (int tx = 0; tx < spriteWidth; tx++) {
              for (int ty = 0; ty < spriteHeight; ty++) {
                int absoluteX = originX + tx;
                int absoluteY = originY + ty;

                stripData.IgnoredTiles.Add(new DPoint(absoluteX, absoluteY));
              }
            }
          }

          return true;
        }
      }

      return false;
    }

    private bool SignalizeComponent(
      DPoint portLocation, DPoint componentLocation, bool signal, CircuitStripData stripData
    ) {
      Tile portTile = Main.tile[portLocation.X, portLocation.Y];
      Tile componentTile = Main.tile[componentLocation.X, componentLocation.Y];
      
      List<DPoint> componentPorts = null;
      switch (componentTile.type) {
        case Terraria.TileId_Switch:
        case Terraria.TileId_Lever:
        case Terraria.TileId_XSecondTimer:
          // A component can not signalize itself if the signal didn't travel at least over one wire.
          if (!(portLocation == stripData.FirstWireLocation && componentLocation == stripData.SenderLocation))
            this.SignalizeSprite(componentLocation, signal, stripData);

          return true;
      }

      switch (componentTile.type) {
        case CircuitProcessor.TileId_NOTGate:
          if (!portTile.active || portTile.type != CircuitProcessor.TileId_InputPort)
            return true;

          signal = !signal;
          break;
        case CircuitProcessor.TileId_ANDGate:
        case CircuitProcessor.TileId_ORGate:
        case CircuitProcessor.TileId_XORGate: {
          if (!portTile.active || portTile.type != CircuitProcessor.TileId_InputPort)
            return true;

          GateStateMetadata metadata;
          if (!this.WorldMetadata.GateStates.TryGetValue(componentLocation, out metadata)) {
            metadata = new GateStateMetadata();
            this.WorldMetadata.GateStates.Add(componentLocation, metadata);
          }

          componentPorts = new List<DPoint> {
            new DPoint(componentLocation.X - 1, componentLocation.Y),
            new DPoint(componentLocation.X, componentLocation.Y - 1),
            new DPoint(componentLocation.X + 1, componentLocation.Y),
            new DPoint(componentLocation.X, componentLocation.Y + 1)
          };
          int inputPorts = 0;
          int signaledPorts = 0;
          bool isInvalid = false;
          for (int i = 0; i < componentPorts.Count; i++) {
            DPoint port = componentPorts[i];
            portTile = Main.tile[port.X, port.Y];
            if (portTile.wire && portTile.active && portTile.type == CircuitProcessor.TileId_InputPort) {
              inputPorts++;

              if (port == portLocation)
                metadata.PortStates[i] = signal;

              if (metadata.PortStates[i] == null)
                isInvalid = true;
            }

            if (metadata.PortStates[i] != null && metadata.PortStates[i].Value)
              signaledPorts++;
          }

          // Gates will not operate as long as the input port states are not clear.
          if (isInvalid)
            return true;

          switch (componentTile.type) {
            case CircuitProcessor.TileId_ANDGate:
              signal = (inputPorts == signaledPorts);
              break;
            case CircuitProcessor.TileId_ORGate:
              signal = (signaledPorts > 0);
              break;
            case CircuitProcessor.TileId_XORGate:
              signal = (signaledPorts != 0 && signaledPorts < inputPorts);
              break;
          }

          break;
        }
        case CircuitProcessor.TileId_Swapper:
          if (!signal)
            return true;

          int activeSwapperIndex = this.WorldMetadata.ActiveSwapperLocations.IndexOf(componentLocation);
          if (activeSwapperIndex == -1) {
            this.WorldMetadata.ActiveSwapperLocations.Add(componentLocation);
            signal = false;
          } else {
            this.WorldMetadata.ActiveSwapperLocations.RemoveAt(activeSwapperIndex);
            signal = true;
          }

          break;
        case CircuitProcessor.TileId_CrossoverBridge:
          break;
        default:
          return false;
      }

      stripData.IgnoredTiles.Add(componentLocation);

      if (componentPorts == null) {
        componentPorts = new List<DPoint> {
          new DPoint(componentLocation.X - 1, componentLocation.Y),
          new DPoint(componentLocation.X, componentLocation.Y - 1),
          new DPoint(componentLocation.X + 1, componentLocation.Y),
          new DPoint(componentLocation.X, componentLocation.Y + 1)
        };
      }
      
      if (componentTile.type == CircuitProcessor.TileId_CrossoverBridge) {
        int inputPortIndex = componentPorts.IndexOf(portLocation);
        int outputPortIndex = inputPortIndex + 2;
        if (outputPortIndex >= componentPorts.Count)
          outputPortIndex = outputPortIndex - componentPorts.Count;

        DPoint outputPort = componentPorts[outputPortIndex];
        componentPorts.Clear();
        componentPorts.Add(outputPort);
      }

      for (int i = 0; i < componentPorts.Count; i++) {
        DPoint port = componentPorts[i];
        if (port == portLocation) {
          componentPorts.RemoveAt(i--);
          continue;
        }

        portTile = Main.tile[port.X, port.Y];
        if (!portTile.wire || (portTile.active && portTile.type == CircuitProcessor.TileId_InputPort)) {
          componentPorts.RemoveAt(i--);
          continue;
        }
      }

      foreach (DPoint port in componentPorts)
        this.StripProcessTile(port, signal, stripData);

      return true;
    }
    #endregion

    #region [Methods: SetSpriteActiveFrame]
    protected void SetSpriteActiveFrame(Terraria.SpriteMeasureData measureData, bool signal, CircuitStripData stripData = null) {
      int originX = measureData.OriginTileLocation.X;
      int originY = measureData.OriginTileLocation.Y;
      int frameXOffsetAdd = measureData.FrameXOffsetAdd;
      
      bool needsFrameUpdate = false;
      int spriteWidth = measureData.Size.X;
      int spriteHeight = measureData.Size.Y;
      short newFrameXOffset = 0;
      short newFrameYOffset = 0;
      if (signal != measureData.HasActiveFrame) {
        if (measureData.SpriteType != Terraria.TileId_Switch && measureData.SpriteType != Terraria.TileId_XSecondTimer) {
          int frameXOffset = (spriteWidth * measureData.TextureTileSize.X) + frameXOffsetAdd;
          if (signal)
            newFrameXOffset = (short)-frameXOffset;
          else
            newFrameXOffset = (short)frameXOffset;
        } else {
          int frameYOffset = (spriteHeight * measureData.TextureTileSize.Y);
          if (measureData.SpriteType == Terraria.TileId_XSecondTimer)
            signal = !signal;

          if (signal)
            newFrameYOffset = (short)-frameYOffset;
          else
            newFrameYOffset = (short)frameYOffset;
        }
        needsFrameUpdate = true;
      }

      for (int tx = 0; tx < spriteWidth; tx++) {
        for (int ty = 0; ty < spriteHeight; ty++) {
          int absoluteX = originX + tx;
          int absoluteY = originY + ty;

          if (needsFrameUpdate) {
            Main.tile[absoluteX, absoluteY].frameX += newFrameXOffset;
            Main.tile[absoluteX, absoluteY].frameY += newFrameYOffset;
          }
                
          if (stripData != null)
            stripData.IgnoredTiles.Add(new DPoint(absoluteX, absoluteY));
        }
      }
            
      if (needsFrameUpdate)
        TSPlayer.All.SendTileSquareEx(originX, originY, Math.Max(spriteWidth, spriteHeight));
    }
    #endregion

    #region [Methods: MeasureTimersExpirationTime]
    protected int MeasureTimersExpirationTime(int x, int y) {
      int frames = -1;
      switch (Main.tile[x, y].frameX) {
        case 0:
          frames = 60;
          break;
        case 18:
          frames = 3 * 60;
          break;
        case 36:
          frames = 5 * 60;
          break;
      }

      return frames;
    }
    #endregion
  }
}
