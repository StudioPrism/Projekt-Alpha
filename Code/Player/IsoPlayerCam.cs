// Copyright Studio Prism. Licensed under PolyForm Shield 1.0.0.
// https://polyformproject.org/licenses/shield/1.0.0
// Required Notice: Copyright Studio Prism (https://github.com/studioprism)
//
// IsoPlayerCam.cs 
//
// Isometric Camera Component, Controls Camera Position and Zoom; Locking Rotation to have true isometric view.
using Sandbox;

public sealed class IsoPlayerCam : Component
{
	[Property] public GameObject Target { get; set; } // Player Entity And, or Camera Component
	[Property] public Component Player { get; set;}

	private static readonly Vector3 RigOffset = new Vector3( -600f, -600f, 500f );

	private float _currentCamZ;
	Vector3 _currentLookAheadOffset = Vector3.Zero;
	private Vector3 _currentPos;
	private float _zoomDistance = 1.0f;
	float _targetZoomDistance = 1.0f;
	private float _trauma = 0f;

	protected override void OnStart()
	{

	}

	protected override void OnFixedUpdate()
	{
		if(!Target.IsValid()) return;

		Vector3 playerPos = Target.WorldPosition;
		Vector3 aimPos = GetAimWorldPosition(); // Build Aim Position in IsoPlayerController.cs
		
		// Zoom Input
		HandleZoomInput();
		UpdateZoom(Time.Delta);
		
		// Soft Z Follow(Smooth like Butter)
		float camZ = UpdateSoftVeticalFollow(playerPos.z, Time.Delta);
	}
}
