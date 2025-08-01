using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class NeverCull : MonoBehaviour {
    void Start() {
        MeshFilter mesh = GetComponent<MeshFilter>();
        mesh.mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
    }
}
