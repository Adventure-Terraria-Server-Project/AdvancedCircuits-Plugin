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
using System.Xml.Schema;
using Terraria.Plugins.Common;
using DPoint = System.Drawing.Point;

using Terraria.Plugins.CoderCow.AdvancedCircuits.Test;
using Terraria.Plugins.Common.Hooks;

using Hooks;
using TShockAPI;

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

    #region [Property: Static LatestInstance]
    private static AdvancedCircuitsPlugin latestInstance;

    public static AdvancedCircuitsPlugin LatestInstance {
      get { return AdvancedCircuitsPlugin.latestInstance; }
    }
    #endregion

    #region [Property: Trace]
    private readonly PluginTrace trace;

    public PluginTrace Trace {
      get { return this.trace; }
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

    #region [Property: GetDataHookHandler]
    private GetDataHookHandler getDataHookHandler;

    protected GetDataHookHandler GetDataHookHandler {
      get { return this.getDataHookHandler; }
    }
    #endregion

    #region [Property: CircuitHandler]
    private CircuitHandler circuitHandler;

    protected CircuitHandler CircuitHandler {
      get { return this.circuitHandler; }
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

    #region [Property: PluginCooperationHandler]
    private PluginCooperationHandler pluginCooperationHandler;

    public PluginCooperationHandler PluginCooperationHandler {
      get { return this.pluginCooperationHandler; }
    }
    #endregion

    private bool hooksEnabled;

    #if Testrun
    private TestRunner testRunner;
    #endif


    #region [Method: Constructor]
    public AdvancedCircuitsPlugin(Main game): base(game) {
      this.pluginInfo = new PluginInfo(
        "Advanced Circuits",
        Assembly.GetAssembly(typeof(AdvancedCircuitsPlugin)).GetName().Version,
        "",
        "CoderCow",
        "Adds advanced wiring mechanics to Terraria servers."
      );

      this.Order = 50;
      #if DEBUG
      if (Debug.Listeners.Count == 0)
        Debug.Listeners.Add(new ConsoleTraceListener());
      #endif

      this.trace = new PluginTrace(AdvancedCircuitsPlugin.TracePrefix);
      this.hooksEnabled = false;

      AdvancedCircuitsPlugin.latestInstance = this;
    }
    #endregion

    #region [Methods: Initialize, Game_PostInitialize]
    public override void Initialize() {
      GameHooks.PostInitialize += this.Game_PostInitialize;

      this.AddHooks();
    }

    private void Game_PostInitialize() {
      GameHooks.PostInitialize -= this.Game_PostInitialize;

      if (!Directory.Exists(AdvancedCircuitsPlugin.AdvancedCircuitsDataDirectory))
        Directory.CreateDirectory(AdvancedCircuitsPlugin.AdvancedCircuitsDataDirectory);
      
      if (!this.InitConfig())
        return;
      if (!this.InitWorldMetdataHandler())
        return;

      this.pluginCooperationHandler = new PluginCooperationHandler(this.Trace);
      #if !Testrun
      this.circuitHandler = new CircuitHandler(
        this.Trace, this.Config, this.WorldMetadataHandler.Metadata, this.PluginCooperationHandler
      );
      #endif

      this.InitUserInteractionHandler();

      this.hooksEnabled = true;

      #if Testrun
      this.testRunner = new TestRunner(this.Trace, this.WorldMetadataHandler, this.PluginCooperationHandler);
      this.testRunner.RunAllTests();
      this.testRunner.TestRunCompleted += (sender, e) => {
        this.circuitHandler = new CircuitHandler(
          this.Trace, this.Config, this.WorldMetadataHandler.Metadata, this.PluginCooperationHandler
        );
        this.config.MaxCircuitLength = 5000;
      };
      #endif
    }

    private bool InitConfig() {
      if (File.Exists(AdvancedCircuitsPlugin.ConfigFilePath)) {
        try {
          this.config = Configuration.Read(AdvancedCircuitsPlugin.ConfigFilePath);
        } catch (Exception ex) {
          string exceptionDetailsText;
          if (ex is XmlSchemaException)
            exceptionDetailsText = ex.Message;
          else
            exceptionDetailsText = ex.ToString();

          this.Trace.WriteLineError(
            "Reading the configuration file failed. This plugin will be disabled. Exception details:\n{0}", exceptionDetailsText
          );

          return false;
        }
      } else {
        this.config = new Configuration();
        this.Trace.WriteLineWarning(string.Format(
          "Configuration file was not found at \"{0}\". Default settings will be used.", AdvancedCircuitsPlugin.ConfigFilePath
        ));
      }

      return true;
    }

    private bool InitWorldMetdataHandler() {
      this.worldMetadataHandler = new WorldMetadataHandler(this.Trace, AdvancedCircuitsPlugin.WorldMetadataDirectory);

      try {
        this.WorldMetadataHandler.InitOrReadMetdata();
        return true;
      } catch (Exception ex) {
        this.Trace.WriteLineError("Failed initializing or reading metdata or its backup. This plugin will be disabled. Exception details:\n" + ex);

        this.Dispose();
        return false;
      }
    }

    private void InitUserInteractionHandler() {
      Action reloadConfiguration = () => {
        if (this.isDisposed)
          return;

        this.config = Configuration.Read(AdvancedCircuitsPlugin.ConfigFilePath);
        if (this.circuitHandler != null) {
          this.circuitHandler = new CircuitHandler(
            this.Trace, this.Config, this.WorldMetadataHandler.Metadata, this.PluginCooperationHandler
          );
        }
      };
      this.userInteractionHandler = new UserInteractionHandler(
        this.Trace, this.PluginInfo, this.Config, this.WorldMetadataHandler.Metadata, this.PluginCooperationHandler, 
        reloadConfiguration
      );
    }
    #endregion

    #region [Methods: Server Hook Handling]
    private void AddHooks() {
      if (this.getDataHookHandler != null)
        throw new InvalidOperationException("Hooks already registered.");
      
      this.getDataHookHandler = new GetDataHookHandler(this.Trace, true);
      this.GetDataHookHandler.HitSwitch += this.Net_HitSwitch;
      this.GetDataHookHandler.TileEdit += this.Net_TileEdit;

      GameHooks.Update += this.Game_Update;
      WorldHooks.SaveWorld += this.World_SaveWorld;
    }

    private void RemoveHooks() {
      if (this.getDataHookHandler != null) 
        this.getDataHookHandler.Dispose();

      GameHooks.Update -= this.Game_Update;
      WorldHooks.SaveWorld -= this.World_SaveWorld;
      GameHooks.PostInitialize -= this.Game_PostInitialize;
    }

    private void Net_HitSwitch(object sender, TileLocationEventArgs e) {
      if (this.isDisposed || !this.hooksEnabled || e.Handled)
        return;

      if (this.CircuitHandler != null)
        e.Handled = this.CircuitHandler.HandleHitSwitch(e.Player, e.Location);
      //  NetMessage.SendData((int)PacketTypes.HitSwitch, -1, e.Msg.whoAmI, string.Empty, x, y);
    }

    private void Net_TileEdit(object sender, TileEditEventArgs e) {
      if (this.isDisposed || !this.hooksEnabled || e.Handled)
        return;

      #if DEBUG || Testrun
      if (e.EditType == TileEditType.DestroyWire) {
        e.Player.SendMessage(e.Location.ToString(), Color.Aqua);

        if (!TerrariaUtils.Tiles[e.Location].active)
          return;

        ObjectMeasureData measureData = TerrariaUtils.Tiles.MeasureObject(e.Location);
        e.Player.SendInfoMessage(string.Format(
          "Origin X: {0}, Origin Y: {1}, Active: {2}", measureData.OriginTileLocation.X, measureData.OriginTileLocation.Y, 
          TerrariaUtils.Tiles.ObjectHasActiveState(measureData)
        ));
      }
      #endif

      if (this.UserInteractionHandler.HandleTileEdit(e.Player, e.EditType, e.BlockType, e.Location, e.ObjectStyle)) {
        e.Handled = true;
        return;
      }
      
      e.Handled = this.WorldMetadataHandler.HandleTileEdit(e.Player, e.EditType, e.BlockType, e.Location, e.ObjectStyle);
    }

    private void World_SaveWorld(bool resettime, HandledEventArgs e) {
      if (this.isDisposed || e.Handled)
        return;

      try {
        this.WorldMetadataHandler.WriteMetadata();
      } catch (Exception ex) {
        this.Trace.WriteLineError("A Save World Handler caused an exception:\n{0}", ex.ToString());
      }
    }

    private void Game_Update() {
      if (this.CircuitHandler != null)
        this.CircuitHandler.HandleGameUpdate();

      #if Testrun
      if (this.testRunner.IsRunning)
        this.testRunner.HandleGameUpdate();
      #endif
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
        this.hooksEnabled = false;
        this.RemoveHooks();
        
        if (this.userInteractionHandler != null)
          this.userInteractionHandler.Dispose();
      }

      base.Dispose(isDisposing);
      this.isDisposed = true;
    }
    #endregion
  }
}
