namespace ProjectCore.DesignPatterns.CommandPattern
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}
