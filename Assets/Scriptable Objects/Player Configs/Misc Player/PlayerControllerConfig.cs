using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerControllerConfig", menuName = "Scriptable Objects/Player Config/PlayerControllerConfig")]
public class PlayerControllerConfig : ScriptableObject {
    [Header("Speed Thresholds")]
    public List<SpeedStageThreshold> Thresholds;

    private void OnValidate() {
        //Thresholds = Thresholds.OrderBy(st => st.SpeedThreshold).ToList();
    }
}
