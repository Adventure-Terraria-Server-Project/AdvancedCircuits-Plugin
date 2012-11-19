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
  public class UserInteractionHandler: UserInteractionHandlerBase, IDisposable {
    #region [Constants]
    public const string ReloadCfg_Permission = "advancedcircuits_reloadcfg";
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
    public UserInteractionHandler(PluginInfo pluginInfo, Configuration config, Action reloadConfigurationCallback): 
      base(AdvancedCircuitsPlugin.Trace
    ) {
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
            AdvancedCircuitsPlugin.Trace.WriteLineInfo("Reloading configuration file.");
            try {
              this.ReloadConfigurationCallback();
              AdvancedCircuitsPlugin.Trace.WriteLineInfo("Configuration file successfully reloaded.");

              if (args.Player != TSPlayer.Server)
                args.Player.SendMessage("Configuration file successfully reloaded.", Color.Yellow);
            } catch (Exception ex) {
              AdvancedCircuitsPlugin.Trace.WriteLineError(
                "Reloading the configuration file failed. Keeping old configuration. Exception details:\n{0}", ex
              );
            }
          } else {
            args.Player.SendMessage("You do not have the necessary permission to do that.", Color.Red);
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
          }

          return true;
        case "toggle":
        case "switch":
          args.Player.SendInfoMessage("Place or destroy a wire on the component you want to toggle.");

          PlayerCommandInteraction interaction = this.StartOrResetCommandInteraction(args.Player);
          interaction.TimeExpiredCallback = (player) => {
            player.SendErrorMessage("Waited too long, no component will be toggled.");
          };

          interaction.TileEditCallback = (player, editType, blockId, x, y) => {
            if (editType == TileEditType.PlaceWire || editType == TileEditType.DestroyWire) {
              CommandInteractionResult result = new CommandInteractionResult { IsHandled = true, IsInteractionCompleted = true };
              Tile tile = Terraria.Tiles[x, y];

              if (!tile.active)
                return new CommandInteractionResult { IsHandled = false, IsInteractionCompleted = false };

              if (TShock.CheckTilePermission(args.Player, x, y)) {
                player.SendErrorMessage("This tile is protected.");
                player.SendTileSquare(x, y, 1);
                return result;
              }

              if (!Terraria.IsSpriteToggleable(tile.type)) {
                player.SendErrorMessage(string.Format(
                  "The state of the tile \"{0}\" can not be changed.", Terraria.Tiles.GetBlockName(tile.type)
                ));
                player.SendTileSquare(x, y, 1);
                return result;
              }
              
              Terraria.SpriteMeasureData measureData = Terraria.MeasureSprite(new DPoint(x, y));
              bool newState = !Terraria.HasSpriteActiveFrame(measureData);
              Terraria.SetSpriteActiveFrame(measureData, newState);

              string newStateString;
              if (newState)
                newStateString = "active";
              else
                newStateString = "inactive";

              player.SendInfoMessage(string.Format(
                "{0}'s state changed to \"{1}\".", AdvancedCircuits.GetComponentName(tile.type), newStateString
              ));

              player.SendTileSquare(x, y, 1);
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
    public bool HandleTileEdit(TSPlayer player, TileEditType editType, int blockId, int x, int y, int tileStyle) {
      if (this.IsDisposed)
        return false;

      if (base.HandleTileEdit(player, editType, blockId, x, y))
        return true;

      if (editType == TileEditType.PlaceTile)
        return this.HandleTilePlacing(player, blockId, x, y, tileStyle);
      else if (editType == TileEditType.DestroyTile)
        return this.HandleTileDestruction(player, x, y);
      else if (editType == TileEditType.PlaceWire)
        return this.HandleWirePlacing(player, x, y);

      return false;
    }

    protected virtual bool HandleTilePlacing(TSPlayer player, int blockId, int x, int y, int tileStyle) {
      if (this.IsDisposed)
        return false;

      if (
        blockId != Terraria.TileId_Statue &&
        blockId != Terraria.TileId_DartTrap && 
        blockId != Terraria.TileId_Boulder && 
        blockId != AdvancedCircuits.TileId_Modifier
      )
        return false;

      bool isModifier = (blockId == AdvancedCircuits.TileId_Modifier);
      List<DPoint> componentLocations = new List<DPoint>();
      if (!isModifier) {
        DPoint originTileLocation;
        switch (blockId) {
          case Terraria.TileId_Statue:
            originTileLocation = new DPoint(x, y - 2);
            break;
          case Terraria.TileId_Boulder:
            originTileLocation = new DPoint(x - 1, y - 1);
            break;
          default:
            originTileLocation = new DPoint(x, y);
            break;
        }

        componentLocations.Add(originTileLocation);
      } else {
        foreach (DPoint anyComponentLocation in AdvancedCircuits.EnumerateModifierAdjacentComponents(new DPoint(x, y))) {
          Terraria.SpriteMeasureData measureData = Terraria.MeasureSprite(anyComponentLocation);
          if (
            measureData.SpriteType != Terraria.TileId_DartTrap
          )
            continue;

          componentLocations.Add(measureData.OriginTileLocation);
        }
      }

      foreach (DPoint componentLocation in componentLocations) {
        int modifiers = AdvancedCircuits.CountComponentModifiers(componentLocation, Terraria.GetSpriteSize(blockId));
        if (isModifier) {
          modifiers++;
          blockId = Terraria.Tiles[componentLocation].type;
        }
        
        ComponentConfigProfile configProfile = AdvancedCircuits.ModifierCountToConfigTarget(modifiers);
        bool hasPermission = true;
        switch (blockId) {
          case Terraria.TileId_Statue: {
            if (!Terraria.IsSpriteWired(componentLocation, new DPoint(2, 3)))
              break;
            StatueType statueType = (StatueType)tileStyle;
            StatueConfig statueConfig;
            if (!this.Config.StatueConfigs.TryGetValue(statueType, out statueConfig) || statueConfig == null)
              break;
            if (statueConfig.WirePermission == null)
              break;

            if (!player.Group.HasPermission(statueConfig.WirePermission)) {
              player.SendTileSquareEx(x, y, 10);

              if (!isModifier)
                Item.NewItem(x * Terraria.TileSize, y * Terraria.TileSize, 32, 32, Terraria.GetItemTypeFromStatueType(statueType));
              else
                Item.NewItem(x * Terraria.TileSize, y * Terraria.TileSize, 16, 16, Terraria.ItemId_CobaltOre);

              hasPermission = false;
              break;
            }

            break;
          }
          case Terraria.TileId_DartTrap: {
            if (!Terraria.Tiles[componentLocation].wire)
              break;
            DartTrapConfig dartTrapConfig;
            if (!this.Config.DartTrapConfigs.TryGetValue(configProfile, out dartTrapConfig))
              break;
            if (dartTrapConfig.WirePermission == null)
              break;

            if (!player.Group.HasPermission(dartTrapConfig.WirePermission)) {
              player.SendTileSquareEx(x, y, 10);

              if (!isModifier)
                Item.NewItem(x * Terraria.TileSize, y * Terraria.TileSize, 16, 16, Terraria.ItemId_DartTrap);
              else
                Item.NewItem(x * Terraria.TileSize, y * Terraria.TileSize, 16, 16, Terraria.ItemId_CobaltOre);

              hasPermission = false;
              break;
            }

            break;
          }
          case Terraria.TileId_Boulder: {
            if (!Terraria.IsSpriteWired(componentLocation, new DPoint(2, 2)))
              break;
            if (this.Config.BoulderWirePermission == null)
              break;

            if (!player.Group.HasPermission(this.Config.BoulderWirePermission)) {
              player.SendTileSquareEx(x, y, 10);

              if (!isModifier)
                Item.NewItem(x * Terraria.TileSize, y * Terraria.TileSize, 32, 32, Terraria.ItemId_Boulder);
              else
                Item.NewItem(x * Terraria.TileSize, y * Terraria.TileSize, 16, 16, Terraria.ItemId_CobaltOre);

              hasPermission = false;
              break;
            }

            break;
          }
          // case SomePortDefiningComponent:
            /*bool willBeWired = Terraria.Tiles[componentLocation].wire;
            if (!willBeWired) {
              foreach (DPoint portLocation in AdvancedCircuits.EnumerateComponentPortLocations(componentLocation, new DPoint(1, 1))) {
                if (!Terraria.Tiles[portLocation].wire)
                  continue;

                willBeWired = true;
                break;
              }
            }*/
        }

        if (!hasPermission) {
          this.TellNoComponentWiringPermission(player, blockId, modifiers);

          return true;
        }
      }

      return false;
    }

    protected virtual bool HandleTileDestruction(TSPlayer player, int x, int y) {
      if (this.IsDisposed)
        return false;

      Tile tile = Terraria.Tiles[x, y];
      if (!tile.active || tile.type != AdvancedCircuits.TileId_Modifier)
        return false;

      DPoint tileLocation = new DPoint(x, y);
      foreach (DPoint anyComponentLocation in AdvancedCircuits.EnumerateModifierAdjacentComponents(tileLocation)) {
        int blockId = Terraria.Tiles[anyComponentLocation].type;
        if (blockId != Terraria.ItemId_DartTrap && blockId != Terraria.TileId_Boulder)
          continue;

        player.SendErrorMessage("Before removing the Modifier, please remove the component");
        player.SendErrorMessage(string.Format("\"{0}\" first.", AdvancedCircuits.GetComponentName(blockId)));

        player.SendTileSquare(x, y, 1);
        return true;
      }

      return false;
    }

    protected virtual bool HandleWirePlacing(TSPlayer player, int x, int y) {
      if (this.IsDisposed)
        return false;

      DPoint tileToWire = new DPoint(x, y);
      DPoint[] tilesToCheck = new[] {
        tileToWire,
        new DPoint(x - 1, y),
        new DPoint(x + 1, y),
        new DPoint(x, y - 1),
        new DPoint(x, y + 1),
      };

      foreach (DPoint tileToCheck in tilesToCheck) {
        Tile tile = Terraria.Tiles[tileToCheck];
        if (!tile.active)
          continue;
        if (tileToCheck != tileToWire && !AdvancedCircuits.IsPortDefiningComponentBlock(tile.type))
          continue;

        bool hasPermission = true;
        Terraria.SpriteMeasureData measureData = Terraria.MeasureSprite(tileToCheck);
        int modifiers = AdvancedCircuits.CountComponentModifiers(measureData);
        ComponentConfigProfile configProfile = AdvancedCircuits.ModifierCountToConfigTarget(modifiers);
        switch (tile.type) {
          case Terraria.TileId_Statue: {
            StatueType statueType = (StatueType)(tile.frameX / (Terraria.DefaultTextureTileSize * 2));
            StatueConfig statueConfig;
            if (!this.Config.StatueConfigs.TryGetValue(statueType, out statueConfig) || statueConfig == null)
              return false;
            if (statueConfig.WirePermission == null)
              return false;

            if (!player.Group.HasPermission(statueConfig.WirePermission))
              hasPermission = false;

            break;
          }
          case Terraria.TileId_DartTrap: {
            DartTrapConfig dartTrapConfig;
            if (!this.Config.DartTrapConfigs.TryGetValue(configProfile, out dartTrapConfig))
              break;
            if (dartTrapConfig.WirePermission == null)
              break;

            if (!player.Group.HasPermission(dartTrapConfig.WirePermission))
              hasPermission = false;

            break;
          }
          case Terraria.TileId_Boulder: {
            if (this.Config.BoulderWirePermission == null)
              break;

            if (!player.Group.HasPermission(this.Config.BoulderWirePermission))
              hasPermission = false;

            break;
          }
        }

        if (!hasPermission) {
          this.TellNoComponentWiringPermission(player, tile.type, modifiers);
          
          player.SendTileSquare(x, y, 1);
          Item.NewItem(x * Terraria.TileSize, y * Terraria.TileSize, 16, 16, Terraria.ItemId_Wire);

          return true;
        }
      }

      return false;
    }

    private void TellNoComponentWiringPermission(TSPlayer player, int blockId, int modifiers) {
      player.SendErrorMessage("You don't have the required permission to wire up");

      string messagePart2;
      if (modifiers == 0)
        messagePart2 = "components of type \"{0}\".";
      else if (modifiers == 1)
        messagePart2 = "components of type \"{0}\" with 1 modifier.";
      else
        messagePart2 = "components of type \"{0}\" with {1} modifiers.";

      string blockName = Terraria.Tiles.GetBlockName(blockId);
      player.SendErrorMessage(string.Format(messagePart2, blockName, modifiers));
      AdvancedCircuitsPlugin.Trace.WriteLineInfo(
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
