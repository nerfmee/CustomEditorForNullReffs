using UnityEditor;
using UnityEngine;

public static class DebugEditorMenu 
{
   [MenuItem("CustomEditor/ValidateNullRefs")]

   public static void ValidateNullRefs()
   {
       var guids = AssetDatabase.FindAssets( "t:Prefab" );
       Debug.Log($"NullRefValidator: Total prefabs: {guids.Length}");

       foreach (var guid in guids)
       {
           var path = AssetDatabase.GUIDToAssetPath(guid);
           var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
           
           TryFindMissingReferencesInPrefab(prefab);
       }
   }
   
   private static void TryFindMissingReferencesInPrefab(GameObject prefab)
   {
       TryFindMissingReference(prefab, prefab.name);
       foreach (Transform partOfPrefab in prefab.transform)
       {
           TryFindMissingReference(partOfPrefab.gameObject, prefab.name);
       }
   }

   private static void TryFindMissingReference(GameObject currentObject, string prefabParentName)
   {
       Component[] components = currentObject.GetComponents(typeof(Component));
       foreach (Component component in components)
       {
           using var serializedObject = new SerializedObject(component);
           using var serializedProperty = serializedObject.GetIterator();
           while (serializedProperty.NextVisible(true))
           {
               if (serializedProperty.propertyType != SerializedPropertyType.ObjectReference)
               {
                   continue;
               }

               if (serializedProperty.objectReferenceValue == null && serializedProperty.objectReferenceInstanceIDValue != 0)
               {
                   Debug.LogError($"NullRefValidator: Prefab: {prefabParentName} Current Object: {currentObject} has missing reference in [{ObjectNames.NicifyVariableName(serializedProperty.name)}]");
               }
           }
       }
   }
}
