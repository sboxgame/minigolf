using System;
using Sandbox;

namespace Minigolf
{
	public class FreeCamera : Camera
	{
		Angles LookAngles;
		Vector3 MoveInput;

		Vector3 TargetPos;
		Rotation TargetRot;

		float MoveSpeed;
		float LerpMode = 0;

		/// <summary>
		/// On the camera becoming activated, snap to the current view position
		/// </summary>
		public override void Activated()
		{
			base.Activated();

			TargetPos = CurrentView.Position;
			TargetRot = CurrentView.Rotation;

			Pos = TargetPos;
			Rot = TargetRot;
			LookAngles = Rot.Angles();
		}

		public override void Deactivated()
		{
			base.Deactivated();
		}

		public override void Update()
		{
			var player = Local.Client;
			if ( player == null ) return;

			var tr = Trace.Ray( Pos, Pos + Rot.Forward * 4096 ).UseHitboxes().Run();

			// DebugOverlay.Box( tr.EndPos, Vector3.One * -1, Vector3.One, Color.Red );

			FieldOfView = 80;

			Viewer = null;
			{
				var lerpTarget = tr.EndPos.Distance( Pos );

				DoFPoint = lerpTarget;// DoFPoint.LerpTo( lerpTarget, Time.Delta * 10 );
			}

			FreeMove();
		}

		public override void BuildInput( InputBuilder input )
		{
			MoveInput = input.AnalogMove;

			MoveSpeed = 1;
			if ( input.Down( InputButton.Run ) ) MoveSpeed = 5;
			if ( input.Down( InputButton.Duck ) ) MoveSpeed = 0.2f;

			if ( input.Down( InputButton.Slot1 ) ) LerpMode = 0.0f;
			if ( input.Down( InputButton.Slot2 ) ) LerpMode = 0.5f;
			if ( input.Down( InputButton.Slot3 ) ) LerpMode = 0.9f;

			if ( input.Down( InputButton.Use ) )
				DoFBlurSize = Math.Clamp( DoFBlurSize + (Time.Delta * 3.0f), 0.0f, 100.0f );

			if ( input.Down( InputButton.Menu ) )
				DoFBlurSize = Math.Clamp( DoFBlurSize - (Time.Delta * 3.0f), 0.0f, 100.0f );

			LookAngles += input.AnalogLook;
			LookAngles.roll = 0;

			input.ClearButton( InputButton.Attack1 );

			input.StopProcessing = true;
		}

		void FreeMove()
		{
			var mv = MoveInput.Normal * 300 * RealTime.Delta * Rot * MoveSpeed;

			TargetRot = Rotation.From( LookAngles );
			TargetPos += mv;

			Pos = Vector3.Lerp( Pos, TargetPos, 10 * RealTime.Delta * (1 - LerpMode) );
			Rot = Rotation.Slerp( Rot, TargetRot, 10 * RealTime.Delta * (1 - LerpMode) );
		}
	}
}
