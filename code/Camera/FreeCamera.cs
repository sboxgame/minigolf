using System;
using Sandbox;

namespace Facepunch.Minigolf;

public class FreeCamera : CameraMode
{
	Angles LookAngles;
	Vector3 MoveInput;

	Vector3 TargetPos;
	Rotation TargetRot;

	float MoveSpeed;
	float LerpMode = 0;

	public FreeCamera()
	{
		TargetPos = CurrentView.Position;
		TargetRot = CurrentView.Rotation;

		Position = TargetPos;
		Rotation = TargetRot;
		LookAngles = Rotation.Angles();
	}

	public override void Update()
	{
		var player = Local.Client;
		if ( player == null ) return;
		
		Viewer = null;

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
		var mv = MoveInput.Normal * 300 * RealTime.Delta * Rotation * MoveSpeed;

		TargetRot = Rotation.From( LookAngles );
		TargetPos += mv;

		Position = Vector3.Lerp( Position, TargetPos, 10 * RealTime.Delta * (1 - LerpMode) );
		Rotation = Rotation.Slerp( Rotation, TargetRot, 10 * RealTime.Delta * (1 - LerpMode) );
	}
}
