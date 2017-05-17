using System;
using System.Collections.Generic;
using OTAPI.Tile;
using Terraria.ID;
using DPoint = System.Drawing.Point;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public static class AdvancedCircuits {
    public const int BlockType_ORGate              = TileID.Copper;
    public const int BlockType_ANDGate             = TileID.Silver;
    public const int BlockType_XORGate             = TileID.Gold;
    public const int BlockType_NOTGate             = TileID.Obsidian;
    public const int BlockType_Swapper             = TileID.Iron;
    public const int BlockType_CrossoverBridge     = TileID.Spikes;
    public const int BlockType_InputPort           = TileID.Glass;
    public const int BlockType_BlockActivator      = TileID.ActiveStoneBlock;
    public const int BlockType_WirelessTransmitter = TileID.Adamantite;

    public const PaintColor Paint_Timer_Mul2 = PaintColor.Red;
    public const PaintColor Paint_Timer_Mul4 = PaintColor.Green;
    public const PaintColor Paint_Timer_Mul8 = PaintColor.Blue;
    public const PaintColor Paint_Timer_Div2 = PaintColor.White;
    public const PaintColor Paint_Timer_Div3 = PaintColor.Grey;
    public const PaintColor Paint_Timer_Div4 = PaintColor.Black;

    public const PaintColor Paint_Switch_ToggleAndForward       = PaintColor.Red;
    public const PaintColor Paint_Switch_ForwardIfEqual         = PaintColor.Green;
    public const PaintColor Paint_Switch_ForwardIfEqualByChance = PaintColor.Blue;
    public const PaintColor Paint_Switch_ForwardByChance        = PaintColor.White;

    public const PaintColor Paint_Gate_TemporaryState = PaintColor.Red;

    public const PaintColor Paint_Swapper_2 = PaintColor.Red;
    public const PaintColor Paint_Swapper_3 = PaintColor.Green;
    public const PaintColor Paint_Swapper_4 = PaintColor.Blue;
    public const PaintColor Paint_Swapper_5 = PaintColor.White;
    public const PaintColor Paint_Swapper_6 = PaintColor.Grey;
    public const PaintColor Paint_Swapper_7 = PaintColor.Black;

    public const PaintColor Paint_Clock_ByDaylight = PaintColor.Blue;
    public const PaintColor Paint_Clock_ByNighttimeAndBloodmoon = PaintColor.Red;
    public const PaintColor Paint_Clock_ByNighttimeAndFullmoon = PaintColor.White;

    public const PaintColor Paint_BlockActivator_Replace = PaintColor.Red;

    private static readonly HashSet<int> portDefiningComponents = new HashSet<int> {
      TileID.Switches,
      TileID.Lever,
      TileID.Timers,
      TileID.OpenDoor,
      TileID.ClosedDoor,
      TileID.TrapdoorOpen,
      TileID.TrapdoorClosed,
      TileID.TallGateOpen,
      TileID.TallGateClosed,
      AdvancedCircuits.BlockType_ORGate,
      AdvancedCircuits.BlockType_ANDGate,
      AdvancedCircuits.BlockType_XORGate,
      AdvancedCircuits.BlockType_NOTGate,
      AdvancedCircuits.BlockType_Swapper,
      AdvancedCircuits.BlockType_CrossoverBridge,
      TileID.GrandfatherClocks,
      AdvancedCircuits.BlockType_BlockActivator,
      AdvancedCircuits.BlockType_WirelessTransmitter,
    };
    public static bool IsPortDefiningComponentBlock(int blockType) {
      return AdvancedCircuits.portDefiningComponents.Contains(blockType);
    }

    private static readonly HashSet<int> modifyableComponents = new HashSet<int> {
      TileID.Timers,
      TileID.Switches,
      TileID.Lever,
      TileID.GrandfatherClocks,
      AdvancedCircuits.BlockType_ANDGate,
      AdvancedCircuits.BlockType_XORGate,
      AdvancedCircuits.BlockType_ORGate,
      TileID.Traps,
      TileID.InletPump,
      TileID.OutletPump,
      AdvancedCircuits.BlockType_WirelessTransmitter,
    };
    public static bool IsPaintSupportingComponent(int blockType) {
      return AdvancedCircuits.modifyableComponents.Contains(blockType);
    }

    private static HashSet<int> customActivatableBlocks = null;
    public static bool IsCustomActivatableBlock(int blockType) {
      if (AdvancedCircuits.customActivatableBlocks == null) {
        AdvancedCircuits.customActivatableBlocks = new HashSet<int> {
          TileID.Dirt,
          TileID.Stone,
          TileID.Platforms,
          TileID.Demonite,
          TileID.CorruptGrass,
          TileID.Ebonstone,
          TileID.WoodBlock,
          TileID.JungleVines,
          TileID.ObsidianBrick,
          TileID.HellstoneBrick,
          TileID.HallowedGrass,
          TileID.Ebonsand,
          TileID.HallowedVines,
          TileID.MagicalIceBlock,
          TileID.ActiveStoneBlock,
          TileID.InactiveStoneBlock,
          TileID.DemoniteBrick,
          TileID.Explosives,
          TileID.Shadewood,
          TileID.Chlorophyte,
          TileID.HoneyBlock,
          TileID.CrispyHoneyBlock,
          TileID.WoodenSpikes,
          TileID.Crimsand,
          TileID.CopperPlating,
          TileID.LivingFire,
        };

        for (int blockId = TileID.GrayBrick; blockId <= TileID.BlueDungeonBrick; blockId++)
          AdvancedCircuits.customActivatableBlocks.Add(blockId);
        for (int blockId = TileID.GreenDungeonBrick; blockId <= TileID.Spikes; blockId++)
          AdvancedCircuits.customActivatableBlocks.Add(blockId);
        for (int blockId = TileID.Cobweb; blockId <= TileID.Glass; blockId++)
          AdvancedCircuits.customActivatableBlocks.Add(blockId);
        for (int blockId = TileID.Obsidian; blockId <= TileID.JungleGrass; blockId++)
          AdvancedCircuits.customActivatableBlocks.Add(blockId);
        for (int blockId = TileID.Sapphire; blockId <= TileID.MushroomGrass; blockId++)
          AdvancedCircuits.customActivatableBlocks.Add(blockId);
        for (int blockId = TileID.Pearlsand; blockId <= TileID.WoodenBeam; blockId++)
          AdvancedCircuits.customActivatableBlocks.Add(blockId);
        for (int blockId = TileID.CandyCaneBlock; blockId <= TileID.SnowBrick; blockId++)
          AdvancedCircuits.customActivatableBlocks.Add(blockId);
        for (int blockId = TileID.AdamantiteBeam; blockId <= TileID.ChristmasTree; blockId++)
          AdvancedCircuits.customActivatableBlocks.Add(blockId);
        for (int blockId = TileID.TinBrick; blockId <= TileID.PlatinumBrick; blockId++)
          AdvancedCircuits.customActivatableBlocks.Add(blockId);
        for (int blockId = TileID.CactusBlock; blockId <= TileID.IceBrick; blockId++)
          AdvancedCircuits.customActivatableBlocks.Add(blockId);
        for (int blockId = TileID.Palladium; blockId <= TileID.LihzahrdBrick; blockId++)
          AdvancedCircuits.customActivatableBlocks.Add(blockId);
        for (int blockId = TileID.PalladiumColumn; blockId <= TileID.SpookyWood; blockId++)
          AdvancedCircuits.customActivatableBlocks.Add(blockId);
        for (int blockId = TileID.Cog; blockId <= TileID.SandStoneSlab; blockId++)
          AdvancedCircuits.customActivatableBlocks.Add(blockId);
        for (int blockId = TileID.DynastyWood; blockId <= TileID.BlueDynastyShingles; blockId++)
          AdvancedCircuits.customActivatableBlocks.Add(blockId);
        for (int blockId = TileID.BorealWood; blockId <= TileID.PalmWood; blockId++)
          AdvancedCircuits.customActivatableBlocks.Add(blockId);
        for (int blockId = TileID.TinPlating; blockId <= TileID.ConfettiBlack; blockId++)
          AdvancedCircuits.customActivatableBlocks.Add(blockId);
      }

      return AdvancedCircuits.customActivatableBlocks.Contains(blockType);
    }

    public static bool IsLogicalGate(int blockType) {
      return (
        blockType == AdvancedCircuits.BlockType_ORGate ||
        blockType == AdvancedCircuits.BlockType_ANDGate ||
        blockType == AdvancedCircuits.BlockType_XORGate
      );
    }

    private static readonly HashSet<int> senderComponents = new HashSet<int> {
      TileID.Timers,
      TileID.Switches,
      TileID.Lever,
      TileID.PressurePlates,
      TileID.GrandfatherClocks,
      TileID.OpenDoor,
      TileID.ClosedDoor,
      TileID.TallGateClosed,
      TileID.TallGateOpen,
      TileID.TrapdoorClosed,
      TileID.TrapdoorOpen,
    };
    public static bool IsOriginSenderComponent(int blockType) {
      return AdvancedCircuits.senderComponents.Contains(blockType);
    }

    public static string GetComponentName(int blockType) {
      switch (blockType) {
        case AdvancedCircuits.BlockType_ORGate:
          return "OR-Gate";
        case AdvancedCircuits.BlockType_ANDGate:
          return "AND-Gate";
        case AdvancedCircuits.BlockType_XORGate:
          return "XOR-Gate";
        case AdvancedCircuits.BlockType_NOTGate:
          return "NOT-Gate";
        case AdvancedCircuits.BlockType_Swapper:
          return "Swapper";
        case AdvancedCircuits.BlockType_CrossoverBridge:
          return "Crossover Bridge";
        case AdvancedCircuits.BlockType_WirelessTransmitter:
          return "Wireless Transmitter";
        default:
          return TerrariaUtils.Tiles.GetBlockTypeName(blockType, 0);
      }
    }

    public static IEnumerable<DPoint> EnumerateComponentPortLocations(DPoint componentOriginLocation, DPoint componentSize) {
      DPoint origin = componentOriginLocation;
      DPoint size = componentSize;

      for (int x = 0; x < size.X; x++) {
        yield return new DPoint(origin.X + x, origin.Y - 1);
        yield return new DPoint(origin.X + x, origin.Y + size.Y);
      }

      for (int y = 0; y < size.Y; y++) {
        yield return new DPoint(origin.X - 1, origin.Y + y);
        yield return new DPoint(origin.X + size.X, origin.Y + y);
      }
    }

    public static IEnumerable<DPoint> EnumerateComponentPortLocations(ObjectMeasureData measureData) {
      DPoint componentOriginLocation = measureData.OriginTileLocation;
      DPoint componentSize = measureData.Size;
      if (measureData.BlockType == TileID.OpenDoor)
        componentSize = new DPoint(1, 3);

      return AdvancedCircuits.EnumerateComponentPortLocations(componentOriginLocation, componentSize);
    }

    public static DPoint GetPortAdjacentComponentTileLocation(ObjectMeasureData measureData, DPoint portLocation) {
      DPoint origin = measureData.OriginTileLocation;
      DPoint size = measureData.Size;
      if (measureData.BlockType == TileID.OpenDoor)
        size = new DPoint(1, 3);
      else if (measureData.BlockType == TileID.TrapdoorOpen)
        size = new DPoint(2, 2);

      if (portLocation.X < origin.X)
        return new DPoint(origin.X, portLocation.Y);
      if (portLocation.Y < origin.Y)
        return new DPoint(portLocation.X, origin.Y);
      if (portLocation.X >= origin.X + size.X)
        return new DPoint(origin.X + size.X - 1, portLocation.Y);
      if (portLocation.Y >= origin.Y + size.Y)
        return new DPoint(portLocation.X, origin.Y + size.Y - 1);

      throw new ArgumentException("The given port location referes to no port of this component at all.", "portLocation");
    }

    public static bool IsComponentWiredByPort(DPoint componentOriginLocation, DPoint componentSize) {
      foreach (DPoint portLocation in AdvancedCircuits.EnumerateComponentPortLocations(componentOriginLocation, componentSize))
        if (TerrariaUtils.Tiles[portLocation].HasWire())
          return true;

      return false;
    }

    public static bool IsComponentWiredByPort(ObjectMeasureData measureData) {
      return AdvancedCircuits.IsComponentWiredByPort(measureData.OriginTileLocation, measureData.Size);
    }

    public static bool? SignalToBool(SignalType signal) {
      if (signal == SignalType.Swap)
        return null;

      return (signal == SignalType.On);
    }

    public static SignalType BoolToSignal(bool? signal) {
      if (signal == null)
        return SignalType.Swap;

      return AdvancedCircuits.BoolToSignal(signal.Value);
    }

    public static SignalType BoolToSignal(bool signal) {
      if (signal)
        return SignalType.On;

      return SignalType.Off;
    }

    public static Direction DirectionFromTileLocations(DPoint startTile, DPoint endTile) {
      if (startTile.X < endTile.X)
        return Direction.Right;
      if (startTile.X > endTile.X)
        return Direction.Left;
      if (startTile.Y < endTile.Y)
        return Direction.Down;
      if (startTile.Y > endTile.Y)
        return Direction.Up;
      
      return Direction.Unknown;
    }

    public static Direction InvertDirection(Direction direction) {
      if (direction == Direction.Left)
        return Direction.Right;
      if (direction == Direction.Up)
        return Direction.Down;
      if (direction == Direction.Right)
        return Direction.Left;
      if (direction == Direction.Down)
        return Direction.Up;

      throw new ArgumentException("The given direction can not be inverted because it is invalid.", nameof(direction));
    }

    public static int MeasureTimerFrameTime(DPoint timerLocation) {
      ITile timerTile = TerrariaUtils.Tiles[timerLocation];
      if (!timerTile.active() || timerTile.type != TileID.Timers)
        throw new ArgumentException("The tile is not active or no timer.", nameof(timerLocation));

      int frames = -1;
      switch (TerrariaUtils.Tiles[timerLocation].frameX) {
        case 0:
          frames = 60;
          break;
        case 18:
          frames = 3 * 60;
          break;
        case 36:
          frames = 5 * 60;
          break;
      }

      switch ((PaintColor)timerTile.color()) {
        case AdvancedCircuits.Paint_Timer_Mul2:
          return frames * 2;
        case AdvancedCircuits.Paint_Timer_Mul4:
          return frames * 4;
        case AdvancedCircuits.Paint_Timer_Mul8:
          return frames * 8;
        case AdvancedCircuits.Paint_Timer_Div2:
          return frames / 2;
        case AdvancedCircuits.Paint_Timer_Div3:
          return frames / 3;
        case AdvancedCircuits.Paint_Timer_Div4:
          return frames / 4;
        default:
          return frames;
      }
    }

    public static int SwapperPaintToCount(PaintColor componentPaint) {
      switch (componentPaint) {
        default:
          return 1;
        case AdvancedCircuits.Paint_Swapper_2:
          return 2;
        case AdvancedCircuits.Paint_Swapper_3:
          return 3;
        case AdvancedCircuits.Paint_Swapper_4:
          return 4;
        case AdvancedCircuits.Paint_Swapper_5:
          return 5;
        case AdvancedCircuits.Paint_Swapper_6:
          return 6;
        case AdvancedCircuits.Paint_Swapper_7:
          return 7;
      }
    }

    public static IEnumerable<WireColor> EnumerateWireColors() {
      for (int colorIndex = 1; colorIndex <= 4; colorIndex++)
        yield return (WireColor)colorIndex;
    } 
  }
}
