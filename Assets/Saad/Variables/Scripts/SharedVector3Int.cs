using UnityEngine;

[CreateAssetMenu(fileName = "vSharedVector3Int_", menuName = "ProjectCore/Variables/Simple/SharedVector3Int")]
public class SharedVector3Int : ScriptableObject
{
    [SerializeField] protected Vector3Int Value;
    [SerializeField] protected Vector3Int DefaultValue;
    [SerializeField] protected bool ResetToDefaultOnPlay;

    private void OnEnable()
    {
        if (ResetToDefaultOnPlay)
            Value = DefaultValue;

    }
    public virtual void SetValue(Vector3Int value)
    { Value = value; }
    public virtual Vector3Int GetValue() { return Value; }
}
