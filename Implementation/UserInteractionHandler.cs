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

using Terraria.Plugins.Common;

using TShockAPI;

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

    #region [Property: RootCommand]
    private readonly Command rootCommand;

    protected Command RootCommand {
      get { return this.rootCommand; }
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
      PluginTrace pluginTrace, PluginInfo pluginInfo, Configuration config, Action reloadConfigurationCallback
    ): 
      base(pluginTrace
    ) {
      this.pluginTrace = pluginTrace;
      this.pluginInfo = pluginInfo;
      this.config = config;
      this.reloadConfigurationCallback = reloadConfigurationCallback;

      this.rootCommand = new Command(this.RootCommand_Exec, "advancedcircuits", "aci", "ac");
      Commands.ChatCommands.Add(this.RootCommand);
    }
    #endregion

    #region [Methods: RootCommand_Exec, TryExecuteSubCommand]
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
      args.Player.SendMessage("Write \"/aci commands\" to see a list of sub-commands.", Color.Yellow);
      args.Player.SendMessage("For help and support refer to the TShock forums.", Color.Yellow);
    }

    private bool TryExecuteSubCommand(string commandNameLC, CommandArgs args) {
      switch (commandNameLC) {
        case "commands":
        case "cmds":
          args.Player.SendMessage("Available Sub-Commands:", Color.White);
          args.Player.SendMessage("/aci blocks", Color.Yellow);
          args.Player.SendMessage("/aci toggle|switch", Color.Yellow);

          if (args.Player.Group.HasPermission(UserInteractionHandler.ReloadCfg_Permission))
            args.Player.SendMessage("/aci reloadcfg", Color.Yellow);

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
          int page = 1;
          if (args.Parameters.Count >= 2) {
            if (args.Parameters[1] == "2")
              page = 2;
          }

          if (page == 1) {
            args.Player.SendMessage("Copper Ore - OR-Gate", Color.White);
            args.Player.SendMessage("Silver Ore - AND-Gate", Color.White);
            args.Player.SendMessage("Gold Ore - XOR-Gate", Color.White);
            args.Player.SendMessage("Obsidian - NOT-Gate / NOT-Port", Color.White);
            args.Player.SendMessage("Iron Ore - Swapper", Color.White);
            args.Player.SendMessage("Spike - Crossover Bridge", Color.White);
            args.Player.SendMessage("Type \"/ac blocks 2\" for the next page.", Color.Yellow);
          }

          if (page == 2) {
            args.Player.SendMessage("Glass - Input Port", Color.White);
            args.Player.SendMessage("Cobalt Ore - Modifier", Color.White);
            args.Player.SendMessage("Active Stone - Active Stone and Block Activator", Color.White);
            args.Player.SendMessage("Meteorite - Wireless Transmitter", Color.White);
          }

          return true;
        case "toggle":
        case "switch":
          args.Player.SendInfoMessage("Place or destroy a wire on the component you want to toggle.");

          PlayerCommandInteraction interaction = this.StartOrResetCommandInteraction(args.Player);
          interaction.TimeExpiredCallback = (player) => {
            player.SendErrorMessage("Waited too long, no component will be toggled.");
          };

          interaction.TileEditCallback = (player, editType, blockType, location, blockStyle) => {
            if (editType == TileEditType.PlaceWire || editType == TileEditType.DestroyWire) {
              CommandInteractionResult result = new CommandInteractionResult { IsHandled = true, IsInteractionCompleted = true };
              Tile tile = TerrariaUtils.Tiles[location];

              if (!tile.active)
                return new CommandInteractionResult { IsHandled = false, IsInteractionCompleted = false };

              if (TShock.CheckTilePermission(args.Player, location.X, location.Y)) {
                player.SendErrorMessage("This tile is protected.");
                player.SendTileSquare(location, 1);
                return result;
              }

              if (!TerrariaUtils.Tiles.IsMultistateObject((BlockType)tile.type)) {
                player.SendErrorMessage(string.Format(
                  "The state of the tile \"{0}\" can not be changed.", TerrariaUtils.Tiles.GetBlockTypeName((BlockType)tile.type)
                ));
                player.SendTileSquare(location, 1);
                return result;
              }
              
              ObjectMeasureData measureData = TerrariaUtils.Tiles.MeasureObject(location);
              bool newState = !TerrariaUtils.Tiles.ObjectHasActiveState(measureData);
              TerrariaUtils.Tiles.SetObjectState(measureData, newState);

              string newStateString;
              if (newState)
                newStateString = "active";
              else
                newStateString = "inactive";

              player.SendInfoMessage(string.Format(
                "{0}'s state changed to \"{1}\".", AdvancedCircuits.GetComponentName((BlockType)tile.type), newStateString
              ));

              player.SendTileSquare(location, 1);
              return result;
            }

            return new CommandInteractionResult { IsHandled = false, IsInteractionCompleted = false };
          };

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

    #region [IDisposable Implementation]
    protected override void Dispose(bool isDisposing) {
      if (this.IsDisposed)
        return;
    
      if (isDisposing) {
        if (Commands.ChatCommands.Contains(this.RootCommand))
          Commands.ChatCommands.Remove(this.RootCommand);

        this.reloadConfigurationCallback = null;
      }
    
      base.Dispose(isDisposing);
    }
    #endregion
  }
}
