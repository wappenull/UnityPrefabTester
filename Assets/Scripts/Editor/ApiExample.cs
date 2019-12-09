using UnityEngine;
using UnityEditor;
using Wappen.Editor;
using System;

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
        _DisplayExtraSelect( );
    }

    private void _DisplayExtraSelect( )
    {
        GUILayout.Space( 40 );
        EditorGUILayout.HelpBox( 
            "Extra sample\n"+
            "This will select gameobject node under CubePlayer/LeftHand/Gun in PrefabAsset: Assets/Prefabs/CubePlayer.prefab\n"+
            "Which is not possible to select by hand in newer version of unity.\n"+
            "This will demonstrate prefab instance nested under prefab asset.", MessageType.Info );
        if( GUILayout.Button( "Example: Set to CubePlayer/LeftHand/Gun in PrefabAsset" ) )
        {
            GameObject g = AssetDatabase.LoadAssetAtPath<GameObject>( "Assets/Prefabs/CubePlayer.prefab" );
            m_TestingPrefab = g.transform.Find( "LeftHand/Gun" ).gameObject;
        }
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
            "Output below.";
            
        EditorGUILayout.HelpBox( helpText, MessageType.Info );
        m_TestingPrefab = EditorGUILayout.ObjectField( "Testing GameObject", m_TestingPrefab, typeof( GameObject ), true );

        
        if( m_TestingPrefab == null )
        {
            m_CurrentDisplayingPrefab = null;
        }

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
        report += "\n";

        if( prop.prefabAssetRoot != null )
        {
            report += "Prefab asset root is: " + prop.prefabAssetRoot.name;
            report += "\n";
        }

        EditorGUILayout.HelpBox( report, MessageType.None );

        if( prop.nearestInstanceRoot != null )
        {
            GUI.enabled = false;
            EditorGUILayout.LabelField( "Resolved nearest instance root" );
            EditorGUI.indentLevel += 1;
            EditorGUILayout.ObjectField( prop.nearestInstanceRoot, typeof(GameObject), true );
            EditorGUI.indentLevel -= 1;
            GUI.enabled = true;
        }
        

        if( prop.prefabAssetRoot != null )
        {
            GUI.enabled = false;
            EditorGUILayout.LabelField( "Resolved prefab asset root" );
            EditorGUI.indentLevel += 1;
            EditorGUILayout.ObjectField( prop.prefabAssetRoot, typeof(GameObject), false );
            EditorGUI.indentLevel -= 1;
            GUI.enabled = true;
        }
    }



}

