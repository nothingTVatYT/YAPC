using FlaxEngine;

namespace YAPC.Player;

/// <summary>
/// PlayerController Script.
/// </summary>
public abstract class PlayerController : Script
{
    public abstract void GrabMouse();
    public abstract void ReleaseMouse();
    public abstract void SetInputEnabled(bool value);
    public abstract bool IsInputEnabled();
    public abstract void RequestTeleport(Vector3 position, Matrix rotation);
}
