using UnityEngine;
using UnityEngine.UI;

namespace Blues.Core.DesignPatterns.CommandPattern
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private Button UndoBtn;
        [SerializeField] private Button RedoBtn;
    
        [SerializeField] private GameManager _gameManager;

        private void OnEnable()
        {
            UndoBtn.onClick.AddListener(OnUndoButtonClick);
            RedoBtn.onClick.AddListener(OnRedoButtonClick);
        }

        private void OnDisable()
        {
            UndoBtn.onClick.RemoveListener(OnUndoButtonClick);
            RedoBtn.onClick.RemoveListener(OnRedoButtonClick);
        }


        // Button click handlers
        public void OnUndoButtonClick() => _gameManager.Undo();
        public void OnRedoButtonClick() => _gameManager.Redo();
    }
}

