using UnityEngine;

public struct PhosphorCommand
{
    public Vector2 Position;
    public float Time;
    public bool IsElectronBeamActive;

    public PhosphorCommand(Vector2 position, float time, bool isElectronBeamActive)
    {
        this.Position = position;
        this.Time = time;
        this.IsElectronBeamActive = isElectronBeamActive;
    }
}
