using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using DPoint = System.Drawing.Point;

using Terraria.Plugins.Common;
using Terraria.Plugins.Common.Hooks;
using Terraria.Plugins.CoderCow.AdvancedCircuits.Test;

using TerrariaApi.Server;
using TShockAPI;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  [ApiVersion(1, 16)]
  public class AdvancedCircuitsPlugin: TerrariaPlugin, IDisposable {
    public const string TracePrefix = @"[Advanced Circuits] ";
    public const string ReloadCfg_Permission = "ac.reloadcfg";
    public const string TriggerTeleporter_Permission = "ac.trigger.teleporter";
    public const string WireTeleporter_Permission = "ac.wire.teleporter";
    public const string WireBoulder_Permission = "ac.wire.boulder";
    public const string TriggerBlockActivator_Permission = "ac.trigger.blockactivator";
    public const string WireSign_Permission = "ac.wire.sign";
    public const string TriggerSignCommand_Permission = "ac.trigger.signcommand";
    public const string PassiveTriggerSign_Permission = "ac.passivetrigger.sign";

    private bool hooksEnabled;
    #if Testrun
    private TestRunner testRunner;
    #endif

    public static AdvancedCircuitsPlugin LatestInstance { get; private set; }

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

    public PluginTrace Trace { get; private set; }
    protected PluginInfo PluginInfo { get; private set; }
    protected Configuration Config { get; private set; }
    protected GetDataHookHandler GetDataHookHandler { get; private set; }
    protected CircuitHandler CircuitHandler { get; private set; }
    protected UserInteractionHandler UserInteractionHandler { get; private set; }
    protected WorldMetadataHandler WorldMetadataHandler { get; private set; }
    public PluginCooperationHandler PluginCooperationHandler { get; private set; }


    public AdvancedCircuitsPlugin(Main game): base(game) {
      this.PluginInfo = new PluginInfo(
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

      this.Trace = new PluginTrace(AdvancedCircuitsPlugin.TracePrefix);
      this.hooksEnabled = false;

      AdvancedCircuitsPlugin.LatestInstance = this;
    }

    public override void Initialize() {
      ServerApi.Hooks.GamePostInitialize.Register(this, this.Game_PostInitialize);

      this.AddHooks();
    }

    private void Game_PostInitialize(EventArgs e) {
      ServerApi.Hooks.GamePostInitialize.Deregister(this, this.Game_PostInitialize);

      if (!Directory.Exists(AdvancedCircuitsPlugin.AdvancedCircuitsDataDirectory))
        Directory.CreateDirectory(AdvancedCircuitsPlugin.AdvancedCircuitsDataDirectory);
      
      if (!this.InitConfig())
        return;
      if (!this.InitWorldMetdataHandler())
        return;

      this.PluginCooperationHandler = new PluginCooperationHandler(this.Trace);
      #if !Testrun
      this.CircuitHandler = new CircuitHandler(
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
          this.Config = Configuration.Read(AdvancedCircuitsPlugin.ConfigFilePath);
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
        this.Config = new Configuration();
        this.Trace.WriteLineWarning(string.Format(
          "Configuration file was not found at \"{0}\". Default settings will be used.", AdvancedCircuitsPlugin.ConfigFilePath
        ));
      }

      return true;
    }

    private bool InitWorldMetdataHandler() {
      this.WorldMetadataHandler = new WorldMetadataHandler(this.Trace, AdvancedCircuitsPlugin.WorldMetadataDirectory);

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

        this.Config = Configuration.Read(AdvancedCircuitsPlugin.ConfigFilePath);
        if (this.CircuitHandler != null) {
          this.CircuitHandler = new CircuitHandler(
            this.Trace, this.Config, this.WorldMetadataHandler.Metadata, this.PluginCooperationHandler
          );
        }
      };
      this.UserInteractionHandler = new UserInteractionHandler(
        this.Trace, this.PluginInfo, this.Config, this.WorldMetadataHandler.Metadata, this.PluginCooperationHandler, 
        reloadConfiguration
      );
    }

    #region [Methods: Server Hook Handling]
    private void AddHooks() {
      if (this.GetDataHookHandler != null)
        throw new InvalidOperationException("Hooks already registered.");
      
      this.GetDataHookHandler = new GetDataHookHandler(this, true, -10);
      this.GetDataHookHandler.HitSwitch += this.Net_HitSwitch;
      this.GetDataHookHandler.TileEdit += this.Net_TileEdit;
      this.GetDataHookHandler.TilePaint += this.Net_TilePaint;
      this.GetDataHookHandler.DoorUse += this.Net_DoorUse;
      this.GetDataHookHandler.SendTileSquare += this.Net_SendTileSquare;

      ServerApi.Hooks.GameUpdate.Register(this, this.Game_Update);
      ServerApi.Hooks.WorldSave.Register(this, this.World_SaveWorld);

      try {
        this.AddExperimentalHooks();
      } catch {}
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddExperimentalHooks() {
      ServerApi.Hooks.NpcTriggerPressurePlate.Register(this, this.Npc_TriggerPressurePlate);
      ServerApi.Hooks.ProjectileTriggerPressurePlate.Register(this, this.Projectile_TriggerPressurePlate);
    }

    private void RemoveHooks() {
      if (this.GetDataHookHandler != null) 
        this.GetDataHookHandler.Dispose();

      ServerApi.Hooks.GameUpdate.Register(this, this.Game_Update);
      ServerApi.Hooks.WorldSave.Register(this, this.World_SaveWorld);

      try {
        this.RemoveExperimentalHooks();
      } catch {}
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void RemoveExperimentalHooks() {
      ServerApi.Hooks.NpcTriggerPressurePlate.Deregister(this, this.Npc_TriggerPressurePlate);
      ServerApi.Hooks.ProjectileTriggerPressurePlate.Deregister(this, this.Projectile_TriggerPressurePlate);
    }

    private void Net_HitSwitch(object sender, TileLocationEventArgs e) {
      if (this.isDisposed || !this.hooksEnabled || e.Handled)
        return;

      if (this.CircuitHandler != null)
        e.Handled = this.CircuitHandler.HandleHitSwitch(e.Player, e.Location);
    }

    private void Net_DoorUse(object sender, DoorUseEventArgs e) {
      if (this.isDisposed || !this.hooksEnabled || e.Handled)
        return;

      if (this.CircuitHandler != null)
        e.Handled = this.CircuitHandler.HandleDoorUse(e.Player, e.Location, e.IsOpening, null, e.Direction);
    }

    private void Net_TileEdit(object sender, TileEditEventArgs e) {
      if (this.isDisposed || !this.hooksEnabled || e.Handled)
        return;

      if (this.UserInteractionHandler.HandleTileEdit(e.Player, e.EditType, e.BlockType, e.Location, e.ObjectStyle)) {
        e.Handled = true;
        return;
      }
      
      e.Handled = this.WorldMetadataHandler.HandleTileEdit(e.Player, e.EditType, e.BlockType, e.Location, e.ObjectStyle);
    }

    private void Net_TilePaint(object sender, TilePaintEventArgs e) {
      if (this.isDisposed || !this.hooksEnabled || e.Handled)
        return;
      
      e.Handled = this.UserInteractionHandler.HandleTilePaint(e.Player, e.Location, e.Color);
    }

    private void Net_SendTileSquare(object sender, SendTileSquareEventArgs e) {
      if (this.isDisposed || !this.hooksEnabled || e.Handled)
        return;

      e.Handled = this.CircuitHandler.HandleSendTileSquare(e.Player, e.Location, e.Size);
    }

    private void Npc_TriggerPressurePlate(TriggerPressurePlateEventArgs<NPC> e) {
      if (this.isDisposed || !this.hooksEnabled || e.Handled)
        return;

      if (this.CircuitHandler != null)
        e.Handled = this.CircuitHandler.HandleTriggerPressurePlate(TSPlayer.Server, new DPoint(e.TileX, e.TileY));
    }

    /*private void Npc_UseDoor(NpcUseDoorEventArgs e) {
      if (this.isDisposed || !this.hooksEnabled || e.Handled)
        return;

      if (this.CircuitHandler != null)
        e.Handled = this.CircuitHandler.HandleDoorUse(TSPlayer.Server, new DPoint(e.X, e.Y), e.IsOpening, e.Npc);
    }*/

    private void Projectile_TriggerPressurePlate(TriggerPressurePlateEventArgs<Projectile> e) {
      if (this.isDisposed || !this.hooksEnabled || e.Handled)
        return;

      if (this.CircuitHandler != null)
        e.Handled = this.CircuitHandler.HandleTriggerPressurePlate(TSPlayer.Server, new DPoint(e.TileX, e.TileY), true);
    }

    private void World_SaveWorld(WorldSaveEventArgs e) {
      if (this.isDisposed || !this.hooksEnabled || e.Handled)
        return;

      try {
        this.WorldMetadataHandler.WriteMetadata();
      } catch (Exception ex) {
        this.Trace.WriteLineError("A Save World Handler has thrown an exception:\n{0}", ex.ToString());
      }
    }

    private void Game_Update(EventArgs e) {
      if (this.isDisposed || !this.hooksEnabled)
        return;

      try {
        if (this.CircuitHandler != null)
          this.CircuitHandler.HandleGameUpdate();

        #if Testrun
        if (this.testRunner.IsRunning)
          this.testRunner.HandleGameUpdate();
        #endif
      } catch (Exception ex) {
        this.Trace.WriteLineError("A Game Update Handler has thrown an exception:\n{0}", ex.ToString());
      }
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
        
        if (this.UserInteractionHandler != null)
          this.UserInteractionHandler.Dispose();
      }

      base.Dispose(isDisposing);
      this.isDisposed = true;
    }
    #endregion
  }
}
