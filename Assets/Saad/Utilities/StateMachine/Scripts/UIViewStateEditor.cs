#if UNITY_EDITOR
using UnityEditor;
using ProjectCore.StateMachine;

[CustomEditor(typeof(UIViewState), true)]
public class UIViewStateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var usePoolingProp = serializedObject.FindProperty("usePooling");
        var stateIdProp = serializedObject.FindProperty("stateId");
        var poolManagerProp = serializedObject.FindProperty("poolManagerSO");

        EditorGUILayout.PropertyField(stateIdProp);
        EditorGUILayout.PropertyField(usePoolingProp);

        if (usePoolingProp.boolValue)
        {
            EditorGUILayout.PropertyField(poolManagerProp);
            EditorGUILayout.HelpBox("Ensure prefab is registered in PoolManagerSO.", MessageType.None);
        }
        else
        {
            EditorGUILayout.HelpBox("Prefab must be in Resources folder, named as stateId.", MessageType.None);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif