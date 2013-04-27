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

namespace Terraria.Plugins.Common.AdvancedCircuits {
  public static class AdvancedCircuits {
    #region [Constants]
    public const BlockType BlockType_ORGate          = BlockType.CopperOre;
    public const BlockType BlockType_ANDGate         = BlockType.SilverOre;
    public const BlockType BlockType_XORGate         = BlockType.GoldOre;
    public const BlockType BlockType_NOTGate         = BlockType.Obsidian;
    public const BlockType BlockType_Swapper         = BlockType.IronOre;
    public const BlockType BlockType_CrossoverBridge = BlockType.Spike;
    public const BlockType BlockType_InputPort       = BlockType.Glass;
    public const BlockType BlockType_Modifier        = BlockType.CobaltOre;
    public const BlockType BlockType_BlockActivator  = BlockType.ActiveStone;
    #endregion


    #region [Methods: IsPortDefiningComponentBlock, IsModifierSupportingComponentBlock]
    public static bool IsPortDefiningComponentBlock(BlockType blockType) {
      return (
        blockType == BlockType.Switch ||
        blockType == BlockType.Lever ||
        blockType == BlockType.XSecondTimer ||
        blockType == AdvancedCircuits.BlockType_ORGate ||
        blockType == AdvancedCircuits.BlockType_ANDGate ||
        blockType == AdvancedCircuits.BlockType_XORGate ||
        blockType == AdvancedCircuits.BlockType_NOTGate ||
        blockType == AdvancedCircuits.BlockType_Swapper ||
        blockType == AdvancedCircuits.BlockType_CrossoverBridge ||
        blockType == BlockType.GrandfatherClock ||
        blockType == AdvancedCircuits.BlockType_BlockActivator
      );
    }

    public static bool IsModifierSupportingComponentBlock(BlockType blockType) {
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
        blockType == BlockType.OutletPump
      );
    }

    public static bool IsCustomActivatableBlock(BlockType blockType) {
      return (
        blockType == BlockType.DirtBlock ||
        blockType == BlockType.StoneBlock ||
        blockType == BlockType.WoodPlatform ||
        blockType == BlockType.EbonstoneBlock ||
        blockType == BlockType.Wood ||
        (blockType >= BlockType.GrayBrick && blockType <= BlockType.BlueBrick) ||
        (blockType >= BlockType.GreenBrick && blockType <= BlockType.Spike) ||
        blockType == BlockType.Glass ||
        blockType == BlockType.AshBlock ||
        blockType == BlockType.MudBlock ||
        blockType == BlockType.ObsidianBrick ||
        blockType == BlockType.HellstoneBrick ||
        (blockType >= BlockType.PearlstoneBlock && blockType <= BlockType.WoodenBeam) ||
        blockType == BlockType.ActiveStone ||
        blockType == BlockType.InactiveStone ||
        blockType == BlockType.DemoniteBrick ||
        (blockType >= BlockType.RedCandyCaneBlock && blockType <= BlockType.SnowBrick)
      );
    }
    #endregion

    #region [Method: GetComponentName]
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
        default:
          return TerrariaUtils.Tiles.GetBlockTypeName(blockType);
      }
    }
    #endregion

    #region [Methods: EnumerateComponentPortLocations, GetPortAdjacentComponentTileLocation]
    public static IEnumerable<DPoint> EnumerateComponentPortLocations(ObjectMeasureData measureData) {
      return AdvancedCircuits.EnumerateComponentPortLocations(measureData.OriginTileLocation, measureData.Size);
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

    public static DPoint GetPortAdjacentComponentTileLocation(ObjectMeasureData measureData, DPoint portLocation) {
      DPoint origin = measureData.OriginTileLocation;
      DPoint size = measureData.Size;

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
    #endregion

    #region [Methods: CountComponentModifiers, GetModifierAdjacentComponents]
    public static int CountComponentModifiers(ObjectMeasureData measureData) {
      return AdvancedCircuits.CountComponentModifiers(measureData.OriginTileLocation, measureData.Size);
    }

    public static int CountComponentModifiers(DPoint componentOriginLocation, DPoint componentSize) {
      DPoint origin = componentOriginLocation;
      int modifierCount = 0;

      Tile possibleModifierTile = TerrariaUtils.Tiles[origin.X - 1, origin.Y - 1];
      if (possibleModifierTile.active && possibleModifierTile.type == (int)AdvancedCircuits.BlockType_Modifier)
        modifierCount++;

      possibleModifierTile = TerrariaUtils.Tiles[origin.X + componentSize.X, origin.Y - 1];
      if (possibleModifierTile.active && possibleModifierTile.type == (int)AdvancedCircuits.BlockType_Modifier)
        modifierCount++;

      possibleModifierTile = TerrariaUtils.Tiles[origin.X + componentSize.X, origin.Y + componentSize.Y];
      if (possibleModifierTile.active && possibleModifierTile.type == (int)AdvancedCircuits.BlockType_Modifier)
        modifierCount++;

      possibleModifierTile = TerrariaUtils.Tiles[origin.X - 1, origin.Y + componentSize.Y];
      if (possibleModifierTile.active && possibleModifierTile.type == (int)AdvancedCircuits.BlockType_Modifier)
        modifierCount++;

      return modifierCount;
    }

    public static IEnumerable<DPoint> EnumerateModifierAdjacentComponents(DPoint modifierLocation) {
      Tile possibleComponentTile = TerrariaUtils.Tiles[modifierLocation.X - 1, modifierLocation.Y - 1];
      if (possibleComponentTile.active && AdvancedCircuits.IsModifierSupportingComponentBlock((BlockType)possibleComponentTile.type))
        yield return new DPoint(modifierLocation.X - 1, modifierLocation.Y - 1);

      possibleComponentTile = TerrariaUtils.Tiles[modifierLocation.X + 1, modifierLocation.Y - 1];
      if (possibleComponentTile.active && AdvancedCircuits.IsModifierSupportingComponentBlock((BlockType)possibleComponentTile.type))
        yield return new DPoint(modifierLocation.X + 1, modifierLocation.Y - 1);

      possibleComponentTile = TerrariaUtils.Tiles[modifierLocation.X + 1, modifierLocation.Y + 1];
      if (possibleComponentTile.active && AdvancedCircuits.IsModifierSupportingComponentBlock((BlockType)possibleComponentTile.type))
        yield return new DPoint(modifierLocation.X + 1, modifierLocation.Y + 1);

      possibleComponentTile = TerrariaUtils.Tiles[modifierLocation.X - 1, modifierLocation.Y + 1];
      if (possibleComponentTile.active && AdvancedCircuits.IsModifierSupportingComponentBlock((BlockType)possibleComponentTile.type))
        yield return new DPoint(modifierLocation.X - 1, modifierLocation.Y + 1);
    }
    #endregion

    #region [Methods: SignalToBool, BoolToSignal]
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
    #endregion

    #region [Methods: DirectionFromTileLocations, InvertDirection]
    public static Direction DirectionFromTileLocations(DPoint startTile, DPoint endTile) {
      if (startTile.X < endTile.X)
        return Direction.Right;
      else if (startTile.X > endTile.X)
        return Direction.Left;
      else if (startTile.Y < endTile.Y)
        return Direction.Down;
      else if (startTile.Y > endTile.Y)
        return Direction.Up;
      else
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
    #endregion

    #region [Method: ModifierCountToConfigProfile]
    public static ComponentConfigProfile ModifierCountToConfigProfile(int modifiers) {
      return (ComponentConfigProfile)modifiers;
    }
    #endregion

    #region [Method: MeasureTimerFrameTime]
    public static int MeasureTimerFrameTime(DPoint timerLocation) {
      if (!TerrariaUtils.Tiles[timerLocation].active || TerrariaUtils.Tiles[timerLocation].type != (int)BlockType.XSecondTimer)
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

      switch (AdvancedCircuits.CountComponentModifiers(TerrariaUtils.Tiles.MeasureObject(timerLocation))) {
        case 1:
          frames *= 2;
          break;
        case 2:
          frames *= 4;
          break;
        case 3:
          frames /= 2;
          break;
        case 4:
          frames /= 4;
          break;
      }

      return frames;
    }
    #endregion
  }
}
