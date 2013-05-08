using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using DPoint = System.Drawing.Point;

using TShockAPI;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class WorldMetadataHandler: WorldMetadataHandlerBase {
    #region [Property: Metadata]
    public new WorldMetadata Metadata {
      get { return (WorldMetadata)base.Metadata; }
    }
    #endregion


    #region [Methods: Constructor, InitMetadata, ReadMetadataFromFile]
    public WorldMetadataHandler(PluginTrace pluginTrace, string metadataDirectoryPath): base(pluginTrace, metadataDirectoryPath) {}

    protected override IMetadataFile InitMetadata() {
      WorldMetadata metadata = new WorldMetadata();

      List<DPoint> ignoredTiles = new List<DPoint>();
      for (int x = 0; x < Main.maxTilesX - 1; x++) {
        for (int y = 0; y < Main.maxTilesY - 1; y++) {
          if (!TerrariaUtils.Tiles[x, y].active)
            continue;

          DPoint tileLocation = new DPoint(x, y);
          if (ignoredTiles.Contains(tileLocation))
            continue;

          switch ((BlockType)TerrariaUtils.Tiles[x, y].type) {
            case BlockType.XSecondTimer: {
              // Is active timer?
              if (TerrariaUtils.Tiles[x, y].frameY > 0)
                metadata.ActiveTimers.Add(tileLocation, new ActiveTimerMetadata());

              ignoredTiles.Add(tileLocation);

              break;
            }
            case BlockType.GrandfatherClock: {
              ObjectMeasureData measureData = TerrariaUtils.Tiles.MeasureObject(tileLocation);

              metadata.Clocks.Add(measureData.OriginTileLocation, new GrandfatherClockMetadata(null));
              for (int sx = 0; sx < measureData.Size.X; sx++)
                for (int sy = 0; sy < measureData.Size.Y; sy++)
                  ignoredTiles.Add(new DPoint(x + sx, y + sy));

              break;
            }
          }
        }
      }

      return metadata;
    }

    protected override IMetadataFile ReadMetadataFromFile(string filePath) {
      WorldMetadata metadata = WorldMetadata.Read(filePath);

      // Invalidate Gates if necessary.
      List<DPoint> gateLocations = new List<DPoint>(metadata.GateStates.Keys);
      foreach (DPoint gateLocation in gateLocations) {
        Tile tile = TerrariaUtils.Tiles[gateLocation];
        if (
          !tile.active || (
            tile.type != (int)AdvancedCircuits.BlockType_ANDGate && 
            tile.type != (int)AdvancedCircuits.BlockType_ORGate && 
            tile.type != (int)AdvancedCircuits.BlockType_XORGate)
        ) {
          metadata.GateStates.Remove(gateLocation);
        }
      }

      // Invalidate active Timers.
      List<DPoint> activeTimerLocations = new List<DPoint>(metadata.ActiveTimers.Keys);
      foreach (DPoint activeTimerLocation in activeTimerLocations) {
        Tile tile = TerrariaUtils.Tiles[activeTimerLocation];
        if (!tile.active || tile.type != (int)BlockType.XSecondTimer)
          metadata.ActiveTimers.Remove(activeTimerLocation);
      }

      // Invalidate Grandfather Clocks.
      List<DPoint> clockLocations = new List<DPoint>(metadata.Clocks.Keys);
      for (int i = 0; i < clockLocations.Count; i++) {
        Tile tile = TerrariaUtils.Tiles[clockLocations[i]];
        if (!tile.active || tile.type != (int)BlockType.GrandfatherClock)
          clockLocations.RemoveAt(i--);
      }

      // Invalidate active Swappers.
      for (int i = 0; i < metadata.ActiveSwapperLocations.Count; i++) {
        Tile tile = TerrariaUtils.Tiles[metadata.ActiveSwapperLocations[i]];
        if (!tile.active || tile.type != (int)AdvancedCircuits.BlockType_Swapper)
          metadata.ActiveSwapperLocations.RemoveAt(i--);
      }

      // Invalidate Wireless Transmitters.
      List<DPoint> wirelessTransmitterLocations = new List<DPoint>(metadata.WirelessTransmitters.Keys);
      foreach (DPoint transmitterLocation in wirelessTransmitterLocations) {
        Tile tile = TerrariaUtils.Tiles[transmitterLocation];
        if (!tile.active || tile.type != (int)AdvancedCircuits.BlockType_WirelessTransmitter)
          metadata.WirelessTransmitters.Remove(transmitterLocation);
      }

      return metadata;
    }
    #endregion

    #region [Method: HandleTileEdit]
    public bool HandleTileEdit(TSPlayer player, TileEditType editType, BlockType blockType, DPoint location, int objectStyle) {
      switch (editType) {
        case TileEditType.PlaceTile: {
          switch (blockType) {
            case BlockType.GrandfatherClock:
              this.Metadata.Clocks.Add(new DPoint(location.X, location.Y - 4), new GrandfatherClockMetadata(player.Name));
              break;
            case AdvancedCircuits.BlockType_WirelessTransmitter:
              if (
                AdvancedCircuits.IsComponentWiredByPort(location, new DPoint(1, 1)) &&
                !this.Metadata.WirelessTransmitters.ContainsKey(location)
              )
                this.Metadata.WirelessTransmitters.Add(location, player.Name);

              break;
          }

          break;
        }
        case TileEditType.PlaceWire: {
          // Check if we just wired an unregistered component's port.
          foreach (DPoint adjacentTileLocation in AdvancedCircuits.EnumerateComponentPortLocations(location, new DPoint(1, 1))) {
            Tile tile = TerrariaUtils.Tiles[adjacentTileLocation];
            if (tile.active && tile.type == (int)AdvancedCircuits.BlockType_WirelessTransmitter) {
              if (!this.Metadata.WirelessTransmitters.ContainsKey(adjacentTileLocation))
                this.Metadata.WirelessTransmitters.Add(adjacentTileLocation, player.Name);
            }
          }

          break;
        }
        case TileEditType.TileKill:
        case TileEditType.TileKillNoItem: {
          if (!TerrariaUtils.Tiles[location].active)
            break;

          switch ((BlockType)TerrariaUtils.Tiles[location].type) {
            case BlockType.XSecondTimer: {
              if (this.Metadata.ActiveTimers.ContainsKey(location))
                this.Metadata.ActiveTimers.Remove(location);

              break;
            }
            case BlockType.GrandfatherClock: {
              ObjectMeasureData measureData = TerrariaUtils.Tiles.MeasureObject(location);
              if (this.Metadata.Clocks.ContainsKey(measureData.OriginTileLocation))
                this.Metadata.Clocks.Remove(measureData.OriginTileLocation);

              break;
            }
            case AdvancedCircuits.BlockType_Swapper: {
              if (this.Metadata.ActiveSwapperLocations.Contains(location))
                this.Metadata.ActiveSwapperLocations.Remove(location);

              break;
            }
            case AdvancedCircuits.BlockType_ANDGate:
            case AdvancedCircuits.BlockType_ORGate:
            case AdvancedCircuits.BlockType_XORGate: {
              if (this.Metadata.GateStates.ContainsKey(location))
                this.Metadata.GateStates.Remove(location);

              break;
            }
            case AdvancedCircuits.BlockType_BlockActivator: {
              if (this.Metadata.BlockActivators.ContainsKey(location))
                this.Metadata.BlockActivators.Remove(location);

              break;
            }
            case AdvancedCircuits.BlockType_WirelessTransmitter: {
              if (this.Metadata.WirelessTransmitters.ContainsKey(location))
                this.Metadata.WirelessTransmitters.Remove(location);

              break;
            }
          }

          break;
        }
      }

      return false;
    }
    #endregion
  }
}
