// Copyright Studio Prism. Licensed under PolyForm Shield 1.0.0.
// https://polyformproject.org/licenses/shield/1.0.0
// Required Notice: Copyright Studio Prism (https://github.com/studioprism)

using System;
using ShrimpleController = ShrimpleCharacterController.ShrimpleCharacterController;

public sealed class camController : Component
{
    [RequireComponent] public ShrimpleController controller { get; set; }
    [Property] public playerController player { get; set; }
    [Property] public SkinnedModelRenderer bodyRenderer { get; set; }
    [Property] public GameObject viewModelRoot { get; set; }

    [Property, Group( "Camera" ), Range( 40f, 80f )] public float standingEyeHeight { get; set; } = 64f;
    [Property, Group( "Camera" ), Range( 24f, 64f )] public float crouchingEyeHeight { get; set; } = 46f;
    [Property, Group( "Camera" ), Range( 1f, 30f )] public float eyeHeightSmoothing { get; set; } = 16f;
    [Property, Group( "Camera" ), Range( 60f, 89f )] public float pitchClamp { get; set; } = 85f;
    [Property, Group( "Camera" ), Range( 60f, 120f )] public float baseFov { get; set; } = 100f;
    [Property, Group( "Camera" ), Range( 0f, 16f )] public float sprintFovKick { get; set; } = 6f;
    [Property, Group( "Camera" ), Range( 1f, 30f )] public float fovSmoothing { get; set; } = 12f;

    [Property, Group( "First Person Body" )] public bool bodyCutoutEnabled { get; set; } = true;
    [Property, Group( "First Person Body" )] public string headAttachmentName { get; set; } = "head";
    [Property, Group( "First Person Body" )] public string headBoneName { get; set; } = "Head";
    [Property, Group( "First Person Body" )] public string headBodyGroupName { get; set; } = "Head";
    [Property, Group( "First Person Body" )] public bool hideHeadBodyGroup { get; set; } = true;
    [Property, Group( "First Person Body" ), Range( 0, 8 )] public int visibleHeadBodyGroup { get; set; } = 0;
    [Property, Group( "First Person Body" ), Range( 0, 8 )] public int hiddenHeadBodyGroup { get; set; } = 1;
    [Property, Group( "First Person Body" ), Range( 1f, 24f )] public float bodyCutoutNearClip { get; set; } = 8f;
    [Property, Group( "First Person Body" ), Range( -8f, 24f )] public float bodyCutoutForwardOffset { get; set; } = 13f;

    [Property, Group( "View Feel" ), Range( 0f, 8f )] public float bobAmplitude { get; set; } = 1.5f;
    [Property, Group( "View Feel" ), Range( 0f, 20f )] public float bobFrequency { get; set; } = 9f;
    [Property, Group( "View Feel" ), Range( 0f, 10f )] public float maxViewRoll { get; set; } = 3f;
    [Property, Group( "View Feel" ), Range( 0f, 0.1f )] public float strafeRollScale { get; set; } = 0.025f;
    [Property, Group( "View Feel" ), Range( 0f, 0.5f )] public float turnRollScale { get; set; } = 0.08f;
    [Property, Group( "View Feel" ), Range( 1f, 30f )] public float rollSmoothing { get; set; } = 10f;
    [Property, Group( "View Feel" ), Range( 0f, 8f )] public float landingDip { get; set; } = 2.5f;
    [Property, Group( "View Feel" ), Range( 1f, 30f )] public float landingReturnSpeed { get; set; } = 12f;

    [Property, Group( "Weapon Sway" ), Range( 0f, 1f )] public float lookSwayPosition { get; set; } = 0.06f;
    [Property, Group( "Weapon Sway" ), Range( 0f, 1f )] public float movementSwayPosition { get; set; } = 0.018f;
    [Property, Group( "Weapon Sway" ), Range( 0f, 2f )] public float lookSwayRotation { get; set; } = 0.45f;
    [Property, Group( "Weapon Sway" ), Range( 0f, 24f )] public float swaySmoothing { get; set; } = 14f;
    [Property, Group( "Weapon Sway" )] public Vector3 sprintViewModelOffset { get; set; } = new Vector3( 4f, -3f, -4f );

    [Sync] public Angles eyeAngles { get; private set; }
    public Vector3 aimDirection => eyeAngles.ToRotation().Forward;
    public Rotation bodyRotation => Rotation.FromYaw( eyeAngles.yaw );
    public GameObject cameraObject => _cameraObject;
    public CameraComponent cameraComponent => _cam;

    private GameObject _cameraObject;
    private CameraComponent _cam;
    private Vector3 _viewModelStartPosition;
    private Rotation _viewModelStartRotation;
    private float _currentEyeHeight;
    private float _currentFov;
    private float _currentRoll;
    private float _bobTime;
    private float _landingOffset;
    private bool _wasGrounded;
    private bool _hasViewModelStartTransform;
    private Angles _lastLookDelta;

    protected override void OnStart()
    {
        player ??= Components.Get<playerController>();
        bodyRenderer ??= player?.renderer;
        bodyRenderer ??= Components.Get<SkinnedModelRenderer>( FindMode.EverythingInSelfAndDescendants );

        _cameraObject = new GameObject( true, "Camera" );
        _cameraObject.SetParent( GameObject );

        _cam = _cameraObject.Components.Create<CameraComponent>();
        _cam.ZFar = 32768f;
        _cam.ZNear = bodyCutoutEnabled ? bodyCutoutNearClip : 2f;
        _cam.FieldOfView = baseFov;

        eyeAngles = new Angles( 0f, WorldRotation.Yaw(), 0f );
        _currentEyeHeight = standingEyeHeight;
        _currentFov = baseFov;
        _wasGrounded = controller.IsOnGround;

        UpdateLocalHeadBodyGroup();
        CacheViewModelTransform();
    }

    protected override void OnUpdate()
    {
        if ( IsProxy ) return;
        if ( !_cameraObject.IsValid() ) return;

        CacheViewModelTransform();
        UpdateLook();
        UpdateCameraFeel();
        UpdateViewModelSway();
        UpdateLocalHeadBodyGroup();
    }

    private void UpdateLook()
    {
        _lastLookDelta = Input.AnalogLook;

        var look = eyeAngles + Input.AnalogLook;
        look.pitch = look.pitch.Clamp( -pitchClamp, pitchClamp );
        look.roll = 0f;

        eyeAngles = look;
    }

    private void UpdateCameraFeel()
    {
        var grounded = controller.IsOnGround;
        var horizontalSpeed = controller.Velocity.WithZ( 0f ).Length;
        var moveAmount = MathX.Clamp( horizontalSpeed / 300f, 0f, 1f );

        if ( grounded && !_wasGrounded )
            _landingOffset = -landingDip * MathX.Clamp( MathF.Abs( controller.Velocity.z ) / 350f, 0.35f, 1f );

        _wasGrounded = grounded;

        if ( grounded && horizontalSpeed > 10f )
            _bobTime += Time.Delta * bobFrequency * (player?.IsSprinting == true ? 1.25f : 1f);
        else
            _bobTime = MathX.Lerp( _bobTime, 0f, Time.Delta * 5f );

        var targetEyeHeight = player?.IsCrouching == true ? crouchingEyeHeight : standingEyeHeight;
        _currentEyeHeight = MathX.Lerp( _currentEyeHeight, targetEyeHeight, Time.Delta * eyeHeightSmoothing );

        var bob = grounded
            ? MathF.Sin( _bobTime ) * bobAmplitude * moveAmount
            : 0f;

        _landingOffset = MathX.Lerp( _landingOffset, 0f, Time.Delta * landingReturnSpeed );

        var localVelocity = controller.Velocity * bodyRotation.Inverse;
        var strafeRoll = -localVelocity.y * strafeRollScale;
        var turnRoll = -_lastLookDelta.yaw * turnRollScale;
        var targetRoll = (strafeRoll + turnRoll).Clamp( -maxViewRoll, maxViewRoll );

        _currentRoll = MathX.Lerp( _currentRoll, targetRoll, Time.Delta * rollSmoothing );

        var viewAngles = eyeAngles;
        viewAngles.roll = _currentRoll;
        var baseEyePosition = TryGetHeadTransform( out var headTransform )
            ? headTransform.Position
            : WorldPosition + Vector3.Up * _currentEyeHeight;

        var eyePosition = baseEyePosition
            + Vector3.Up * (bob + _landingOffset)
            + bodyRotation.Forward * (bodyCutoutEnabled ? bodyCutoutForwardOffset : 0f);

        _cameraObject.WorldPosition = eyePosition;
        _cameraObject.WorldRotation = viewAngles.ToRotation();

        var targetFov = baseFov + (player?.IsSprinting == true ? sprintFovKick : 0f);
        _currentFov = MathX.Lerp( _currentFov, targetFov, Time.Delta * fovSmoothing );

        if ( _cam.IsValid() )
        {
            _cam.ZNear = bodyCutoutEnabled ? bodyCutoutNearClip : 2f;
            _cam.FieldOfView = _currentFov;
        }
    }

    private void UpdateViewModelSway()
    {
        if ( !viewModelRoot.IsValid() ) return;

        var localVelocity = controller.Velocity * bodyRotation.Inverse;
        var sprintOffset = player?.IsSprinting == true ? sprintViewModelOffset : Vector3.Zero;

        var targetPosition = _viewModelStartPosition
            + new Vector3(
                -_lastLookDelta.yaw * lookSwayPosition,
                _lastLookDelta.pitch * lookSwayPosition,
                0f )
            + new Vector3(
                -localVelocity.y * movementSwayPosition,
                0f,
                -MathF.Abs( localVelocity.x ) * movementSwayPosition * 0.25f )
            + sprintOffset;

        var targetRotation = _viewModelStartRotation
            * Rotation.FromPitch( _lastLookDelta.pitch * lookSwayRotation )
            * Rotation.FromYaw( -_lastLookDelta.yaw * lookSwayRotation )
            * Rotation.FromRoll( -localVelocity.y * movementSwayPosition * 2f );

        viewModelRoot.LocalPosition = Vector3.Lerp( viewModelRoot.LocalPosition, targetPosition, Time.Delta * swaySmoothing );
        viewModelRoot.LocalRotation = Rotation.Slerp( viewModelRoot.LocalRotation, targetRotation, Time.Delta * swaySmoothing );
    }

    private void CacheViewModelTransform()
    {
        if ( !viewModelRoot.IsValid() ) return;
        if ( !_hasViewModelStartTransform )
        {
            _viewModelStartPosition = viewModelRoot.LocalPosition;
            _viewModelStartRotation = viewModelRoot.LocalRotation;
            _hasViewModelStartTransform = true;
        }
    }

    private bool TryGetHeadTransform( out Transform transform )
    {
        if ( bodyRenderer.IsValid() )
        {
            var attachment = bodyRenderer.GetAttachment( headAttachmentName, true );
            if ( attachment.HasValue )
            {
                transform = attachment.Value;
                return true;
            }

            if ( bodyRenderer.TryGetBoneTransform( headBoneName, out transform ) )
                return true;

            if ( bodyRenderer.TryGetBoneTransform( headBoneName.ToLower(), out transform ) )
                return true;
        }

        transform = default;
        return false;
    }

    private void UpdateLocalHeadBodyGroup()
    {
        if ( !bodyRenderer.IsValid() ) return;
        if ( string.IsNullOrWhiteSpace( headBodyGroupName ) ) return;

        bodyRenderer.SetBodyGroup( headBodyGroupName, bodyCutoutEnabled && hideHeadBodyGroup ? hiddenHeadBodyGroup : visibleHeadBodyGroup );
    }
}
