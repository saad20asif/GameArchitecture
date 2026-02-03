using System.Collections;

namespace Blues.Core.UI
{
    public interface IShowable
    {
        void Show();
        IEnumerator Hide();
        void Pause();
        void Resume();
    }
}
