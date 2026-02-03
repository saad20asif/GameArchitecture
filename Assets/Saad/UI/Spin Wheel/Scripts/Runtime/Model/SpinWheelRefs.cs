using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Featrues.SpinWheel
{
    public class SpinWheelRefs : BaseRefs
    {
        public Image background;
        public Image foreground;
        public Image center;
        public Image frame;
        public RectTransform sliceParent;
        public List<Slice> slices;
    }
}