using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using DPoint = System.Drawing.Point;

using TShockAPI;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class UserInteractionHandler: UserInteractionHandlerBase, IDisposable {
    #region [Constants]
    public const string ReloadCfg_Permission = "advancedcircuits_reloadcfg";
    #endregion

    #region [Property: PluginTrace]
    private readonly PluginTrace pluginTrace;

    protected PluginTrace PluginTrace {
      get { return this.pluginTrace; }
    }
    #endregion

    #region [Property: PluginInfo]
    private readonly PluginInfo pluginInfo;

    protected PluginInfo PluginInfo {
      get { return this.pluginInfo; }
    }
    #endregion

    #region [Property: Config]
    private readonly Configuration config;

    protected Configuration Config {
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
    private PluginCooperationHandler pluginCooperationHandler;

    public PluginCooperationHandler PluginCooperationHandler {
      get { return this.pluginCooperationHandler; }
    }
    #endregion

    #region [Property: ReloadConfigurationCallback]
    private Action reloadConfigurationCallback;

    protected Action ReloadConfigurationCallback {
      get { return this.reloadConfigurationCallback; }
    }
    #endregion


    #region [Method: Constructor]
    public UserInteractionHandler(
      PluginTrace pluginTrace, PluginInfo pluginInfo, Configuration config, 
      WorldMetadata worldMetadata, PluginCooperationHandler pluginCooperationHandler, Action reloadConfigurationCallback
    ): base(pluginTrace) {
      Contract.Requires<ArgumentNullException>(pluginTrace != null);
      Contract.Requires<ArgumentNullException>(config != null);
      Contract.Requires<ArgumentNullException>(worldMetadata != null);
      Contract.Requires<ArgumentNullException>(pluginCooperationHandler != null);
      Contract.Requires<ArgumentNullException>(reloadConfigurationCallback != null);

      this.pluginTrace = pluginTrace;
      this.pluginInfo = pluginInfo;
      this.config = config;
      this.worldMetadata = worldMetadata;
      this.pluginCooperationHandler = pluginCooperationHandler;
      this.reloadConfigurationCallback = reloadConfigurationCallback;

      this.RegisterCommand(new[] { "advancedcircuits", "ac" }, this.RootCommand_Exec);
    }
    #endregion

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

          if (args.Player.Group.HasPermission(UserInteractionHandler.ReloadCfg_Permission))
            args.Player.SendMessage("/ac reloadcfg", Color.Yellow);

          return true;
        case "reloadcfg":
          if (args.Player.Group.HasPermission(UserInteractionHandler.ReloadCfg_Permission)) {
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
          if (!PaginationUtil.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
            return true;

          PaginationUtil.SendPage(
            args.Player, pageNumber, 
            new List<string>() {
              "Copper Ore - OR-Gate",
              "Silver Ore - AND-Gate",
              "Gold Ore - XOR-Gate",
              "Obsidian - NOT-Gate / NOT-Port",
              "Iron Ore - Swapper",
              "Spike - Crossover Bridge",
              "Glass - Input Port",
              "Cobalt Ore - Modifier",
              "Active Stone - Active Stone and Block Activator",
              "Meteorite - Wireless Transmitter"
            },
            new PaginationUtil.Settings {
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

          PlayerCommandInteraction interaction = this.StartOrResetCommandInteraction(args.Player);
          interaction.DoesNeverComplete = persistentMode;
          interaction.TileEditCallback = (player, editType, blockType, location, blockStyle) => {
            if (
              editType == TileEditType.TileKill || 
              editType == TileEditType.TileKillNoItem || 
              editType == TileEditType.PlaceWire || 
              editType == TileEditType.DestroyWire
            ) {
              CommandInteractionResult result = new CommandInteractionResult { IsHandled = true, IsInteractionCompleted = true };
              Tile tile = TerrariaUtils.Tiles[location];

              if (
                TShock.CheckTilePermission(args.Player, location.X, location.Y) || (
                  this.PluginCooperationHandler.IsProtectorAvailable &&
                  this.PluginCooperationHandler.Protector__CheckProtected(args.Player, location, false)
                )
              ) {
                player.SendErrorMessage("This object is protected.");
                player.SendTileSquare(location, 1);
                return result;
              }

              BlockType hitBlockType = (BlockType)tile.type;
              if (tile.active && hitBlockType == BlockType.ActiveStone) {
                if (newState == null || newState == false)
                  TerrariaUtils.Tiles.SetBlock(location, BlockType.InactiveStone);
                else
                  args.Player.SendTileSquare(location, 1);
              } else if (hitBlockType == BlockType.InactiveStone) {
                if (tile.active &&  newState == null || newState == true)
                  TerrariaUtils.Tiles.SetBlock(location, BlockType.ActiveStone);
                else
                  args.Player.SendTileSquare(location, 1);
              } else if (tile.active && TerrariaUtils.Tiles.IsMultistateObject(hitBlockType)) {
                ObjectMeasureData measureData = TerrariaUtils.Tiles.MeasureObject(location);
                bool currentState = TerrariaUtils.Tiles.ObjectHasActiveState(measureData);
                if (newState == null)
                  newState = !TerrariaUtils.Tiles.ObjectHasActiveState(measureData);

                if (currentState != newState.Value)
                  TerrariaUtils.Tiles.SetObjectState(measureData, newState.Value);
                else
                  args.Player.SendTileSquare(location, 1);
              } else if (
                hitBlockType == AdvancedCircuits.BlockType_ORGate ||
                hitBlockType == AdvancedCircuits.BlockType_ANDGate ||
                hitBlockType == AdvancedCircuits.BlockType_XORGate
              ) {
                if (
                  TShock.CheckTilePermission(args.Player, location.X, location.Y) || (
                    this.PluginCooperationHandler.IsProtectorAvailable &&
                    this.PluginCooperationHandler.Protector__CheckProtected(args.Player, location, false)
                  )
                ) {
                  player.SendErrorMessage("This gate is protected.");
                  player.SendTileSquare(location, 1);
                  return result;
                }

                int modifiers = AdvancedCircuits.CountComponentModifiers(location, new DPoint(1, 1));
                if (modifiers == 1) {
                  player.SendErrorMessage("The gate has one modifier, there's no point in initializing it.");
                  args.Player.SendTileSquare(location, 1);
                  return result;
                }

                GateStateMetadata gateState;
                if (!this.WorldMetadata.GateStates.TryGetValue(location, out gateState)) {
                  gateState = new GateStateMetadata();
                  this.WorldMetadata.GateStates.Add(location, gateState);
                }

                List<DPoint> gatePortLocations = new List<DPoint>(AdvancedCircuits.EnumerateComponentPortLocations(location, new DPoint(1, 1)));
                for (int i = 0; i < 4; i++) {
                  Tile gatePort = TerrariaUtils.Tiles[gatePortLocations[i]];
                  if (!gatePort.active || gatePort.type != (int)AdvancedCircuits.BlockType_InputPort)
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
                args.Player.SendTileSquare(location, 1);
              } else if (tile.active && tile.type == (int)AdvancedCircuits.BlockType_InputPort) {
                foreach (DPoint adjacentTileLocation in AdvancedCircuits.EnumerateComponentPortLocations(location, new DPoint(1, 1))) {
                  Tile adjacentTile = TerrariaUtils.Tiles[adjacentTileLocation];
                  if (!adjacentTile.active || !AdvancedCircuits.IsLogicalGate((BlockType)adjacentTile.type))
                    continue;

                  if (
                    TShock.CheckTilePermission(args.Player, adjacentTileLocation.X, adjacentTileLocation.Y) || (
                      this.PluginCooperationHandler.IsProtectorAvailable &&
                      this.PluginCooperationHandler.Protector__CheckProtected(args.Player, adjacentTileLocation, false)
                    )
                  ) {
                    player.SendErrorMessage("This gate is protected.");
                    player.SendTileSquare(location, 1);
                    return result;
                  }

                  int modifiers = AdvancedCircuits.CountComponentModifiers(adjacentTileLocation, new DPoint(1, 1));
                  if (modifiers == 1) {
                    player.SendErrorMessage("The gate has one modifier, there's no point in initializing it.");
                    args.Player.SendTileSquare(location, 1);
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
                  args.Player.SendTileSquare(location, 1);
                  return result;
                }

                player.SendErrorMessage(string.Format(
                  "The state of \"{0}\" can not be changed.", TerrariaUtils.Tiles.GetBlockTypeName(hitBlockType)
                ));

                player.SendTileSquare(location, 1);
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

    #region [Methods: HandleTileEdit, HandleTilePlacing, HandleWirePlacing]
    public override bool HandleTileEdit(TSPlayer player, TileEditType editType, BlockType blockType, DPoint location, int objectStyle) {
      if (this.IsDisposed)
        return false;
      if (base.HandleTileEdit(player, editType, blockType, location, objectStyle))
        return true;

      if (editType == TileEditType.PlaceTile)
        return this.HandleTilePlacing(player, blockType, location, objectStyle);
      else if (editType == TileEditType.TileKill || editType == TileEditType.TileKillNoItem)
        return this.HandleTileDestruction(player, location);
      else if (editType == TileEditType.PlaceWire)
        return this.HandleWirePlacing(player, location);

      return false;
    }

    private bool HandleTilePlacing(TSPlayer player, BlockType blockType, DPoint location, int objectStyle) {
      if (this.IsDisposed)
        return false;

      List<DPoint> componentLocations;
      bool isModifier = (blockType == AdvancedCircuits.BlockType_Modifier);
      if (!isModifier) {
        DPoint originTileLocation;
        switch (blockType) {
          case BlockType.Statue:
            originTileLocation = new DPoint(location.X, location.Y - 2);
            break;
          case BlockType.DartTrap:
          case AdvancedCircuits.BlockType_WirelessTransmitter:
            originTileLocation = location;
            break;
          case BlockType.Boulder:
          case BlockType.InletPump:
          case BlockType.OutletPump:
            originTileLocation = new DPoint(location.X - 1, location.Y - 1);
            break;
          default:
            return false;
        }

        componentLocations = new List<DPoint>();
        componentLocations.Add(originTileLocation);
      } else {
        componentLocations = new List<DPoint>();

        foreach (DPoint anyComponentLocation in AdvancedCircuits.EnumerateModifierAdjacentComponents(location)) {
          ObjectMeasureData measureData = TerrariaUtils.Tiles.MeasureObject(anyComponentLocation);
          if (
            measureData.BlockType != BlockType.DartTrap &&
            measureData.BlockType != BlockType.InletPump &&
            measureData.BlockType != BlockType.OutletPump && 
            measureData.BlockType != AdvancedCircuits.BlockType_WirelessTransmitter
          )
            continue;

          componentLocations.Add(measureData.OriginTileLocation);
        }
      }

      foreach (DPoint componentLocation in componentLocations) {
        int modifiers = AdvancedCircuits.CountComponentModifiers(componentLocation, TerrariaUtils.Tiles.GetObjectSize(blockType));
        if (isModifier) {
          modifiers++;
          blockType = (BlockType)TerrariaUtils.Tiles[componentLocation].type;
        }
        
        ComponentConfigProfile configProfile = AdvancedCircuits.ModifierCountToConfigProfile(modifiers);
        bool hasPermission = true;
        switch (blockType) {
          case BlockType.Statue: {
            if (!TerrariaUtils.Tiles.IsObjectWired(componentLocation, new DPoint(2, 3)))
              break;
            StatueStyle statueType = (StatueStyle)objectStyle;
            StatueConfig statueConfig;
            if (!this.Config.StatueConfigs.TryGetValue(statueType, out statueConfig) || statueConfig == null)
              break;
            if (statueConfig.WirePermission == null)
              break;

            if (!player.Group.HasPermission(statueConfig.WirePermission)) {
              player.SendTileSquareEx(location, 10);

              ItemType itemTypeToDrop;
              if (!isModifier)
                itemTypeToDrop = TerrariaUtils.Tiles.GetItemTypeFromStatueStyle(statueType);
              else
                itemTypeToDrop = ItemType.CobaltOre;

              Item.NewItem(location.X * TerrariaUtils.TileSize, location.Y * TerrariaUtils.TileSize, 32, 32, (int)itemTypeToDrop);

              hasPermission = false;
              break;
            }

            break;
          }
          case BlockType.DartTrap: {
            if (!TerrariaUtils.Tiles[componentLocation].wire)
              break;
            DartTrapConfig dartTrapConfig;
            if (!this.Config.DartTrapConfigs.TryGetValue(configProfile, out dartTrapConfig))
              break;
            if (dartTrapConfig.WirePermission == null)
              break;

            if (!player.Group.HasPermission(dartTrapConfig.WirePermission)) {
              player.SendTileSquareEx(location, 10);

              ItemType itemTypeToDrop;
              if (!isModifier)
                itemTypeToDrop = ItemType.DartTrap;
              else
                itemTypeToDrop = ItemType.CobaltOre;

              Item.NewItem(location.X * TerrariaUtils.TileSize, location.Y * TerrariaUtils.TileSize, 16, 16, (int)itemTypeToDrop);

              hasPermission = false;
              break;
            }

            break;
          }
          case BlockType.Boulder: {
            if (!TerrariaUtils.Tiles.IsObjectWired(componentLocation, new DPoint(2, 2)))
              break;
            if (this.Config.BoulderWirePermission == null)
              break;

            if (!player.Group.HasPermission(this.Config.BoulderWirePermission)) {
              player.SendTileSquareEx(location, 10);

              ItemType itemTypeToDrop;
              if (!isModifier)
                itemTypeToDrop = ItemType.Boulder;
              else
                itemTypeToDrop = ItemType.CobaltOre;

              Item.NewItem(location.X * TerrariaUtils.TileSize, location.Y * TerrariaUtils.TileSize, 32, 32, (int)itemTypeToDrop);

              hasPermission = false;
              break;
            }

            break;
          }
          case BlockType.InletPump:
          case BlockType.OutletPump: {
            if (!TerrariaUtils.Tiles.IsObjectWired(componentLocation, new DPoint(2, 2)))
              break;
            PumpConfig pumpConfig;
            if (!this.Config.PumpConfigs.TryGetValue(configProfile, out pumpConfig))
              break;
            if (pumpConfig.WirePermission == null)
              break;

            if (!player.Group.HasPermission(pumpConfig.WirePermission)) {
              player.SendTileSquareEx(location, 10);

              ItemType itemTypeToDrop;
              if (!isModifier)
                if (blockType  == BlockType.InletPump)
                  itemTypeToDrop = ItemType.InletPump;
                else
                  itemTypeToDrop = ItemType.OutletPump;
              else
                itemTypeToDrop = ItemType.CobaltOre;

              Item.NewItem(location.X * TerrariaUtils.TileSize, location.Y * TerrariaUtils.TileSize, 32, 32, (int)itemTypeToDrop);

              hasPermission = false;
              break;
            }

            break;
          }
          case AdvancedCircuits.BlockType_WirelessTransmitter: {
            if (!AdvancedCircuits.IsComponentWiredByPort(componentLocation, new DPoint(1, 1)))
              break;
            WirelessTransmitterConfig transmitterConfig;
            if (!this.Config.WirelessTransmitterConfigs.TryGetValue(configProfile, out transmitterConfig))
              break;
            if (transmitterConfig.WirePermission == null)
              break;

            if (!player.Group.HasPermission(transmitterConfig.WirePermission)) {
              player.SendTileSquareEx(location, 1);

              ItemType itemTypeToDrop;
              if (!isModifier)
                itemTypeToDrop = ItemType.Meteorite;
              else
                itemTypeToDrop = ItemType.CobaltOre;

              Item.NewItem(location.X * TerrariaUtils.TileSize, location.Y * TerrariaUtils.TileSize, 32, 32, (int)itemTypeToDrop);

              hasPermission = false;
              break;
            }

            break;
          }
        }

        if (!hasPermission) {
          this.TellNoComponentWiringPermission(player, blockType, modifiers);

          return true;
        }
      }

      return false;
    }

    private bool HandleTileDestruction(TSPlayer player, DPoint location) {
      if (this.IsDisposed)
        return false;

      Tile tile = TerrariaUtils.Tiles[location];
      if (!tile.active || tile.type != (int)AdvancedCircuits.BlockType_Modifier)
        return false;

      foreach (DPoint anyComponentLocation in AdvancedCircuits.EnumerateModifierAdjacentComponents(location)) {
        BlockType blockType = (BlockType)TerrariaUtils.Tiles[anyComponentLocation].type;
        if (
          blockType != BlockType.DartTrap && 
          blockType != BlockType.Boulder && 
          blockType != BlockType.InletPump &&
          blockType != BlockType.OutletPump &&
          blockType != AdvancedCircuits.BlockType_WirelessTransmitter
        )
          continue;

        player.SendErrorMessage(string.Format(
          "Please remove the component \"{0}\" first before removing its modifier.", AdvancedCircuits.GetComponentName(blockType)
        ));

        player.SendTileSquare(location, 1);
        return true;
      }

      return false;
    }

    private bool HandleWirePlacing(TSPlayer player, DPoint location) {
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
        Tile tile = TerrariaUtils.Tiles[tileToCheck];
        if (!tile.active)
          continue;
        if (tileToCheck != location && tile.type != (int)AdvancedCircuits.BlockType_WirelessTransmitter)
          continue;

        bool hasPermission = true;
        ObjectMeasureData measureData = TerrariaUtils.Tiles.MeasureObject(tileToCheck);
        int modifiers = AdvancedCircuits.CountComponentModifiers(measureData);
        ComponentConfigProfile configProfile = AdvancedCircuits.ModifierCountToConfigProfile(modifiers);
        switch ((BlockType)tile.type) {
          case BlockType.Statue: {
            StatueStyle statueStyle = (StatueStyle)(tile.frameX / (TerrariaUtils.DefaultTextureTileSize * 2));
            StatueConfig statueConfig;
            if (!this.Config.StatueConfigs.TryGetValue(statueStyle, out statueConfig))
              return false;
            if (statueConfig.WirePermission == null)
              return false;

            hasPermission = !player.Group.HasPermission(statueConfig.WirePermission);
            break;
          }
          case BlockType.DartTrap: {
            DartTrapConfig dartTrapConfig;
            if (!this.Config.DartTrapConfigs.TryGetValue(configProfile, out dartTrapConfig))
              break;
            if (dartTrapConfig.WirePermission == null)
              break;

            hasPermission = !player.Group.HasPermission(dartTrapConfig.WirePermission);
            break;
          }
          case BlockType.Boulder: {
            if (this.Config.BoulderWirePermission == null)
              break;

            hasPermission = !player.Group.HasPermission(this.Config.BoulderWirePermission);
            break;
          }
          case BlockType.InletPump:
          case BlockType.OutletPump: {
            PumpConfig pumpConfig;
            if (!this.Config.PumpConfigs.TryGetValue(configProfile, out pumpConfig))
              break;
            if (pumpConfig.WirePermission == null)
              break;

            hasPermission = !player.Group.HasPermission(pumpConfig.WirePermission);
            break;
          }
          case AdvancedCircuits.BlockType_WirelessTransmitter: {
            WirelessTransmitterConfig transmitterConfig;
            if (!this.Config.WirelessTransmitterConfigs.TryGetValue(configProfile, out transmitterConfig))
              break;
            if (transmitterConfig.WirePermission == null)
              break;

            hasPermission = !player.Group.HasPermission(transmitterConfig.WirePermission);
            break;
          }
        }

        if (!hasPermission) {
          this.TellNoComponentWiringPermission(player, (BlockType)tile.type, modifiers);
          
          player.SendTileSquare(location, 1);
          Item.NewItem(location.X * TerrariaUtils.TileSize, location.Y * TerrariaUtils.TileSize, 16, 16, (int)ItemType.Wire);

          return true;
        }
      }

      return false;
    }

    private void TellNoComponentWiringPermission(TSPlayer player, BlockType blockType, int modifiers) {
      player.SendErrorMessage("You don't have the required permission to wire up");

      string messagePart2;
      if (modifiers == 0)
        messagePart2 = "components of type \"{0}\".";
      else if (modifiers == 1)
        messagePart2 = "components of type \"{0}\" with 1 modifier.";
      else
        messagePart2 = "components of type \"{0}\" with {1} modifiers.";

      string blockName = TerrariaUtils.Tiles.GetBlockTypeName(blockType);
      player.SendErrorMessage(string.Format(messagePart2, blockName, modifiers));
      this.PluginTrace.WriteLineInfo(
        "Player \"{0}\" tried to wire a component of type \"{1}\" ({2} modifiers) but didn't have the necessary permissions to do so.", 
        player.Name, blockName, modifiers
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
    
      if (isDisposing) {
        this.reloadConfigurationCallback = null;
      }
    
      base.Dispose(isDisposing);
    }
    #endregion
  }
}
