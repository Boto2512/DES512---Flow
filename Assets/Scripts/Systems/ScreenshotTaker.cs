using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ScreenshotTaker : MonoBehaviour {
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        ScreenCapture.CaptureScreenshot("Menu Screenshot.png", 10);
    }

    // Update is called once per frame
    void Update() {

    }
}
