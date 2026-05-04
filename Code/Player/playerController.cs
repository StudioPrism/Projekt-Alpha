// Copyright Studio Prism. Licensed under PolyForm Shield 1.0.0.
// https://polyformproject.org/licenses/shield/1.0.0
// Required Notice: Copyright Studio Prism (https://github.com/studioprism)

using System;
using ShrimpleController = ShrimpleCharacterController.ShrimpleCharacterController;

public sealed class playerController : Component
{
    public enum MovementState
    {
        Idle,
        Walking,
        Sprinting,
        Crouching,
        Airborne,
        Falling
    }

    [RequireComponent] public ShrimpleController controller { get; set; }
    [Property] public camController cameraController { get; set; }
    [Property] public SkinnedModelRenderer renderer { get; set; }

    // --- Movement ---
    [Property, Group( "Movement" ), Range( 50f, 250f )]   public float walkSpeed             { get; set; } = 140f;
    [Property, Group( "Movement" ), Range( 100f, 500f )]  public float sprintSpeed           { get; set; } = 290f;
    [Property, Group( "Movement" ), Range( 25f, 160f )]   public float crouchSpeed           { get; set; } = 80f;
    [Property, Group( "Movement" ), Range( 150f, 600f )]  public float jumpStrength          { get; set; } = 350f;
    [Property, Group( "Movement" ), Range( 0.1f, 1f )]    public float sprintForwardThreshold{ get; set; } = 0.5f;

    // --- Physics ---
    [Property, Group( "Physics" ), Range( 1f, 30f )]      public float groundAcceleration   { get; set; } = 14f;
    [Property, Group( "Physics" ), Range( 1f, 15f )]      public float airAcceleration      { get; set; } = 2.5f;
    [Property, Group( "Physics" ), Range( 1f, 20f )]      public float groundFriction       { get; set; } = 8f;
    [Property, Group( "Physics" ), Range( 0.1f, 2f )]     public float gravityScale         { get; set; } = 0.9f;

    // --- Crouch ---
    [Property, Group( "Crouch" ), Range( 20f, 80f )]      public float standHeight          { get; set; } = 64f;
    [Property, Group( "Crouch" ), Range( 10f, 60f )]      public float crouchHeight         { get; set; } = 36f;
    [Property, Group( "Crouch" ), Range( 1f, 20f )]       public float crouchLerpSpeed      { get; set; } = 12f;

    // --- Body ---
    [Property, Group( "Body" ), Range( 0f, 30f )]         public float bodyTurnSmoothing    { get; set; } = 18f;
    [Property, Group( "Body" )]                           public bool  rotateBodyToCameraYaw { get; set; } = true;

    // --- Sync ---
    [Sync] public MovementState CurrentState  { get; private set; }
    [Sync] public Vector3       WishMove      { get; private set; }
    [Sync] public bool          IsMoving      { get; private set; }
    [Sync] public bool          IsSprinting   { get; private set; }
    [Sync] public bool          IsCrouching   { get; private set; }
    [Sync] public bool          IsGrounded    { get; private set; }
    [Sync] public float         HorizontalSpeed { get; private set; }

    // --- Public read-only (weapon sway consumers) ---
    public float    MoveFraction  { get; private set; }
    public Rotation BodyRotation  => Rotation.FromYaw( cameraController?.eyeAngles.yaw ?? WorldRotation.Yaw() );

    // --- Private state ---
    private float _currentCapsuleHeight;

    protected override void OnStart()
    {
        cameraController ??= Components.Get<camController>();
        renderer         ??= Components.Get<SkinnedModelRenderer>( FindMode.EverythingInSelfAndDescendants );

        _currentCapsuleHeight = standHeight;
    }

    protected override void OnFixedUpdate()
    {
        if ( IsProxy ) return;

        ReadMovementInput();
        UpdateCrouchHeight();
        ApplyGravityScale();
        UpdateJump();
        UpdateBodyRotation();
        UpdateRendererParameters();
    }

    // -----------------------------------------------------------------------
    // Input + Quake-CPM acceleration
    // -----------------------------------------------------------------------

    private void ReadMovementInput()
    {
        var analogMove = Input.AnalogMove;
        WishMove   = analogMove;
        IsMoving   = analogMove.WithZ( 0f ).Length > 0.05f;
        IsGrounded = controller.IsOnGround;
        IsCrouching = Input.Down( "Duck" );

        // Sprint: dot product against body-forward so backward sprinting is impossible
        var wantsSprint    = Input.Down( "Run" );
        var worldForward   = BodyRotation.Forward.WithZ( 0f ).Normal;
        var currentHorizDir = controller.Velocity.WithZ( 0f ).Normal;
        var forwardDot     = Vector3.Dot( currentHorizDir, worldForward );
        IsSprinting = wantsSprint && forwardDot > sprintForwardThreshold && !IsCrouching && IsGrounded;

        var wishSpeed = IsCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed;
        var yawRotation  = BodyRotation;
        var wishDir      = (analogMove.Normal * yawRotation).WithZ( 0f ).Normal;

        if ( IsGrounded )
	        PM_Friction( Time.Delta );

		// Only accelerate if there's actual input — don't feed PM_Accelerate a zero wishDir
        if ( IsMoving )
        {
	        var accel = IsGrounded ? groundAcceleration : airAcceleration;
	        PM_Accelerate( wishDir, wishSpeed, accel, Time.Delta );
        }
        HorizontalSpeed = controller.Velocity.WithZ( 0f ).Length;
        MoveFraction    = IsSprinting
            ? MathX.Remap( HorizontalSpeed, walkSpeed, sprintSpeed, 0.5f, 1f )
            : MathX.Remap( HorizontalSpeed, 0f, walkSpeed, 0f, 0.5f );
        MoveFraction    = MathX.Clamp( MoveFraction, 0f, 1f );

        CurrentState = BuildState();
    }

    /// <summary>
    /// Quake PM_Accelerate — adds velocity without exceeding wishSpeed cap.
    /// Preserves horizontal momentum through turns and jumps.
    /// </summary>
    private void PM_Accelerate( Vector3 wishDir, float wishSpeed, float accel, float dt )
    {
        var currentVel  = controller.Velocity;
        var currentSpeed = Vector3.Dot( currentVel, wishDir );
        var addSpeed     = MathX.Clamp( wishSpeed - currentSpeed, 0f, accel * wishSpeed * dt );

        controller.WishVelocity = currentVel + wishDir * addSpeed;
    }

    /// <summary>
    /// Quake PM_Friction — bleeds speed before acceleration to prevent ice-skating.
    /// Only applied while grounded.
    /// </summary>
    private void PM_Friction( float dt )
    {
	    var vel   = controller.Velocity.WithZ( 0f );
	    var speed = vel.Length;
	    if ( speed < 1f )
	    {
		    // Force a complete stop — kill any remaining horizontal velocity
		    controller.WishVelocity = new Vector3( 0f, 0f, controller.Velocity.z );
		    return;
	    }

	    var drop     = speed * groundFriction * dt;
	    var newSpeed = MathF.Max( speed - drop, 0f );

	    if ( newSpeed < 8f ) newSpeed = 0f;

	    var scale = newSpeed / speed;
	    controller.WishVelocity = new Vector3( vel.x * scale, vel.y * scale, controller.Velocity.z );
    }

    // -----------------------------------------------------------------------
    // Crouch — lerped capsule height with headroom trace
    // -----------------------------------------------------------------------

    private void UpdateCrouchHeight()
    {
        var targetHeight = IsCrouching ? crouchHeight : standHeight;

        // Headroom check: prevent standing up into solid geometry
        if ( !IsCrouching && _currentCapsuleHeight < standHeight )
        {
            var headPos   = WorldPosition + Vector3.Up * _currentCapsuleHeight;
            var traceEnd  = headPos + Vector3.Up * (standHeight - _currentCapsuleHeight);
            var tr        = Scene.Trace.Ray( headPos, traceEnd ).WithoutTags( "player" ).Run();
            if ( tr.Hit )
                targetHeight = _currentCapsuleHeight; // blocked — stay crouched
        }

        _currentCapsuleHeight = MathX.Lerp( _currentCapsuleHeight, targetHeight, Time.Delta * crouchLerpSpeed );
       
        var capsule = Components.Get<CapsuleCollider>( FindMode.EverythingInSelf );
        if ( capsule.IsValid() )
	        capsule.End = new Vector3( 0f, 0f, _currentCapsuleHeight );
    }

    // -----------------------------------------------------------------------
    // Gravity scaling — Destiny-style floaty-but-predictable arc
    // -----------------------------------------------------------------------

    private void ApplyGravityScale()
    {
        if ( IsGrounded ) return;

        // Counter the default gravity fraction then re-apply scaled version.
        // AppliedGravity is the raw gravity vector s&box is about to integrate —
        // we nudge velocity by the delta to effectively scale it.
        var defaultGravity = controller.AppliedGravity;
        var scaledDelta    = defaultGravity * (gravityScale - 1f) * Time.Delta;
        controller.WishVelocity += scaledDelta;
    }

    // -----------------------------------------------------------------------
    // Jump — unchanged except reads updated IsGrounded
    // -----------------------------------------------------------------------

    private void UpdateJump()
    {
        if ( IsCrouching ) return;
        if ( !Input.Pressed( "Jump" ) ) return;
        if ( !controller.IsOnGround ) return;

        controller.Punch( -controller.AppliedGravity.Normal * jumpStrength );
        IsGrounded   = false;
        CurrentState = MovementState.Airborne;

        if ( renderer.IsValid() )
            renderer.Set( "b_jump", true );
    }

    // -----------------------------------------------------------------------
    // State — unchanged
    // -----------------------------------------------------------------------

    private MovementState BuildState()
    {
        if ( !controller.IsOnGround )
            return controller.Velocity.z < -10f ? MovementState.Falling : MovementState.Airborne;
        if ( IsCrouching )  return MovementState.Crouching;
        if ( IsSprinting )  return MovementState.Sprinting;
        if ( IsMoving )     return MovementState.Walking;
        return MovementState.Idle;
    }

    // -----------------------------------------------------------------------
    // Body rotation — unchanged
    // -----------------------------------------------------------------------

    private void UpdateBodyRotation()
    {
        if ( !rotateBodyToCameraYaw ) return;

        var targetRotation = BodyRotation;
        WorldRotation = bodyTurnSmoothing <= 0f
            ? targetRotation
            : Rotation.Slerp( WorldRotation, targetRotation, Time.Delta * bodyTurnSmoothing );
    }

    // -----------------------------------------------------------------------
    // Renderer parameters — unchanged
    // -----------------------------------------------------------------------

    private void UpdateRendererParameters()
    {
        if ( !renderer.IsValid() ) return;

        var runScale = 1f / renderer.Transform.World.UniformScale;
        var moveX    = Vector3.Dot( controller.Velocity, renderer.WorldRotation.Forward ) * runScale;
        var moveY    = Vector3.Dot( controller.Velocity, renderer.WorldRotation.Right )   * runScale;

        renderer.Set( "move_x",    MathX.Lerp( renderer.GetFloat( "move_x" ), moveX, Time.Delta * 10f ) );
        renderer.Set( "move_y",    MathX.Lerp( renderer.GetFloat( "move_y" ), moveY, Time.Delta * 10f ) );
        renderer.Set( "b_grounded", controller.IsOnGround );
        renderer.Set( "duck",       IsCrouching ? 1f : 0f );
    }
}
