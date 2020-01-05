using System;
using DPoint = System.Drawing.Point;

using SignCommands;

using TShockAPI;

using Terraria.Plugins.Common;
using Terraria.Plugins.CoderCow.Protector;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class PluginCooperationHandler {
    protected PluginTrace PluginTrace { get; private set; }
    public bool IsProtectorAvailable { get; private set; }
    public bool IsSignCommandsAvailable { get; private set; }


    public PluginCooperationHandler(PluginTrace pluginTrace) {
      if (pluginTrace == null) throw new ArgumentNullException();

      const string ProtectorSomeTypeQualifiedName = "Terraria.Plugins.CoderCow.Protector.ProtectorPlugin, Protector";
      const string SignCommandsSomeTypeQualifiedName = "SignCommands.ScSign, SignCommands";

      this.PluginTrace = pluginTrace;
      
      this.IsProtectorAvailable = (Type.GetType(ProtectorSomeTypeQualifiedName, false) != null);
      this.IsSignCommandsAvailable = (Type.GetType(SignCommandsSomeTypeQualifiedName, false) != null);
    }

    public bool Protector_CheckProtected(TSPlayer player, DPoint tileLocation, bool fullAccessRequired) {
      try {
        return !ProtectorPlugin.LatestInstance.ProtectionManager.CheckBlockAccess(player, tileLocation, fullAccessRequired);
      } catch (Exception ex) {
        throw new CooperatingPluginException(null, ex);
      }
    }

    public bool SignCommands_CheckIsSignCommand(string text) {
      try {
        return text.StartsWith(SignCommands.SignCommands.config.DefineSignCommands, StringComparison.CurrentCultureIgnoreCase);
      } catch (Exception ex) {
        throw new CooperatingPluginException(ex);
      }
    }

    public void SignCommands_ExecuteSignCommand(TSPlayer player, DPoint signLocation, string text) {
      try {
        SignCommands.SignCommands.OnSignHit(signLocation.X, signLocation.Y, text, player.Index);
      } catch (Exception ex) {
        throw new CooperatingPluginException(null, ex);
      }
    }
  }
}
