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

    #region [Property: CircuitProcessor]
    private CircuitProcessor circuitProcessor;

    protected CircuitProcessor CircuitProcessor {
      get { return this.circuitProcessor; }
    }
    #endregion


    #region [Method: Constructor]
    public TestRunner(): base(AdvancedCircuitsPlugin.Trace) {
      this.RegisterTest(@"Vanilla\All Sprites State Toggle Test", this.Vanilla_AllSpritesToggleState);
      this.RegisterTest(@"Vanilla\Basic Door Toggling Test", this.Vanilla_BasicDoorToggling);
      this.RegisterTest(@"Vanilla\Blocked Door Toggling 1", this.Vanilla_BlockedDoorToggling1);
      this.RegisterTest(@"Vanilla\Blocked Door Toggling 2", this.Vanilla_BlockedDoorToggling2);
      this.RegisterTest(@"Vanilla\Pumps", this.Vanilla_Pumps);
      this.RegisterTest(@"Vanilla\Statues", this.Vanilla_Statues);
      this.RegisterTest(@"Vanilla\ComponentActivation", this.Vanilla_ComponentActivation);

      this.RegisterTest(@"AC\All Sprites State Toggle Test", this.AC_AllSpritesToggleState);
      this.RegisterTest(@"AC\Basic Door Toggling Test", this.AC_BasicDoorToggling);
      this.RegisterTest(@"AC\Blocked Door Toggling 1", this.AC_BlockedDoorToggling1);
      this.RegisterTest(@"AC\Blocked Door Toggling 2", this.AC_BlockedDoorToggling2);
      this.RegisterTest(@"AC\Pumps", this.AC_Pumps);
      this.RegisterTest(@"AC\Statues", this.AC_Statues);
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
      //this.RegisterTest(@"AC\", this.AC_InputPortInbetween);
    }
    #endregion

    #region [Methods: TestInit, TestCleanup]
    protected override void TestInit() {
      this.config = Configuration.Read(TestRunner.TestDataGlobalConfigFilePath);
      this.metadataHandler = new WorldMetadataHandler(TestRunner.TestDataGlobalMetadataPath);
      this.circuitProcessor = new CircuitProcessor(this.Config, this.MetadataHandler.Metadata);
    }

    protected override void TestCleanup() {}
    #endregion

    #region [Vanilla Tests]
    private void Vanilla_AllSpritesToggleState(TestContext context) {
      DPoint testOffset = new DPoint(46, 280);

      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y + 5);
      this.CheckAllSpritesState(testOffset, false);

      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y + 5);
      this.CheckAllSpritesState(testOffset, true);

      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y + 5);
      this.CheckAllSpritesState(testOffset, false);
    }

    private void Vanilla_BasicDoorToggling(TestContext context) {
      DPoint testOffset = new DPoint(75, 282);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 2, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 4, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 8, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 10, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 12, testOffset.Y + 3, Terraria.TileId_DoorClosed);

      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 2, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 4, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 8, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 10, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 12, testOffset.Y + 3, Terraria.TileId_DoorOpened);
    }

    private void Vanilla_BlockedDoorToggling1(TestContext context) {
      DPoint testOffset = new DPoint(92, 282);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X + 2, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 6, testOffset.Y + 3, Terraria.TileId_DoorOpened);

      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X + 1, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 7, testOffset.Y + 3, Terraria.TileId_DoorClosed);
    }

    private void Vanilla_BlockedDoorToggling2(TestContext context) {
      DPoint testOffset = new DPoint(104, 282);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 8, testOffset.Y + 3, Terraria.TileId_DoorOpened);

      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X + 1, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 7, testOffset.Y + 3, Terraria.TileId_DoorClosed);
    }

    private void Vanilla_Pumps(TestContext context) {
      DPoint testOffset = new DPoint(116, 283);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 5, testOffset.Y + 2);
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
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y + 3);
      context.DelayedActions.Add(new TestDelay(5, (contextLocal => {
        TAssert.AreNPCsInBlockRect(testOffset.X + 1, testOffset.Y + 1, 2, 3, TestRunner.NPCId_Guide, 1);
        TAssert.AreNPCsInBlockRect(testOffset.X + 4, testOffset.Y + 1, 2, 3, TestRunner.NPCId_Slime, 1);
        TAssert.AreItemsInBlockRect(testOffset.X + 7, testOffset.Y + 1, 2, 3, TestRunner.ItemId_IronPickaxe, 1);

        contextLocal.Phase = "2";
        this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y + 3);
      })));

      context.DelayedActions.Add(new TestDelay(15, (contextLocal => {
        TAssert.AreNPCsInBlockRect(testOffset.X + 1, testOffset.Y + 1, 2, 3, TestRunner.NPCId_Guide, 1);
        TAssert.AreNPCsInBlockRect(testOffset.X + 4, testOffset.Y + 1, 2, 3, TestRunner.NPCId_Slime, 2);
        TAssert.AreItemsInBlockRect(testOffset.X + 7, testOffset.Y + 1, 2, 3, TestRunner.ItemId_IronPickaxe, 2);
      })));
    }

    private void Vanilla_ComponentActivation(TestContext context) {
      DPoint testOffset = new DPoint(148, 279);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 3);

      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 3);
    }
    #endregion

    #region [AC Basic Tests]
    private void AC_AllSpritesToggleState(TestContext context) {
      DPoint testOffset = new DPoint(46, 343);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y + 5);
      this.CheckAllSpritesState(testOffset, false);

      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y + 5);
      this.CheckAllSpritesState(testOffset, true);

      context.Phase = "3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y + 5);
      this.CheckAllSpritesState(testOffset, false);
    }

    private void AC_BasicDoorToggling(TestContext context) {
      DPoint testOffset = new DPoint(75, 345);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 2, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 4, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 8, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 10, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 12, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      
      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 2, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 4, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 8, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 10, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 12, testOffset.Y + 3, Terraria.TileId_DoorOpened);

      context.Phase = "3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 2, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 4, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 8, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 10, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 12, testOffset.Y + 3, Terraria.TileId_DoorClosed);
    }

    private void AC_BlockedDoorToggling1(TestContext context) {
      DPoint testOffset = new DPoint(92, 345);

      Terraria.SetSpriteActiveFrame(Terraria.MeasureSprite(new DPoint(testOffset.X + 4, testOffset.Y + 3)), false);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X + 2, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 6, testOffset.Y + 3, Terraria.TileId_DoorOpened);

      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X + 1, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 7, testOffset.Y + 3, Terraria.TileId_DoorClosed);
    }

    private void AC_BlockedDoorToggling2(TestContext context) {
      DPoint testOffset = new DPoint(104, 345);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X + 1, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 7, testOffset.Y + 3, Terraria.TileId_DoorClosed);

      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X, testOffset.Y + 3, Terraria.TileId_DoorOpened);
      TAssert.IsTileId(testOffset.X + 8, testOffset.Y + 3, Terraria.TileId_DoorOpened);

      context.Phase = "3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 3);
      TAssert.IsTileId(testOffset.X + 1, testOffset.Y + 3, Terraria.TileId_DoorClosed);
      TAssert.IsTileId(testOffset.X + 7, testOffset.Y + 3, Terraria.TileId_DoorClosed);
    }

    private void AC_Pumps(TestContext context) {
      DPoint testOffset = new DPoint(116, 346);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 5, testOffset.Y + 2);
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
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y + 3);
      context.DelayedActions.Add(new TestDelay(5, (contextLocal => {
        TAssert.AreNPCsInBlockRect(testOffset.X + 1, testOffset.Y + 1, 2, 3, TestRunner.NPCId_Guide, 1);
        TAssert.AreNPCsInBlockRect(testOffset.X + 4, testOffset.Y + 1, 2, 3, TestRunner.NPCId_Slime, 1);
        TAssert.AreItemsInBlockRect(testOffset.X + 7, testOffset.Y + 1, 2, 3, TestRunner.ItemId_IronPickaxe, 1);

        contextLocal.Phase = "2";
        this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y + 3);
      })));

      context.DelayedActions.Add(new TestDelay(15, (contextLocal => {
        TAssert.AreNPCsInBlockRect(testOffset.X + 1, testOffset.Y + 1, 2, 3, TestRunner.NPCId_Guide, 1);
        TAssert.AreNPCsInBlockRect(testOffset.X + 4, testOffset.Y + 1, 2, 3, TestRunner.NPCId_Slime, 2);
        TAssert.AreItemsInBlockRect(testOffset.X + 7, testOffset.Y + 1, 2, 3, TestRunner.ItemId_IronPickaxe, 2);
      })));
    }

    private void AC_ComponentActivation(TestContext context) {
      DPoint testOffset = new DPoint(148, 342);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 3);

      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 3);

      context.Phase = "3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 3);
    }
    #endregion

    #region [AC Port Tests]
    private void AC_SingleTileTriggerPorts(TestContext context) {
      DPoint testOffset = new DPoint(43, 372);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 6);

      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
    }

    private void AC_SingleTileTriggerInputPorts(TestContext context) {
      DPoint testOffset = new DPoint(53, 372);

      // Middle Switch
      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);

      // Top Switch
      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y);

      // Left Switch
      context.Phase = "3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 3);

      // Right Switch
      context.Phase = "4";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y + 3);

      // Bottom Switch
      context.Phase = "5";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y + 6);

      // Middle Switch
      context.Phase = "6";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
    }

    private void AC_SingleTileTriggerPortInput(TestContext context) {
      DPoint testOffset = new DPoint(63, 372);

      // Middle Switch
      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 6);

      // Middle Switch
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);

      // Top Switch
      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y);

      // Left Switch
      context.Phase = "3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 3);

      // Right Switch
      context.Phase = "4";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y + 3);

      // Bottom Switch
      context.Phase = "5";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y + 6);

      // Middle Switch
      context.Phase = "6";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
    }

    private void AC_SelfSwitching(TestContext context) {
      DPoint testOffset = new DPoint(73, 375);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y);
    }

    private void AC_MultiPortSending(TestContext context) {
      DPoint testOffset = new DPoint(85, 374);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 4);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + 4);
    }

    private void AC_GreenPressurePlate(TestContext context) {
      DPoint testOffset = new DPoint(91, 375);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
    }

    private void AC_RedPressurePlate(TestContext context) {
      DPoint testOffset = new DPoint(95, 375);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
    }

    private void AC_MultiReceiving(TestContext context) {
      DPoint testOffset = new DPoint(99, 374);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 1);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 1);
    }

    private void AC_NotGate(TestContext context) {
      DPoint testOffset = new DPoint(106, 375);

      // Switch #1
      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 6);

      // Switch #2
      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);

      // Switch #3
      context.Phase = "3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 6);

      // Switch #4
      context.Phase = "4";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);

      // Switch #5
      context.Phase = "5";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 10, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 10, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 10, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);

      // Switch #4/#5
      context.Phase = "6";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 10, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
    }

    private void AC_Swapper(TestContext context) {
      DPoint testOffset = new DPoint(120, 375);
      
      // Prepare
      this.MakeTorchInactive(testOffset.X, testOffset.Y + 6);
      this.MakeTorchInactive(testOffset.X + 2, testOffset.Y + 6);
      this.MakeTorchInactive(testOffset.X + 4, testOffset.Y + 6);
      this.MakeTorchInactive(testOffset.X + 6, testOffset.Y + 6);
      this.MakeTorchInactive(testOffset.X + 8, testOffset.Y + 6);
      this.MakeTorchInactive(testOffset.X + 12, testOffset.Y + 6);
      this.MakeTorchInactive(testOffset.X + 14, testOffset.Y + 6);

      // Plate #1
      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);

      // Plate #2
      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);

      // Plate #3
      context.Phase = "3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);

      // Plate #4
      context.Phase = "4";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);

      // Plate #5
      context.Phase = "5";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 10, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 10, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 10, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);

      // Plate #6
      context.Phase = "6";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 12, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 12, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 12, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 12, testOffset.Y + 6);

      // Plate #7
      context.Phase = "7";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
    }

    private void AC_AndGate(TestContext context) {
      DPoint testOffset = new DPoint(138, 375);
      
      // Prepare
      this.MakeTorchInactive(testOffset.X, testOffset.Y + 6);
      this.MakeTorchInactive(testOffset.X + 2, testOffset.Y + 6);
      this.MakeTorchInactive(testOffset.X + 4, testOffset.Y + 6);
      this.MakeTorchInactive(testOffset.X + 8, testOffset.Y + 6);
      this.MakeTorchInactive(testOffset.X + 10, testOffset.Y + 6);
      this.MakeTorchInactive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #1
      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);

      // Switch #2
      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);

      // Switch #3
      context.Phase = "3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);

      // Switch #4
      context.Phase = "4";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #5
      context.Phase = "5";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #4/#5
      context.Phase = "6";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #6
      context.Phase = "7";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #7
      context.Phase = "8";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #8
      context.Phase = "9";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #6/#7/#8
      context.Phase = "10";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 12, testOffset.Y);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
    }

    private void AC_OrGate(TestContext context) {
      DPoint testOffset = new DPoint(158, 375);
      
      // Prepare
      this.MakeTorchInactive(testOffset.X, testOffset.Y + 6);
      this.MakeTorchInactive(testOffset.X + 2, testOffset.Y + 6);
      this.MakeTorchInactive(testOffset.X + 4, testOffset.Y + 6);
      this.MakeTorchInactive(testOffset.X + 8, testOffset.Y + 6);
      this.MakeTorchInactive(testOffset.X + 10, testOffset.Y + 6);
      this.MakeTorchInactive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #1
      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 6);

      // Switch #2
      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);

      // Switch #3
      context.Phase = "3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 6);

      // Switch #4
      context.Phase = "4";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #5
      context.Phase = "5";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #4/#5
      context.Phase = "6";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #6
      context.Phase = "7";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #7
      context.Phase = "8";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #8
      context.Phase = "9";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #6/#7/#8
      context.Phase = "10";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 12, testOffset.Y);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
    }

    private void AC_XorGate(TestContext context) {
      DPoint testOffset = new DPoint(178, 375);
      
      // Switch #1
      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 6);

      // Switch #2
      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);

      // Switch #3
      context.Phase = "3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 6);

      // Switch #4
      context.Phase = "4";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #5
      context.Phase = "5";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #4/#5
      context.Phase = "6";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteActive(testOffset.X + 10, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 6);
      TAssert.IsSpriteInactive(testOffset.X + 10, testOffset.Y + 6);

      // Switch #6
      context.Phase = "7";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #7
      context.Phase = "8";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #8
      context.Phase = "9";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);

      // Switch #6/#7/#8
      context.Phase = "10";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 12, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 12, testOffset.Y);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 14, testOffset.Y);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 16, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 14, testOffset.Y + 6);
    }

    private void AC_InputPortInbetween(TestContext context) {
      DPoint testOffset = new DPoint(198, 375);

      // Switch #1
      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);

      // Switch #2
      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 5);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 5);

      // Switch #3
      context.Phase = "3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 5);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
    }

    private void AC_SingleWiredPorts(TestContext context) {
      DPoint testOffset = new DPoint(206, 362);
      
      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 2);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + 2);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 2);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + 2);

      context.Phase = "2";
      testOffset = new DPoint(206, 366);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + 2);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + 2);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 1);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + 2);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y + 1);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 2);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y + 2);

      context.Phase = "3";
      testOffset = new DPoint(206, 370);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 1);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y + 1);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y + 1);

      context.Phase = "4";
      testOffset = new DPoint(206, 374);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 1);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 1, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 1, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 1);

      context.Phase = "5";
      testOffset = new DPoint(207, 378);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);

      context.Phase = "6";
      testOffset = new DPoint(207, 382);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 2);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 1);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 2);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 1);
    }

    private void AC_SelfDeactivatingTimers(TestContext context) {
      DPoint testOffset = new DPoint(212, 358);
      
      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y + 17);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y + 17);
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
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y + 7);
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
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y + 7);
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
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 8);

      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 8);

      context.Phase = "3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);

      context.Phase = "4";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 6, testOffset.Y + 7);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 6, testOffset.Y + 8);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 6, testOffset.Y + 7);
    }

    private void AC_CrossoverBridge(TestContext context) {
      DPoint testOffset = new DPoint(249, 362);
      
      context.Phase = "1-1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 5);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);

      context.Phase = "1-2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);

      context.Phase = "1-3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 5);

      context.Phase = "1-4";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 5);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 5);

      testOffset = new DPoint(249, 370);
      context.Phase = "2-1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);

      context.Phase = "2-2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);

      context.Phase = "2-3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 5);

      context.Phase = "2-4";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 5);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 5);

      testOffset = new DPoint(249, 378);
      context.Phase = "3-1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 5);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y);

      context.Phase = "3-2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);

      context.Phase = "3-3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 5);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 5);

      context.Phase = "3-4";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 5);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 5);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 5);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 5);
    }

    private void AC_MultiTileTriggerPorts1(TestContext context) {
      DPoint testOffset = new DPoint(257, 372);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 5, testOffset.Y + 3);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y + 6);

      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 5, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y + 6);
    }

    private void AC_MultiTileTriggerPorts2(TestContext context) {
      DPoint testOffset = new DPoint(266, 372);

      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 2, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 5, testOffset.Y + 2);
      TAssert.IsSpriteInactive(testOffset.X + 2, testOffset.Y + 6);

      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 3, testOffset.Y + 3);
      TAssert.IsSpriteActive(testOffset.X + 3, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 5, testOffset.Y + 2);
      TAssert.IsSpriteActive(testOffset.X + 2, testOffset.Y + 6);
    }

    private void AC_MultiTileTriggerInputPorts1(TestContext context) {
      DPoint testOffset = new DPoint(275, 370);

      // Middle Switch
      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);

      // Top Switch
      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);

      // Left Switch
      context.Phase = "3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 4);

      // Right Switch
      context.Phase = "4";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y + 4);

      // Bottom Switch
      context.Phase = "5";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 8);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 8);

      // Middle Switch
      context.Phase = "6";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
    }

    private void AC_MultiTileTriggerInputPorts2(TestContext context) {
      DPoint testOffset = new DPoint(288, 370);

      // Middle Switch
      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);

      // Top Switch
      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);

      // Left Switch
      context.Phase = "3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 4);

      // Right Switch
      context.Phase = "4";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y + 4);

      // Bottom Switch
      context.Phase = "5";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 8);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 8);

      // Middle Switch
      context.Phase = "6";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
    }

    private void AC_MultiTileTriggerPorts3(TestContext context) {
      DPoint testOffset = new DPoint(301, 370);

      // Middle Switch
      context.Phase = "1";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);

      // Top Switch
      context.Phase = "2";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y);

      // Left Switch
      context.Phase = "3";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X, testOffset.Y + 4);

      // Right Switch
      context.Phase = "4";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 8, testOffset.Y + 4);

      // Bottom Switch
      context.Phase = "5";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 8);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 8);

      // Middle Switch
      context.Phase = "6";
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteInactive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteInactive(testOffset.X + 4, testOffset.Y + 8);
      this.CircuitProcessor.HandleHitSwitch(this.GetTestPlayer(), testOffset.X + 4, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y);
      TAssert.IsSpriteActive(testOffset.X, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 8, testOffset.Y + 4);
      TAssert.IsSpriteActive(testOffset.X + 4, testOffset.Y + 8);
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

      int doorTileId = Terraria.TileId_DoorClosed;
      if (expectedState)
        doorTileId = Terraria.TileId_DoorOpened;

      // Opened Door
      TAssert.IsTileId(tileOffset.X + 22, tileOffset.Y + 15, doorTileId);
    }

    private TSPlayer GetTestPlayer() {
      return TSPlayer.Server;
    }

    private void MakeTorchInactive(int x, int y) {
      Tile tile = Main.tile[x, y];
      if (!tile.active)
        throw new ArgumentException("Tile not active.");
      if (tile.type != Terraria.TileId_Torch)
        throw new ArgumentException("Tile not a torch.");
      // Is already active?
      if (tile.frameX >= 66)
        return;

      tile.frameX += 66;
    }
  }
  #endif
}
