using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using DPoint = System.Drawing.Point;

using TShockAPI;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class CircuitProcessingResult {
    public DPoint SenderLocation { get; set; }
    public bool SenderWasSwitched { get; set; }
    public bool IsAdvancedCircuit { get; set; }
    public SignalType OriginSignal { get; set; }
    public TSPlayer TriggeringPlayer { get; set; }
    public bool TriggeredPassively { get; set; }
    public int CircuitLength { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public int ProcessedBranchCount { get; set; }
    public int SignaledComponentsCounter { get; set; }
    public int SignaledPortDefiningComponentsCounter { get; set; }
    public int SignaledTraps { get; set; }
    public int SignaledStatues { get; set; }
    public int SignaledPumps { get; set; }
    public int TransferedWater { get; set; }
    public int TransferedLava { get; set; }
    public CircuitWarnReason WarnReason { get; set; }
    public int WarnRelatedComponentType { get; set; }
    public int CancellationRelatedComponentType { get; set; }
    public CircuitCancellationReason CancellationReason { get; set; }


    public CircuitProcessingResult() {
      this.CancellationRelatedComponentType = -1;
    }
  }
}
