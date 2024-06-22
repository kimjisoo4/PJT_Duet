using UnityEditor;
using UnityEngine;
using StudioScor.BodySystem;

namespace StudioScor.BodySystem.Editor
{
    public static class CreateBodyPart
    {
        [MenuItem("GameObject/StudioScor/BodySystem/Create BodyPart")]
        private static void CreateDefaultBodyPart()
        {
            var selects = Selection.gameObjects;

            if (selects is null)
                return;

            foreach (var select in selects)
            {
                if (select is null)
                    continue;

                var name = "BodyPart_" + select.name;

                if (select.transform.Find(name))
                    continue;

                GameObject gameObject = new(name);
                
                gameObject.transform.SetParent(select.transform);

                gameObject.transform.localPosition = default;
                gameObject.transform.localRotation = default;
                gameObject.transform.localScale = Vector3.one;

                gameObject.AddComponent<BodyPartComponent>();
            }
        }
    }
    public static class BodySystemPathUtility
    {
        private static string _RootFolder;
        public static string RootFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_RootFolder))
                {
                    _RootFolder = PathOf("studioscor_bodysystem_root");
                }

                return _RootFolder;
            }
        }

        private static string PathOf(string fileName)
        {
            var files = AssetDatabase.FindAssets(fileName);

            if (files.Length == 0)
                return string.Empty;

            var assetPath = AssetDatabase.GUIDToAssetPath(files[0]).Replace(fileName, string.Empty);

            return assetPath;
        }
    }
}
