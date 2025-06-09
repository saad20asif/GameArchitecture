using System;

namespace ProjectCore.UI
{
    public interface IShowable
    {
        void Show();
        void Hide(Action  callback);
        void Pause();
        void Resume();
    }
}
