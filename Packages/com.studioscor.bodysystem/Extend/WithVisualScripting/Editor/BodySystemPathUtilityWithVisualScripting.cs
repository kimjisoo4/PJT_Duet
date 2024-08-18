﻿#if SCOR_ENABLE_VISUALSCRIPTING
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Unity.VisualScripting;

using StudioScor.BodySystem.Editor;

namespace StudioScor.BodySystem.VisualScripting.Editor
{

    public static class BodySystemPathUtilityWithVisualScripting
    {
        public static string VisualScriptingResources => BodySystemPathUtility.RootFolder + "Extend/WithVisualScripting/Editor/Icons/";

        private readonly static Dictionary<string, EditorTexture> _EditorTextures = new Dictionary<string, EditorTexture>();

        public static EditorTexture Load(string name)
        {
            if (_EditorTextures.ContainsKey(name))
            {
                return GetStateTexture(name);
            }

            var _path = VisualScriptingResources;

            var editorTexture = EditorTexture.Single(AssetDatabase.LoadAssetAtPath<Texture2D>(_path + name + ".png"));

            _EditorTextures.Add(name, editorTexture);

            return GetStateTexture(name);
        }

        private static EditorTexture GetStateTexture(string name)
        {
            return _EditorTextures[name];
        }
    }
}
#endif