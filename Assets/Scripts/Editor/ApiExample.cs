using UnityEngine;
using UnityEditor;
using Wappen.Editor;
using System;
using System.Collections.Generic;

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

        if( m_TestingPrefab != m_CurrentDisplayingPrefab || m_CurrentProperties == null )
        {
            m_CurrentDisplayingPrefab = m_TestingPrefab;
            m_CurrentProperties = PrefabHelper.GetPrefabProperties( m_TestingPrefab as GameObject );
        }

        if( m_CurrentProperties == null )
            return;

        // Display output
        var prop = m_CurrentProperties;
        string report = "";
        if( prop.isPartOfPrefabStage )
        {
            report += "This is ";
            if( prop.isPrefabStageRoot ) report += "root of ";
            else report += "child of ";
            report += "prefabStage";
            report += "\n";
        }

        if( prop.isPartOfPrefabAsset )
        {
            report += "This is ";
            if( prop.isPrefabAssetRoot ) report += "root of ";
            else report += "child of ";
            report += "prefabAsset";
            report += "\n";
        }

        if( prop.isPartOfPrefabInstance )
        {
            report += "This is ";
            if( prop.isPrefabInstanceRoot ) report += "root of ";
            else report += "child of ";
            report += "prefabInstance";
            report += "\n";
        }

        if( prop.isPrefabAssetVariant )
        {
            report += "This is variant";
            report += "\n";
        }

        if( prop.isSceneObject )
        {
            report += "This is sceneObject";
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
            EditorGUILayout.LabelField( "Resolved nearest instance root" );
            EditorGUI.indentLevel += 1;
            GUILayout.Label( Helper.GetGameObjectPath( prop.nearestInstanceRoot ) );
            GUI.enabled = false;
            EditorGUILayout.ObjectField( prop.nearestInstanceRoot, typeof(GameObject), true );
            GUI.enabled = true;
            EditorGUI.indentLevel -= 1;
        }
        

        if( prop.prefabAssetRoot != null )
        {
            EditorGUILayout.LabelField( "Resolved prefab asset root, this object was from this asset:" );
            EditorGUI.indentLevel += 1;
            GUILayout.Label( Helper.GetGameObjectPath( prop.prefabAssetRoot ) );
            GUI.enabled = false;
            EditorGUILayout.ObjectField( prop.prefabAssetRoot, typeof(GameObject), false );
            GUI.enabled = true;
            EditorGUI.indentLevel -= 1;
        }

        int level = 1;
        GameObject oneLevelUp = prop.GetSourcePrefab( );
        while( oneLevelUp != null )
        {
            if( level > 10 )
            {
                GUILayout.Label( "AND MORE!...." );
                break;
            }

            EditorGUILayout.LabelField( $"Sourced from ({level} level up), this object was made from:" );
            EditorGUI.indentLevel += 1;
            GUILayout.Label( Helper.GetGameObjectPath( oneLevelUp ) );
            GUI.enabled = false;
            EditorGUILayout.ObjectField( oneLevelUp, typeof(GameObject), false );
            GUI.enabled = true;
            EditorGUI.indentLevel -= 1;

            // To next
            level++;
            oneLevelUp = PrefabUtility.GetCorrespondingObjectFromSource( oneLevelUp ); // this is function behind GetSourcePrefab
        }
    }

    static class Helper
    {
        /// <summary>
        /// Get game object path in hierarchy.
        /// </summary>
        public static string GetGameObjectPath( GameObject gameObject )
        {
            List<string> path = new List<string>( );
            Transform iter = gameObject.transform;
            while( iter != null )
            {
                if( path.Count > 20 )
                    break; // Safety measure

                path.Insert( 0, iter.name );
                iter = iter.parent;
            }

            var scene = gameObject.scene;
            if( scene.isLoaded && !string.IsNullOrEmpty( scene.name ) )
                path.Insert( 0, $"{scene.name}:" );
            else
                path.Insert( 0, $"NoScene:" );

            return  string.Join( "/", path.ToArray( ) );
        }
    }

}

