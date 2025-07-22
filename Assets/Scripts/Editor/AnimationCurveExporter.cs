#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AnimationCurveExporter : EditorWindow {
    private AnimatorController animatorController;
    private AnimationClip selectedClip;

    [MenuItem("Tools/Camera Shake Curve Exporter")]
    static void Init() {
        GetWindow<AnimationCurveExporter>("Curve Exporter");
    }

    void OnGUI() {
        GUILayout.Label("Export Animation Curves", EditorStyles.boldLabel);

        animatorController = (AnimatorController)EditorGUILayout.ObjectField("Animator Controller", animatorController, typeof(AnimatorController), false);

        if (animatorController != null) {
            var clips = animatorController.animationClips;
            if (clips.Length == 0) {
                EditorGUILayout.HelpBox("This controller has no animation clips.", MessageType.Warning);
                return;
            }

            string[] clipNames = System.Array.ConvertAll(clips, clip => clip.name);
            int selectedIndex = Mathf.Max(0, System.Array.IndexOf(clips, selectedClip));
            selectedIndex = EditorGUILayout.Popup("Select Clip", selectedIndex, clipNames);
            selectedClip = clips[selectedIndex];

            if (GUILayout.Button("Export Curves")) {
                ExportCurves(selectedClip);
            }
        }
    }

    void ExportCurves(AnimationClip clip) {
        var bindings = AnimationUtility.GetCurveBindings(clip);

        if (bindings.Length == 0) {
            Debug.Log("No curve bindings");
            return;
        }

        foreach (var binding in bindings) {
            if (binding.propertyName.Contains("Position") || binding.propertyName.Contains("Rotation") || binding.propertyName.Contains("Angles")) {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                Debug.Log($"Curve: {binding.propertyName}");
                foreach (var key in curve.keys) {
                    Debug.Log($"Time: {key.time}, Value: {key.value}");
                }
            }
            else {
                Debug.Log("Curve doesn't contain 'Position' or 'Rotation'");
            }
        }

        Debug.Log($"Export complete for clip: {clip.name}");
    }
}
#endif
