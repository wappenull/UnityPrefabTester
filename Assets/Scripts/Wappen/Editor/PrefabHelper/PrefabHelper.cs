using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

namespace Wappen.Editor
{
    public static class PrefabHelper
    {
        public struct Properties
        {
            /* Prefab stage ///////////////////////*/

            /// <summary>
            /// Is root or child node under prefab stage.
            /// </summary>
            public bool isPartOfPrefabStage;

            /// <summary>
            /// This  node is root of editing prefab stage.
            /// </summary>
            public bool isPrefabStageRoot;

            /* Prefab instance /////////////////////*/

            /// <summary>
            /// Is root or child node under prefab instance.
            /// </summary>
            public bool isPartOfPrefabInstance;

            /// <summary>
            /// This node is prefab instance root.
            /// But it could be positioned in scene, under prefab stage or under prefab asset.
            /// </summary>
            public bool isPrefabInstanceRoot;

            /// <summary>
            /// Get node that is nearest instance root above this node.
            /// Or itself if isPrefabInstanceRoot=true.
            /// </summary>
            public GameObject nearestInstanceRoot;

            /* Prefab asset ////////////////////////*/

            /// <summary>
            /// Is root of prefab asset.
            /// </summary>
            public bool isPrefabAssetRoot;

            /// <summary>
            /// Is root or part of prefab asset.
            /// </summary>
            public bool isPartOfPrefabAsset;

#if false
            /// <summary>
            /// This is variant prefab of other prefab asset.
            /// Warning: Limited support, may not accurate on every cases
            /// </summary>
            public bool isVariant;
#endif

            /// <summary>
            /// Nearest Asset path of prefab around selected gameObject.
            /// null if selected object is not part of any prefab.
            /// </summary>
            public string prefabAssetPath;

            /// <summary>
            /// Valid only when isPrefabAssetRoot = true.
            /// Top most prefab asset root.
            /// Is this object itself when isPrefabAssetRoot = true, could be otherwise when false.
            /// </summary>
            public GameObject prefabAssetRoot;

            /* Misc /////////////////////////////////*/

            /// <summary>
            /// This is object placed into scene.
            /// </summary>
            public bool isSceneObject => (!isPartOfPrefabAsset && !isPartOfPrefabStage);

            /// <summary>
            /// This game object is root or part of any prefab instance or asset prefab or prefab in prefab stage.
            /// </summary>
            public bool isPartOfAnyPrefab => prefabAssetPath != null;

            /// <summary>
            /// This game object is indeed root of some prefab.
            /// </summary>
            public bool isRootOfAnyPrefab => isPrefabAssetRoot || isPrefabInstanceRoot || isPrefabStageRoot;
        }

        /// <summary>
        /// Get every basic prefab-related info about this node.
        /// </summary>
        public static Properties GetPrefabProperties( GameObject gameObject )
        {
            Properties p = new Properties( );
            
            // Wappen hack: From https://forum.unity.com/threads/problem-with-prefab-api-upgrade.537074/
            // Check if it is nesting prefab
            //bool isPartOfPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(gameObject); // Does not required, also generate warning in Editor
            p.isPartOfPrefabAsset = PrefabUtility.IsPartOfPrefabAsset( gameObject );

            // Second test, using direct AssetDatabase
            if( !p.isPartOfPrefabAsset )
                p.isPartOfPrefabAsset = !string.IsNullOrEmpty( AssetDatabase.GetAssetPath( gameObject ) );

            // Prefab Asset Root method (See obsolete warning in FindPrefabRoot)
            // Use gameObject.transform.root.gameObject to test for prefab asset root.
            // Use PrefabUtility.GetNearestPrefabInstanceRoot to test for prefab instance root.
            
            if( p.isPartOfPrefabAsset )
            {
                p.prefabAssetRoot = gameObject.transform.root.gameObject;
                p.isPrefabAssetRoot = (gameObject == p.prefabAssetRoot);
            }

            // It could be prefab instance nested inside asset prefab
            // Test its own root-ness first with nearest instance root method regardless of isPartOfPrefabAsset status
            
            if( !p.isPrefabAssetRoot ) // If it is not prefab asset root, check if it is prefab instance root
            {
                // Note: Cause warning message on editor:
                // SendMessage cannot be called during Awake, CheckConsistency, or OnValidate
                p.nearestInstanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot( gameObject );
                p.isPartOfPrefabInstance = (p.nearestInstanceRoot != null);
                p.isPrefabInstanceRoot = (gameObject == p.nearestInstanceRoot); // Equivalent to PrefabUtility.IsAnyPrefabInstanceRoot
            }


            // Prefab stage needed to be checked separately as it is very special rule
            var editorPrefabStage = PrefabStageUtility.GetCurrentPrefabStage( );
            if( editorPrefabStage != null ) // We are in prefab stage, but is selected gameobject really an object from prefab stage?
            {
                // Not 100% sure, but from the fact:
                // - root object editing in prefab stage wont have connection to its original prefab (gray gameobject icon) 
                // - editorPrefabStage.prefabContentsRoot cannot be use to == test with gameObject.transform.root.gameObject (which is gameobject instance)
                //   So with best effort we can test only name there. (The best approach is to test asset path, but API is yet to be found)
                if( p.isPartOfPrefabAsset == false )
                    p.isPartOfPrefabStage = true;

                // Root object has no more parent
                if( p.isPartOfPrefabStage && gameObject.transform.parent == null )
                    p.isPrefabStageRoot = true;
            }

            // AssetPath determination
            // Has to be done correctly in this order based on API priority
            if( p.isRootOfAnyPrefab )
            {
                if( p.isPrefabStageRoot )
                {
                    p.prefabAssetPath = editorPrefabStage.prefabAssetPath;
                }
                else if( p.isPrefabInstanceRoot ) // Prioritize displaying prefab instance before root asset
                {
                    // Trace back from prefab instance to original prefab asset
                    // Note: GetPrefabAssetPathOfNearestInstanceRoot is the only way to obtain real asset path of this (sub/nested)prefab
                    // internally it uses PrefabUtility.GetOriginalSourceOrVariantRoot which is internal to Unity
                    p.prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot( p.nearestInstanceRoot );
                }
                else if( p.isPrefabAssetRoot )
                {
                    p.prefabAssetPath = AssetDatabase.GetAssetPath( gameObject );
                }
            }
            else
            {
                // This object is not root, but could still be part of some prefab, find that asset path
                // Nearest asset path
                if( p.isPartOfPrefabStage )
                {
                    p.prefabAssetPath = editorPrefabStage.prefabAssetPath;
                }
                else if( p.isPartOfPrefabInstance ) // Prioritize displaying prefab instance before root asset
                {
                    p.prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot( p.nearestInstanceRoot );
                }
                else if( p.isPartOfPrefabAsset )
                {
                    p.prefabAssetPath = AssetDatabase.GetAssetPath( gameObject.transform.root.gameObject );
                }
            }

#if false
            // Variant checking: has limited support and may not be accurate all the time.
            // Note: Unity has no method for refering back to variant source. (?)
            // You only know if it is variant or not. Also, why you should bother?
            p.isVariant = PrefabUtility.IsPartOfVariantPrefab( gameObject );
#endif
            return p;
        }
    }
}