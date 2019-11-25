# UnityPrefabTester
Ultimate prefab property tester for nested prefab era.

It will test selected game object if it is "part of" or "root of" any prefab type.
Including
- Prefab Asset (The one that sits in Asset folder)
- Prefab Instance (The one that dragged into scene)
- Prefab Stage (Editing prefab intermediate in UnityEditor prefab edit mode)

Including resolve nearest asset path if available.

To try the example API, import package or open demo project.
Select menu Tool->Prefab tester API example

Related API used:
- PrefabUtility.IsPartOfPrefabAsset
- PrefabUtility.GetNearestPrefabInstanceRoot
- PrefabStageUtility.GetCurrentPrefabStage
- PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot
- AssetDatabase.GetAssetPath