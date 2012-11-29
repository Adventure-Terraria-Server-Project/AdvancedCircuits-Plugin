// This file is provided unter the terms of the 
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// 
// Written by CoderCow

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.InteropServices;
using DPoint = System.Drawing.Point;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class PluginCooperationHandler {
    #region [Constants]
    private const string InfiniteSignsGuid = "d1c86597-66c0-4590-aace-7b381d332294";
    private const string InfiniteSignsClassName = "InfiniteSigns.InfiniteSigns";
    #endregion

    #region [Property: InfiniteSignsPlugin]
    private readonly dynamic infiniteSignsPlugin;

    protected dynamic InfiniteSignsPlugin {
      get { return this.infiniteSignsPlugin; }
    }

    public bool IsInfiniteSignsAvailable {
      get { return (this.infiniteSignsPlugin != null); }
    }
    #endregion


    #region [Method: Constructor]
    public PluginCooperationHandler() {
      Assembly[] domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
      foreach (Assembly assembly in domainAssemblies) {
        GuidAttribute[] guidAttributes = (GuidAttribute[])assembly.GetCustomAttributes(typeof(GuidAttribute), true);
        if (guidAttributes.Length == 0)
          continue;

        string assemblyGuid = guidAttributes[0].Value;
        if (!assemblyGuid.Equals(PluginCooperationHandler.InfiniteSignsGuid, StringComparison.InvariantCultureIgnoreCase))
          continue;

        Type pluginType = assembly.GetType(PluginCooperationHandler.InfiniteSignsClassName);
        this.infiniteSignsPlugin = pluginType.InvokeMember("LatestInstance", BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public, null, null, null);
      }
    }
    #endregion

    #region [Infinite Signs]
    public string InfiniteSigns_GetSignText(DPoint location) {
      if (!this.IsInfiniteSignsAvailable)
        return null;
      
      try {
        return this.InfiniteSignsPlugin.GetSignText(location.X, location.Y);
      } catch (Exception ex) {
        AdvancedCircuitsPlugin.Trace.WriteLineError("An exception was thrown when cooperating with a plugin:\n{0}", ex.ToString());
        return null;
      }
    }
    #endregion
  }
}
