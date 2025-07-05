using ProjectCore.Events;
using ProjectCore.Variables;
using UnityEditor;
using UnityEngine;

public static class ScriptableObjectCreator
{
    // Main menu path prefix
    private const string MenuRoot = "Architecture/";
    
    // Persistent Variables
    [MenuItem(MenuRoot + "Persistent/DBInt %#i", false, 10)]
    private static void CreateDBInt() => CreateAsset<DBInt>("v_");
    
    [MenuItem(MenuRoot + "Persistent/DBBool %#b", false, 11)]
    private static void CreateDBBool() => CreateAsset<DBBool>("v_");
    
    [MenuItem(MenuRoot + "Persistent/DBFloat %#f", false, 12)]
    private static void CreateDBFloat() => CreateAsset<DBFloat>("v_");
    
    [MenuItem(MenuRoot + "Persistent/DBString %#s", false, 13)]
    private static void CreateDBString() => CreateAsset<DBString>("v_");
    
    [MenuItem(MenuRoot + "Persistent/DBIntWithEvent %#&i", false, 14)]
    private static void CreateDBIntWithEvent() => CreateAsset<DBIntWithEvent>("v_");
    
    [MenuItem(MenuRoot + "Persistent/DBBoolWithEvent %#&b", false, 15)]
    private static void CreateDBBoolWithEvent() => CreateAsset<DBBoolWithEvent>("v_");
    
    [MenuItem(MenuRoot + "Persistent/DBFloatWithEvent %#&f", false, 16)]
    private static void CreateDBFloatWithEvent() => CreateAsset<DBFloatWithEvent>("v_");
    
    [MenuItem(MenuRoot + "Persistent/DBStringWithEvent %#&s", false, 17)]
    private static void CreateDBStringWithEvent() => CreateAsset<DBStringWithEvent>("v_");

    // Primitive Variables
    [MenuItem(MenuRoot + "Primitives/Int %&i", false, 20)]
    private static void CreateInt() => CreateAsset<Int>("v_");
    
    [MenuItem(MenuRoot + "Primitives/Bool %&b", false, 21)]
    private static void CreateBool() => CreateAsset<Bool>("v_");
    
    [MenuItem(MenuRoot + "Primitives/Float %&f", false, 22)]
    private static void CreateFloat() => CreateAsset<Float>("v_");
    
    [MenuItem(MenuRoot + "Primitives/Double %&d", false, 23)]
    private static void CreateDouble() => CreateAsset<Double>("v_");
    
    [MenuItem(MenuRoot + "Primitives/String %&s", false, 24)]
    private static void CreateString() => CreateAsset<SharedString>("v_");
    
    [MenuItem(MenuRoot + "Primitives/IntWithEvent %#e", false, 25)]
    private static void CreateIntEvent() => CreateAsset<IntWithEvent>("e_");
    
    [MenuItem(MenuRoot + "Primitives/BoolWithEvent %#b", false, 26)]
    private static void CreateBoolEvent() => CreateAsset<BoolWithEvent>("e_");
    
    [MenuItem(MenuRoot + "Primitives/FloatWithEvent %#f", false, 27)]
    private static void CreateFloatEvent() => CreateAsset<FloatWithEvent>("e_");
    
    [MenuItem(MenuRoot + "Primitives/StringWithEvent %#s", false, 28)]
    private static void CreateStringEvent() => CreateAsset<StringWithEvent>("e_");

    // Vector Variables
    [MenuItem(MenuRoot + "Vectors/Vector2 %&2", false, 30)]
    private static void CreateVector2() => CreateAsset<SharedVector2>("v_");
    
    [MenuItem(MenuRoot + "Vectors/Vector2Int %#2", false, 31)]
    private static void CreateVector2Int() => CreateAsset<SharedVector2Int>("v_");
    
    [MenuItem(MenuRoot + "Vectors/Vector3 %&3", false, 32)]
    private static void CreateVector3() => CreateAsset<SharedVector3>("v_");
    
    [MenuItem(MenuRoot + "Vectors/Vector3Int %#3", false, 33)]
    private static void CreateVector3Int() => CreateAsset<SharedVector3Int>("v_");
    


    // Events
    [MenuItem(MenuRoot + "Game Events/Basic %&e", false, 40)]
    private static void CreateGameEvent() => CreateAsset<GameEvent>("e_");
    
    [MenuItem(MenuRoot + "Game Events/EventWithInt %&i", false, 41)]
    private static void CreateGameEventWithInt() => CreateAsset<GameEventWithInt>("e_");
    
    // Variables With Events
    
    

    // Utility function
    private static void CreateAsset<T>(string prefix) where T : ScriptableObject
    {
        var asset = ScriptableObject.CreateInstance<T>();
        string path = GetSelectedPathOrFallback();
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{prefix}.asset");
        
        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }

    private static string GetSelectedPathOrFallback()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
            return "Assets";
        
        if (!string.IsNullOrEmpty(System.IO.Path.GetExtension(path)))
            return System.IO.Path.GetDirectoryName(path);
        
        return path;
    }
}