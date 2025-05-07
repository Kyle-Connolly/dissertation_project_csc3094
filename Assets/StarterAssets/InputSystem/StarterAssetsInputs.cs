using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
        public bool vectorDrive; //NEW ADDITION
        public bool vectorLaunch; //NEW ADDITION

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
        //NEW ADDITION
        public void OnVectorDrive(InputValue value) 
        {
            VectorDriveInput(value.isPressed);
        }
        //NEW ADDITION
        public void OnVectorLaunch(InputValue value)
        {
            VectorLaunchInput(value.isPressed);
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
        //NEW ADDITION
        public void VectorDriveInput(bool newVectorDriveState)
        {
            vectorDrive = newVectorDriveState;
        }
        //NEW ADDITION
        public void ResetVectorDriveInput()
        {
			vectorDrive = false;
        }
        //NEW ADDITION
        public void VectorLaunchInput(bool newVectorLaunchState)
        {
            vectorLaunch = newVectorLaunchState;
        }
        //NEW ADDITION
        public void ResetVectorLaunchInput()
        {
            vectorLaunch = false;
        }
       

        private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}