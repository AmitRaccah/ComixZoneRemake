using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Automatically ensures required shaders are included in builds
/// Place this in an "Editor" folder
/// </summary>
public class AutoIncludeShaders
{
    [InitializeOnLoadMethod]
    static void EnsureShadersIncluded()
    {
        // List of shaders that must be included in builds
        string[] requiredShaders = new string[]
        {
            "Custom/BrightGroundGlow",
            "Custom/ItemHighlightGlow",
            "Custom/VideoScreenEmissive",
            "Custom/SimpleGroundGlow"
        };
        
        // Get graphics settings
        var graphicsSettingsObj = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0];
        var serializedObject = new SerializedObject(graphicsSettingsObj);
        
        var arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");
        
        bool needsUpdate = false;
        var includedShaders = new List<Shader>();
        
        // Get currently included shaders
        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            var shader = arrayProp.GetArrayElementAtIndex(i).objectReferenceValue as Shader;
            if (shader != null)
            {
                includedShaders.Add(shader);
            }
        }
        
        // Check each required shader
        foreach (string shaderName in requiredShaders)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader != null && !includedShaders.Contains(shader))
            {
                arrayProp.arraySize++;
                var newElement = arrayProp.GetArrayElementAtIndex(arrayProp.arraySize - 1);
                newElement.objectReferenceValue = shader;
                needsUpdate = true;
                Debug.Log($"[AutoIncludeShaders] Added shader to build: {shaderName}");
            }
        }
        
        if (needsUpdate)
        {
            serializedObject.ApplyModifiedProperties();
            Debug.Log("[AutoIncludeShaders] Graphics settings updated. Shaders will now be included in builds.");
        }
    }
    
    [MenuItem("Tools/Verify Shaders in Build")]
    static void VerifyShaders()
    {
        string[] requiredShaders = new string[]
        {
            "Custom/BrightGroundGlow",
            "Custom/ItemHighlightGlow",
            "Custom/VideoScreenEmissive",
            "Custom/SimpleGroundGlow"
        };
        
        Debug.Log("=== Shader Build Inclusion Check ===");
        
        // Get graphics settings
        var graphicsSettingsObj = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0];
        var serializedObject = new SerializedObject(graphicsSettingsObj);
        
        var arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");
        
        var includedShaders = new List<string>();
        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            var shader = arrayProp.GetArrayElementAtIndex(i).objectReferenceValue as Shader;
            if (shader != null)
            {
                includedShaders.Add(shader.name);
            }
        }
        
        foreach (string shaderName in requiredShaders)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogWarning($"❌ Shader not found in project: {shaderName}");
            }
            else if (includedShaders.Contains(shaderName))
            {
                Debug.Log($"✅ Shader included in build: {shaderName}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Shader NOT included in build: {shaderName}");
            }
        }
        
        Debug.Log("=== Check Complete ===");
        Debug.Log("If any shaders are missing, they will be added automatically on next script reload.");
    }
}