using UnityEngine;

namespace ProjectCore.DesignPatterns.CommandPattern
{
    public class MoveCommand : ICommand
    {
        private readonly Transform _transform;
        private readonly Vector3 _movement;

        public MoveCommand(Transform transform, Vector2 direction, float distance)
        {
            _transform = transform;
            _movement = new Vector3(direction.x, direction.y, 0) * distance;
        }

        public void Execute() => _transform.position += _movement;
        public void Undo() => _transform.position -= _movement;
    }
}
