using System;

namespace Blues.Core.UI
{
    public interface IShowable
    {
        void Show();
        void Hide(Action  callback);
        void Pause();
        void Resume();
    }
}
