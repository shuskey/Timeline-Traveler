using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	// NOTE: (SDH) I added the menu and start inputs to the StarterAssetsInputs class
	// because the ThirdPersonController uses the StarterAssetsInputs class and I need to be able to send messages to the Tribe GameObject
	// from the ThirdPersonController.
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool menu;
		public bool start;
		public bool debugNextPersonOfInterest;
		public bool debugPreviousPersonOfInterest;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnMenu(InputValue value)
		{
			MenuInput(value.isPressed);
		}

		public void OnStart(InputValue value)
		{
			StartInput(value.isPressed);
		}

		public void OnDebugNextPersonOfInterest(InputValue value)
		{
			DebugNextPersonOfInterestInput(value.isPressed);
		}

		public void OnDebugPreviousPersonOfInterest(InputValue value)
		{
			DebugPreviousPersonOfInterestInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
		
		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

		private void MenuInput(bool newMenuState)
		{
			menu = newMenuState;
		}

		private void StartInput(bool newStartState)
		{
			start = newStartState;
		}

		private void DebugNextPersonOfInterestInput(bool newDebugNextPersonOfInterestState)
		{
			debugNextPersonOfInterest = newDebugNextPersonOfInterestState;
		}

		private void DebugPreviousPersonOfInterestInput(bool newDebugPreviousPersonOfInterestState)
		{
			debugPreviousPersonOfInterest = newDebugPreviousPersonOfInterestState;
		}
		
	}
	
}