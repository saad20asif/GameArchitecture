using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "UiConfig", menuName = "ProjectCore/UiConfig")]
public class UiConfig : ScriptableObject
{
    public float easeInDuration = 0.5f;
    public float easeOutDuration = 0.5f;
    public Ease easeIn = Ease.InOutSine;
    public Ease easeOut = Ease.InOutSine;
}
