using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using DPoint = System.Drawing.Point;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public static class AdvancedCircuits {
    public const BlockType BlockType_ORGate              = BlockType.CopperOre;
    public const BlockType BlockType_ANDGate             = BlockType.SilverOre;
    public const BlockType BlockType_XORGate             = BlockType.GoldOre;
    public const BlockType BlockType_NOTGate             = BlockType.Obsidian;
    public const BlockType BlockType_Swapper             = BlockType.IronOre;
    public const BlockType BlockType_CrossoverBridge     = BlockType.Spike;
    public const BlockType BlockType_InputPort           = BlockType.Glass;
    public const BlockType BlockType_BlockActivator      = BlockType.ActiveStone;
    public const BlockType BlockType_WirelessTransmitter = BlockType.AdamantiteOre;

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


    public static bool IsPortDefiningComponentBlock(BlockType blockType) {
      return (
        blockType == BlockType.Switch ||
        blockType == BlockType.Lever ||
        blockType == BlockType.XSecondTimer ||
        blockType == BlockType.DoorOpened ||
        blockType == BlockType.DoorClosed ||
        blockType == AdvancedCircuits.BlockType_ORGate ||
        blockType == AdvancedCircuits.BlockType_ANDGate ||
        blockType == AdvancedCircuits.BlockType_XORGate ||
        blockType == AdvancedCircuits.BlockType_NOTGate ||
        blockType == AdvancedCircuits.BlockType_Swapper ||
        blockType == AdvancedCircuits.BlockType_CrossoverBridge ||
        blockType == BlockType.GrandfatherClock ||
        blockType == AdvancedCircuits.BlockType_BlockActivator ||
        blockType == AdvancedCircuits.BlockType_WirelessTransmitter
      );
    }

    public static bool IsPaintSupportingComponent(BlockType blockType) {
      return (
        blockType == BlockType.XSecondTimer ||
        blockType == BlockType.Switch ||
        blockType == BlockType.Lever ||
        blockType == BlockType.GrandfatherClock ||
        blockType == AdvancedCircuits.BlockType_ANDGate ||
        blockType == AdvancedCircuits.BlockType_XORGate ||
        blockType == AdvancedCircuits.BlockType_ORGate ||
        blockType == BlockType.DartTrap ||
        blockType == BlockType.InletPump ||
        blockType == BlockType.OutletPump ||
        blockType == AdvancedCircuits.BlockType_WirelessTransmitter
      );
    }

    public static bool IsCustomActivatableBlock(BlockType blockType) {
      return (
        blockType == BlockType.DirtBlock ||
        blockType == BlockType.StoneBlock ||
        blockType == BlockType.WoodPlatform ||
        blockType == BlockType.DemoniteOre ||
        blockType == BlockType.CorruptGrass ||
        blockType == BlockType.EbonstoneBlock ||
        blockType == BlockType.Wood ||
        (blockType >= BlockType.GrayBrick && blockType <= BlockType.BlueBrick) ||
        (blockType >= BlockType.GreenBrick && blockType <= BlockType.WaterCandle) ||
        (blockType >= BlockType.Cobweb && blockType <= BlockType.Glass) ||
        (blockType >= BlockType.Obsidian && blockType <= BlockType.JungleGrass) ||
        blockType == BlockType.JungleVine ||
        (blockType >= BlockType.SapphireBlock && blockType <= BlockType.MushroomGrass) ||
        blockType == BlockType.ObsidianBrick ||
        blockType == BlockType.HellstoneBrick ||
        blockType == BlockType.HallowedGrass ||
        blockType == BlockType.EbonsandBlock ||
        blockType == BlockType.HallowedVine ||
        (blockType >= BlockType.PearlsandBlock && blockType <= BlockType.WoodenBeam) ||
        blockType == BlockType.IceRodBlock ||
        blockType == BlockType.ActiveStone ||
        blockType == BlockType.InactiveStone ||
        blockType == BlockType.DemoniteBrick ||
        blockType == BlockType.Explosives ||
        (blockType >= BlockType.RedCandyCaneBlock && blockType <= BlockType.SnowBrick)
      );
    }

    public static bool IsLogicalGate(BlockType blockType) {
      return (
        blockType == AdvancedCircuits.BlockType_ORGate ||
        blockType == AdvancedCircuits.BlockType_ANDGate ||
        blockType == AdvancedCircuits.BlockType_XORGate
      );
    }

    public static bool IsOriginSenderComponent(BlockType blockType) {
      return (
        blockType == BlockType.XSecondTimer ||
        blockType == BlockType.Switch ||
        blockType == BlockType.Lever ||
        blockType == BlockType.PressurePlate ||
        blockType == BlockType.GrandfatherClock ||
        blockType == BlockType.DoorOpened ||
        blockType == BlockType.DoorClosed
      );
    }

    public static string GetComponentName(BlockType blockType) {
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
          return TerrariaUtils.Tiles.GetBlockTypeName(blockType);
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
      if (measureData.BlockType == BlockType.DoorOpened)
        componentSize = new DPoint(1, 3);

      return AdvancedCircuits.EnumerateComponentPortLocations(componentOriginLocation, componentSize);
    }

    public static DPoint GetPortAdjacentComponentTileLocation(ObjectMeasureData measureData, DPoint portLocation) {
      DPoint origin = measureData.OriginTileLocation;
      DPoint size = measureData.Size;
      if (measureData.BlockType == BlockType.DoorOpened)
        size = new DPoint(1, 3);

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

      throw new ArgumentException("The given direction can not be inverted because it is invalid.", "direction");
    }

    public static int MeasureTimerFrameTime(DPoint timerLocation) {
      Tile timerTile = TerrariaUtils.Tiles[timerLocation];
      if (!timerTile.active() || timerTile.type != (int)BlockType.XSecondTimer)
        throw new ArgumentException("The tile is not active or no timer.", "timerLocation");

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
  }
}
