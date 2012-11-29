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

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public static class AdvancedCircuits {
    #region [Constants]
    public const int TileId_ORGate          = Terraria.TileId_CopperOre;
    public const int TileId_ANDGate         = Terraria.TileId_SilverOre;
    public const int TileId_XORGate         = Terraria.TileId_GoldOre;
    public const int TileId_NOTGate         = Terraria.TileId_Obsidian;
    public const int TileId_Swapper         = Terraria.TileId_IronOre;
    public const int TileId_CrossoverBridge = Terraria.TileId_Spike;
    public const int TileId_InputPort       = Terraria.TileId_Glass;
    public const int TileId_Modifier        = Terraria.TileId_CobaltOre;
    public const int TileId_BlockActivator  = Terraria.TileId_ActiveStone;
    #endregion


    #region [Methods: IsPortDefiningComponentBlock, IsModifierSupportingComponentBlock]
    public static bool IsPortDefiningComponentBlock(int blockId) {
      return (
        blockId == Terraria.TileId_Switch ||
        blockId == Terraria.TileId_Lever ||
        blockId == Terraria.TileId_XSecondTimer ||
        blockId == AdvancedCircuits.TileId_ORGate ||
        blockId == AdvancedCircuits.TileId_ANDGate ||
        blockId == AdvancedCircuits.TileId_XORGate ||
        blockId == AdvancedCircuits.TileId_NOTGate ||
        blockId == AdvancedCircuits.TileId_Swapper ||
        blockId == AdvancedCircuits.TileId_CrossoverBridge ||
        blockId == Terraria.TileId_GrandfatherClock ||
        blockId == AdvancedCircuits.TileId_BlockActivator
      );
    }

    public static bool IsModifierSupportingComponentBlock(int blockId) {
      return (
        blockId == Terraria.TileId_XSecondTimer ||
        blockId == Terraria.TileId_Switch ||
        blockId == Terraria.TileId_Lever ||
        blockId == Terraria.TileId_GrandfatherClock ||
        blockId == AdvancedCircuits.TileId_ANDGate ||
        blockId == AdvancedCircuits.TileId_XORGate ||
        blockId == AdvancedCircuits.TileId_ORGate ||
        blockId == Terraria.TileId_DartTrap
      );
    }

    public static bool IsCustomActivatableBlock(int blockId) {
      return (
        blockId == Terraria.TileId_DirtBlock ||
        blockId == Terraria.TileId_StoneBlock ||
        blockId == Terraria.TileId_WoodPlatform ||
        blockId == Terraria.TileId_EbonstoneBlock ||
        blockId == Terraria.TileId_Wood ||
        (blockId >= Terraria.TileId_GrayBrick && blockId <= Terraria.TileId_BlueBrick) ||
        (blockId >= Terraria.TileId_GreenBrick && blockId <= Terraria.TileId_Spike) ||
        blockId == Terraria.TileId_Glass ||
        blockId == Terraria.TileId_AshBlock ||
        blockId == Terraria.TileId_MudBlock ||
        blockId == Terraria.TileId_ObsidianBrick ||
        blockId == Terraria.TileId_HellstoneBrick ||
        (blockId >= Terraria.TileId_PearlstoneBlock && blockId <= Terraria.TileId_WoodenBeam) ||
        blockId == Terraria.TileId_ActiveStone ||
        blockId == Terraria.TileId_InactiveStone ||
        blockId == Terraria.TileId_DemoniteBrick ||
        (blockId >= Terraria.TileId_RedCandyCaneBlock && blockId <= Terraria.TileId_SnowBrick)
      );
    }
    #endregion

    #region [Method: GetComponentName]
    public static string GetComponentName(int blockId) {
      switch (blockId) {
        case AdvancedCircuits.TileId_ORGate:
          return "OR-Gate";
        case AdvancedCircuits.TileId_ANDGate:
          return "AND-Gate";
        case AdvancedCircuits.TileId_XORGate:
          return "XOR-Gate";
        case AdvancedCircuits.TileId_NOTGate:
          return "NOT-Gate";
        case AdvancedCircuits.TileId_Swapper:
          return "Swapper";
        case AdvancedCircuits.TileId_CrossoverBridge:
          return "Crossover Bridge";
        default:
          return Terraria.Tiles.GetBlockName(blockId);
      }
    }
    #endregion

    #region [Methods: EnumerateComponentPortLocations, GetPortAdjacentComponentTileLocation]
    public static IEnumerable<DPoint> EnumerateComponentPortLocations(Terraria.SpriteMeasureData measureData) {
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

    public static DPoint GetPortAdjacentComponentTileLocation(Terraria.SpriteMeasureData measureData, DPoint portLocation) {
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
    public static int CountComponentModifiers(Terraria.SpriteMeasureData measureData) {
      return AdvancedCircuits.CountComponentModifiers(measureData.OriginTileLocation, measureData.Size);
    }

    public static int CountComponentModifiers(DPoint componentOriginLocation, DPoint componentSize) {
      DPoint origin = componentOriginLocation;
      int modifierCount = 0;

      Tile possibleModifierTile = Terraria.Tiles[origin.X - 1, origin.Y - 1];
      if (possibleModifierTile.active && possibleModifierTile.type == AdvancedCircuits.TileId_Modifier)
        modifierCount++;

      possibleModifierTile = Terraria.Tiles[origin.X + componentSize.X, origin.Y - 1];
      if (possibleModifierTile.active && possibleModifierTile.type == AdvancedCircuits.TileId_Modifier)
        modifierCount++;

      possibleModifierTile = Terraria.Tiles[origin.X + componentSize.X, origin.Y + componentSize.Y];
      if (possibleModifierTile.active && possibleModifierTile.type == AdvancedCircuits.TileId_Modifier)
        modifierCount++;

      possibleModifierTile = Terraria.Tiles[origin.X - 1, origin.Y + componentSize.Y];
      if (possibleModifierTile.active && possibleModifierTile.type == AdvancedCircuits.TileId_Modifier)
        modifierCount++;

      return modifierCount;
    }

    public static IEnumerable<DPoint> EnumerateModifierAdjacentComponents(DPoint modifierLocation) {
      Tile possibleComponentTile = Terraria.Tiles[modifierLocation.X - 1, modifierLocation.Y - 1];
      if (possibleComponentTile.active && AdvancedCircuits.IsModifierSupportingComponentBlock(possibleComponentTile.type))
        yield return new DPoint(modifierLocation.X - 1, modifierLocation.Y - 1);

      possibleComponentTile = Terraria.Tiles[modifierLocation.X + 1, modifierLocation.Y - 1];
      if (possibleComponentTile.active && AdvancedCircuits.IsModifierSupportingComponentBlock(possibleComponentTile.type))
        yield return new DPoint(modifierLocation.X + 1, modifierLocation.Y - 1);

      possibleComponentTile = Terraria.Tiles[modifierLocation.X + 1, modifierLocation.Y + 1];
      if (possibleComponentTile.active && AdvancedCircuits.IsModifierSupportingComponentBlock(possibleComponentTile.type))
        yield return new DPoint(modifierLocation.X + 1, modifierLocation.Y + 1);

      possibleComponentTile = Terraria.Tiles[modifierLocation.X - 1, modifierLocation.Y + 1];
      if (possibleComponentTile.active && AdvancedCircuits.IsModifierSupportingComponentBlock(possibleComponentTile.type))
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
      if (!Terraria.Tiles[timerLocation].active || Terraria.Tiles[timerLocation].type != Terraria.TileId_XSecondTimer)
        throw new ArgumentException("The tile is not active or no timer.", "timerLocation");

      int frames = -1;
      switch (Terraria.Tiles[timerLocation].frameX) {
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

      switch (AdvancedCircuits.CountComponentModifiers(Terraria.MeasureSprite(timerLocation))) {
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
