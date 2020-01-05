using System;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public struct TrapConfigKey {
    public static readonly TrapConfigKey Invalid = default(TrapConfigKey);

    public TrapStyle TrapStyle { get; private set; }
    public PaintColor Paint { get; private set; }


    public TrapConfigKey(TrapStyle trapStyle, PaintColor paint): this() {
      this.TrapStyle = trapStyle;
      this.Paint = paint;
    }

    public override int GetHashCode() {
      return (int)this.TrapStyle ^ (int)this.Paint;
    }

    public bool Equals(TrapConfigKey other) {
      return (
        this.TrapStyle == other.TrapStyle &&
        this.Paint == other.Paint
      );
    }

    public override bool Equals(object obj) {
      if (!(obj is TrapConfigKey))
        return false;

      return this.Equals((TrapConfigKey)obj);
    }

    public static bool operator ==(TrapConfigKey a, TrapConfigKey b) {
      return a.Equals(b);
    }

    public static bool operator !=(TrapConfigKey a, TrapConfigKey b) {
      return !a.Equals(b);
    }

    public override string ToString() {
      return string.Format("{{TrapStyle = {0}, Paint = {1}}}", this.TrapStyle, this.Paint);
    }
  }
}
