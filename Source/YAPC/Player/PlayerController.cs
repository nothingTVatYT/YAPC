using FlaxEngine;

namespace YAPC.Player;

/// <summary>
/// PlayerController Script.
/// </summary>
public abstract class PlayerController : Script
{
    [Tooltip("Maximum walking speed (cm/s)")]
    public float WalkingSpeed = 500;
    [Tooltip("Maximum running speed (cm/s)")]
    public float RunSpeed = 1500;

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

    /// <summary>
    /// Get the current speed in the horizontal plane
    /// </summary>
    public abstract float CurrentSpeed { get; }

    /// <summary>
    /// Returns true if the player is on a surface, false if in the air (e.g. jumping, falling)
    /// </summary>
    public abstract bool IsGrounded { get; }
}
