using UnityEngine;
using UnityEngine.InputSystem;
using FutharksAdventure.Core;


namespace FutharksAdventure.Game
{
    public class InputController : FutharksInputMap.IGameplayActions
    {

        private readonly FutharksInputMap _inputActions;
        private readonly TurnManager _turnManager;


        //--- ctor -------------------
        public InputController(FutharksInputMap inputActions, TurnManager turnManager)
        {
            _inputActions = inputActions;
            _turnManager = turnManager;
        }

        //--- LifeCycle -----
        public void Enable()
        {
            _inputActions.Gameplay.AddCallbacks(this);
            _inputActions.Gameplay.Enable();
        }

        public void Disable()
        {
            _inputActions.Gameplay.Disable();
            _inputActions.Gameplay.RemoveCallbacks(this);
        }

        //--- CallBacks ---------------
        public void OnMove(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            Vector2 moveDir = context.ReadValue<Vector2>();

            Direction? direction = (moveDir.x, moveDir.y) switch
            {
                (0f, 1f) => Direction.North,
                (1f, 0f) => Direction.East,
                (0f, -1f) => Direction.South,
                (-1f, 0f) => Direction.West,
                _ => null
            };

            if (direction == null)
                return;
            _turnManager.SubmitMove(direction.Value); 
        }

        public void OnRangedAttack(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            _turnManager.SubmitThrow();
        }
    }
}
