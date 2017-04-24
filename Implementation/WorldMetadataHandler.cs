using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Terraria.ID;
using DPoint = System.Drawing.Point;

using TShockAPI;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class WorldMetadataHandler: WorldMetadataHandlerBase {
    public new WorldMetadata Metadata {
      get { return (WorldMetadata)base.Metadata; }
    }


    public WorldMetadataHandler(PluginTrace pluginTrace, string metadataDirectoryPath): base(pluginTrace, metadataDirectoryPath) {}

    protected override IMetadataFile InitMetadata() {
      WorldMetadata metadata = new WorldMetadata();

      this.PluginTrace.WriteLineInfo("Starting one time metadata initialization...");
      for (int x = 0; x < Main.maxTilesX - 1; x++) {
        for (int y = 0; y < Main.maxTilesY - 1; y++) {
          if (!TerrariaUtils.Tiles[x, y].active())
            continue;

          DPoint tileLocation = new DPoint(x, y);
          switch (TerrariaUtils.Tiles[x, y].type) {
            case TileID.Timers: {
              // Is active timer?
              if (TerrariaUtils.Tiles[x, y].frameY > 0)
                if (!metadata.ActiveTimers.ContainsKey(tileLocation))
                  metadata.ActiveTimers.Add(tileLocation, new ActiveTimerMetadata());

              break;
            }
            case TileID.GrandfatherClocks: {
              ObjectMeasureData measureData = TerrariaUtils.Tiles.MeasureObject(tileLocation);

              if (!metadata.Clocks.ContainsKey(measureData.OriginTileLocation))
                metadata.Clocks.Add(measureData.OriginTileLocation, new GrandfatherClockMetadata(null));

              break;
            }
            case AdvancedCircuits.BlockType_WirelessTransmitter: {
              foreach (DPoint portLocation in AdvancedCircuits.EnumerateComponentPortLocations(tileLocation, new DPoint(1, 1)))
                if (TerrariaUtils.Tiles[portLocation].HasWire() && !metadata.WirelessTransmitters.ContainsKey(tileLocation))
                  metadata.WirelessTransmitters.Add(tileLocation, "{Server}");

              break;
            }
          }
        }
      }
      this.PluginTrace.WriteLineInfo("Metadata initialization complete.");

      return metadata;
    }

    protected override IMetadataFile ReadMetadataFromFile(string filePath) {
      WorldMetadata metadata = WorldMetadata.Read(filePath);

      // Invalidate Gates
      List<DPoint> gateLocations = new List<DPoint>(metadata.GateStates.Keys);
      foreach (DPoint gateLocation in gateLocations) {
        Tile tile = TerrariaUtils.Tiles[gateLocation];
        if (
          !tile.active() || (
            tile.type != (int)AdvancedCircuits.BlockType_ANDGate && 
            tile.type != (int)AdvancedCircuits.BlockType_ORGate && 
            tile.type != (int)AdvancedCircuits.BlockType_XORGate)
        ) {
          metadata.GateStates.Remove(gateLocation);
        }
      }

      // Invalidate Active Timers
      List<DPoint> activeTimerLocations = new List<DPoint>(metadata.ActiveTimers.Keys);
      foreach (DPoint activeTimerLocation in activeTimerLocations) {
        Tile tile = TerrariaUtils.Tiles[activeTimerLocation];
        if (!tile.active() || tile.type != TileID.Timers)
          metadata.ActiveTimers.Remove(activeTimerLocation);
      }

      // Invalidate Grandfather Clocks
      List<DPoint> clockLocations = new List<DPoint>(metadata.Clocks.Keys);
      for (int i = 0; i < clockLocations.Count; i++) {
        Tile tile = TerrariaUtils.Tiles[clockLocations[i]];
        if (!tile.active() || tile.type != TileID.GrandfatherClocks)
          clockLocations.RemoveAt(i--);
      }

      // Invalidate Swappers
      List<DPoint> swapperLocations = new List<DPoint>(metadata.Swappers.Keys);
      foreach (DPoint swapperLocation in swapperLocations) {
        Tile tile = TerrariaUtils.Tiles[swapperLocation];
        if (!tile.active() || tile.type != (int)AdvancedCircuits.BlockType_Swapper)
          metadata.Swappers.Remove(swapperLocation);
      }

      // Invalidate Wireless Transmitters
      List<DPoint> wirelessTransmitterLocations = new List<DPoint>(metadata.WirelessTransmitters.Keys);
      foreach (DPoint transmitterLocation in wirelessTransmitterLocations) {
        Tile tile = TerrariaUtils.Tiles[transmitterLocation];
        if (!tile.active() || tile.type != (int)AdvancedCircuits.BlockType_WirelessTransmitter)
          metadata.WirelessTransmitters.Remove(transmitterLocation);
      }

      return metadata;
    }

    #region [Hook Handlers]
    public bool HandleTileEdit(TSPlayer player, TileEditType editType, int blockType, DPoint location, int objectStyle) {
      switch (editType) {
        case TileEditType.PlaceTile: {
          switch (blockType) {
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
        case TileEditType.PlaceWire:
        case TileEditType.PlaceWireBlue:
        case TileEditType.PlaceWireGreen: {
          // Check if we just wired an unregistered component's port.
          foreach (DPoint adjacentTileLocation in AdvancedCircuits.EnumerateComponentPortLocations(location, new DPoint(1, 1))) {
            Tile tile = TerrariaUtils.Tiles[adjacentTileLocation];
            if (tile.active() && tile.type == AdvancedCircuits.BlockType_WirelessTransmitter) {
              if (!this.Metadata.WirelessTransmitters.ContainsKey(adjacentTileLocation))
                this.Metadata.WirelessTransmitters.Add(adjacentTileLocation, player.Name);
            }
          }

          break;
        }
        case TileEditType.TileKill:
        case TileEditType.TileKillNoItem: {
          if (!TerrariaUtils.Tiles[location].active())
            break;

          switch (TerrariaUtils.Tiles[location].type) {
            case TileID.Timers: {
              if (this.Metadata.ActiveTimers.ContainsKey(location))
                this.Metadata.ActiveTimers.Remove(location);

              break;
            }
            case TileID.GrandfatherClocks: {
              ObjectMeasureData measureData = TerrariaUtils.Tiles.MeasureObject(location);
              if (this.Metadata.Clocks.ContainsKey(measureData.OriginTileLocation))
                this.Metadata.Clocks.Remove(measureData.OriginTileLocation);

              break;
            }
            case AdvancedCircuits.BlockType_Swapper: {
              if (this.Metadata.Swappers.ContainsKey(location))
                this.Metadata.Swappers.Remove(location);

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

    public bool HandleObjectPlacement(TSPlayer player, int blockType, DPoint location, int objectStyle) {
      switch (blockType) {
        case TileID.GrandfatherClocks:
          this.Metadata.Clocks.Add(new DPoint(location.X, location.Y - 4), new GrandfatherClockMetadata(player.Name));
          break;
      }

      return false;
    }
    #endregion
  }
}
