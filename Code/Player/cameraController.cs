// Copyright Studio Prism. Licensed under PolyForm Shield 1.0.0.
// https://polyformproject.org/licenses/shield/1.0.0
// Required Notice: Copyright Studio Prism (https://github.com/studioprism)
//
// cameraController.cs 
//
// 
using Sandbox;

public sealed class cameraController : Component
{
	[Property] public GameObject cameraObject { get; set; }
	[Property, Range( 40f, 80f )] public float eyeHeight { get; set; }
	[Property, Range( 60f, 89f )] public float pitchClamp { get; set; }
	[Property, Range( 60f, 120f )] public float baseFov { get; set; }
	[Sync] public Angles eyeAngles { get; set; }
	public Vector3 aimDirection => eyeAngles.ToRotation().Forward;
	public Rotation bodyRotation => Rotation.FromYaw( eyeAngles.yaw );

	// Cache for Camera
	private CameraComponent _cam;

	protected override void OnStart()
	{
		if ( cameraObject.IsValid() )
			_cam = cameraObject.GetComponent<CameraComponent>();
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;
		// Track/Accumulate Look input
		var look = eyeAngles;
		look += Input.AnalogLook;
		// Clamp Pitch
		look.pitch = look.pitch.Clamp( -pitchClamp, pitchClamp );
		look.roll = 0f; // Kill any Roll that could accumulate.
		eyeAngles = look;
		// Apply to Camera, No Smoothing or Lerp
		if ( cameraObject.IsValid() )
		{
			// World Rotation & Position, recomputed each frame, due to player Controller moving Independently.
			cameraObject.WorldRotation = eyeAngles.ToRotation();
			cameraObject.WorldPosition = WorldPosition + Vector3.Up * eyeHeight;
		}

		if ( _cam.IsValid() )
			_cam.FieldOfView = baseFov;
	}
}

