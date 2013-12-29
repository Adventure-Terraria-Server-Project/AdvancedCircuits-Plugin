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
    #region [Property: PluginTrace]
    private readonly PluginTrace pluginTrace;

    protected PluginTrace PluginTrace {
      get { return this.pluginTrace; }
    }
    #endregion

    #region [Property: IsProtectorAvailable]
    private readonly bool isProtectorAvailable;

    public bool IsProtectorAvailable {
      get { return this.isProtectorAvailable; }
    }
    #endregion

    #region [Property: IsSignCommandsAvailable]
    private readonly bool isSignCommandsAvailable;

    public bool IsSignCommandsAvailable {
      get { return this.isSignCommandsAvailable; }
    }
    #endregion


    #region [Method: Constructor]
    public PluginCooperationHandler(PluginTrace pluginTrace) {
      Contract.Requires<ArgumentNullException>(pluginTrace != null);

      const string ProtectorSomeTypeQualifiedName = 
        "Terraria.Plugins.CoderCow.Protector.ProtectorPlugin, Protector";
      const string SignCommandsSomeTypeQualifiedName = "SignCommands.scSign, SignCommands";

      this.pluginTrace = pluginTrace;
      
      this.isProtectorAvailable = (Type.GetType(ProtectorSomeTypeQualifiedName, false) != null);
      this.isSignCommandsAvailable = (Type.GetType(SignCommandsSomeTypeQualifiedName, false) != null);
    }
    #endregion

    #region [Protector]
    public bool Protector__CheckProtected(TSPlayer player, DPoint tileLocation, bool fullAccessRequired) {
      try {
        return !ProtectorPlugin.LatestInstance.ProtectionManager.CheckBlockAccess(player, tileLocation, fullAccessRequired);
      } catch (Exception ex) {
        throw new CooperatingPluginException(null, ex);
      }
    }
    #endregion

    #region [SignCommands]
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
    #endregion
  }
}
