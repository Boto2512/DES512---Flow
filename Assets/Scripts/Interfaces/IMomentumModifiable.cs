using UnityEngine;

public interface IMomentumModifiable {
    public Vector3 GetPosition();
    public Vector3 GetMomentum();
    public void SetMomentum(Vector3 value);
    public void AddMomentum(Vector3 value, ForceMode additionType = ForceMode.Impulse) {
        SetMomentum(GetMomentum() + value);
    }
    public void BeenBombBounced();
}
