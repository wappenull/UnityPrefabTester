using UnityEngine;
using UnityEditor;
using Wappen.Editor;

public class PrefabApiTest : EditorWindow
{
    [MenuItem( "Tool/Prefab tester API example" )]
    static void OpenConfig( )
    {
        EditorWindow.GetWindow<PrefabApiTest>( true, "Prefab tester API example" );
    }

    private void OnGUI( )
    {
        _DisplayApiExample( );
    }

    private UnityEngine.Object m_TestingPrefab;
    private UnityEngine.Object m_CurrentDisplayingPrefab;
    private PrefabHelper.Properties m_CurrentProperties;

    private void _DisplayApiExample( )
    {
        string helpText =
            "TRYME! Try to drag game object from various places and Display prefab properties.\n" +
            "- From scene\n" +
            "- From asset folder\n" +
            "- From prefab editing stage\n" +
            "- Nested prefab, child of nested prefab, non-prefab\n" +
            "Output will be in console log.";
            
        EditorGUILayout.HelpBox( helpText, MessageType.Info );
        m_TestingPrefab = EditorGUILayout.ObjectField( "Source prefab", m_TestingPrefab, typeof( GameObject ), true );

        if( m_TestingPrefab != m_CurrentDisplayingPrefab )
        {
            m_CurrentDisplayingPrefab = m_TestingPrefab;
            m_CurrentProperties = PrefabHelper.GetPrefabProperties( m_TestingPrefab as GameObject );
        }

        // Display output
        var prop = m_CurrentProperties;
        string report = "";
        if( prop.isPartOfPrefabStage )
        {
            report += "Is ";
            if( prop.isPrefabStageRoot ) report += "root of ";
            else report += "child of ";
            report += "prefabStage";
            report += "\n";
        }

        if( prop.isPartOfPrefabAsset )
        {
            report += "Is ";
            if( prop.isPrefabAssetRoot ) report += "root of ";
            else report += "child of ";
            report += "prefabAsset";
            report += "\n";
        }

        if( prop.isPartOfPrefabInstance )
        {
            report += "Is ";
            if( prop.isPrefabInstanceRoot ) report += "root of ";
            else report += "child of ";
            report += "prefabInstance";
            report += "\n";
        }
#if false
        if( prop.isVariant )
        {
            report += "variant";
            report += " ";
        }
#endif
        if( prop.isSceneObject )
        {
            report += "Is sceneObject";
            report += "\n";
        }

        report += "nearest AssetPath: " + prop.prefabAssetPath;

        EditorGUILayout.HelpBox( report, MessageType.None );

    }



}

