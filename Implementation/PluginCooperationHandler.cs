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
using TShockAPI;
using Terraria.Plugins.Common;
using DPoint = System.Drawing.Point;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class PluginCooperationHandler {
    #region [Constants]
    private const string ProtectorGuid = "0d1df7a2-004f-4e62-830a-49477ea0b9be";
    private const string ProtectorClassName = "Terraria.Plugins.CoderCow.Protector.ProtectorPlugin";
    private const string Protector_ProtectionEntryClassName = "Terraria.Plugins.CoderCow.Protector.ProtectionEntry";
    #endregion

    #region [Property: PluginTrace]
    private readonly PluginTrace pluginTrace;

    protected PluginTrace PluginTrace {
      get { return this.pluginTrace; }
    }
    #endregion

    #region [Properties: ProtectorAssembly, ProtectorPlugin, ProtectionManager, IsProtectorAvailable]
    private readonly Assembly protectorAssembly;
    private dynamic protectionManager;
    private readonly dynamic protectorPlugin;

    protected Assembly ProtectorAssembly {
      get { return this.protectorAssembly; }
    }

    protected dynamic ProtectorPlugin {
      get { return this.protectorPlugin; }
    }

    protected dynamic ProtectionManager {
      get { return this.protectionManager; }
    }

    public bool IsProtectorAvailable {
      get { return (this.ProtectorPlugin != null); }
    }
    #endregion


    #region [Method: Constructor]
    public PluginCooperationHandler(PluginTrace pluginTrace) {
      Contract.Requires<ArgumentNullException>(pluginTrace != null);

      this.pluginTrace = pluginTrace;

      Assembly[] domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
      foreach (Assembly assembly in domainAssemblies) {
        GuidAttribute[] guidAttributes = (GuidAttribute[])assembly.GetCustomAttributes(typeof(GuidAttribute), true);
        if (guidAttributes.Length == 0)
          continue;

        string assemblyGuid = guidAttributes[0].Value;
        if (!assemblyGuid.Equals(PluginCooperationHandler.ProtectorGuid, StringComparison.InvariantCultureIgnoreCase))
          continue;

        this.protectorAssembly = assembly;

        Type pluginType = assembly.GetType(PluginCooperationHandler.ProtectorClassName);
        this.protectorPlugin = pluginType.InvokeMember("LatestInstance", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public, null, null, null);
        this.protectionManager = this.protectorPlugin.ProtectionManager;
      }
    }
    #endregion

    #region [Protector]
    private MethodInfo checkBlockAccessMethodInfo;
    public bool Protector__CheckProtected(TSPlayer player, DPoint tileLocation, bool fullAccessRequired) {
      if (!this.IsProtectorAvailable)
        return false;
      
      if (checkBlockAccessMethodInfo == null) {
        Type protectionManagerType = this.ProtectionManager.GetType();
        this.checkBlockAccessMethodInfo = protectionManagerType.GetMethod(
          "CheckBlockAccess", new[] { typeof(TSPlayer), typeof(DPoint), typeof(bool) }
        );
      }

      return !this.checkBlockAccessMethodInfo.Invoke(
        this.ProtectionManager, new object[] { player, tileLocation, fullAccessRequired }
      );
    }
    #endregion
  }
}
