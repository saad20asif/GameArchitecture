using UnityEngine;

namespace Featrues.SpinWheel
{
    public abstract class BaseView<T> : MonoBehaviour where T : BaseRefs
    {
        protected T ViewRefs;

        protected virtual void Awake()
        {
            if(ViewRefs == null)
            {
                if(TryGetComponent(out T baseRefs))
                {
                    ViewRefs = baseRefs;
                }
            }
        }
        public abstract void Show();
        public abstract void Hide();
    }
}
