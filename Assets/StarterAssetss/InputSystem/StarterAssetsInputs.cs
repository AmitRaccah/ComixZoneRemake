using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
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
        public bool punch;
        public bool heavyPunch;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        public bool crouch;
        public bool lookUp;
        public bool pickUp;

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
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

        public void OnPunch(InputValue v)
        {
            if (v.isPressed)
                InputBuffer.Instance.Add(InputType.Punch);
        }
        public void OnHeavyPunch(InputValue v)
        {
            if (v.isPressed)
                InputBuffer.Instance.Add(InputType.HeavyPunch);
        }

        public void OnPickUp(InputValue value)
        {
            bool pressed = value.isPressed;
            pickUp = pressed;

            if (pressed)
            {
                Debug.Log("[INPUT] PickUp pressed");   
                CoreBus.Publish(new PlayerPickUpEvent());
            }
        }


#if ENABLE_INPUT_SYSTEM
#endif

        public void OnCrouch(InputValue value)
        {
            bool pressed = value.Get<float>() > 0.5f;

            if (pressed && !crouch)
                CoreBus.Publish(new PlayerCrouchEvent());
            else if (!pressed && crouch)
                CoreBus.Publish(new PlayerUncrouchEvent());

            crouch = pressed;
        }

        public void OnLookUp(InputValue value)
        {
            bool pressed = value.Get<float>() > 0.5f;

            if (pressed && !lookUp)
                CoreBus.Publish(new PlayerLookUpEvent());
            else if (!pressed && lookUp)
                CoreBus.Publish(new PlayerUnLookUpEvent());

            lookUp = pressed;
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
    }

}