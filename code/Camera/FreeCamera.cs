using System;
using Sandbox;

namespace Facepunch.Minigolf;

public class FreeCamera : BaseCamera
{
	Angles LookAngles;
	Vector3 MoveInput;

	Vector3 TargetPos;
	Rotation TargetRot;

	float MoveSpeed;
	float LerpMode = 0;

	public FreeCamera()
	{
		TargetPos = Camera.Position;
		TargetRot = Camera.Rotation;

		Camera.Position = TargetPos;
		Camera.Rotation = TargetRot;
		LookAngles = Camera.Rotation.Angles();
	}

	public override void Update()
	{
		var player = Sandbox.Game.LocalClient;
		if ( player == null ) return;
		
		Camera.FirstPersonViewer = null;

		FreeMove();
	}

	public override void BuildInput()
	{
		MoveInput = Input.AnalogMove;

		MoveSpeed = 1;
		if ( Input.Down( InputButton.Run ) ) MoveSpeed = 5;
		if ( Input.Down( InputButton.Duck ) ) MoveSpeed = 0.2f;

		if ( Input.Down( InputButton.Slot1 ) ) LerpMode = 0.0f;
		if ( Input.Down( InputButton.Slot2 ) ) LerpMode = 0.5f;
		if ( Input.Down( InputButton.Slot3 ) ) LerpMode = 0.9f;

		LookAngles += Input.AnalogLook;
		LookAngles.roll = 0;

		Input.ClearButton( InputButton.PrimaryAttack );
		Input.StopProcessing = true;
	}

	void FreeMove()
	{
		var mv = MoveInput.Normal * 300 * RealTime.Delta * Camera.Rotation * MoveSpeed;

		TargetRot = Rotation.From( LookAngles );
		TargetPos += mv;

		Camera.Position = Vector3.Lerp( Camera.Position, TargetPos, 10 * RealTime.Delta * (1 - LerpMode) );
		Camera.Rotation = Rotation.Slerp( Camera.Rotation, TargetRot, 10 * RealTime.Delta * (1 - LerpMode) );
	}
}
