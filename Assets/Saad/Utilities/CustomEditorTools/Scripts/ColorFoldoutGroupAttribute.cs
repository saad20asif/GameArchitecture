using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

namespace CustomEditorScripts
{
    public class ColorFoldoutGroupAttribute : FoldoutGroupAttribute
    {
        public float R, G, B, A;

        public ColorFoldoutGroupAttribute(string path) : base(path)
        {
        
        }

        public ColorFoldoutGroupAttribute(string path, float r,float g,float b, float a = 1f) : base(path)
        {
            R = r; G=g; B=b; A = a; 
        }
        protected override void CombineValuesWith(PropertyGroupAttribute other)
        {
            var otherAttr = other as ColorFoldoutGroupAttribute;
            R = Mathf.Max(otherAttr.R, R);
            G = Mathf.Max(otherAttr.G, G);
            B = Mathf.Max(otherAttr.B, B);
            A = Mathf.Max(otherAttr.A, A);
        }
    }
    public class ColorFoldoutGroupAttributeDrawer : OdinGroupDrawer<ColorFoldoutGroupAttribute>
    {
        private LocalPersistentContext<bool> isExpanded;
        protected override void Initialize()
        {
            this.isExpanded = this.GetPersistentValue<bool>("ColorFoldoutGroupAttributeDrawer.isExpanded",
                GeneralDrawerConfig.Instance.ExpandFoldoutByDefault);
        }
        protected override void DrawPropertyLayout(GUIContent label)
        {
            GUIHelper.PushColor(new Color(Attribute.R, Attribute.G, Attribute.B, Attribute.A));
            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();
            GUIHelper.PopColor();

            this.isExpanded.Value = SirenixEditorGUI.Foldout(this.isExpanded.Value, label);
            SirenixEditorGUI.EndBoxHeader();

            if(SirenixEditorGUI.BeginFadeGroup(this, this.isExpanded.Value))
            {
                for (int i = 0; i<Property.Children.Count; i++)
                {
                    Property.Children[i].Draw();
                }
            }
            SirenixEditorGUI.EndFadeGroup();
            SirenixEditorGUI.EndBox();
        }
    }
}
