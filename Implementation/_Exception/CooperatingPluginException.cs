using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  [Serializable]
  public class CooperatingPluginException: Exception {
    public CooperatingPluginException(string message, Exception inner) : base(message, inner) {}

    public CooperatingPluginException(Exception inner): 
      base("A cooperating plugin has caused an exception. See inner exception for details.", inner) {}

    protected CooperatingPluginException(SerializationInfo info, StreamingContext context) : base(info, context) {}
  }
}