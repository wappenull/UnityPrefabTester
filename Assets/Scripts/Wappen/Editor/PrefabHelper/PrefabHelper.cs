using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

namespace Wappen.Editor
{
    /// <summary>
    /// Helps determine every aspect of GameObject prefab state.
    /// </summary>
    public static class PrefabHelper
    {
        // Good explanation is here: 
        // https://docs.unity3d.com/ScriptReference/PrefabUtility.IsOutermostPrefabInstanceRoot.html


        public class Properties
        {
            readonly GameObject originalGameObject;

            public Properties( GameObject g )
            {
                originalGameObject = g;
            }

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

            /// <summary>
            /// Type of prefab asset, only if isPartOfPrefabAsset=true.
            /// </summary>
            public PrefabAssetType prefabAssetType;

            /// <summary>
            /// This is variant prefab of other prefab asset.
            /// </summary>
            public bool isPrefabAssetVariant => prefabAssetType == PrefabAssetType.Variant;

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

            /* Extra queries ////////////////////////*/

            /// <summary>
            /// Walk one level up of prefab inheritance step.
            /// If this object is variant, this returns object it created from. (Could be another variant or prefab asset)
            /// If this object is prefab instance, this returns prefab it instanced from.
            /// </summary>
            public GameObject GetSourcePrefab( )
            {
                return PrefabUtility.GetCorrespondingObjectFromSource( originalGameObject );
            }

            /// <summary>
            /// It is like calling GetSourcePrefab (GetCorrespondingObjectFromSource) in chain all the way to the base.
            /// </summary>
            public GameObject GetFirstSourcePrefab( )
            {
                return PrefabUtility.GetCorrespondingObjectFromOriginalSource( originalGameObject );
            }
        }

        /// <summary>
        /// Get every basic prefab-related info about this node.
        /// </summary>
        public static Properties GetPrefabProperties( GameObject gameObject )
        {
            if( gameObject == null )
                return null;

            Properties p = new Properties( gameObject );

#if false // Does not required anymore, also generate warning in Editor
            // Wappen hack: From https://forum.unity.com/threads/problem-with-prefab-api-upgrade.537074/
            // Check if it is nesting prefab
            //bool isPartOfPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(gameObject); 
#endif

            // First group of check is to test everything about "Prefab Asset"
            {
                // First is to determine isPartOfPrefabAsset
                p.isPartOfPrefabAsset = PrefabUtility.IsPartOfPrefabAsset( gameObject );

                // Second alternate test, using direct AssetDatabase
                if( !p.isPartOfPrefabAsset )
                    p.isPartOfPrefabAsset = !string.IsNullOrEmpty( AssetDatabase.GetAssetPath( gameObject ) );

                // If it is under prefab asset, determine its root
                // Use gameObject.transform.root.gameObject to test for prefab asset root.
                if( p.isPartOfPrefabAsset )
                {
                    p.prefabAssetRoot = gameObject.transform.root.gameObject;
                    p.isPrefabAssetRoot = (gameObject == p.prefabAssetRoot);
                    p.prefabAssetType = PrefabUtility.GetPrefabAssetType( gameObject );
                }
            }

            // Second group of check is to test everything about "Prefab Instance" 
            // This is when you instance an prefab asset into scene or nested under other prefab.
            // First check is obvious, it should not and cannot be prefab asset root in the first place.
            if( !p.isPrefabAssetRoot )
            {
                // Use PrefabUtility.GetNearestPrefabInstanceRoot to test for prefab instance root.

                /* Basic visualization
                 * The node with its [Name] inside bracket is blue gameobject node.
                 * See image in https://docs.unity3d.com/ScriptReference/PrefabUtility.GetNearestPrefabInstanceRoot.html
                 * For possible prefab configuration
                 * 
                 * - [OUTERMOST]
                 *   - CHILD 1
                 *   - CHILD 2
                 *   - [prefab instance head] < If we are running on this node, isPrefabInstanceRoot = true
                 *     - child 1
                 *     - child 2 < If we are running on this node, isPrefabInstanceRoot = false, but we still have nearestInstanceRoot points to 'prefab instance head'
                 */

                // Note: "PrefabUtility.GetNearestPrefabInstanceRoot" Causes following warning message on editor:
                // SendMessage cannot be called during Awake, CheckConsistency, or OnValidate
                // If this API is called in such timing, it is unavoidable, live with it
                // https://forum.unity.com/threads/sendmessage-cannot-be-called-during-awake-checkconsistency-or-onvalidate-can-we-suppress.537265/
                p.nearestInstanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot( gameObject );
                p.isPartOfPrefabInstance = (p.nearestInstanceRoot != null);
                p.isPrefabInstanceRoot = gameObject == p.nearestInstanceRoot; // Equivalent to PrefabUtility.IsAnyPrefabInstanceRoot, see UnityReferenceSource
            }

            // Third test is testing everything about Prefab stage property
            // needed to be checked separately as it is very special rule
            var editorPrefabStage = PrefabStageUtility.GetCurrentPrefabStage( );
            if( editorPrefabStage != null ) 
            {
                // We are in prefab stage, but is selected gameobject really an object from prefab stage?
                // There is no 100% direct API to determine this, plus there is following observed obstacle when in prefab stage:
                // - root object editing in prefab stage wont have connection to its original prefab (it becomes gray gameobject icon), so we cannot test it with any prefab API. (bad)
                // - Also cannot test for 'editorPrefabStage.prefabContentsRoot == gameObject.transform.root.gameObject' directly, both seems to always be different object instance.
                // but we could derive from the following fact:

                // If it was NOT under prefab asset (from above test)
                // we can almost sure that it is in prefab stage
                if( p.isPartOfPrefabAsset == false )
                    p.isPartOfPrefabStage = true;

                // Editor prefab stage root object wont have transform.parent
                // That's how we determine if it is isPrefabStageRoot
                if( p.isPartOfPrefabStage && gameObject.transform.parent == null )
                    p.isPrefabStageRoot = true;
            }

            // AssetPath determination
            // PrefabHelper will try its best to detect most accurate prefab path for this prefab
            // Has to be done separately based on API priority
            if( p.isRootOfAnyPrefab )
            {
                if( p.isPrefabStageRoot )
                {
                    p.prefabAssetPath = editorPrefabStage.prefabAssetPath;
                }
                else if( p.isPrefabInstanceRoot ) // It is nested prefab instance inside another prefab
                {
                    // Note: GetPrefabAssetPathOfNearestInstanceRoot is the only way to obtain real asset path of this nested prefab instance
                    // internally it uses PrefabUtility.GetOriginalSourceOrVariantRoot which is internal function to Unity, see UnityReferenceSource
                    // FYI: Calling GetAssetPath will get the "top/outermost" one. Not this nested one. (dont want that)
                    p.prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot( p.nearestInstanceRoot );
                }
                else if( p.isPrefabAssetRoot )
                {
                    // Ordinary prefab asset root
                    p.prefabAssetPath = AssetDatabase.GetAssetPath( gameObject );
                }
            }
            else
            {
                // This object is not root of any prefab, but could still be part of some prefab, find that nearest asset path
                if( p.isPartOfPrefabStage )
                {
                    p.prefabAssetPath = editorPrefabStage.prefabAssetPath;
                }
                else if( p.isPartOfPrefabInstance )
                {
                    p.prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot( p.nearestInstanceRoot );
                }
                else if( p.isPartOfPrefabAsset )
                {
                    p.prefabAssetPath = AssetDatabase.GetAssetPath( gameObject.transform.root.gameObject );
                }
            }

            return p;
        }


        public static bool IsPartOfPrefabStage( GameObject gameObject, out PrefabStage pfs )
        {
            pfs = PrefabStageUtility.GetPrefabStage( gameObject );
            return pfs != null;
        }

        public static bool IsRootOfPrefabStage( GameObject gameObject, out PrefabStage pfs )
        {
            // Note: Cannot use pfs.IsPartOfPrefabContents or pfs.prefabContentsRoot
            // InvalidOperationException: Requesting 'prefabContentsRoot' from Awake and OnEnable are not supported
            if( IsPartOfPrefabStage( gameObject, out pfs ) && IsRootOfScene( gameObject ) )
                return true;
            return false;
        }

        public static bool IsRootOfScene( GameObject gameObject )
        {
            if( gameObject.transform.parent == null )
            {
                // Note: on first frame of entering prefab stage. It will called with scene not loaded
                if( gameObject.scene.isLoaded == false )
                    return true; 

                // Also see if it is first root object in stage
                GameObject[] allRoots = gameObject.scene.GetRootGameObjects( );
                return allRoots[0] == gameObject;
            }
            return false;
        }
    }
}