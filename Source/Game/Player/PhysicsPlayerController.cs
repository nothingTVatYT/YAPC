using FlaxEngine;

namespace Game.Player;

/// <summary>
/// PhysicsPlayerController Script for a rigidbody-based FPS-style player controller 
///
/// This controller expects the actions "Crouch" and "Jump" on top of the defaults in the input settings.
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
    [Tooltip("Maximum walking speed")]
    public float WalkingSpeed = 500;
    [Tooltip("Maximum running speed")]
    public float RunSpeed = 1500;
    public float MouseSpeed = 1f;
    public float AccelerationForce = 1000;
    private bool _inputEnabled = true;
    private float _bodyRotationY;
    private Vector3 _movementLocalDirection;
    private float _speed;
    private Vector3 _playerTargetDirection = Vector3.Forward;

    private RigidBody _rigidBody;
    private CapsuleCollider _playerCollider;
    private bool _isGrounded;
    private bool _startJumping;
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
        _bodyRotationY = 0;
        if (!_inputEnabled || _playerCollider == null)
            return;

        _crouching = Input.GetAction("Crouch") && _isGrounded;
        _speed = Input.GetAction("Sprint") && !_crouching ? RunSpeed : WalkingSpeed;

        if (_crouching)
        {
            if (_playerCollider.Height > 0.61f)
                Debug.Log("Start crouching");
            _playerCollider.Height = 0.6f;
            PlayerCamera.LocalPosition = new Vector3(0, 0.2, 0);
        }
        else
        {
            _playerCollider.Height = _initialColliderHeight;
            PlayerCamera.LocalPosition = new Vector3(0, _initialCameraHeight, 0);
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

    public override void OnFixedUpdate()
    {
        if (_rigidBody == null)
            return;

        var heightOverGround = 200f;
        if (Physics.SphereCastAll(Actor.Position, 0.2f, Vector3.Down, out var results, 200))
        {
            foreach (var hit in results)
            {
                if (hit.Collider.Equals(_playerCollider))
                    continue;
                heightOverGround = (hit.Point - Actor.Position).Y;
            }
        }

        _isGrounded = heightOverGround < 5f;
        if (!_isGrounded)
            _movementLocalDirection = Vector3.Zero;
        
        _rigidBody.Direction = Vector3.Lerp(_rigidBody.Direction, _playerTargetDirection, 0.5f);

        // limit speed
        var localVelocity = Actor.Transform.WorldToLocalVector(_rigidBody.LinearVelocity);
        if (localVelocity.Z > _speed || localVelocity.Z < -_speed || localVelocity.X > _speed || localVelocity.X < -_speed)
        {
            _rigidBody.AddForce(-_rigidBody.LinearVelocity, ForceMode.Acceleration);
            _movementLocalDirection.Z = 0;
        }

        if (_startJumping)
        {
            _movementLocalDirection.Y += 6;
            _startJumping = false;
        }
        _rigidBody.AddForce(Actor.Transform.TransformDirection(_movementLocalDirection) * AccelerationForce, ForceMode.Acceleration);

        // decelerate if there is no input
        if (_isGrounded && _movementLocalDirection.LengthSquared < Mathf.Epsilon)
        {
            if (_rigidBody.LinearVelocity.LengthSquared > Mathf.Epsilon)
                _rigidBody.AddForce(-_rigidBody.LinearVelocity, ForceMode.Acceleration);
        }
    }

    public override void ReleaseMouse()
    {
        _inputEnabled = false;
        Screen.CursorVisible = true;
        Screen.CursorLock = CursorLockMode.None;
        if (CrossHair != null)
            CrossHair.Control.Visible = false;
    }

    public override void SetInputEnabled(bool value)
    {
        if (value)
            GrabMouse();
        else
        {
            ReleaseMouse();
        }
    }

    public override bool IsInputEnabled()
    {
        return _inputEnabled;
    }

    public override void RequestTeleport(Vector3 position, Matrix rotation)
    {
        throw new System.NotImplementedException();
    }

    public override void GrabMouse()
    {
        _inputEnabled = true;
        Screen.CursorVisible = false;
        Screen.CursorLock = CursorLockMode.Locked;
        if (CrossHair != null)
            CrossHair.Control.Visible = true;
    }

    public override void OnDebugDraw()
    {
        DebugDraw.DrawRay(Actor.Position, _playerTargetDirection * 100, Color.Red );
    }
}
