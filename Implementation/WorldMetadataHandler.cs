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
  public class WorldMetadataHandler: WorldMetadataHandlerBase {
    #region [Property: Metadata]
    public new WorldMetadata Metadata {
      get { return (WorldMetadata)base.Metadata; }
    }
    #endregion


    #region [Methods: Constructor, InitMetadata, ReadMetadataFromFile]
    public WorldMetadataHandler(string metadataDirectoryPath): base(AdvancedCircuitsPlugin.Trace, metadataDirectoryPath) {}

    protected override IWorldMetadata InitMetadata() {
      WorldMetadata metadata = new WorldMetadata();

      List<DPoint> ignoredTiles = new List<DPoint>();
      for (int x = 0; x < Main.maxTilesX - 1; x++) {
        for (int y = 0; y < Main.maxTilesY - 1; y++) {
          if (!Terraria.Tiles[x, y].active)
            continue;

          DPoint tileLocation = new DPoint(x, y);
          if (ignoredTiles.Contains(tileLocation))
            continue;

          switch (Terraria.Tiles[x, y].type) {
            case Terraria.TileId_XSecondTimer: {
              // Is active timer?
              if (Terraria.Tiles[x, y].frameY > 0)
                metadata.ActiveTimers.Add(tileLocation, new ActiveTimerMetadata());

              ignoredTiles.Add(tileLocation);

              break;
            }
            case Terraria.TileId_GrandfatherClock: {
              Terraria.SpriteMeasureData measureData = Terraria.MeasureSprite(tileLocation);

              metadata.Clocks.Add(measureData.OriginTileLocation, new GrandfatherClockMetadata(null));
              for (int sx = 0; sx < measureData.Size.X; sx++) {
                for (int sy = 0; sy < measureData.Size.Y; sy++) {
                  ignoredTiles.Add(new DPoint(x + sx, y + sy));
                }
              }

              break;
            }
          }
        }
      }

      return metadata;
    }

    protected override IWorldMetadata ReadMetadataFromFile(string filePath) {
      WorldMetadata metadata = WorldMetadata.Read(filePath);

      // Invalidate Gates if necessary.
      List<DPoint> gateLocations = new List<DPoint>(metadata.GateStates.Keys);
      foreach (DPoint gateLocation in gateLocations) {
        Tile tile = Terraria.Tiles[gateLocation];
        if (
          !tile.active || (
            tile.type != AdvancedCircuits.TileId_ANDGate && 
            tile.type != AdvancedCircuits.TileId_ORGate && 
            tile.type != AdvancedCircuits.TileId_XORGate)
        ) {
          metadata.GateStates.Remove(gateLocation);
        }
      }

      // Invalidate active Timers.
      List<DPoint> activeTimerLocations = new List<DPoint>(metadata.ActiveTimers.Keys);
      foreach (DPoint activeTimerLocation in activeTimerLocations) {
        Tile tile = Main.tile[activeTimerLocation.X, activeTimerLocation.Y];
        if (!tile.active || tile.type != Terraria.TileId_XSecondTimer)
          metadata.ActiveTimers.Remove(activeTimerLocation);
      }

      // Invalidate Grandfather Clocks.
      List<DPoint> clockLocations = new List<DPoint>(metadata.Clocks.Keys);
      for (int i = 0; i < clockLocations.Count; i++) {
        Tile tile = Main.tile[clockLocations[i].X, clockLocations[i].Y];
        if (!tile.active || tile.type != Terraria.TileId_GrandfatherClock)
          clockLocations.RemoveAt(i--);
      }

      // Invalidate active Swappers.
      for (int i = 0; i < metadata.ActiveSwapperLocations.Count; i++) {
        Tile tile = Main.tile[metadata.ActiveSwapperLocations[i].X, metadata.ActiveSwapperLocations[i].Y];
        if (!tile.active || tile.type != AdvancedCircuits.TileId_Swapper)
          metadata.ActiveSwapperLocations.RemoveAt(i--);
      }

      return metadata;
    }
    #endregion

    #region [Methods: HandleTilePlacing, HandleTileDestroying]
    public void HandleTilePlacing(TSPlayer player, int blockId, int x, int y, int tileStyle) {
      switch (blockId) {
        case Terraria.TileId_GrandfatherClock:
          this.Metadata.Clocks.Add(new DPoint(x, y - 4), new GrandfatherClockMetadata(player.Name));
          break;
      }
    }

    public void HandleTileDestroying(TSPlayer player, int x, int y) {
      if (!Terraria.Tiles[x, y].active)
        return;

      switch (Terraria.Tiles[x, y].type) {
        case Terraria.TileId_XSecondTimer: {
          DPoint location = new DPoint(x, y);
          if (this.Metadata.ActiveTimers.ContainsKey(location))
            this.Metadata.ActiveTimers.Remove(location);

          break;
        }
        case Terraria.TileId_GrandfatherClock: {
          DPoint location = new DPoint(x, y);

          Terraria.SpriteMeasureData measureData = Terraria.MeasureSprite(location);
          if (this.Metadata.Clocks.ContainsKey(measureData.OriginTileLocation))
            this.Metadata.Clocks.Remove(measureData.OriginTileLocation);

          break;
        }
        case AdvancedCircuits.TileId_Swapper: {
          DPoint location = new DPoint(x, y);
          if (this.Metadata.ActiveSwapperLocations.Contains(location))
            this.Metadata.ActiveSwapperLocations.Remove(location);

          break;
        }
        case AdvancedCircuits.TileId_ANDGate:
        case AdvancedCircuits.TileId_ORGate:
        case AdvancedCircuits.TileId_XORGate: {
          DPoint location = new DPoint(x, y);
          if (this.Metadata.GateStates.ContainsKey(location))
            this.Metadata.GateStates.Remove(location);

          break;
        }
        case AdvancedCircuits.TileId_BlockActivator: {
          DPoint location = new DPoint(x, y);
          if (this.Metadata.BlockActivators.ContainsKey(location))
            this.Metadata.BlockActivators.Remove(location);

          break;
        }
      }
    }
    #endregion
  }
}
