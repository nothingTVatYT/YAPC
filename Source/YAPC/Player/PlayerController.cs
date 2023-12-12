using FlaxEngine;

namespace YAPC.Player;

/// <summary>
/// PlayerController Script.
/// </summary>
public abstract class PlayerController : Script
{
    /// <summary>
    /// The player controller can grab the mouse (it may be locked)
    /// </summary>
    public abstract void GrabMouse();
    /// <summary>
    /// The player controller should release the mouse e.g. when a GUI is shown
    /// </summary>
    public abstract void ReleaseMouse();
    /// <summary>
    /// If set to false all input should be ignored by the controller
    /// </summary>
    /// <param name="value"></param>
    public abstract void SetInputEnabled(bool value);
    /// <summary>
    /// Returns true if the controller currently ignores input.
    /// </summary>
    /// <returns></returns>
    public abstract bool IsInputEnabled();
    /// <summary>
    /// Game logic wants the player to be teleported to the given location.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public abstract void RequestTeleport(Vector3 position, Matrix rotation);
}
