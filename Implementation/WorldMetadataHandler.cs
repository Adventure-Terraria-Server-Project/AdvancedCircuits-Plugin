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


    #region [Methods: Constructor, InitMetadata]
    public WorldMetadataHandler(string metadataDirectoryPath): base(AdvancedCircuitsPlugin.Trace, metadataDirectoryPath) {}

    protected override IWorldMetadata InitMetadata() {
      WorldMetadata metadata = new WorldMetadata();

      List<DPoint> ignoredTiles = new List<DPoint>();
      for (int x = 0; x < Main.maxTilesX; x++) {
        for (int y = 0; y < Main.maxTilesY; y++) {
          if (!Main.tile[x, y].active)
            continue;

          DPoint tileLocation = new DPoint(x, y);
          if (ignoredTiles.Contains(tileLocation))
            continue;

          switch (Main.tile[x, y].type) {
            case Terraria.TileId_XSecondTimer: {
              // Is active timer?
              if (Main.tile[x, y].frameY > 0)
                metadata.ActiveTimers.Add(tileLocation, new ActiveTimerMetadata());

              ignoredTiles.Add(tileLocation);

              break;
            }
            case Terraria.TileId_GrandfatherClock: {
              Terraria.SpriteMeasureData? measureData;
              if (!Terraria.MeasureSprite(tileLocation, out measureData) || measureData == null)
                continue;

              metadata.ClockLocations.Add(measureData.Value.OriginTileLocation);
              for (int sx = 0; sx < measureData.Value.Size.X; sx++) {
                for (int sy = 0; sy < measureData.Value.Size.Y; sy++) {
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
      return WorldMetadata.Read(filePath);
    }
    #endregion

    #region [Methods: HandleTilePlacing, HandleTileDestroying]
    public void HandleTilePlacing(TSPlayer player, int tileId, int x, int y, int tileStyle) {
      switch (tileId) {
        case Terraria.TileId_GrandfatherClock:
          this.Metadata.ClockLocations.Add(new DPoint(x, y - 4));
          break;
      }
    }

    public void HandleTileDestroying(TSPlayer player, int x, int y) {
      switch (Main.tile[x, y].type) {
        case Terraria.TileId_XSecondTimer: {
          DPoint location = new DPoint(x, y);
          if (this.Metadata.ActiveTimers.ContainsKey(location))
            this.Metadata.ActiveTimers.Remove(location);

          break;
        }
        case Terraria.TileId_GrandfatherClock: {
          DPoint location = new DPoint(x, y);
          Terraria.SpriteMeasureData? measureData;
          if (!Terraria.MeasureSprite(location, out measureData) || measureData == null)
            return;

          int clockIndex = this.Metadata.ClockLocations.IndexOf(measureData.Value.OriginTileLocation);
          if (clockIndex != -1)
            this.Metadata.ClockLocations.RemoveAt(clockIndex);

          break;
        }
      }
    }
    #endregion
  }
}
