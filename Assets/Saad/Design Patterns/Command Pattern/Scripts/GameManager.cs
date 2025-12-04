using UnityEngine;

namespace Blues.Core.DesignPatterns.CommandPattern
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private PlayerInputHandler _playerInputHandler;
        private CommandManager _commandManager;

        private void Awake()
        {
            _commandManager = new CommandManager();
            _playerController.Initialize(_commandManager,_playerInputHandler);
        }

        // Called by UI buttons
        public void Undo() => _commandManager.Undo();
        public void Redo() => _commandManager.Redo();
    }
}
