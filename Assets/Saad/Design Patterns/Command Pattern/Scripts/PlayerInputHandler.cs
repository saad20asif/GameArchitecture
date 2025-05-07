using UnityEngine;

namespace ProjectCore.DesignPatterns.CommandPattern
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public System.Action<Vector2> OnMovementInput;
    
        private PlayerInput _inputActions;

        private void Awake()
        {
            _inputActions = new PlayerInput();
            _inputActions.Player.Enable();
        }

        private void OnEnable() => BindInputActions();
        private void OnDisable() => UnbindInputActions();

        private void BindInputActions()
        {
            _inputActions.Player.MoveUp.performed += _ => OnMovementInput?.Invoke(Vector2.up);
            _inputActions.Player.MoveDown.performed += _ => OnMovementInput?.Invoke(Vector2.down);
            _inputActions.Player.MoveLeft.performed += _ => OnMovementInput?.Invoke(Vector2.left);
            _inputActions.Player.MoveRight.performed += _ => OnMovementInput?.Invoke(Vector2.right);
        }

        private void UnbindInputActions()
        {
            _inputActions.Player.MoveUp.performed -= _ => OnMovementInput?.Invoke(Vector2.up);
            _inputActions.Player.MoveDown.performed -= _ => OnMovementInput?.Invoke(Vector2.down);
            _inputActions.Player.MoveLeft.performed -= _ => OnMovementInput?.Invoke(Vector2.left);
            _inputActions.Player.MoveRight.performed -= _ => OnMovementInput?.Invoke(Vector2.right);
        }
    }
}
