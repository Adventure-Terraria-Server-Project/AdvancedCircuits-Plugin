using System;
using System.Xml;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class NullStatueAction {
    public static NullStatueAction FromXmlElement(XmlElement xmlData) {
      return new NullStatueAction();
    }
  }
}
