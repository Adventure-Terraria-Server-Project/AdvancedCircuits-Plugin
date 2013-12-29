using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
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
      Contract.Requires<ArgumentNullException>(pluginTrace != null);

      const string ProtectorSomeTypeQualifiedName = "Terraria.Plugins.CoderCow.Protector.ProtectorPlugin, Protector";
      const string SignCommandsSomeTypeQualifiedName = "SignCommands.scSign, SignCommands";

      this.PluginTrace = pluginTrace;
      
      this.IsProtectorAvailable = (Type.GetType(ProtectorSomeTypeQualifiedName, false) != null);
      this.IsSignCommandsAvailable = (Type.GetType(SignCommandsSomeTypeQualifiedName, false) != null);
    }

    public bool Protector__CheckProtected(TSPlayer player, DPoint tileLocation, bool fullAccessRequired) {
      try {
        return !ProtectorPlugin.LatestInstance.ProtectionManager.CheckBlockAccess(player, tileLocation, fullAccessRequired);
      } catch (Exception ex) {
        throw new CooperatingPluginException(null, ex);
      }
    }

    public bool SignCommands_CheckIsSignCommand(string text) {
      try {
        return text.StartsWith(SignCommands.SignCommands.Config.DefineSignCommands, StringComparison.CurrentCultureIgnoreCase);
      } catch (Exception ex) {
        throw new CooperatingPluginException(ex);
      }
    }

    public void SignCommands_ExecuteSignCommand(TSPlayer player, DPoint signLocation, string text) {
      scPlayer scPlayer = SignCommands.SignCommands.scPlayers[player.Index];
      if (scPlayer == null)
        throw new InvalidOperationException("Sign Commands does not recognize the given player.");

      try {
        scSign scSign = new scSign(text, new Point(signLocation.X, signLocation.Y));
        scSign.ExecuteCommands(scPlayer);
      } catch (Exception ex) {
        throw new CooperatingPluginException(null, ex);
      }
    }
  }
}
