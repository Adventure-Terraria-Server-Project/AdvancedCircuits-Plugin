using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using DPoint = System.Drawing.Point;

using TShockAPI;

using Terraria.Plugins.Common;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class CircuitProcessingResult {
    #region [Property: SenderLocation]
    private DPoint senderLocation;

    public DPoint SenderLocation {
      get { return this.senderLocation; }
      set { this.senderLocation = value; }
    }
    #endregion

    #region [Property: IsAdvancedCircuit]
    private bool isAdvancedCircuit;

    public bool IsAdvancedCircuit {
      get { return this.isAdvancedCircuit; }
      set { this.isAdvancedCircuit = value; }
    }
    #endregion

    #region [Property: OriginSignal]
    private SignalType originSignal;

    public SignalType OriginSignal {
      get { return this.originSignal; }
      set { this.originSignal = value; }
    }
    #endregion

    #region [Property: TriggeringPlayer]
    private TSPlayer triggeringPlayer;

    public TSPlayer TriggeringPlayer {
      get { return this.triggeringPlayer; }
      set { this.triggeringPlayer = value; }
    }
    #endregion

    #region [Property: TriggeredPassively]
    private bool triggeredPassively;

    public bool TriggeredPassively {
      get { return this.triggeredPassively; }
      set { this.triggeredPassively = value; }
    }
    #endregion

    #region [Property: CircuitLength]
    private int circuitLength;

    public int CircuitLength {
      get { return this.circuitLength; }
      set { this.circuitLength = value; }
    }
    #endregion

    #region [Property: ProcessingTime]
    private TimeSpan processingTime;

    public TimeSpan ProcessingTime {
      get { return this.processingTime; }
      set { this.processingTime = value; }
    }
    #endregion

    #region [Property: ProcessedBranchCount]
    private int processedBranchCount;

    public int ProcessedBranchCount {
      get { return this.processedBranchCount; }
      set { this.processedBranchCount = value; }
    }
    #endregion

    #region [Property: SignaledComponentsCounter]
    private int signaledComponentsCounter;

    public int SignaledComponentsCounter {
      get { return this.signaledComponentsCounter; }
      set { this.signaledComponentsCounter = value; }
    }
    #endregion

    #region [Property: SignaledPortDefiningComponentsCounter]
    private int signaledPortDefiningComponentsCounter;

    public int SignaledPortDefiningComponentsCounter {
      get { return this.signaledPortDefiningComponentsCounter; }
      set { this.signaledPortDefiningComponentsCounter = value; }
    }
    #endregion

    #region [Property: SignaledDartTraps]
    private int signaledDartTraps;

    public int SignaledDartTraps {
      get { return this.signaledDartTraps; }
      set { this.signaledDartTraps = value; }
    }
    #endregion

    #region [Property: SignaledStatues]
    private int signaledStatues;

    public int SignaledStatues {
      get { return this.signaledStatues; }
      set { this.signaledStatues = value; }
    }
    #endregion

    #region [Property: SignaledPumps]
    private int signaledPumps;

    public int SignaledPumps {
      get { return this.signaledPumps; }
      set { this.signaledPumps = value; }
    }
    #endregion

    #region [Property: TransferedWater]
    private int transferedWater;

    public int TransferedWater {
      get { return this.transferedWater; }
      set { this.transferedWater = value; }
    }
    #endregion

    #region [Property: TransferedLava]
    private int transferedLava;

    public int TransferedLava {
      get { return this.transferedLava; }
      set { this.transferedLava = value; }
    }
    #endregion

    #region [Property: WarnReason]
    private CircuitWarnReason warnReason;

    public CircuitWarnReason WarnReason {
      get { return this.warnReason; }
      set { this.warnReason = value; }
    }
    #endregion

    #region [Property: WarnRelatedComponentType]
    private BlockType warnRelatedComponentType;

    public BlockType WarnRelatedComponentType {
      get { return this.warnRelatedComponentType; }
      set { this.warnRelatedComponentType = value; }
    }
    #endregion

    #region [Property: CancellationRelatedComponentType]
    private BlockType cancellationRelatedComponentType;

    public BlockType CancellationRelatedComponentType {
      get { return this.cancellationRelatedComponentType; }
      set { this.cancellationRelatedComponentType = value; }
    }
    #endregion

    #region [Property: CancellationReason]
    private CircuitCancellationReason cancellationReason;

    public CircuitCancellationReason CancellationReason {
      get { return this.cancellationReason; }
      set { this.cancellationReason = value; }
    }
    #endregion


    #region [Method: Constructor]
    public CircuitProcessingResult() {
      this.CancellationRelatedComponentType = BlockType.Invalid;
    }
    #endregion
  }
}
