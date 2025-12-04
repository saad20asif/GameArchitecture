namespace Blues.Core.DesignPatterns.CommandPattern
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}
