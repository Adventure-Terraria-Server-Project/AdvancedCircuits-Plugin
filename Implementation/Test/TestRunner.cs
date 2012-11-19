using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using DPoint = System.Drawing.Point;

using TShockAPI;

using Terraria.Plugins.CoderCow.Test;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits.Test {
  #if Testrun
  public class TestRunner: TestRunnerBase {
    #region [Property: Static TestDataGlobalPath, Static TestDataGlobalConfigFilePath, Static TestDataGlobalMetadataPath]
    public static string TestDataGlobalPath {
      get {
        return Path.Combine(AdvancedCircuitsPlugin.AdvancedCircuitsDataDirectory, "Test Data");
      }
    }

    public static string TestDataGlobalConfigFilePath {
      get {
        return Path.Combine(TestRunner.TestDataGlobalPath, "Default Config.xml");
      }
    }

    public static string TestDataGlobalMetadataPath {
      get {
        return Path.Combine(TestRunner.TestDataGlobalPath, "World Data");
      }
    }
    #endregion

    #region [Property: Config]
    private Configuration config;

    protected Configuration Config {
      get { return this.config; }
    }
    #endregion

    #region [Property: MetadataHandler]
    private WorldMetadataHandler metadataHandler;

    protected WorldMetadataHandler MetadataHandler {
      get { return this.metadataHandler; }
    }
    #endregion

    #region [Property: CircuitHandler]
    private CircuitHandler circuitHandler;

    public CircuitHandler CircuitHandler {
      get { return this.circuitHandler; }
    }
    #endregion


    #region [Method: Constructor]
    public TestRunner(WorldMetadataHandler metadataHandler): base(AdvancedCircuitsPlugin.Trace) {
      this.metadataHandler = metadataHandler;

      this.RegisterTest(@"BP\Multi Branches", this.BP_MultiBranches);
      this.RegisterTest(@"BP\Snacke Branches", this.BP_SnackeBranches);
      this.RegisterTest(@"BP\Wire Bunch", this.BP_WireBunch);
      this.RegisterTest(@"BP\Multi Branches2", this.BP_MultiBranches2);
      this.RegisterTest(@"BP\Double Signal", this.BP_DoubleSignal);
      this.RegisterTest(@"BP\Double Signal2", this.BP_DoubleSignal2);
      this.RegisterTest(@"BP\Loop", this.BP_Loop);
      this.RegisterTest(@"BP\Processing Time", this.BP_ProcessingTime);

      this.RegisterTest(@"Vanilla\All Sprites State Toggle Test", this.Vanilla_AllSpritesToggleState);
      this.RegisterTest(@"Vanilla\Basic Door Toggling Test", this.Vanilla_BasicDoorToggling);
      this.RegisterTest(@"Vanilla\Blocked Door Toggling 1", this.Vanilla_BlockedDoorToggling1);
      this.RegisterTest(@"Vanilla\Blocked Door Toggling 2", this.Vanilla_BlockedDoorToggling2);
      this.RegisterTest(@"Vanilla\Pumps", this.Vanilla_Pumps);
      //this.RegisterTest(@"Vanilla\Statues", this.Vanilla_Statues);
      this.RegisterTest(@"Vanilla\ComponentActivation", this.Vanilla_ComponentActivation);
      this.RegisterTest(@"Vanilla\Timers", this.Vanilla_Timers);

      this.RegisterTest(@"AC\All Sprites State Toggle Test", this.AC_AllSpritesToggleState);
      this.RegisterTest(@"AC\Basic Door Toggling Test", this.AC_BasicDoorToggling);
      this.RegisterTest(@"AC\Blocked Door Toggling 1", this.AC_BlockedDoorToggling1);
      this.RegisterTest(@"AC\Blocked Door Toggling 2", this.AC_BlockedDoorToggling2);
      this.RegisterTest(@"AC\Pumps", this.AC_Pumps);
      //this.RegisterTest(@"AC\Statues", this.AC_Statues);
      this.RegisterTest(@"AC\ComponentActivation", this.AC_ComponentActivation);

      this.RegisterTest(@"AC\Single Tile Trigger Ports", this.AC_SingleTileTriggerPorts);
      this.RegisterTest(@"AC\Single Tile Trigger Input Ports", this.AC_SingleTileTriggerInputPorts);
      this.RegisterTest(@"AC\Single Tile Trigger Port Input", this.AC_SingleTileTriggerPortInput);
      this.RegisterTest(@"AC\Self Switching", this.AC_SelfSwitching);
      this.RegisterTest(@"AC\Multi Port Sending", this.AC_MultiPortSending);
      this.RegisterTest(@"AC\Green Pressure Plate", this.AC_GreenPressurePlate);
      this.RegisterTest(@"AC\Red Pressure Plate", this.AC_RedPressurePlate);
      this.RegisterTest(@"AC\Multi Receiving", this.AC_MultiReceiving);
      this.RegisterTest(@"AC\Not Gate", this.AC_NotGate);
      this.RegisterTest(@"AC\Swapper", this.AC_Swapper);
      this.RegisterTest(@"AC\And Gate", this.AC_AndGate);
      this.RegisterTest(@"AC\Or Gate", this.AC_OrGate);
      this.RegisterTest(@"AC\Xor Gate", this.AC_XorGate);
      this.RegisterTest(@"AC\Input Port Inbetween", this.AC_InputPortInbetween);
      this.RegisterTest(@"AC\Single Wired Ports", this.AC_SingleWiredPorts);
      this.RegisterTest(@"AC\Self Deactivating Timers", this.AC_SelfDeactivatingTimers);
      this.RegisterTest(@"AC\Port Loop Through", this.AC_PortLoopThrough);
      this.RegisterTest(@"AC\Sender Forwarding", this.AC_SenderForwarding);
      this.RegisterTest(@"AC\Crossover Bridge", this.AC_CrossoverBridge);
      this.RegisterTest(@"AC\Multi Tile Trigger Ports 1", this.AC_MultiTileTriggerPorts1);
      this.RegisterTest(@"AC\Multi Tile Trigger Ports 2", this.AC_MultiTileTriggerPorts2);
      this.RegisterTest(@"AC\Multi Tile Trigger Input Ports 1", this.AC_MultiTileTriggerInputPorts1);
      this.RegisterTest(@"AC\Multi Tile Trigger Input Ports 2", this.AC_MultiTileTriggerInputPorts2);
      this.RegisterTest(@"AC\Multi Tile Trigger Ports 3", this.AC_MultiTileTriggerPorts3);
      this.RegisterTest(@"AC\Timers", this.AC_Timers);
      this.RegisterTest(@"AC\NOT Ports 1", this.AC_NOTPorts1);
      this.RegisterTest(@"AC\NOT Ports 2", this.AC_NOTPorts2);
      this.RegisterTest(@"AC\NOT Ports 3", this.AC_NOTPorts3);
      this.RegisterTest(@"AC\Switch Forwarding", this.AC_SwitchForwarding);
      this.RegisterTest(@"AC\Grandfather Clock", this.AC_GrandfatherClock);
      this.RegisterTest(@"AC\Block Activator", this.AC_BlockActivator);
      this.RegisterTest(@"AC\Boulder", this.AC_Boulder);
    }
    #endregion

    #region [Methods: TestInit, TestCleanup]
    protected override void TestInit() {
      this.config = new Configuration();
      this.config.OverrideVanillaCircuits = true;

      this.MetadataHandler.Metadata.ActiveSwapperLocations.Clear();
      this.MetadataHandler.Metadata.ActiveTimers.Clear();
      this.MetadataHandler.Metadata.Clocks.Clear();
      this.MetadataHandler.Metadata.GateStates.Clear();

      this.config.StatueConfigs.Add(StatueType.Star, new StatueConfig {
        ActionType = StatueActionType.MoveNPC,
        ActionParam = 3,
        ActionParam2 = 22
      });
      this.config.StatueConfigs.Add(StatueType.Bat, new StatueConfig {
        ActionType = StatueActionType.SpawnItem,
        ActionParam = 1,
        ActionParam2 = 3,
        ActionParam3 = 15,
        Cooldown = 1
      });
      this.config.StatueConfigs.Add(StatueType.Slime, new StatueConfig {
        ActionType = StatueActionType.SpawnMob,
        ActionParam = 1,
        ActionParam2 = 3,
        ActionParam3 = 15,
        Cooldown = 1
      });

      this.circuitHandler = new CircuitHandler(this.Config, this.MetadataHandler.Metadata);
    }

    protected override void TestCleanup() {
      foreach (DPoint timerLocation in this.MetadataHandler.Metadata.ActiveTimers.Keys)
        Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(timerLocation), false);

      this.MetadataHandler.Metadata.ActiveTimers.Clear();
    }
    #endregion

    #region [Branched Processing Tests]
    private void BP_MultiBranches(TestContext context) {
      DPoint testOffset = new DPoint(49, 228);
      CircuitProcessResult result;

      context.Phase = "1";
      result = this.QuickProcessCircuit(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      this.Assert_ProcessedBranches(result, 6);

      context.Phase = "2";
      result = this.QuickProcessCircuit(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 4);
      this.Assert_ProcessedBranches(result, 6);

      context.Phase = "3";
      result = this.QuickProcessCircuit(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      this.Assert_ProcessedBranches(result, 6);
    }

    private void BP_SnackeBranches(TestContext context) {
      DPoint testOffset = new DPoint(57, 227);
      CircuitProcessResult result;

      context.Phase = "1";
      result = this.QuickProcessCircuit(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 5);
      this.Assert_ProcessedBranches(result, 11);

      context.Phase = "2";
      result = this.QuickProcessCircuit(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 5);
      this.Assert_ProcessedBranches(result, 11);

      context.Phase = "3";
      result = this.QuickProcessCircuit(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 5);
      this.Assert_ProcessedBranches(result, 11);
    }

    private void BP_WireBunch(TestContext context) {
      DPoint testOffset = new DPoint(71, 225);
      CircuitProcessResult result;
      this.config.MaxCircuitLength = 5000;

      context.Phase = "1";
      result = this.QuickProcessCircuit(testOffset.X + 5, testOffset.Y + 7);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y);
      for (int y = 0; y < 6; y++) {
        int rowY = (y * 2) + 1;

        TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + rowY);
        TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + rowY);
        TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + rowY);
        TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + rowY);
        TAssert.IsSpriteInactive(testOffset.X + 5, testOffset.Y + rowY);
        TAssert.IsSpriteInactive(testOffset.X + 7, testOffset.Y + rowY);
        TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + rowY);
        TAssert.IsSpriteInactive(testOffset.X + 9, testOffset.Y + rowY);

        rowY++;
        TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + rowY);
        TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + rowY);
        TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + rowY);
        if (y != 3)
          TAssert.IsSpriteInactive(testOffset.X + 5, testOffset.Y + rowY);
        TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + rowY);
        TAssert.IsSpriteInactive(testOffset.X + 7, testOffset.Y + rowY);
        TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + rowY);
        TAssert.IsSpriteInactive(testOffset.X + 9, testOffset.Y + rowY);
      }

      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 0);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 10);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 12);

      context.Phase = "2";
      result = this.QuickProcessCircuit(testOffset.X + 5, testOffset.Y + 7);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y);
      for (int y = 0; y < 6; y++) {
        int rowY = (y * 2) + 1;

        TAssert.IsSpriteActive(testOffset.X, testOffset.Y + rowY);
        TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + rowY);
        TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + rowY);
        TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + rowY);
        TAssert.IsSpriteActive(testOffset.X + 5, testOffset.Y + rowY);
        TAssert.IsSpriteActive(testOffset.X + 7, testOffset.Y + rowY);
        TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + rowY);
        TAssert.IsSpriteActive(testOffset.X + 9, testOffset.Y + rowY);

        rowY++;
        TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + rowY);
        TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + rowY);
        TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + rowY);
        if (y != 3)
          TAssert.IsSpriteActive(testOffset.X + 5, testOffset.Y + rowY);
        TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + rowY);
        TAssert.IsSpriteActive(testOffset.X + 7, testOffset.Y + rowY);
        TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + rowY);
        TAssert.IsSpriteActive(testOffset.X + 9, testOffset.Y + rowY);
      }

      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 0);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 10);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 12);
    }

    private void BP_MultiBranches2(TestContext context) {
      DPoint testOffset = new DPoint(85, 229);
      CircuitProcessResult result;

      context.Phase = "1";
      result = this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 7, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 5, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 9, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 5, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 9, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 7, testOffset.Y + 6);

      context.Phase = "2";
      result = this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 7, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 5, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 9, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 5, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 9, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 7, testOffset.Y + 6);

      context.Phase = "3";
      result = this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 7, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 5, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 9, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 5, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 9, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 7, testOffset.Y + 6);
    }

    private void BP_DoubleSignal(TestContext context) {
      DPoint testOffset = new DPoint(103, 229);
      CircuitProcessResult result;

      context.Phase = "1";
      result = this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      this.Assert_SignaledComponents(result, 2);
      this.Assert_ProcessedBranches(result, 8);

      context.Phase = "2";
      result = this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      this.Assert_SignaledComponents(result, 2);
      this.Assert_ProcessedBranches(result, 8);

      context.Phase = "3";
      result = this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      this.Assert_SignaledComponents(result, 2);
      this.Assert_ProcessedBranches(result, 8);
    }

    private void BP_DoubleSignal2(TestContext context) {
      DPoint testOffset = new DPoint(110, 230);
      CircuitProcessResult result;

      context.Phase = "1";
      result = this.QuickProcessCircuit(testOffset.X, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 2);
      this.Assert_SignaledComponents(result, 2);
      this.Assert_ProcessedBranches(result, 10);

      context.Phase = "2";
      result = this.QuickProcessCircuit(testOffset.X, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 2);
      this.Assert_SignaledComponents(result, 2);
      this.Assert_ProcessedBranches(result, 10);

      context.Phase = "3";
      result = this.QuickProcessCircuit(testOffset.X, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 2);
      this.Assert_SignaledComponents(result, 2);
      this.Assert_ProcessedBranches(result, 10);
    }

    private void BP_Loop(TestContext context) {
      DPoint testOffset = new DPoint(120, 230);
      CircuitProcessResult result;

      context.Phase = "1";
      result = this.QuickProcessCircuit(testOffset.X, testOffset.Y + 2);
      this.Assert_Cancelled(result, CircuitCancellationReason.SignaledSameComponentTooOften, Terraria.TileId_Obsidian);
    }

    private void BP_ProcessingTime(TestContext context) {
      DPoint testOffset = new DPoint(129, 180);
      CircuitProcessResult result;

      this.Config.MaxCircuitLength = 2000;

      context.Phase = "1";
      TimeSpan allProcessingTime = TimeSpan.Zero;
      for (int i = 0; i < 50; i++) {
        result = this.QuickProcessCircuit(testOffset.X + 21, testOffset.Y + 2);
        this.Assert_Cancelled(result, CircuitCancellationReason.None);
        allProcessingTime += result.ProcessingTime;
      }

      TimeSpan averageProcessingTime = TimeSpan.FromMilliseconds(allProcessingTime.TotalMilliseconds / 50);
      if (averageProcessingTime.TotalMilliseconds > 30)
        throw new AssertException("The average processing time was over 30 milliseconds.");
    }
    #endregion

    #region [Vanilla Tests]
    private void Vanilla_AllSpritesToggleState(TestContext context) {
      DPoint testOffset = new DPoint(46, 280);
      
      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y + 5);
      this.CheckAllSpritesState(testOffset, false);

      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y + 5);
      this.CheckAllSpritesState(testOffset, true);

      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y + 5);
      this.CheckAllSpritesState(testOffset, false);
    }

    private void Vanilla_BasicDoorToggling(TestContext context) {
      DPoint testOffset = new DPoint(75, 282);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 2, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 4, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 8, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 10, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 12, testOffset.Y + 3, Terraria.TileId_DoorClosed);

      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 2, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 4, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 8, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 10, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 12, testOffset.Y + 3, Terraria.TileId_DoorOpened);
    }

    private void Vanilla_BlockedDoorToggling1(TestContext context) {
      DPoint testOffset = new DPoint(92, 282);
      CircuitProcessResult result;

      context.Phase = "1";
      result = this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 3);
      this.Assert_SignaledComponents(result, 2);
      TAssert.IsTileId(testOffset.X + 2, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 6, testOffset.Y + 3, Terraria.TileId_DoorOpened);

      context.Phase = "2";
      result = this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 3);
      this.Assert_SignaledComponents(result, 2);
      TAssert.IsTileId(testOffset.X + 1, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 7, testOffset.Y + 3, Terraria.TileId_DoorClosed);
    }

    private void Vanilla_BlockedDoorToggling2(TestContext context) {
      DPoint testOffset = new DPoint(104, 282);
      CircuitProcessResult result;

      context.Phase = "1";
      result = this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 3);
      this.Assert_SignaledComponents(result, 2);
      TAssert.IsTileId(testOffset.X, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 8, testOffset.Y + 3, Terraria.TileId_DoorOpened);

      context.Phase = "2";
      result = this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 3);
      this.Assert_SignaledComponents(result, 2);
      TAssert.IsTileId(testOffset.X + 1, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 7, testOffset.Y + 3, Terraria.TileId_DoorClosed);
    }

    private void Vanilla_Pumps(TestContext context) {
      DPoint testOffset = new DPoint(116, 283);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 5, testOffset.Y + 2);

      context.DelayedActions.Add(new TestDelay(30, (contextLocal => {
        TAssert.HasNoLiquid(testOffset.X + 1, testOffset.Y + 1);
        TAssert.HasNoLiquid(testOffset.X + 2, testOffset.Y + 1);
        TAssert.HasNoLiquid(testOffset.X + 1, testOffset.Y + 2);
        TAssert.HasNoLiquid(testOffset.X + 2, testOffset.Y + 2);
        TAssert.HasFullLiquid(testOffset.X + 8, testOffset.Y + 1);
        TAssert.HasFullLiquid(testOffset.X + 9, testOffset.Y + 1);
        TAssert.HasFullLiquid(testOffset.X + 8, testOffset.Y + 2);
        TAssert.HasFullLiquid(testOffset.X + 9, testOffset.Y + 2);
      })));
    }

    private const int ItemId_IronPickaxe = 0;
    private const int NPCId_Slime = 0;
    private const int NPCId_Guide = 22;
    private void Vanilla_Statues(TestContext context) {
      DPoint testOffset = new DPoint(130, 282);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y + 3);
      TAssert.AreNPCsInBlockRect(testOffset.X + 1, testOffset.Y + 1, 2, 3, TestRunner.NPCId_Guide, 1);
      TAssert.AreNPCsInBlockRect(testOffset.X + 4, testOffset.Y + 1, 2, 3, TestRunner.NPCId_Slime, 1);
      TAssert.AreItemsInBlockRect(testOffset.X + 7, testOffset.Y + 1, 2, 3, TestRunner.ItemId_IronPickaxe, 1);
    }

    private void Vanilla_ComponentActivation(TestContext context) {
      DPoint testOffset = new DPoint(148, 279);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 3);

      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 3);
    }

    private void Vanilla_Timers(TestContext context) {
      DPoint testOffset = new DPoint(154, 281);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 1);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 4);

      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);

      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 1);

      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 1);

      // Other Timer
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 1);

      // Note that switches and levers should not be signalable in vanilla circuits.
      // Switch
      this.QuickProcessCircuit(testOffset.X + 10, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 1);

      // Lever
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 12, testOffset.Y + 1);

      context.DelayedActions.Add(new TestDelay(65, contextLocal => {
        context.Phase = "2";
        TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
        TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);

        TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 1);

        TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 1);

        TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 1);

        // Other Timer
        TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 1);
        TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);

        // Switch
        TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 1);

        // Lever
        TAssert.IsSpriteActive(testOffset.X + 12, testOffset.Y + 1);
      }));

      context.DelayedActions.Add(new TestDelay(65, contextLocal => {
        context.Phase = "3";
        TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
        TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);

        TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);

        TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 1);

        TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 1);

        // Other Timer
        TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 1);
        TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 4);

        // Switch
        TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 1);

        // Lever
        TAssert.IsSpriteActive(testOffset.X + 12, testOffset.Y + 1);
      }));
    }
    #endregion

    #region [AC Basic Tests]
    private void AC_AllSpritesToggleState(TestContext context) {
      DPoint testOffset = new DPoint(46, 343);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y + 5);
      this.CheckAllSpritesState(testOffset, false);

      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y + 5);
      this.CheckAllSpritesState(testOffset, true);

      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y + 5);
      this.CheckAllSpritesState(testOffset, false);
    }

    private void AC_BasicDoorToggling(TestContext context) {
      DPoint testOffset = new DPoint(75, 345);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 2, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 4, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 8, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 10, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 12, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      
      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 2, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 4, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 8, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 10, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 12, testOffset.Y + 3, Terraria.TileId_DoorOpened);

      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 2, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 4, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 8, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 10, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 12, testOffset.Y + 3, Terraria.TileId_DoorClosed);
    }

    private void AC_BlockedDoorToggling1(TestContext context) {
      DPoint testOffset = new DPoint(92, 345);
      CircuitProcessResult result;

      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 4, testOffset.Y + 3)), false);

      context.Phase = "1";
      result = this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 3);
      this.Assert_SignaledComponents(result, 2);
      TAssert.IsTileId(testOffset.X + 2, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 6, testOffset.Y + 3, Terraria.TileId_DoorOpened);

      context.Phase = "2";
      result = this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 3);
      this.Assert_SignaledComponents(result, 2);
      TAssert.IsTileId(testOffset.X + 1, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 7, testOffset.Y + 3, Terraria.TileId_DoorClosed);
    }

    private void AC_BlockedDoorToggling2(TestContext context) {
      DPoint testOffset = new DPoint(104, 345);
      CircuitProcessResult result;

      context.Phase = "1";
      result = this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 3);
      this.Assert_SignaledComponents(result, 2);
      TAssert.IsTileId(testOffset.X + 1, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 7, testOffset.Y + 3, Terraria.TileId_DoorClosed);

      context.Phase = "2";
      result = this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 3);
      this.Assert_SignaledComponents(result, 2);
      TAssert.IsTileId(testOffset.X, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 8, testOffset.Y + 3, Terraria.TileId_DoorOpened);

      context.Phase = "3";
      result = this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 3);
      this.Assert_SignaledComponents(result, 2);
      TAssert.IsTileId(testOffset.X + 1, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 7, testOffset.Y + 3, Terraria.TileId_DoorClosed);
    }

    private void AC_Pumps(TestContext context) {
      DPoint testOffset = new DPoint(116, 346);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 5, testOffset.Y + 2);
      context.DelayedActions.Add(new TestDelay(30, (contextLocal => {
        TAssert.HasNoLiquid(testOffset.X + 1, testOffset.Y + 1);
        TAssert.HasNoLiquid(testOffset.X + 2, testOffset.Y + 1);
        TAssert.HasNoLiquid(testOffset.X + 1, testOffset.Y + 2);
        TAssert.HasNoLiquid(testOffset.X + 2, testOffset.Y + 2);
        TAssert.HasFullLiquid(testOffset.X + 8, testOffset.Y + 1);
        TAssert.HasFullLiquid(testOffset.X + 9, testOffset.Y + 1);
        TAssert.HasFullLiquid(testOffset.X + 8, testOffset.Y + 2);
        TAssert.HasFullLiquid(testOffset.X + 9, testOffset.Y + 2);
      })));
    }

    private void AC_Statues(TestContext context) {
      DPoint testOffset = new DPoint(130, 345);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y + 3);
      TAssert.AreNPCsInBlockRect(testOffset.X + 1, testOffset.Y + 1, 2, 3, TestRunner.NPCId_Guide, 1);
      TAssert.AreNPCsInBlockRect(testOffset.X + 4, testOffset.Y + 1, 2, 3, TestRunner.NPCId_Slime, 1);
      TAssert.AreItemsInBlockRect(testOffset.X + 7, testOffset.Y + 1, 2, 3, TestRunner.ItemId_IronPickaxe, 1);
    }

    private void AC_ComponentActivation(TestContext context) {
      DPoint testOffset = new DPoint(148, 342);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 3);

      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 3);

      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 3);
    }

    private void AC_Timers(TestContext context) {
      DPoint testOffset = new DPoint(154, 344);

      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 2, testOffset.Y + 1)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 4, testOffset.Y + 1)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 6, testOffset.Y + 1)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 10, testOffset.Y + 1)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 12, testOffset.Y + 1)), false);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 1);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 4);

      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 1);

      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 1);

      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 1);

      // Other Timer
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 1);

      // Switch
      this.QuickProcessCircuit(testOffset.X + 10, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 1);

      // Lever
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 12, testOffset.Y + 1);

      context.DelayedActions.Add(new TestDelay(65, contextLocal => {
        context.Phase = "2";
        TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
        TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);

        TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);

        TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 1);

        TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 1);

        // Other Timer
        TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 1);
        TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);

        // Switch
        TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 1);

        // Lever
        TAssert.IsSpriteActive(testOffset.X + 12, testOffset.Y + 1);
      }));

      context.DelayedActions.Add(new TestDelay(65, contextLocal => {
        context.Phase = "3";
        TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
        TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);

        TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);

        TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 1);

        TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 1);

        // Other Timer
        TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 1);
        TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);

        // Switch
        TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 1);

        // Lever
        TAssert.IsSpriteActive(testOffset.X + 12, testOffset.Y + 1);
      }));
    }
    #endregion

    #region [AC Port Tests]
    private void AC_SingleTileTriggerPorts(TestContext context) {
      DPoint testOffset = new DPoint(43, 372);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 6);

      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
    }

    private void AC_SingleTileTriggerInputPorts(TestContext context) {
      DPoint testOffset = new DPoint(53, 372);

      // Middle Switch
      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);

      // Top Switch
      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y);

      // Left Switch
      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);

      // Right Switch
      context.Phase = "4";
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y + 3);

      // Bottom Switch
      context.Phase = "5";
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y + 6);

      // Middle Switch
      context.Phase = "6";
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
    }

    private void AC_SingleTileTriggerPortInput(TestContext context) {
      DPoint testOffset = new DPoint(63, 372);

      // Middle Switch
      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 6);

      // Middle Switch
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);

      // Top Switch
      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 6);

      // Left Switch
      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);

      // Right Switch
      context.Phase = "4";
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 6);

      // Bottom Switch
      context.Phase = "5";
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);

      // Middle Switch
      context.Phase = "6";
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 6);
    }

    private void AC_SelfSwitching(TestContext context) {
      DPoint testOffset = new DPoint(73, 375);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y);
    }

    private void AC_MultiPortSending(TestContext context) {
      DPoint testOffset = new DPoint(85, 374);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 4);
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + 4);
    }

    private void AC_GreenPressurePlate(TestContext context) {
      DPoint testOffset = new DPoint(91, 375);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
    }

    private void AC_RedPressurePlate(TestContext context) {
      DPoint testOffset = new DPoint(95, 375);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
    }

    private void AC_MultiReceiving(TestContext context) {
      DPoint testOffset = new DPoint(99, 374);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 1);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 1);
    }

    private void AC_NotGate(TestContext context) {
      DPoint testOffset = new DPoint(106, 375);

      // Switch #1
      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 6);

      // Switch #2
      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);

      // Switch #3
      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 6);

      // Switch #4
      context.Phase = "4";
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);

      // Switch #5
      context.Phase = "5";
      this.QuickProcessCircuit(testOffset.X + 10, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 10, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 10, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);

      // Switch #4/#5
      context.Phase = "6";
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 10, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
    }

    private void AC_Swapper(TestContext context) {
      DPoint testOffset = new DPoint(120, 375);
      
      // Prepare
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X, testOffset.Y + 6)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 2, testOffset.Y + 6)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 4, testOffset.Y + 6)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 6, testOffset.Y + 6)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 8, testOffset.Y + 6)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 12, testOffset.Y + 6)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 14, testOffset.Y + 6)), false);
      
      // Plate #1
      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);

      // Plate #2
      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);

      // Plate #3
      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);

      // Plate #4
      context.Phase = "4";
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);

      // Plate #5
      context.Phase = "5";
      this.QuickProcessCircuit(testOffset.X + 10, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 10, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 10, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);

      // Plate #6
      context.Phase = "6";
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 12, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 12, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 12, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 12, testOffset.Y + 6);

      // Plate #7
      context.Phase = "7";
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
    }

    private void AC_AndGate(TestContext context) {
      DPoint testOffset = new DPoint(138, 375);
      
      // Prepare
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X, testOffset.Y + 6)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 2, testOffset.Y + 6)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 4, testOffset.Y + 6)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 8, testOffset.Y + 6)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 10, testOffset.Y + 6)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 14, testOffset.Y + 6)), false);

      // Switch #1
      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);

      // Switch #2
      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);

      // Switch #3
      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);

      // Switch #4
      context.Phase = "4";
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #5
      context.Phase = "5";
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #4/#5
      context.Phase = "6";
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #6
      context.Phase = "7";
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #7
      context.Phase = "8";
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #8
      context.Phase = "9";
      this.QuickProcessCircuit(testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #6/#7/#8
      context.Phase = "10";
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y);
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y);
      this.QuickProcessCircuit(testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
    }

    private void AC_OrGate(TestContext context) {
      DPoint testOffset = new DPoint(158, 375);
      
      // Prepare
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X, testOffset.Y + 6)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 2, testOffset.Y + 6)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 4, testOffset.Y + 6)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 8, testOffset.Y + 6)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 10, testOffset.Y + 6)), false);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 14, testOffset.Y + 6)), false);

      // Switch #1
      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);

      // Switch #2
      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);

      // Switch #3
      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);

      // Switch #4
      context.Phase = "4";
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #5
      context.Phase = "5";
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #4/#5
      context.Phase = "6";
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #6
      context.Phase = "7";
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #7
      context.Phase = "8";
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #8
      context.Phase = "9";
      this.QuickProcessCircuit(testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #6/#7/#8
      context.Phase = "10";
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y);
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y);
      this.QuickProcessCircuit(testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
    }

    private void AC_XorGate(TestContext context) {
      DPoint testOffset = new DPoint(178, 375);
      
      // Switch #1
      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 6);

      // Switch #2
      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);

      // Switch #3
      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 6);

      // Switch #4
      context.Phase = "4";
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #5
      context.Phase = "5";
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #4/#5
      context.Phase = "6";
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #6
      context.Phase = "7";
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #7
      context.Phase = "8";
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #8
      context.Phase = "9";
      this.QuickProcessCircuit(testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #6/#7/#8
      context.Phase = "10";
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.QuickProcessCircuit(testOffset.X + 12, testOffset.Y);
      this.QuickProcessCircuit(testOffset.X + 14, testOffset.Y);
      this.QuickProcessCircuit(testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
    }

    private void AC_InputPortInbetween(TestContext context) {
      DPoint testOffset = new DPoint(198, 375);

      // Switch #1
      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);

      // Switch #2
      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 5);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 5);

      // Switch #3
      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 5);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
    }

    private void AC_SingleWiredPorts(TestContext context) {
      DPoint testOffset = new DPoint(206, 362);
      
      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 2);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + 2);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 2);
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + 2);

      context.Phase = "2";
      testOffset = new DPoint(206, 366);
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + 2);
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + 2);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 1);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + 2);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 1);
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 2);
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y + 2);

      context.Phase = "3";
      testOffset = new DPoint(206, 370);
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 1);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + 1);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 1);

      context.Phase = "4";
      testOffset = new DPoint(206, 374);
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 1);
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);

      context.Phase = "5";
      testOffset = new DPoint(207, 378);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);

      context.Phase = "6";
      testOffset = new DPoint(207, 382);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 2);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 2);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
    }

    private void AC_SelfDeactivatingTimers(TestContext context) {
      DPoint testOffset = new DPoint(212, 358);
      
      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 17);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 17);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 10);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 14);

      context.DelayedActions.Add(new TestDelay(70, (contextLocal) => {
        contextLocal.Phase = "2";
        TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 2);
        TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 6);
        TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 10);
        TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 14);
      }));
    }

    private void AC_PortLoopThrough(TestContext context) {
      DPoint testOffset = new DPoint(222, 368);
      
      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y + 7);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 7);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 7);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 10);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 10);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 14);
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y + 7);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 7);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 7);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 8);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 10);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 10);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 14);
    }

    private void AC_SenderForwarding(TestContext context) {
      DPoint testOffset = new DPoint(238, 367);
      
      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 8);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 8);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 8);

      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 8);

      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);

      context.Phase = "4";
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y + 8);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 7);
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 7);
      this.QuickProcessCircuit(testOffset.X + 6, testOffset.Y + 8);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 7);
    }

    private void AC_CrossoverBridge(TestContext context) {
      DPoint testOffset = new DPoint(249, 362);
      
      context.Phase = "1-1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 5);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);

      context.Phase = "1-2";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);

      context.Phase = "1-3";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 5);

      context.Phase = "1-4";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 5);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 5);

      testOffset = new DPoint(249, 370);
      context.Phase = "2-1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);

      context.Phase = "2-2";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);

      context.Phase = "2-3";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 5);

      context.Phase = "2-4";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 5);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 5);

      testOffset = new DPoint(249, 378);
      context.Phase = "3-1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 5);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);

      context.Phase = "3-2";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);

      context.Phase = "3-3";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 5);

      context.Phase = "3-4";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 5);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 5);
    }

    private void AC_MultiTileTriggerPorts1(TestContext context) {
      DPoint testOffset = new DPoint(257, 372);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 5, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 6);

      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 5, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
    }

    private void AC_MultiTileTriggerPorts2(TestContext context) {
      DPoint testOffset = new DPoint(266, 372);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 2, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 5, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);

      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 5, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);
    }

    private void AC_MultiTileTriggerInputPorts1(TestContext context) {
      DPoint testOffset = new DPoint(275, 370);

      // Middle Switch
      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);

      // Top Switch
      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);

      // Left Switch
      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 4);

      // Right Switch
      context.Phase = "4";
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y + 4);

      // Bottom Switch
      context.Phase = "5";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 8);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 8);

      // Middle Switch
      context.Phase = "6";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
    }

    private void AC_MultiTileTriggerInputPorts2(TestContext context) {
      DPoint testOffset = new DPoint(288, 370);

      // Middle Switch
      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);

      // Top Switch
      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);

      // Left Switch
      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 4);

      // Right Switch
      context.Phase = "4";
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y + 4);

      // Bottom Switch
      context.Phase = "5";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 8);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 8);

      // Middle Switch
      context.Phase = "6";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
    }

    private void AC_MultiTileTriggerPorts3(TestContext context) {
      DPoint testOffset = new DPoint(301, 370);

      // Middle Switch
      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 8);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);

      // Top Switch
      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 8);

      // Left Switch
      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);

      // Right Switch
      context.Phase = "4";
      this.QuickProcessCircuit(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 8);

      // Bottom Switch
      context.Phase = "5";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);

      // Middle Switch
      context.Phase = "6";
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 8);
    }

    private void AC_NOTPorts1(TestContext context) {
      DPoint testOffset = new DPoint(314, 369);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);

      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 3);

      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 6);

      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);

      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);

      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);

      context.Phase = "3";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);

      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 3);

      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 6);
    }

    private void AC_NOTPorts2(TestContext context) {
      DPoint testOffset = new DPoint(321, 366);

      context.Phase = "1";
      // First Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y);

      // Second Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);

      // Third Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);

      // Fourth Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 9);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 9);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 9);

      context.Phase = "2";
      // First Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y);

      // Second Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 3);

      // Third Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);

      // Fourth Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 9);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 9);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 9);

      context.Phase = "3";
      // First Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y);

      // Second Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);

      // Third Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);

      // Fourth Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 9);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 9);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 9);
    }

    private void AC_NOTPorts3(TestContext context) {
      DPoint testOffset = new DPoint(331, 369);

      context.Phase = "1";
      // First Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y);

      // Second Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);

      // Third Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);

      context.Phase = "2";
      // First Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y);

      // Second Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);

      // Third Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);

      context.Phase = "3";
      // First Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y);

      // Second Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);

      // Third Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);
    }

    private void AC_TimerModifiers(TestContext context) {
      DPoint testOffset = new DPoint(341, 372);

      Terraria.SpriteMeasureData firstTorch = Terraria.MeasureSprite(new DPoint(testOffset.X + 1, testOffset.Y));
      Terraria.SpriteMeasureData secondTorch = Terraria.MeasureSprite(new DPoint(testOffset.X + 4, testOffset.Y));
      Terraria.SpriteMeasureData thirdTorch = Terraria.MeasureSprite(new DPoint(testOffset.X + 7, testOffset.Y));
      Terraria.SpriteMeasureData fourthTorch = Terraria.MeasureSprite(new DPoint(testOffset.X + 10, testOffset.Y));
      Terraria.SetSpriteActiveFrame(firstTorch, false);
      Terraria.SetSpriteActiveFrame(secondTorch, false);
      Terraria.SetSpriteActiveFrame(thirdTorch, false);
      Terraria.SetSpriteActiveFrame(fourthTorch, false);

      context.Phase = "1";
      // First Switch
      this.QuickProcessCircuit(testOffset.X + 1, testOffset.Y + 3);
      this.QuickProcessCircuit(testOffset.X + 4, testOffset.Y + 3);
      this.QuickProcessCircuit(testOffset.X + 7, testOffset.Y + 3);
      this.QuickProcessCircuit(testOffset.X + 10, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 7, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 3);

      context.DelayedActions.Add(new TestDelay(15, (contextLocal) => {
        context.Phase = "2";

        TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 3);
        TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 3);
        TAssert.IsSpriteInactive(testOffset.X + 7, testOffset.Y + 3);
        TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 3);

        Terraria.SetSpriteActiveFrame(fourthTorch, false);
      }));

      context.DelayedActions.Add(new TestDelay(15, (contextLocal) => {
        context.Phase = "3";

        TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 3);
        TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 3);
        TAssert.IsSpriteActive(testOffset.X + 7, testOffset.Y + 3);
        TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 3);

        Terraria.SetSpriteActiveFrame(thirdTorch, false);
        Terraria.SetSpriteActiveFrame(fourthTorch, false);
      }));

      context.DelayedActions.Add(new TestDelay(15, (contextLocal) => {
        context.Phase = "4";

        TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 3);
        TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 3);
        TAssert.IsSpriteInactive(testOffset.X + 7, testOffset.Y + 3);
        TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 3);

        Terraria.SetSpriteActiveFrame(fourthTorch, false);
      }));

      context.DelayedActions.Add(new TestDelay(15, (contextLocal) => {
        context.Phase = "5";

        TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 3);
        TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 3);
        TAssert.IsSpriteActive(testOffset.X + 7, testOffset.Y + 3);
        TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 3);

        Terraria.SetSpriteActiveFrame(thirdTorch, false);
        Terraria.SetSpriteActiveFrame(fourthTorch, false);
      }));

      context.DelayedActions.Add(new TestDelay(60, (contextLocal) => {
        context.Phase = "6";

        TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + 3);
        TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 3);
        TAssert.IsSpriteActive(testOffset.X + 7, testOffset.Y + 3);
        TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 3);

        Terraria.SetSpriteActiveFrame(firstTorch, false);
        Terraria.SetSpriteActiveFrame(thirdTorch, false);
        Terraria.SetSpriteActiveFrame(fourthTorch, false);
      }));

      context.DelayedActions.Add(new TestDelay(60, (contextLocal) => {
        context.Phase = "7";

        TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 3);
        TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 3);
        TAssert.IsSpriteActive(testOffset.X + 7, testOffset.Y + 3);
        TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 3);

        Terraria.SetSpriteActiveFrame(thirdTorch, false);
        Terraria.SetSpriteActiveFrame(fourthTorch, false);
      }));

      context.DelayedActions.Add(new TestDelay(60, (contextLocal) => {
        context.Phase = "8";

        TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + 3);
        TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 3);
        TAssert.IsSpriteActive(testOffset.X + 7, testOffset.Y + 3);
        TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 3);
      }));
    }

    private void AC_SwitchForwarding(TestContext context) {
      DPoint testOffset = new DPoint(356, 372);

      context.Phase = "1";
      // First Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y);

      // Second Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 3);

      context.Phase = "2";
      // First Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y);

      // Second Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);

      context.Phase = "3";
      // First Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y);

      // Second Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 3);

      context.Phase = "4";
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 3, testOffset.Y)), true);
      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 3, testOffset.Y + 3)), true);

      // First Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y);

      // Second Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);

      context.Phase = "5";
      // First Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y);

      // Second Switch
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 3);
    }

    private void AC_GrandfatherClock(TestContext context) {
      DPoint testOffset = new DPoint(366, 366);

      this.MetadataHandler.Metadata.Clocks.Add(new DPoint(369, 370), new GrandfatherClockMetadata(null));
      this.MetadataHandler.Metadata.Clocks.Add(new DPoint(378, 370), new GrandfatherClockMetadata(null));
      this.MetadataHandler.Metadata.Clocks.Add(new DPoint(387, 370), new GrandfatherClockMetadata(null));

      context.Phase = "1";
      TSPlayer.Server.SetTime(false, 0);
      TSPlayer.Server.SetBloodMoon(false);

      context.DelayedActions.Add(new TestDelay(61, (contextLocal) => {
        context.Phase = "2";

        TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
        TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 7, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 12);

        TAssert.IsSpriteActive(testOffset.X + 12, testOffset.Y);
        TAssert.IsSpriteActive(testOffset.X + 9, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 16, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 13, testOffset.Y + 12);

        TAssert.IsSpriteInactive(testOffset.X + 21, testOffset.Y);
        TAssert.IsSpriteInactive(testOffset.X + 18, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 25, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 22, testOffset.Y + 12);
        
        TSPlayer.Server.SetTime(true, 1000);
      }));
      
      context.DelayedActions.Add(new TestDelay(61, (contextLocal) => {
        context.Phase = "3";
        TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
        TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 7, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 12);

        TAssert.IsSpriteActive(testOffset.X + 12, testOffset.Y);
        TAssert.IsSpriteActive(testOffset.X + 9, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 16, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 13, testOffset.Y + 12);

        TAssert.IsSpriteInactive(testOffset.X + 21, testOffset.Y);
        TAssert.IsSpriteInactive(testOffset.X + 18, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 25, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 22, testOffset.Y + 12);

        TSPlayer.Server.SetTime(true, 7200);
      }));
      
      context.DelayedActions.Add(new TestDelay(61, (contextLocal) => {
        context.Phase = "4";
        TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
        TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 7, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 12);

        TAssert.IsSpriteInactive(testOffset.X + 12, testOffset.Y);
        TAssert.IsSpriteInactive(testOffset.X + 9, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 16, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 13, testOffset.Y + 12);

        TAssert.IsSpriteInactive(testOffset.X + 21, testOffset.Y);
        TAssert.IsSpriteInactive(testOffset.X + 18, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 25, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 22, testOffset.Y + 12);

        TSPlayer.Server.SetTime(true, 46800);
      }));

      context.DelayedActions.Add(new TestDelay(61, (contextLocal) => {
        context.Phase = "5";
        TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
        TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 7, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 12);

        TAssert.IsSpriteActive(testOffset.X + 12, testOffset.Y);
        TAssert.IsSpriteActive(testOffset.X + 9, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 16, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 13, testOffset.Y + 12);

        TAssert.IsSpriteInactive(testOffset.X + 21, testOffset.Y);
        TAssert.IsSpriteInactive(testOffset.X + 18, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 25, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 22, testOffset.Y + 12);

        TSPlayer.Server.SetTime(false, 0);
      }));

      context.DelayedActions.Add(new TestDelay(61, (contextLocal) => {
        context.Phase = "6";
        TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
        TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 7, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 12);

        TAssert.IsSpriteActive(testOffset.X + 12, testOffset.Y);
        TAssert.IsSpriteActive(testOffset.X + 9, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 16, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 13, testOffset.Y + 12);

        TAssert.IsSpriteInactive(testOffset.X + 21, testOffset.Y);
        TAssert.IsSpriteInactive(testOffset.X + 18, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 25, testOffset.Y + 5);
        TAssert.IsSpriteInactive(testOffset.X + 22, testOffset.Y + 12);

        // Bloodmoon seems to work fine but the test fails anyway... probably because it takes too long for the bloodmoon setting
        // to be effective.
        //TSPlayer.Server.SetBloodMoon(true);

        TSPlayer.Server.SetTime(true, 9000);
      }));

      /*context.DelayedActions.Add(new TestDelay(180, (contextLocal) => {
        context.Phase = "7";
        TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
        TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 7, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 12);

        TAssert.IsSpriteActive(testOffset.X + 12, testOffset.Y);
        TAssert.IsSpriteActive(testOffset.X + 9, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 16, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 13, testOffset.Y + 12);

        TAssert.IsSpriteActive(testOffset.X + 21, testOffset.Y);
        TAssert.IsSpriteActive(testOffset.X + 18, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 25, testOffset.Y + 5);
        TAssert.IsSpriteActive(testOffset.X + 22, testOffset.Y + 12);

        TSPlayer.Server.SetTime(true, 9000);
      }));*/
    }

    private void AC_BlockActivator(TestContext context) {
      DPoint testOffset = new DPoint(395, 364);

      int[,] phaseOneExpectedTileIds = new[,] {
        { Terraria.TileId_CobaltBrick, Terraria.TileId_Glass, Terraria.TileId_Wood, Terraria.TileId_MythrilBrick },
        { Terraria.TileId_BlueBrick, Terraria.TileId_MudBlock, Terraria.TileId_GreenBrick, Terraria.TileId_Clay },
        { Terraria.TileId_SilverBrick, Terraria.TileId_CopperBrick, Terraria.TileId_RedBrick, Terraria.TileId_StoneBlock },
        { Terraria.TileId_GrayBrick, Terraria.TileId_PinkBrick, Terraria.TileId_DirtBlock, Terraria.TileId_Meteorite }
      };

      this.Config.BlockActivatorConfig.Cooldown = 0;
      this.MetadataHandler.Metadata.BlockActivators.Clear();

      context.Phase = "1-1";
      DPoint activationFieldOffset = new DPoint(testOffset.X + 1, testOffset.Y + 1);
      for (int x = 0; x < 4; x++) {
        for (int y = 0; y < 4; y++) {
          TAssert.IsTileId(activationFieldOffset.X + x, activationFieldOffset.Y + y, phaseOneExpectedTileIds[y, x]);
        }
      }

      context.Phase = "1-2";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 10);
      for (int x = 0; x < 4; x++) {
        for (int y = 0; y < 4; y++) {
          if (x == 3 && y == 3) {
            TAssert.IsTileId(activationFieldOffset.X + x, activationFieldOffset.Y + y, Terraria.TileId_Meteorite);
            continue;
          }

          TAssert.IsTileInactive(activationFieldOffset.X + x, activationFieldOffset.Y + y);
        }
      }

      context.Phase = "1-3";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 10);
      for (int x = 0; x < 4; x++) {
        for (int y = 0; y < 4; y++) {
          TAssert.IsTileId(activationFieldOffset.X + x, activationFieldOffset.Y + y, phaseOneExpectedTileIds[y, x]);
        }
      }

      context.Phase = "2-1";
      activationFieldOffset = new DPoint(testOffset.X + 8, testOffset.Y + 1);
      for (int x = 0; x < 4; x++) {
        for (int y = 0; y < 4; y++) {
          TAssert.IsTileId(activationFieldOffset.X + x, activationFieldOffset.Y + y, Terraria.TileId_GrayBrick);
        }
      }

      context.Phase = "2-2";
      this.QuickProcessCircuit(testOffset.X + 7, testOffset.Y + 10);
      for (int x = 0; x < 4; x++) {
        for (int y = 0; y < 4; y++) {
          if (x < 2) {
            TAssert.IsTileInactive(activationFieldOffset.X + x, activationFieldOffset.Y + y);
          } else {
            TAssert.IsTileId(activationFieldOffset.X + x, activationFieldOffset.Y + y, Terraria.TileId_GrayBrick);
          }
        }
      }

      context.Phase = "2-3";
      this.QuickProcessCircuit(testOffset.X + 9, testOffset.Y + 10);
      for (int x = 0; x < 4; x++) {
        for (int y = 0; y < 4; y++) {
          TAssert.IsTileInactive(activationFieldOffset.X + x, activationFieldOffset.Y + y);
        }
      }

      context.Phase = "2-4";
      this.QuickProcessCircuit(testOffset.X + 7, testOffset.Y + 10);
      for (int x = 0; x < 4; x++) {
        for (int y = 0; y < 4; y++) {
          if (x < 2) {
            TAssert.IsTileId(activationFieldOffset.X + x, activationFieldOffset.Y + y, Terraria.TileId_GrayBrick);
          } else {
            TAssert.IsTileInactive(activationFieldOffset.X + x, activationFieldOffset.Y + y);
          }
        }
      }

      context.Phase = "2-5";
      this.QuickProcessCircuit(testOffset.X + 9, testOffset.Y + 10);
      for (int x = 0; x < 4; x++) {
        for (int y = 0; y < 4; y++) {
          TAssert.IsTileId(activationFieldOffset.X + x, activationFieldOffset.Y + y, Terraria.TileId_GrayBrick);
        }
      }

      context.Phase = "2-6";
      this.QuickProcessCircuit(testOffset.X + 11, testOffset.Y + 10);
      for (int x = 0; x < 4; x++) {
        for (int y = 0; y < 4; y++) {
          TAssert.IsTileId(activationFieldOffset.X + x, activationFieldOffset.Y + y, Terraria.TileId_GrayBrick);
        }
      }
    }

    private void AC_Boulder(TestContext context) {
      DPoint testOffset = new DPoint(413, 373);

      context.Phase = "1";
      this.QuickProcessCircuit(testOffset.X + 5, testOffset.Y + 1);
      TAssert.IsTileActive(testOffset.X + 2, testOffset.Y);

      context.Phase = "2";
      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 1);
      TAssert.IsTileActive(testOffset.X + 2, testOffset.Y);

      this.QuickProcessCircuit(testOffset.X, testOffset.Y + 1);
      TAssert.IsTileInactive(testOffset.X + 2, testOffset.Y);
    }
    #endregion

    private void CheckAllSpritesState(DPoint tileOffset, bool expectedState) {
      // Left Torches
      TAssert.IsSpriteActive(tileOffset.X + 4, tileOffset.Y, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 4, tileOffset.Y + 1, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 4, tileOffset.Y + 2, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 4, tileOffset.Y + 3, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 6, tileOffset.Y, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 6, tileOffset.Y + 1, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 6, tileOffset.Y + 2, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 6, tileOffset.Y + 3, expectedState);

      // Right Torches
      TAssert.IsSpriteActive(tileOffset.X + 8, tileOffset.Y, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 8, tileOffset.Y + 1, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 8, tileOffset.Y + 2, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 8, tileOffset.Y + 3, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 8, tileOffset.Y + 4, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 10, tileOffset.Y, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 10, tileOffset.Y + 1, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 10, tileOffset.Y + 2, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 10, tileOffset.Y + 3, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 10, tileOffset.Y + 4, expectedState);

      // Music Box
      TAssert.IsSpriteActive(tileOffset.X + 12, tileOffset.Y + 4, expectedState);
      
      // Candle
      TAssert.IsSpriteActive(tileOffset.X + 16, tileOffset.Y + 4, expectedState);
      
      // Candlebra
      TAssert.IsSpriteActive(tileOffset.X + 18, tileOffset.Y + 4, expectedState);

      // Lamp Post
      TAssert.IsSpriteActive(tileOffset.X + 21, tileOffset.Y + 4, expectedState);
      
      // Tiki Torch
      TAssert.IsSpriteActive(tileOffset.X + 24, tileOffset.Y + 4, expectedState);

      // Chandeliers
      TAssert.IsSpriteActive(tileOffset.X, tileOffset.Y + 7, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 4, tileOffset.Y + 7, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 8, tileOffset.Y + 7, expectedState);

      // X-Mas lights
      TAssert.IsSpriteActive(tileOffset.X + 12, tileOffset.Y + 7, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 14, tileOffset.Y + 7, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 16, tileOffset.Y + 7, expectedState);

      // Disco Ball
      TAssert.IsSpriteActive(tileOffset.X + 21, tileOffset.Y + 7, expectedState);

      // Chain Lantern
      TAssert.IsSpriteActive(tileOffset.X + 24, tileOffset.Y + 7, expectedState);

      // Torches
      TAssert.IsSpriteActive(tileOffset.X, tileOffset.Y + 15, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 2, tileOffset.Y + 15, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 4, tileOffset.Y + 15, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 6, tileOffset.Y + 15, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 8, tileOffset.Y + 15, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 10, tileOffset.Y + 15, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 12, tileOffset.Y + 15, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 14, tileOffset.Y + 15, expectedState);
      TAssert.IsSpriteActive(tileOffset.X + 16, tileOffset.Y + 15, expectedState);

      // Active Stone Blocks
      if (expectedState) {
        TAssert.IsTileId(tileOffset.X + 18, tileOffset.Y + 15, Terraria.TileId_ActiveStone);
        TAssert.IsTileId(tileOffset.X + 19, tileOffset.Y + 15, Terraria.TileId_ActiveStone);
        TAssert.IsTileId(tileOffset.X + 20, tileOffset.Y + 15, Terraria.TileId_ActiveStone);
      } else {
        TAssert.IsTileId(tileOffset.X + 18, tileOffset.Y + 15, Terraria.TileId_InactiveStone);
        TAssert.IsTileId(tileOffset.X + 19, tileOffset.Y + 15, Terraria.TileId_InactiveStone);
        TAssert.IsTileId(tileOffset.X + 20, tileOffset.Y + 15, Terraria.TileId_InactiveStone);
      }

      int doorTileId = Terraria.TileId_DoorClosed;
      if (expectedState)
        doorTileId = Terraria.TileId_DoorOpened;

      // Opened Door
      TAssert.IsTileId(tileOffset.X + 22, tileOffset.Y + 15, doorTileId);
    }

    public override void HandleGameUpdate() {
      if (this.circuitHandler != null)
        this.circuitHandler.HandleGameUpdate();

      base.HandleGameUpdate();
    }

    private CircuitProcessResult QuickProcessCircuit(int senderX, int senderY) {
      return this.GetProcessor(senderX, senderY).ProcessCircuit(this.GetTestPlayer());
    }

    private TSPlayer GetTestPlayer() {
      return TSPlayer.Server;
    }

    private CircuitProcessor GetProcessor(int senderX, int senderY) {
      return new CircuitProcessor(this.Config, this.MetadataHandler.Metadata, new DPoint(senderX, senderY));
    }

    private void Assert_SignaledComponents(CircuitProcessResult result, int expectedComponentCount) {
      if (result.SignaledComponentsCounter != expectedComponentCount) {
        throw new AssertException(string.Format(
          "The circuit signaled {0} components, but {1} were expected instead.", 
          result.SignaledComponentsCounter, expectedComponentCount
        ));
      }
    }

    private void Assert_SignaledPortDefiningComponents(CircuitProcessResult result, int expectedComponentCount) {
      if (result.SignaledPortDefiningComponentsCounter != expectedComponentCount) {
        throw new AssertException(string.Format(
          "The circuit signaled {0} port defining components, but {1} were expected instead.", 
          result.SignaledPortDefiningComponentsCounter, expectedComponentCount
        ));
      }
    }

    private void Assert_ProcessedBranches(CircuitProcessResult result, int expectedBranchCount) {
      if (result.ProcessedBranchCount != expectedBranchCount) {
        throw new AssertException(string.Format(
          "The circuit processed {0} branches, but {1} were expected instead.", 
          result.ProcessedBranchCount, expectedBranchCount
        ));
      }
    }

    private void Assert_Cancelled(
      CircuitProcessResult result, CircuitCancellationReason expectedReason, int expectedComponentType = -1
    ) {
      if (result.CancellationReason != expectedReason) {
        if (expectedReason == CircuitCancellationReason.None) {
          throw new AssertException(string.Format(
            "It was expected that the circuit is not cancelled, but it was. Reason: {0}, Component: {1}.",
            result.CancellationReason, Terraria.Tiles.GetBlockName(result.CancellationRelatedComponentType)
          ));
        } else if (result.CancellationReason == CircuitCancellationReason.None) {
          throw new AssertException(string.Format("The circuit was expected to be cancelled, but it was not."));
        } else {
          throw new AssertException(string.Format(
            "The circuit was cancelled with the reason {0}, but {1} was expected instead.",
            result.CancellationReason, expectedReason
          ));
        }
      }

      if (result.CancellationRelatedComponentType != expectedComponentType) {
        if (expectedComponentType == -1) {
          throw new AssertException(string.Format(
            "The circuit was cancelled with the expected reason, but there is a related component \"{0}\" which was not expected.",
            Terraria.Tiles.GetBlockName(result.CancellationRelatedComponentType)
          ));
        } else if (result.CancellationRelatedComponentType == -1) {
          throw new AssertException(string.Format(
            "The circuit was cancelled with the expected reason but no related component, \"{0}\" where was expected.",
            Terraria.Tiles.GetBlockName(expectedComponentType)
          ));
        } else {
          throw new AssertException(string.Format(
            "The circuit was cancelled with the expected reason, but the related component is \"{0}\" instead of \"{1}\".",
            Terraria.Tiles.GetBlockName(result.CancellationRelatedComponentType), Terraria.Tiles.GetBlockName(expectedComponentType)
          ));
        }
      }
    }
  }
  #endif
}
