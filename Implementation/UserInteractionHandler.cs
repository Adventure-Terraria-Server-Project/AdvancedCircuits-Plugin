﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OTAPI.Tile;
using Terraria.ID;
using DPoint = System.Drawing.Point;

using TShockAPI;

using Terraria.Plugins.Common;
using Terraria.Plugins.Common.Hooks;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class UserInteractionHandler: UserInteractionHandlerBase, IDisposable {
    protected PluginInfo PluginInfo { get; private set; }
    protected Configuration Config { get; private set; }
    public WorldMetadata WorldMetadata { get; private set; }
    public PluginCooperationHandler PluginCooperationHandler { get; private set; }
    protected Action ReloadConfigurationCallback { get; private set; }


    public UserInteractionHandler(
      PluginTrace pluginTrace, PluginInfo pluginInfo, Configuration config, 
      WorldMetadata worldMetadata, PluginCooperationHandler pluginCooperationHandler, Action reloadConfigurationCallback
    ): base(pluginTrace) {
      if (pluginTrace == null) throw new ArgumentNullException();
      if (config == null) throw new ArgumentNullException();
      if (worldMetadata == null) throw new ArgumentNullException();
      if (pluginCooperationHandler == null) throw new ArgumentNullException();
      if (reloadConfigurationCallback == null) throw new ArgumentNullException();

      this.PluginInfo = pluginInfo;
      this.Config = config;
      this.WorldMetadata = worldMetadata;
      this.PluginCooperationHandler = pluginCooperationHandler;
      this.ReloadConfigurationCallback = reloadConfigurationCallback;

      this.RegisterCommand(new[] { "advancedcircuits", "ac" }, this.RootCommand_Exec);
    }

    #region [Command Handling /advancedcircuits]
    private void RootCommand_Exec(CommandArgs args) {
      if (args == null || this.IsDisposed)
        return;
      
      if (args.Parameters.Count >= 1) {
        string subCommand = args.Parameters[0].ToLowerInvariant();

        if (this.TryExecuteSubCommand(subCommand, args))
          return;
      }

      args.Player.SendMessage(this.PluginInfo.ToString(), Color.White);
      args.Player.SendMessage(this.PluginInfo.Description, Color.White);
      args.Player.SendMessage(string.Empty, Color.Yellow);
      args.Player.SendMessage("Write \"/ac commands\" to see a list of sub-commands.", Color.Yellow);
      args.Player.SendMessage("For help and support refer to the TShock forums.", Color.Yellow);
    }

    private bool TryExecuteSubCommand(string commandNameLC, CommandArgs args) {
      switch (commandNameLC) {
        case "commands":
        case "cmds":
          args.Player.SendMessage("Available Sub-Commands:", Color.White);
          args.Player.SendMessage("/ac blocks", Color.Yellow);
          args.Player.SendMessage("/ac toggle|switch", Color.Yellow);

          if (args.Player.Group.HasPermission(AdvancedCircuitsPlugin.ReloadCfg_Permission))
            args.Player.SendMessage("/ac reloadcfg", Color.Yellow);

          return true;
        case "reloadcfg":
          if (args.Player.Group.HasPermission(AdvancedCircuitsPlugin.ReloadCfg_Permission)) {
            this.PluginTrace.WriteLineInfo("Reloading configuration file.");
            try {
              this.ReloadConfigurationCallback();
              this.PluginTrace.WriteLineInfo("Configuration file successfully reloaded.");

              if (args.Player != TSPlayer.Server)
                args.Player.SendMessage("Configuration file successfully reloaded.", Color.Yellow);
            } catch (Exception ex) {
              this.PluginTrace.WriteLineError(
                "Reloading the configuration file failed. Keeping old configuration. Exception details:\n{0}", ex
              );
            }
          } else {
            args.Player.SendErrorMessage("You do not have the necessary permission to do that.");
          }

          return true;
        case "blocks":
        case "ores":
        case "tiles":
          int pageNumber;
          if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
            return true;

          PaginationTools.SendPage(
            args.Player, pageNumber, 
            new List<string>() {
              "Copper Ore - OR-Gate",
              "Silver Ore - AND-Gate",
              "Gold Ore - XOR-Gate / XOR-Port",
              "Obsidian - NOT-Gate / NOT-Port",
              "Iron Ore - Swapper",
              "Spike - Crossover Bridge",
              "Glass - Input Port",
              "Active Stone - Active Stone and Block Activator",
              "Adamantite Ore - Wireless Transmitter"
            },
            new PaginationTools.Settings {
              HeaderFormat = "Advanced Circuits Special Blocks (Page {0} of {1})",
              HeaderTextColor = Color.Lime,
              LineTextColor = Color.LightGray,
              MaxLinesPerPage = 4,
            }
          );

          return true;
        case "toggle":
        case "switch":
          args.Player.SendInfoMessage("Place or destroy a wire on the component you want to toggle.");

          if (args.Parameters.Count > 3) {
            args.Player.SendErrorMessage("Proper syntax: /ac switch [state] [+p]");
            args.Player.SendInfoMessage("Type /ac switch help to get more help to this command.");
            return true;
          }

          bool persistentMode = false;
          bool? newState = null;
          if (args.Parameters.Count > 1) {
            int newStateRaw;
            if (int.TryParse(args.Parameters[1], out newStateRaw))
              newState = (newStateRaw == 1);

            persistentMode = args.ContainsParameter("+p", StringComparison.InvariantCultureIgnoreCase);
          }

          CommandInteraction interaction = this.StartOrResetCommandInteraction(args.Player);
          interaction.DoesNeverComplete = persistentMode;
          interaction.TileEditCallback = (player, editType, blockType, location, blockStyle) => {
            if (
              editType != TileEditType.PlaceTile || 
              editType != TileEditType.PlaceWall || 
              editType != TileEditType.DestroyWall || 
              editType != TileEditType.PlaceActuator
            ) {
              CommandInteractionResult result = new CommandInteractionResult { IsHandled = true, IsInteractionCompleted = true };
              ITile tile = TerrariaUtils.Tiles[location];

              if (
                !args.Player.HasBuildPermission(location.X, location.Y) || (
                this.PluginCooperationHandler.IsProtectorAvailable &&
                this.PluginCooperationHandler.Protector_CheckProtected(args.Player, location, false)
              )) {
                player.SendErrorMessage("This object is protected.");
                player.SendTileSquare(location, 1);
                return result;
              }

              int hitBlockType = tile.type;
              if (tile.active() && hitBlockType == TileID.ActiveStoneBlock) {
                if (newState == null || newState == false)
                  TerrariaUtils.Tiles.SetBlock(location, TileID.InactiveStoneBlock);
                else
                  args.Player.SendTileSquare(location);
              } else if (hitBlockType == TileID.InactiveStoneBlock) {
                if (tile.active() &&  newState == null || newState == true)
                  TerrariaUtils.Tiles.SetBlock(location, TileID.ActiveStoneBlock);
                else
                  args.Player.SendTileSquare(location);
              } else if (tile.active() && TerrariaUtils.Tiles.IsMultistateObject(hitBlockType)) {
                ObjectMeasureData measureData = TerrariaUtils.Tiles.MeasureObject(location);
                bool currentState = TerrariaUtils.Tiles.ObjectHasActiveState(measureData);
                if (newState == null)
                  newState = !TerrariaUtils.Tiles.ObjectHasActiveState(measureData);

                if (currentState != newState.Value)
                  TerrariaUtils.Tiles.SetObjectState(measureData, newState.Value);
                else
                  args.Player.SendTileSquare(location);
              } else if (
                hitBlockType == AdvancedCircuits.BlockType_ORGate ||
                hitBlockType == AdvancedCircuits.BlockType_ANDGate ||
                hitBlockType == AdvancedCircuits.BlockType_XORGate
              ) {
                if (
                  !args.Player.HasBuildPermission(location.X, location.Y) || (
                  this.PluginCooperationHandler.IsProtectorAvailable &&
                  this.PluginCooperationHandler.Protector_CheckProtected(args.Player, location, false)
                )) {
                  player.SendErrorMessage("This gate is protected.");
                  player.SendTileSquare(location);
                  return result;
                }

                PaintColor paint = (PaintColor)TerrariaUtils.Tiles[location].color();
                if (paint == AdvancedCircuits.Paint_Gate_TemporaryState) {
                  player.SendErrorMessage("The gate is painted {0}, there's no point in initializing it.", AdvancedCircuits.Paint_Gate_TemporaryState);
                  args.Player.SendTileSquare(location);
                  return result;
                }

                GateStateMetadata gateState;
                if (!this.WorldMetadata.GateStates.TryGetValue(location, out gateState)) {
                  gateState = new GateStateMetadata();
                  this.WorldMetadata.GateStates.Add(location, gateState);
                }

                List<DPoint> gatePortLocations = new List<DPoint>(AdvancedCircuits.EnumerateComponentPortLocations(location, new DPoint(1, 1)));
                for (int i = 0; i < 4; i++) {
                  ITile gatePort = TerrariaUtils.Tiles[gatePortLocations[i]];
                  if (!gatePort.active() || gatePort.type != (int)AdvancedCircuits.BlockType_InputPort)
                    continue;

                  if (newState == null) {
                    if (gateState.PortStates[i] == null)
                      gateState.PortStates[i] = true;
                    else
                      gateState.PortStates[i] = !gateState.PortStates[i];
                  } else {
                    gateState.PortStates[i] = newState.Value;
                  }
                }

                player.SendSuccessMessage("The states of this gate's ports are now:");
                this.SendGatePortStatesInfo(args.Player, gateState);
                args.Player.SendTileSquare(location);
              } else if (tile.active() && tile.type == (int)AdvancedCircuits.BlockType_InputPort) {
                foreach (DPoint adjacentTileLocation in AdvancedCircuits.EnumerateComponentPortLocations(location, new DPoint(1, 1))) {
                  ITile adjacentTile = TerrariaUtils.Tiles[adjacentTileLocation];
                  if (!adjacentTile.active() || !AdvancedCircuits.IsLogicalGate(adjacentTile.type))
                    continue;

                  if (
                    !args.Player.HasBuildPermission(adjacentTileLocation.X, adjacentTileLocation.Y) || (
                    this.PluginCooperationHandler.IsProtectorAvailable &&
                    this.PluginCooperationHandler.Protector_CheckProtected(args.Player, adjacentTileLocation, false)
                  )) {
                    player.SendErrorMessage("This gate is protected.");
                    player.SendTileSquare(location);
                    return result;
                  }

                  PaintColor paint = (PaintColor)TerrariaUtils.Tiles[location].color();
                  if (paint == AdvancedCircuits.Paint_Gate_TemporaryState) {
                    player.SendErrorMessage("The gate is painted {0}, there's no point in initializing it.", AdvancedCircuits.Paint_Gate_TemporaryState);
                    args.Player.SendTileSquare(location);
                    return result;
                  }

                  GateStateMetadata gateState;
                  if (!this.WorldMetadata.GateStates.TryGetValue(adjacentTileLocation, out gateState)) {
                    gateState = new GateStateMetadata();
                    this.WorldMetadata.GateStates.Add(adjacentTileLocation, gateState);
                  }

                  int portIndex;
                  switch (AdvancedCircuits.DirectionFromTileLocations(adjacentTileLocation, location)) {
                    case Direction.Up:
                      portIndex = 0;
                      break;
                    case Direction.Down:
                      portIndex = 1;
                      break;
                    case Direction.Left:
                      portIndex = 2;
                      break;
                    case Direction.Right:
                      portIndex = 3;
                      break;
                    default:
                      return result;
                  }

                  if (newState == null) {
                    if (gateState.PortStates[portIndex] == null)
                      gateState.PortStates[portIndex] = true;
                    else
                      gateState.PortStates[portIndex] = !gateState.PortStates[portIndex];
                  } else {
                    gateState.PortStates[portIndex] = newState.Value;
                  }

                  player.SendSuccessMessage("The states of this gate's ports are now:");
                  this.SendGatePortStatesInfo(args.Player, gateState);
                  args.Player.SendTileSquare(location);
                  return result;
                }

                player.SendErrorMessage($"The state of \"{TerrariaUtils.Tiles.GetBlockTypeName(hitBlockType, 0)}\" can not be changed.");
                player.SendTileSquare(location);
              }

              return result;
            }

            return new CommandInteractionResult { IsHandled = false, IsInteractionCompleted = false };
          };
          interaction.TimeExpiredCallback = (player) => {
            player.SendErrorMessage("Waited too long, no component will be toggled.");
          };

          args.Player.SendSuccessMessage("Hit an object to change its state.");
          return true;
      }

      return false;
    }
    #endregion

    #region [Hook Handling]
    public override bool HandleTileEdit(TSPlayer player, TileEditType editType, int blockType, DPoint location, int objectStyle) {
      if (this.IsDisposed)
        return false;
      if (base.HandleTileEdit(player, editType, blockType, location, objectStyle))
        return true;

      if (editType == TileEditType.PlaceTile)
        return this.HandleTilePlace(player, blockType, location, objectStyle);
      if (editType == TileEditType.TileKill || editType == TileEditType.TileKillNoItem)
        return this.HandleTileDestruction(player, location);
      if (editType == TileEditType.PlaceWire || editType == TileEditType.PlaceWireBlue || editType == TileEditType.PlaceWireGreen || editType == TileEditType.PlaceWireYellow)
        return this.HandleWirePlace(player, location);

      #if DEBUG || Testrun
      if (editType == TileEditType.DestroyWire) {
        player.SendMessage(location.ToString(), Color.Aqua);

        if (!TerrariaUtils.Tiles[location].active())
          return false;

        ObjectMeasureData measureData = TerrariaUtils.Tiles.MeasureObject(location);
        player.SendInfoMessage(string.Format(
          "X: {0}, Y: {1}, FrameX: {2}, FrameY: {3}, Origin X: {4}, Origin Y: {5}, Active State: {6} Paint:{7}", 
          location.X, location.Y, TerrariaUtils.Tiles[location].frameX, TerrariaUtils.Tiles[location].frameY,
          measureData.OriginTileLocation.X, measureData.OriginTileLocation.Y, 
          TerrariaUtils.Tiles.ObjectHasActiveState(measureData), TerrariaUtils.Tiles[location].color()
        ));
      }
      #endif

      return false;
    }

    public bool HandleTilePaint(TSPlayer player, DPoint location, PaintColor paint) {
      if (this.IsDisposed)
        return false;
      
      ITile componentTile = TerrariaUtils.Tiles[location];
      int objectStyle;
      if (componentTile.type == TileID.Statues)
        objectStyle = componentTile.frameX / 36;
      else if (componentTile.type == TileID.Traps)
        objectStyle = componentTile.frameY / 18;
      else
        objectStyle = 0;

      bool hasPermission = this.CheckTilePermission(player, location, componentTile.type, objectStyle, paint);
      return !hasPermission;
    }

    private bool CheckTilePermission(TSPlayer player, DPoint location, int blockType, int objectStyle, PaintColor paint, bool dropItem = false) {
      switch (blockType) {
        case TileID.Statues: {
          DPoint originTileLocation = new DPoint(location.X - 1, location.Y - 2);
          if (!TerrariaUtils.Tiles.IsObjectWired(originTileLocation, new DPoint(2, 3)))
            break;
          StatueStyle statueStyle = TerrariaUtils.Tiles.GetStatueStyle(objectStyle);
          StatueConfig statueConfig;
          if (!this.Config.StatueConfigs.TryGetValue(statueStyle, out statueConfig) || statueConfig == null)
            break;

          if (!player.Group.HasPermission(statueConfig.WirePermission)) {
            player.SendTileSquareEx(location, 10);

            if (dropItem) {
              int itemTypeToDrop;
              itemTypeToDrop = TerrariaUtils.Tiles.GetItemTypeFromStatueStyle(statueStyle);

              Item.NewItem(location.X * TerrariaUtils.TileSize, location.Y * TerrariaUtils.TileSize, 0, 0, itemTypeToDrop);
            }

            this.TellNoStatueWiringPermission(player, statueStyle);
            return false;
          }

          break;
        }
        case TileID.Traps: {
          ITile destTile = TerrariaUtils.Tiles[location];
          if (!destTile.HasWire())
            break;
          TrapConfig trapConfig;
          TrapStyle trapStyle = TerrariaUtils.Tiles.GetTrapStyle(destTile.frameY / 18);

          TrapConfigKey configKey = new TrapConfigKey(trapStyle, paint);
          if (!this.Config.TrapConfigs.TryGetValue(configKey, out trapConfig))
            break;

          if (!player.Group.HasPermission(trapConfig.WirePermission)) {
            player.SendTileSquareEx(location, 10);

            if (dropItem) {
              int itemTypeToDrop = TerrariaUtils.Tiles.GetItemTypeFromTrapStyle(trapStyle);
              Item.NewItem(location.X * TerrariaUtils.TileSize, location.Y * TerrariaUtils.TileSize, 0, 0, itemTypeToDrop);
            }

            this.TellMissingComponentWiringPermission(player, blockType);
            return false;
          }

          break;
        }
        case TileID.Boulder: {
          DPoint originTileLocation = new DPoint(location.X - 1, location.Y - 1);
          if (!TerrariaUtils.Tiles.IsObjectWired(originTileLocation, new DPoint(2, 2)))
            break;

          if (!player.Group.HasPermission(AdvancedCircuitsPlugin.WireBoulder_Permission)) {
            player.SendTileSquareEx(location, 10);

            if (dropItem)
              Item.NewItem(location.X * TerrariaUtils.TileSize, location.Y * TerrariaUtils.TileSize, 0, 0, ItemID.Boulder);

            this.TellMissingComponentWiringPermission(player, blockType);
            return false;
          }

          break;
        }
        case TileID.Signs: {
          if (!TerrariaUtils.Tiles.IsObjectWired(location, new DPoint(2, 2)))
            break;

          if (!player.Group.HasPermission(AdvancedCircuitsPlugin.WireSign_Permission)) {
            player.SendTileSquareEx(location, 10);

            if (dropItem)
              Item.NewItem(location.X * TerrariaUtils.TileSize, location.Y * TerrariaUtils.TileSize, 0, 0, ItemID.Sign);

            this.TellMissingComponentWiringPermission(player, blockType);
            return false;
          }

          break;
        }
        case TileID.InletPump:
        case TileID.OutletPump: {
          DPoint originTileLocation = new DPoint(location.X - 1, location.Y - 1);
          if (!TerrariaUtils.Tiles.IsObjectWired(originTileLocation, new DPoint(2, 2)))
            break;
          PumpConfig pumpConfig;
          if (!this.Config.PumpConfigs.TryGetValue(paint, out pumpConfig))
            break;
          if (string.IsNullOrEmpty(pumpConfig.WirePermission))
            break;

          if (!player.Group.HasPermission(pumpConfig.WirePermission)) {
            player.SendTileSquareEx(location, 10);

            if (dropItem) {
              int itemTypeToDrop = ItemID.OutletPump;
              if (blockType == ItemID.InletPump)
                itemTypeToDrop = ItemID.InletPump;

              Item.NewItem(location.X * TerrariaUtils.TileSize, location.Y * TerrariaUtils.TileSize, 0, 0, itemTypeToDrop);
            }

            this.TellMissingComponentWiringPermission(player, blockType);
            return false;
          }

          break;
        }
        case AdvancedCircuits.BlockType_WirelessTransmitter: {
          if (!AdvancedCircuits.IsComponentWiredByPort(location, new DPoint(1, 1)))
            break;
          WirelessTransmitterConfig transmitterConfig;
          if (!this.Config.WirelessTransmitterConfigs.TryGetValue(paint, out transmitterConfig))
            break;

          if (!player.Group.HasPermission(transmitterConfig.WirePermission)) {
            player.SendTileSquareEx(location, 1);

            if (dropItem)
              Item.NewItem(location.X * TerrariaUtils.TileSize, location.Y * TerrariaUtils.TileSize, 0, 0, ItemID.AdamantiteOre);

            this.TellMissingComponentWiringPermission(player, blockType);
            return false;
          }

          break;
        }
        case ItemID.Teleporter: {
          DPoint originTileLocation = new DPoint(location.X - 1, location.Y - 1);
          if (!TerrariaUtils.Tiles.IsObjectWired(originTileLocation, new DPoint(3, 1)))
            break;

          if (!player.Group.HasPermission(AdvancedCircuitsPlugin.WireTeleporter_Permission)) {
            player.SendTileSquareEx(location, 10);

            if (dropItem)
              Item.NewItem(location.X * TerrariaUtils.TileSize, location.Y * TerrariaUtils.TileSize, 0, 0, ItemID.Teleporter);

            this.TellMissingComponentWiringPermission(player, blockType);
            return false;
          }

          break;
        }
      }

      return true;
    }

    private bool HandleTilePlace(TSPlayer player, int blockType, DPoint location, int objectStyle) {
      if (this.IsDisposed)
        return false;
      
      bool hasPermission = this.CheckTilePermission(player, location, blockType, objectStyle, (PaintColor)TerrariaUtils.Tiles[location].color(), true);
      return !hasPermission;
    }

    public bool HandleObjectPlacement(TSPlayer player, int blockType, DPoint location, int objectStyle) {
      if (this.IsDisposed)
        return false;

      bool hasPermission = this.CheckTilePermission(player, location, blockType, objectStyle, (PaintColor)TerrariaUtils.Tiles[location].color(), true);
      return !hasPermission;
    }

    private bool HandleTileDestruction(TSPlayer player, DPoint location) {
      return false;
    }

    public bool HandleMassWireOperation(TSPlayer player, MassWireOperationEventArgs e) {
      if (this.IsDisposed)
        return false;

      var wireLocations = TShock.Utils.GetMassWireOperationRange(e.StartLocation.ToXnaPoint(), e.EndLocation.ToXnaPoint(), e.Player.TPlayer.direction == 1);
      foreach (Point wireLocation in wireLocations)
        if (this.HandleWirePlace(player, new DPoint(wireLocation.X, wireLocation.Y)))
          return true;

      return false;
    }

    private bool HandleWirePlace(TSPlayer player, DPoint location) {
      if (this.IsDisposed)
        return false;

      DPoint[] tilesToCheck = new[] {
        location,
        new DPoint(location.X - 1, location.Y),
        new DPoint(location.X + 1, location.Y),
        new DPoint(location.X, location.Y - 1),
        new DPoint(location.X, location.Y + 1),
      };

      foreach (DPoint tileToCheck in tilesToCheck) {
        ITile tile = TerrariaUtils.Tiles[tileToCheck];
        if (!tile.active())
          continue;
        if (tileToCheck != location && tile.type != AdvancedCircuits.BlockType_WirelessTransmitter)
          continue;

        bool hasPermission = true;
        ObjectMeasureData measureData = TerrariaUtils.Tiles.MeasureObject(tileToCheck);
        PaintColor componentPaint = (PaintColor)TerrariaUtils.Tiles[measureData.OriginTileLocation].color();
        switch (tile.type) {
          case TileID.Statues: {
            StatueStyle statueStyle = TerrariaUtils.Tiles.GetStatueStyle(tile);
            StatueConfig statueConfig;
            if (!this.Config.StatueConfigs.TryGetValue(statueStyle, out statueConfig))
              return false;
            if (string.IsNullOrEmpty(statueConfig.WirePermission))
              return false;

            hasPermission = player.Group.HasPermission(statueConfig.WirePermission);
            if (!hasPermission) {
              this.TellNoStatueWiringPermission(player, statueStyle);
              player.SendTileSquare(location, 1);

              return true;
            }
            break;
          }
          case TileID.Traps: {
            TrapConfig trapConfig;
            TrapConfigKey configKey = new TrapConfigKey(TerrariaUtils.Tiles.GetTrapStyle(TerrariaUtils.Tiles[location].frameY / 18), componentPaint);
            if (!this.Config.TrapConfigs.TryGetValue(configKey, out trapConfig))
              break;
            if (string.IsNullOrEmpty(trapConfig.WirePermission))
              break;

            hasPermission = player.Group.HasPermission(trapConfig.WirePermission);
            break;
          }
          case TileID.Boulder: {
            hasPermission = player.Group.HasPermission(AdvancedCircuitsPlugin.WireBoulder_Permission);
            break;
          }
          case TileID.Signs: {
            hasPermission = player.Group.HasPermission(AdvancedCircuitsPlugin.WireSign_Permission);
            break;
          }
          case TileID.InletPump:
          case TileID.OutletPump: {
            PumpConfig pumpConfig;
            if (!this.Config.PumpConfigs.TryGetValue(componentPaint, out pumpConfig))
              break;

            hasPermission = player.Group.HasPermission(pumpConfig.WirePermission);
            break;
          }
          case AdvancedCircuits.BlockType_WirelessTransmitter: {
            WirelessTransmitterConfig transmitterConfig;
            if (!this.Config.WirelessTransmitterConfigs.TryGetValue(componentPaint, out transmitterConfig))
              break;

            hasPermission = player.Group.HasPermission(transmitterConfig.WirePermission);
            break;
          }
          case TileID.Teleporter: {
            hasPermission = player.Group.HasPermission(AdvancedCircuitsPlugin.WireTeleporter_Permission);
            break;
          }
        }

        if (!hasPermission) {
          this.TellMissingComponentWiringPermission(player, tile.type);
          
          player.SendTileSquare(location, 1);
          return true;
        }
      }

      return false;
    }

    private void TellMissingComponentWiringPermission(TSPlayer player, int blockType) {
      string blockName = TerrariaUtils.Tiles.GetBlockTypeName(blockType, 0);
      player.SendErrorMessage("You don't have the required permission to wire up components of type " + blockName);
      
      this.PluginTrace.WriteLineInfo(
        "Player \"{0}\" tried to wire a component of type \"{1}\" but didn't have the necessary permissions to do so.", 
        player.Name, blockName
      );
    }

    private void TellNoStatueWiringPermission(TSPlayer player, StatueStyle statue) {
      player.SendErrorMessage("You don't have the required permission to wire up statues of type ");
      player.SendErrorMessage(string.Concat('"', statue.ToString(), '"'));

      this.PluginTrace.WriteLineInfo(
        "Player \"{0}\" tried to wire a statue of type \"{1}\" but didn't have the necessary permissions to do so.", 
        player.Name, statue.ToString()
      );
    }
    #endregion

    private void SendGatePortStatesInfo(TSPlayer player, GateStateMetadata gateState) {
      player.SendMessage("Top Port: " + this.GatePortStateToString(gateState.PortStates[0]), Color.LightGray);
      player.SendMessage("Left Port: " + this.GatePortStateToString(gateState.PortStates[2]), Color.LightGray);
      player.SendMessage("Bottom Port: " + this.GatePortStateToString(gateState.PortStates[1]), Color.LightGray);
      player.SendMessage("Right Port: " + this.GatePortStateToString(gateState.PortStates[3]), Color.LightGray);
    }

    private string GatePortStateToString(bool? portState) {
      if (portState == null)
        return "not initialized";
      else if (portState.Value)
        return "1";
      else
        return "0";
    }

    #region [IDisposable Implementation]
    protected override void Dispose(bool isDisposing) {
      if (this.IsDisposed)
        return;
    
      if (isDisposing)
        this.ReloadConfigurationCallback = null;
    
      base.Dispose(isDisposing);
    }
    #endregion
  }
}
