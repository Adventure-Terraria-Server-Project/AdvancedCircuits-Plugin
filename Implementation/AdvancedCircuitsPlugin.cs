// This file is provided unter the terms of the 
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// 
// Written by CoderCow

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using DPoint = System.Drawing.Point;

using Hooks;
using TShockAPI;

using Terraria.Plugins.CoderCow.AdvancedCircuits.Test;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  [APIVersion(1, 12)]
  public class AdvancedCircuitsPlugin: TerrariaPlugin, IDisposable {
    #region [Constants]
    public const string TracePrefix = @"[Advanced Circuits] ";
    #endregion

    #region [Properties: Static AdvancedCircuitsDataDirectory, Static ConfigFilePath, Static WorldMetadataDirectory]
    public static string AdvancedCircuitsDataDirectory {
      get {
        return Path.Combine(TShock.SavePath, "Advanced Circuits");
      }
    }

    public static string ConfigFilePath {
      get {
        return Path.Combine(AdvancedCircuitsPlugin.AdvancedCircuitsDataDirectory, "Config.xml");
      }
    }

    public static string WorldMetadataDirectory {
      get {
        return Path.Combine(AdvancedCircuitsPlugin.AdvancedCircuitsDataDirectory, "World Data");
      }
    }
    #endregion

    #region [Property: Static Trace]
    private static PluginTrace trace;

    public static PluginTrace Trace {
      get {
        if (AdvancedCircuitsPlugin.trace == null)
          AdvancedCircuitsPlugin.trace = new PluginTrace(AdvancedCircuitsPlugin.TracePrefix);
        
        return AdvancedCircuitsPlugin.trace;
      }
    }
    #endregion

    #region [Property: PluginInfo]
    private readonly PluginInfo pluginInfo;

    protected PluginInfo PluginInfo {
      get { return this.pluginInfo; }
    }
    #endregion

    #region [Property: Config]
    private Configuration config;

    protected Configuration Config {
      get { return this.config; }
    }
    #endregion

    #region [Property: CircuitProcessor]
    private CircuitProcessor circuitProcessor;

    protected CircuitProcessor CircuitProcessor {
      get { return this.circuitProcessor; }
    }
    #endregion

    #region [Property: UserInteractionHandler]
    private UserInteractionHandler userInteractionHandler;

    protected UserInteractionHandler UserInteractionHandler {
      get { return this.userInteractionHandler; }
    }
    #endregion

    #region [Property: WorldMetadataHandler]
    private WorldMetadataHandler worldMetadataHandler;

    protected WorldMetadataHandler WorldMetadataHandler {
      get { return this.worldMetadataHandler; }
    }
    #endregion

    #if Testrun
    private TestRunner testRunner = new TestRunner();
    #endif


    #region [Method: Constructor]
    public AdvancedCircuitsPlugin(Main game): base(game) {
      this.pluginInfo = new PluginInfo(
        "Advanced Circuits",
        Assembly.GetAssembly(typeof(AdvancedCircuitsPlugin)).GetName().Version,
        "Beta", // TODO: Remove Beta on stable release
        "CoderCow",
        "Adds advanced wiring mechanics to Terraria servers."
      );
    }
    #endregion

    #region [Method: Initialize]
    public override void Initialize() {
      GameHooks.PostInitialize += this.Game_PostInitialize;
    }

    private void Game_PostInitialize() {
      if (!Directory.Exists(AdvancedCircuitsPlugin.AdvancedCircuitsDataDirectory))
        Directory.CreateDirectory(AdvancedCircuitsPlugin.AdvancedCircuitsDataDirectory);
      
      if (File.Exists(AdvancedCircuitsPlugin.ConfigFilePath)) {
        try {
          this.config = Configuration.Read(AdvancedCircuitsPlugin.ConfigFilePath);
        } catch (Exception ex) {
          AdvancedCircuitsPlugin.Trace.WriteLineError(
            "Reading the configuration file failed. This plugin will be disabled. Exception details:\n{0}", ex
          );

          return;
        }
      } else {
        this.config = new Configuration();
      }

      this.worldMetadataHandler = new WorldMetadataHandler(AdvancedCircuitsPlugin.WorldMetadataDirectory);
      this.circuitProcessor = new CircuitProcessor(this.Config, this.WorldMetadataHandler.Metadata);

      Action reloadConfiguration = () => {
        if (this.isDisposed)
          return;

        this.config = Configuration.Read(AdvancedCircuitsPlugin.ConfigFilePath);
        if (this.circuitProcessor != null)
          this.circuitProcessor = new CircuitProcessor(this.Config, this.WorldMetadataHandler.Metadata);
      };
      this.userInteractionHandler = new UserInteractionHandler(this.PluginInfo, this.Config, reloadConfiguration);

      GameHooks.Update += this.OnGameUpdate;
      NetHooks.GetData += this.NetHooks_GetData;
      WorldHooks.SaveWorld += this.World_SaveWorld;

      GameHooks.PostInitialize -= this.Game_PostInitialize;

      #if Testrun
      this.testRunner.RunAllTests();
      #endif
    }
    #endregion

    #region [Method: NetHooks_GetData, World_SaveWorld]
    private void NetHooks_GetData(GetDataEventArgs e) {
      if (e == null || this.isDisposed || e.Handled)
        return;

      TSPlayer player = TShock.Players[e.Msg.whoAmI];
      if (player == null || !player.ConnectionAlive || player.RequiresPassword)
        return;
      
      switch (e.MsgID) {
        // Why not the TShock TileEdit handler? Because it doesn't read the style value from the data packet...
        case PacketTypes.Tile: {
          if (e.Msg.readBuffer.Length < 11)
            break;

          byte editType = e.Msg.readBuffer[e.Index];
          int x = BitConverter.ToInt32(e.Msg.readBuffer, e.Index + 1);
          int y = BitConverter.ToInt32(e.Msg.readBuffer, e.Index + 5);
          byte tileId = e.Msg.readBuffer[e.Index + 9];
          byte tileStyle = e.Msg.readBuffer[e.Index + 10];

          if (!Terraria.IsValidCoord(x, y) || editType < 0 || editType > 6)
            return;

          e.Handled = this.OnTileEdit(player, (TileEditType)editType, x, y, tileId, tileStyle);
          break;
        }
        case PacketTypes.HitSwitch: {
          if (e.Msg.readBuffer.Length < 5)
            break;

          int x = BitConverter.ToInt32(e.Msg.readBuffer, e.Index);
          int y = BitConverter.ToInt32(e.Msg.readBuffer, e.Index + 4);
      
          if (!Terraria.IsValidCoord(x, y) || !Main.tile[x, y].active)
            return;
      
          e.Handled = this.OnHitSwitch(player, x, y);
          if (Main.netMode == 2)
            NetMessage.SendData((int)PacketTypes.HitSwitch, -1, e.Msg.whoAmI, string.Empty, x, y);

          break;
        }
      }
    }

    private void World_SaveWorld(bool resettime, HandledEventArgs e) {
      if (this.isDisposed || e.Handled)
        return;

      e.Handled = this.OnWorldSaved();
    }
    #endregion

    #region [Methods: OnHitSwitch, OnGameUpdate, OnTileEdit, OnWorldSaved]
    protected virtual bool OnHitSwitch(TSPlayer player, int x, int y) {
      if (this.isDisposed)
        return false;

      return this.CircuitProcessor.HandleHitSwitch(player, x, y);
    }

    protected virtual void OnGameUpdate() {
      if (this.isDisposed)
        return;

      this.CircuitProcessor.HandleGameUpdate();
      #if Testrun
      this.testRunner.HandleGameUpdate();
      #endif
    }

    protected virtual bool OnTileEdit(TSPlayer player, TileEditType editType, int x, int y, int tileId, int tileStyle) {
      if (this.isDisposed)
        return false;

      switch (editType) {
        case TileEditType.PlaceTile:
          if (this.UserInteractionHandler.HandleTilePlacing(player, tileId, x, y, tileStyle))
            return true;

          this.WorldMetadataHandler.HandleTilePlacing(player, tileId, x, y, tileStyle);
          break;

        case TileEditType.DestroyTile:
          this.WorldMetadataHandler.HandleTileDestroying(player, x, y);

          break;

        case TileEditType.PlaceWire:
          if (this.UserInteractionHandler.HandleWirePlacing(player, x, y))
            return true;

          break;

        case TileEditType.DestroyWire:
          #if DEBUG || Testrun
          player.SendMessage(string.Format("X: {0}, Y: {1}", x, y), Color.Aqua);

          if (!Main.tile[x, y].active)
            break;

          Terraria.SpriteMeasureData measureData = Terraria.MeasureSprite(new DPoint(x, y));
          player.SendMessage(string.Format(
            "Origin X: {0}, Origin Y: {1}", measureData.OriginTileLocation.X, measureData.OriginTileLocation.Y));
          #endif
          break;
      }

      return false;
    }

    protected virtual bool OnWorldSaved() {
      this.WorldMetadataHandler.WriteMetadata();

      return false;
    }
    #endregion

    #region [TerrariaPlugin Overrides]
    public override string Name {
      get { return this.PluginInfo.PluginName; }
    }

    public override Version Version {
      get { return this.PluginInfo.VersionNumber; }
    }

    public override string Author {
      get { return this.PluginInfo.Author; }
    }

    public override string Description {
      get { return this.PluginInfo.Description; }
    }
    #endregion

    #region [IDisposable Implementation]
    private bool isDisposed;

    public bool IsDisposed {
      get { return this.isDisposed; } 
    }

    protected override void Dispose(bool isDisposing) {
      if (this.IsDisposed)
        return;
    
      if (isDisposing) {
        GameHooks.PostInitialize -= this.Game_PostInitialize;
        GameHooks.Update -= this.OnGameUpdate;
        NetHooks.GetData -= this.NetHooks_GetData;
        WorldHooks.SaveWorld -= this.World_SaveWorld;
    
        if (this.userInteractionHandler != null)
          this.userInteractionHandler.Dispose();
      }

      base.Dispose(isDisposing);
      this.isDisposed = true;
    }
    #endregion
  }
}
