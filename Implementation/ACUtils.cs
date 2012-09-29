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
  public static class ACUtils {
    public static IEnumerable<DPoint> EnumerateComponentPortLocations(Terraria.SpriteMeasureData measureData) {
      DPoint origin = measureData.OriginTileLocation;
      DPoint size = measureData.Size;

      for (int x = 0; x < size.X; x++) {
        yield return new DPoint(origin.X + x, origin.Y - 1);
        yield return new DPoint(origin.X + x, origin.Y + size.Y);
      }

      for (int y = 0; y < size.Y; y++) {
        yield return new DPoint(origin.X - 1, origin.Y + y);
        yield return new DPoint(origin.X + size.X, origin.Y + y);
      }
    }
  }
}
