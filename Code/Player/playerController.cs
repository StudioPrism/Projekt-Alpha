// Copyright Studio Prism. Licensed under PolyForm Shield 1.0.0.
// https://polyformproject.org/licenses/shield/1.0.0
// Required Notice: Copyright Studio Prism (https://github.com/studioprism)

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

    [Property, Group( "Movement" ), Range( 50f, 250f )] public float walkSpeed { get; set; } = 140f;
    [Property, Group( "Movement" ), Range( 100f, 500f )] public float sprintSpeed { get; set; } = 290f;
    [Property, Group( "Movement" ), Range( 25f, 160f )] public float crouchSpeed { get; set; } = 80f;
    [Property, Group( "Movement" ), Range( 150f, 600f )] public float jumpStrength { get; set; } = 350f;
    [Property, Group( "Movement" ), Range( 0.1f, 1f )] public float sprintForwardThreshold { get; set; } = 0.45f;

    [Property, Group( "Body" ), Range( 0f, 30f )] public float bodyTurnSmoothing { get; set; } = 18f;
    [Property, Group( "Body" )] public bool rotateBodyToCameraYaw { get; set; } = true;

    [Sync] public MovementState CurrentState { get; private set; }
    [Sync] public Vector3 WishMove { get; private set; }
    [Sync] public bool IsMoving { get; private set; }
    [Sync] public bool IsSprinting { get; private set; }
    [Sync] public bool IsCrouching { get; private set; }
    [Sync] public bool IsGrounded { get; private set; }
    [Sync] public float HorizontalSpeed { get; private set; }
    public Rotation BodyRotation => Rotation.FromYaw( cameraController?.eyeAngles.yaw ?? WorldRotation.Yaw() );

    protected override void OnStart()
    {
        cameraController ??= Components.Get<camController>();
        renderer ??= Components.Get<SkinnedModelRenderer>( FindMode.EverythingInSelfAndDescendants );
    }

    protected override void OnFixedUpdate()
    {
        if ( IsProxy ) return;

        ReadMovementInput();
        UpdateJump();
        UpdateBodyRotation();
        UpdateRendererParameters();
    }

    private void ReadMovementInput()
    {
        var analogMove = Input.AnalogMove;
        WishMove = analogMove;
        IsMoving = analogMove.WithZ( 0f ).Length > 0.05f;
        IsGrounded = controller.IsOnGround;
        IsCrouching = Input.Down( "Duck" );

        var wantsSprint = Input.Down( "Run" );
        var movingForwardEnough = analogMove.x > sprintForwardThreshold;
        IsSprinting = wantsSprint && movingForwardEnough && !IsCrouching && IsGrounded;

        var wishSpeed = IsCrouching
            ? crouchSpeed
            : IsSprinting
                ? sprintSpeed
                : walkSpeed;

        var yawRotation = BodyRotation;
        var wishDirection = analogMove.Normal * yawRotation;
        controller.WishVelocity = wishDirection * wishSpeed;

        HorizontalSpeed = controller.Velocity.WithZ( 0f ).Length;
        CurrentState = BuildState();
    }

    private void UpdateJump()
    {
        if ( IsCrouching ) return;
        if ( !Input.Pressed( "Jump" ) ) return;
        if ( !controller.IsOnGround ) return;

        controller.Punch( -controller.AppliedGravity.Normal * jumpStrength );
        IsGrounded = false;
        CurrentState = MovementState.Airborne;

        if ( renderer.IsValid() )
            renderer.Set( "b_jump", true );
    }

    private MovementState BuildState()
    {
        if ( !controller.IsOnGround )
            return controller.Velocity.z < -10f ? MovementState.Falling : MovementState.Airborne;

        if ( IsCrouching )
            return MovementState.Crouching;

        if ( IsSprinting )
            return MovementState.Sprinting;

        if ( IsMoving )
            return MovementState.Walking;

        return MovementState.Idle;
    }

    private void UpdateBodyRotation()
    {
        if ( !rotateBodyToCameraYaw ) return;

        var targetRotation = BodyRotation;
        WorldRotation = bodyTurnSmoothing <= 0f
            ? targetRotation
            : Rotation.Slerp( WorldRotation, targetRotation, Time.Delta * bodyTurnSmoothing );
    }

    private void UpdateRendererParameters()
    {
        if ( !renderer.IsValid() ) return;

        var runScale = 1f / renderer.Transform.World.UniformScale;
        var moveX = Vector3.Dot( controller.Velocity, renderer.WorldRotation.Forward ) * runScale;
        var moveY = Vector3.Dot( controller.Velocity, renderer.WorldRotation.Right ) * runScale;

        renderer.Set( "move_x", MathX.Lerp( renderer.GetFloat( "move_x" ), moveX, Time.Delta * 10f ) );
        renderer.Set( "move_y", MathX.Lerp( renderer.GetFloat( "move_y" ), moveY, Time.Delta * 10f ) );
        renderer.Set( "b_grounded", controller.IsOnGround );
        renderer.Set( "duck", IsCrouching ? 1f : 0f );
    }
}
