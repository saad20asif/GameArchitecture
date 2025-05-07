using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.DesignPatterns.CommandPattern
{
    public class CommandManager
    {
        private readonly Stack<ICommand> _commandHistory = new();
        private readonly Stack<ICommand> _redoStack = new();

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            _commandHistory.Push(command);
            _redoStack.Clear();
            
            Debug.Log($"Command executed : {_commandHistory.Count}  {_redoStack.Count}");
        }

        public void Undo()
        {
            if (_commandHistory.Count <= 0) return;
        
            var command = _commandHistory.Pop();
            command.Undo();
            _redoStack.Push(command);
            
            Debug.Log($"Command executed : {_commandHistory.Count}  {_redoStack.Count}");
        }

        public void Redo()
        {
            if (_redoStack.Count <= 0) return;
        
            var command = _redoStack.Pop();
            command.Execute();
            _commandHistory.Push(command);
            
            Debug.Log($"Command executed : {_commandHistory.Count}  {_redoStack.Count}");
        }
    }
}
