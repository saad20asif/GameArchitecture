using UnityEngine;

namespace Blues.Core.DesignPatterns.CommandPattern
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _moveDistance = 1f;
        private CommandManager _commandManager;
        private PlayerInputHandler _inputHandler;

        public void Initialize(CommandManager commandManager, PlayerInputHandler inputHandler)
        {
            _commandManager = commandManager;
            _inputHandler = inputHandler;
            _inputHandler.OnMovementInput += HandleMovement;
        }

        private void OnDestroy()
        {
            if (_inputHandler != null)
                _inputHandler.OnMovementInput -= HandleMovement;
        }

        private void HandleMovement(Vector2 direction)
        {
            var moveCommand = new MoveCommand(transform, direction, _moveDistance);
            _commandManager.ExecuteCommand(moveCommand);
        }
    }
}
