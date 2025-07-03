using System.Collections.Generic;

public interface IHasSpeedThresholds {
    public List<SpeedStageThreshold> Thresholds { get; }
}
