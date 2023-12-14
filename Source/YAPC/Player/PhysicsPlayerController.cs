using FlaxEngine;
using YAPC.Tools;

namespace YAPC.Player;

/// <summary>
/// PhysicsPlayerController Script for a rigidbody-based FPS-style player controller 
///
/// This controller expects the actions "Crouch", "Sprint" and "Jump" on top of the defaults in the input settings.
/// 
/// </summary>
public class PhysicsPlayerController : PlayerController
{
    public Camera PlayerCamera;
    public Actor PlayerModel;
    [Tooltip("UI control marking the center of the screen")]
    public UIControl CrossHair;
    public bool HidePlayerModelOnStart = true;
    [Tooltip("Use the mouse to rotate the body, not only the camera")]
    public bool RotateBodyWithCamera = true;
    public float KeyboardRotationSpeed = 1000f;
    public float MouseSpeed = 1f;
    public float AccelerationForce = 10000;
    public float DecelerationForceFactor = 50;
    public float JumpForceFactor = 3.5f;
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
    private bool _isGrounded;
    private bool _startJumping;
    private bool _breaking;
    private float _stopTime;
    private bool _crouching;
    private float _initialColliderHeight;
    private float _initialCameraHeight;

    /// <inheritdoc/>
    public override void OnStart()
    {
        PlayerCamera = Actor.FindActor<Camera>();
        if (PlayerModel != null && HidePlayerModelOnStart)
            PlayerModel.IsActive = false;
        _rigidBody = Actor.As<RigidBody>();
        _playerCollider = Actor.FindActor<Collider>() as CapsuleCollider;
        _playerTargetDirection = Actor.Direction;
        _initialColliderHeight = _playerCollider?.Height ?? 120;
        _initialCameraHeight = PlayerCamera.LocalPosition.Y;
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
        _maxSpeed = Input.GetAction("Sprint") && !_crouching ? RunSpeed : WalkingSpeed;

        if (_crouching && !wasCrouching)
        {
            Debug.Log("Start crouching");
            _playerCollider.Height = 20f;
            PlayerCamera.LocalPosition = new Vector3(0, 1, 0);
        }
        if (!_crouching && wasCrouching)
        {
            // check max head room
            if (Physics.SphereCastAll(Actor.Position, _playerCollider.Radius, Vector3.Up, out var results, 200))
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
                PlayerCamera.LocalPosition = new Vector3(0, _initialCameraHeight, 0);
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

        var heightOverGround = 200f;
        if (Physics.SphereCastAll(Actor.Position, 20f, Vector3.Down, out var results, 200))
        {
            foreach (var hit in results)
            {
                if (hit.Collider.Equals(_playerCollider))
                    continue;
                heightOverGround = Mathf.Min(heightOverGround, (hit.Point - Actor.Position).Y);
            }
        }

        _isGrounded = heightOverGround < 5f;
        if (!_isGrounded)
            _movementLocalDirection = Vector3.Zero;

        // release break
        if (_movementLocalDirection.LengthSquared > Mathf.Epsilon && _breaking)
        {
            _breaking = false;
        }

        // decelerate on ground if there is no input
        if (_isGrounded)
        {
            if (_movementLocalDirection.LengthSquared < Mathf.Epsilon)
            {
                if (!_breaking)
                {
                    _breaking = true;
                    _stopTime = Time.GameTime + 0.5f;
                }
                else
                {
                    var velocityXz = _rigidBody.LinearVelocity;
                    velocityXz.Y = 0;
                    if (Time.GameTime >= _stopTime && velocityXz.LengthSquared > Mathf.Epsilon)
                        _rigidBody.AddForce(-velocityXz * DecelerationForceFactor, ForceMode.Acceleration);
                }
            }
        }

        _rigidBody.Direction = Vector3.Lerp(_rigidBody.Direction, _playerTargetDirection, 0.5f);

        // limit speed
        var speedXz = _rigidBody.LinearVelocity;
        speedXz.Y = 0;
        var speedScalar = speedXz.Length;
        _speedAverage.Add(speedScalar);
        if (speedScalar > _maxSpeed)
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
        // force the rigidbody to a full stop
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
