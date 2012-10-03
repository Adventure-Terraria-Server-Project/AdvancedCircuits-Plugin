// This file is provided unter the terms of the 
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// 
// Written by CoderCow

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using DPoint = System.Drawing.Point;

using Hooks;
using TShockAPI;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class UserInteractionHandler: IDisposable {
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
    public UserInteractionHandler(PluginInfo pluginInfo, Configuration config, Action reloadConfigurationCallback) {
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
          args.Player.SendMessage("Available Sub-Commands:", Color.Yellow);
          args.Player.SendMessage("/aci blocks", Color.Yellow);

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
        case "components":
          args.Player.SendMessage("Copper Ore - OR-Gate", Color.Yellow);
          args.Player.SendMessage("Silver Ore - AND-Gate", Color.Yellow);
          args.Player.SendMessage("Gold Ore - XOR-Gate", Color.Yellow);
          args.Player.SendMessage("Obsidian - NOT-Gate", Color.Yellow);
          args.Player.SendMessage("Iron Ore - Swapper", Color.Yellow);
          args.Player.SendMessage("Spike - Crossover Bridge", Color.Yellow);
          args.Player.SendMessage("Glass - Input Port", Color.Yellow);

          return true;
      }

      return false;
    }
    #endregion

    #region [Methods: HandleTilePlacing, HandleWirePlacing]
    public bool HandleTilePlacing(TSPlayer player, int tileId, int x, int y, int tileStyle) {
      if (this.IsDisposed)
        return false;

      if (tileId != Terraria.TileId_Statue)
        return false;
      
      bool isWired = false;
      for (int rx = 0; rx < 2 && !isWired; rx++) {
        for (int ry = 0; ry < 3; ry++) {
          int absoluteX = x + rx;
          int absoluteY = y + ry;

          if (Main.tile[absoluteX, absoluteY].wire) {
            isWired = true;
            break;
          }
        }
      }
      if (!isWired)
        return false;

      StatueType statueType = (StatueType)tileStyle;
      StatueConfig statueConfig;
      if (!this.Config.StatueConfigs.TryGetValue(statueType, out statueConfig) || statueConfig == null)
        return false;
      if (statueConfig.RequiredPermission == null)
        return false;

      if (!player.Group.HasPermission(statueConfig.RequiredPermission)) {
        player.SendMessage("You don't have the required permission to wire up this statue.", Color.Red);
        player.SendTileSquare(x, y, 6);

        AdvancedCircuitsPlugin.Trace.WriteLineInfo(
          "Player \"{0}\" tried to wire a statue of type \"{1}\" but didn't have the necessary permissions to do so.", 
          player.Name, statueType.ToString()
        );
        return true;
      }

      return false;
    }

    public bool HandleWirePlacing(TSPlayer player, int x, int y) {
      if (this.IsDisposed)
        return false;

      Tile tile = Main.tile[x, y];

      if (tile.type == Terraria.TileId_Statue) {
        StatueType statueType = (StatueType)(tile.frameX / (Terraria.DefaultTextureTileSize * 2));

        StatueConfig statueConfig;
        if (!this.Config.StatueConfigs.TryGetValue(statueType, out statueConfig) || statueConfig == null)
          return false;
        if (statueConfig.RequiredPermission == null)
          return false;

        if (!player.Group.HasPermission(statueConfig.RequiredPermission)) {
          player.SendMessage("You don't have the required permission to wire up this statue.", Color.Red);
          player.SendTileSquareEx(x, y, 3);

          AdvancedCircuitsPlugin.Trace.WriteLineInfo(
            "Player \"{0}\" tried to wire a statue of type \"{1}\" but didn't have the necessary permissions to do so.", 
            player.Name, statueType.ToString()
          );

          return true;
        }
      }

      return false;
    }
    #endregion

    #region [IDisposable Implementation]
    private bool isDisposed;

    public bool IsDisposed {
      get { return this.isDisposed; } 
    }

    protected virtual void Dispose(bool isDisposing) {
      if (this.isDisposed)
        return;
    
      if (isDisposing) {
        if (Commands.ChatCommands.Contains(this.RootCommand))
          Commands.ChatCommands.Remove(this.RootCommand);

        this.reloadConfigurationCallback = null;
      }
    
      this.isDisposed = true;
    }

    public void Dispose() {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~UserInteractionHandler() {
      this.Dispose(false);
    }
    #endregion
  }
}
