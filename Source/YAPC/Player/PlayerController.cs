using FlaxEngine;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace YAPC.Player;

/// <summary>
/// A struct for player default values
/// </summary>
public struct PlayerDefinition
{
    [Tooltip("The player's height in total when standing in cm")]
    public float Height;
    [Tooltip("The player's height when crouching in cm")]
    public float CrouchingHeight;
    [Tooltip("Half the minimum gap (horizontally) the player can pass in cm")]
    public float CollisionRadius;
    [Tooltip("Maximum walking speed (cm/s)")]
    public float MaxWalkingSpeed;
    [Tooltip("Maximum running speed (cm/s)")]
    public float MaxRunningSpeed;

    public static PlayerDefinition DefaultPlayer = new()
    {
        Height = 180,
        CrouchingHeight = 60,
        CollisionRadius = 25,
        MaxWalkingSpeed = 500,
        MaxRunningSpeed = 1500
    };
}

/// <summary>
/// PlayerController Script.
/// </summary>
public abstract class PlayerController : Script
{
    /// <summary>
    /// Default values for the player
    /// </summary>
    [Tooltip("Defaults of the player")]
    // ReSharper disable once MemberCanBeProtected.Global
    public PlayerDefinition PlayerValues = PlayerDefinition.DefaultPlayer;

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

    public const string CrosshairTagName = "Crosshair";
}
