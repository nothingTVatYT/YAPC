using System;
using System.Linq;
using FlaxEngine;
using YAPC.Tools;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace YAPC.Player;

/// <summary>
/// PhysicsPlayerController Script for a RigidBody-based FPS-style player controller 
///
/// This controller expects the actions "Crouch", "Sprint" and "Jump" on top of the defaults in the input settings.
/// 
/// </summary>
public class PhysicsPlayerController : PlayerController
{
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    // ReSharper disable ConvertToConstant.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnassignedField.Global
    /// <summary>
    /// Adapt the collider and the camera local position to the player values
    /// </summary>
    [Tooltip("Adapt the collider and camera location to the player values")]
    public bool AdaptCollider = true;
    [Tooltip("The camera simulating the player's eyes")]
    public Camera PlayerCamera;
    [Tooltip("A model as a placeholder in editor mode for the player")]
    public Actor PlayerModel;
    [Tooltip("UI control marking the center of the screen")]
    public UIControl CrossHair;
    [Tooltip("Set to true to hide the placeholder in play mode")]
    public bool HidePlayerModelOnStart = true;
    [Tooltip("Use the mouse to rotate the body, not only the camera")]
    public bool RotateBodyWithCamera = true;
    [Tooltip("Counteract forward momentum when there is no movement requested")]
    public bool DecelerateOnIdle = false;
    [Tooltip("Simulate a higher friction when the player is sliding left or right")]
    public bool SimulateSideFriction = true;
    [Tooltip("Limit the speed of the player to the defined speeds (walk/run)")]
    public bool LimitSpeed = true;
    [Tooltip("Rotation speed in degrees/sec when using the keyboard")]
    public float KeyboardRotationSpeed = 1000f;
    [Tooltip("Rotation factor when using the mouse")]
    public float MouseSpeed = 1f;
    [Tooltip("The force to be applied to make the player move forward")]
    public float AccelerationForce = 10000;
    [Tooltip("A factor for the jump force")]
    public float JumpForceFactor = 3.5f;
    // ReSharper restore ConvertToConstant.Global
    // ReSharper restore FieldCanBeMadeReadOnly.Global
    // ReSharper restore MemberCanBePrivate.Global
    // ReSharper restore UnassignedField.Global

    /// <inheritdoc />
    public override float CurrentSpeed => _speedAverage.Average;
    /// <inheritdoc />
    public override bool IsGrounded => _isGrounded;
    private bool _inputEnabled = true;
    private float _bodyRotationY;
    private Vector3 _movementLocalDirection;
    private float _maxSpeed;
    private readonly FloatAverage _speedAverage = new(30);
    private Vector3 _playerTargetDirection = Vector3.Forward;

    private RigidBody _rigidBody;
    private CapsuleCollider _playerCollider;
    private Vector3 _groundVelocity = Vector3.Zero;
    private bool _isGrounded;
    private bool _isRunning;
    private bool _startJumping;
    private bool _breaking;
    private float _stopTime;
    private bool _crouching;
    private float _initialColliderHeight;
    private Vector3 _initialCameraLocation;
    private FootstepsSound _footsteps;

    /// <inheritdoc/>
    public override void OnStart()
    {
        PlayerCamera = Actor.FindActor<Camera>();
        var crosshairTag = Tags.Get(CrosshairTagName);
        if (crosshairTag != null)
        {
            var actorsWithTag = Level.FindActors(crosshairTag);
            if (actorsWithTag is { Length: > 0 })
                CrossHair = actorsWithTag.First().As<UIControl>();
        }

        if (PlayerModel != null && HidePlayerModelOnStart)
            PlayerModel.IsActive = false;
        _rigidBody = Actor.As<RigidBody>();
        _playerCollider = Actor.FindActor<Collider>() as CapsuleCollider;
        if (_playerCollider != null && AdaptCollider)
        {
            _playerCollider.Radius = PlayerValues.CollisionRadius;
            _playerCollider.Height = PlayerValues.Height - 2 * PlayerValues.CollisionRadius;
            var cameraHeight = PlayerCamera.LocalPosition;
            cameraHeight.Y = (PlayerValues.Height - PlayerValues.CollisionRadius) / 2;
            PlayerCamera.LocalPosition = cameraHeight;
        }

        _playerTargetDirection = Actor.Direction;
        _initialColliderHeight = _playerCollider?.Height ?? 120;
        _initialCameraLocation = PlayerCamera.LocalPosition;
        _footsteps = Actor.FindScript<FootstepsSound>();

    }
    
    /// <inheritdoc/>
    public override void OnEnable()
    {
        GrabMouse();
    }

    /// <inheritdoc/>
    public override void OnDisable()
    {
        ReleaseMouse();
    }

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        // RMB toggles mouse cursor locking
        if (Input.Mouse.GetButtonDown(MouseButton.Right))
        {
            if (Screen.CursorLock == CursorLockMode.Locked)
                ReleaseMouse();
            else
                GrabMouse();
        }

        _bodyRotationY = 0;
        if (!_inputEnabled || _playerCollider == null)
        {
            _speedAverage.Add(0);
            return;
        }

        var wasCrouching = _crouching;
        _crouching = Input.GetAction("Crouch") && _isGrounded;
        if (Input.GetAction("Sprint") && !_crouching)
        {
            _maxSpeed = PlayerValues.MaxRunningSpeed;
            _isRunning = true;
        }
        else
        {
            _maxSpeed = PlayerValues.MaxWalkingSpeed;
            _isRunning = false;
        }

        if (_crouching && !wasCrouching)
        {
            _playerCollider.Height = PlayerValues.CrouchingHeight - PlayerValues.CollisionRadius * 2;
            PlayerCamera.LocalPosition = new Vector3(_initialCameraLocation.X, 1, _initialCameraLocation.Z);
        }
        if (!_crouching && wasCrouching)
        {
            // check max head room
            var maxDist = PlayerValues.Height - PlayerValues.CrouchingHeight;
            if (Physics.SphereCastAll(Actor.Position, _playerCollider.Radius, Vector3.Up,
                    out var results, maxDist))
            {
                foreach (var result in results)
                {
                    if (result.Collider.Equals(_playerCollider))
                        continue;
                    _crouching = true;
                    break;
                }
            }

            if (!_crouching)
            {
                _playerCollider.Height = _initialColliderHeight;
                PlayerCamera.LocalPosition = _initialCameraLocation;
            }
        }

        // forward and backward movement (e.g. W + S keys)
        var verticalInput = Input.GetAxis("Vertical");
        // turning (e.g. A + D keys)
        var horizontalInput = Input.GetAxis("Horizontal");
        _movementLocalDirection = new Vector3(RotateBodyWithCamera ? horizontalInput : 0, 0, verticalInput);
        if (_isGrounded && Input.GetAction("Jump"))
            _startJumping = true;

        // mouse look
        var mouseX = Input.GetAxis("Mouse X");
        var mouseY = Input.GetAxis("Mouse Y");
        var cameraRotation = PlayerCamera.LocalEulerAngles;
        cameraRotation.X = Mathf.Clamp(mouseY * MouseSpeed + cameraRotation.X, -80, 80);
        var rotationY = Mathf.Clamp(mouseX * MouseSpeed + cameraRotation.Y, -80, 80);
        if (!RotateBodyWithCamera)
        {
            cameraRotation.Y = rotationY;
            _bodyRotationY = horizontalInput * KeyboardRotationSpeed * Time.DeltaTime;
            _playerTargetDirection *= Quaternion.Euler(0, _bodyRotationY, 0);
        }
        else
        {
            _bodyRotationY = mouseX * MouseSpeed;
            _playerTargetDirection *= Quaternion.Euler(0, _bodyRotationY * Time.DeltaTime * 180, 0);
        }
        PlayerCamera.LocalEulerAngles = cameraRotation;
    }

    /// <inheritdoc />
    public override void OnFixedUpdate()
    {
        if (_rigidBody == null)
            return;

        _groundVelocity = Vector3.Zero;
        var heightOverGround = PlayerValues.Height;
        if (Physics.SphereCastAll(Actor.Position, _playerCollider.Radius, Vector3.Down,
                out var results, PlayerValues.Height))
        {
            var nearestHitPoint = Vector3.Zero;
            RigidBody nearestRigidBody = null;
            foreach (var hit in results)
            {
                if (hit.Collider.Equals(_playerCollider))
                    continue;
                var heightOverHitPoint = (Actor.Position - hit.Point).Y - _playerCollider.Radius - _playerCollider.Height/2; 
                if (heightOverHitPoint < heightOverGround)
                {
                    heightOverGround = heightOverHitPoint;
                    nearestHitPoint = hit.Point;
                    nearestRigidBody = hit.Collider.AttachedRigidBody;
                }
                
                if (_footsteps != null)
                    try
                    {
                        _footsteps.GroundTags = hit.Collider.Tags;
                    }
                    catch (Exception)
                    {
                        _footsteps.GroundTags = null;
                    }
            }

            if (nearestRigidBody != null)
            {
                _groundVelocity = CalculateVelocity(nearestRigidBody, nearestHitPoint);
                _playerTargetDirection *= Quaternion.Euler(nearestRigidBody.AngularVelocity);
            }
        }

        _isGrounded = heightOverGround < 5f;
        if (!_isGrounded)
        {
            _movementLocalDirection = Vector3.Zero;
            if (_footsteps != null)
                _footsteps.Movement = FootstepsSound.MovementType.Idle;
        }

        // release break
        if (_movementLocalDirection.LengthSquared > Mathf.Epsilon && _breaking)
        {
            _breaking = false;
        }

        // decelerate on ground if there is no input and no ground movement
        if (_isGrounded && DecelerateOnIdle && _groundVelocity.LengthSquared < Mathf.Epsilon)
        {
            if (_movementLocalDirection.LengthSquared < Mathf.Epsilon)
            {
                if (!_breaking)
                {
                    _breaking = true;
                    _stopTime = Time.GameTime + 0.25f;
                }
                else
                {
                    var velocityXz = _rigidBody.LinearVelocity;
                    if (Time.GameTime >= _stopTime && velocityXz.LengthSquared > Mathf.Epsilon)
                    {
                        _rigidBody.AddForce(-velocityXz, ForceMode.VelocityChange);
                    }
                }
            }
        }

        // simulate a higher friction side to side than forward if not strafing
        if (_isGrounded && SimulateSideFriction)
        {
            if (Mathf.Abs(_movementLocalDirection.X) < Mathf.Epsilon)
            {
                var velocityX = _rigidBody.Transform.WorldToLocalVector(_rigidBody.LinearVelocity - _groundVelocity);
                velocityX.Y = 0;
                velocityX.Z = 0;
                _rigidBody.AddForce(_rigidBody.Transform.TransformDirection(-velocityX), ForceMode.Acceleration);
            }
        }

        _rigidBody.Direction = Vector3.Lerp(_rigidBody.Direction, _playerTargetDirection, 0.5f);

        // limit speed
        var speedXz = _rigidBody.LinearVelocity - _groundVelocity;
        speedXz.Y = 0;
        var speedScalar = speedXz.Length;
        _speedAverage.Add(speedScalar);
        if (speedScalar > _maxSpeed && LimitSpeed)
        {
            _rigidBody.AddForce(-speedXz, ForceMode.Acceleration);
            _movementLocalDirection.Z = 0;
            _movementLocalDirection.X = 0;
        }

        if (_startJumping)
        {
            _movementLocalDirection.Y += JumpForceFactor;
            _startJumping = false;
        }
        _rigidBody.AddForce(Actor.Transform.TransformDirection(_movementLocalDirection) * AccelerationForce, ForceMode.Acceleration);
        if (_footsteps != null && _isGrounded)
            _footsteps.Movement = speedScalar > 1
                ? (_isRunning ? FootstepsSound.MovementType.Running : FootstepsSound.MovementType.Walking)
                : FootstepsSound.MovementType.Idle;
    }

    private static Vector3 CalculateVelocity(RigidBody rigidBody, Vector3 point)
    {
        var r = point - rigidBody.Position;
        return r + Vector3.Cross(rigidBody.AngularVelocity, r) + rigidBody.LinearVelocity;
    }

    /// <inheritdoc />
    public override void ReleaseMouse()
    {
        _inputEnabled = false;
        Screen.CursorVisible = true;
        Screen.CursorLock = CursorLockMode.None;
        if (CrossHair != null)
            CrossHair.Control.Visible = false;
    }

    /// <inheritdoc />
    public override void SetInputEnabled(bool value)
    {
        if (value)
            GrabMouse();
        else
        {
            ReleaseMouse();
        }
    }

    /// <inheritdoc />
    public override bool IsInputEnabled()
    {
        return _inputEnabled;
    }

    /// <inheritdoc />
    public override void RequestTeleport(Vector3 position, Matrix rotation)
    {
        // force the RigidBody to a full stop
        _rigidBody.LinearVelocity = Vector3.Zero;
        Actor.Position = position;
        Actor.Rotation = rotation;
    }

    /// <inheritdoc />
    public override void GrabMouse()
    {
        _inputEnabled = true;
        Screen.CursorVisible = false;
        Screen.CursorLock = CursorLockMode.Locked;
        if (CrossHair != null)
            CrossHair.Control.Visible = true;
    }

    /// <inheritdoc />
    public override void OnDebugDraw()
    {
        DebugDraw.DrawRay(Actor.Position, _playerTargetDirection * 100, Color.Red );
    }
}
