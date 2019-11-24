using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityEditor
{
    //
    //  This extension allow to drag&drop subassets while pressing 'Alt'
    // 
    //
    
    [InitializeOnLoad]
    public static class DragAndDropSubAssetsExtension
    {
        static DragAndDropSubAssetsExtension()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
        }

        private static void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
        {
            // Break - key modifier doen't pressed
            var activated = Event.current.alt;
            if (activated == false) return;

            // Break - OnGUI() call not for mouse target
            var within = selectionRect.Contains(Event.current.mousePosition);
            if (within == false) return;

            // Break - destination match one of sources 
            var target = AssetDatabase.GUIDToAssetPath(guid);
            var targetInSources = Array.IndexOf(DragAndDrop.paths, target) != -1;
            if (targetInSources) return;

            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            }
            else if (Event.current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                Move(DragAndDrop.objectReferences, target);
            }
        }

        private static void Move(IEnumerable<UnityEngine.Object> sources, string destinationPath)
        {
            var destinationIsFolder = AssetDatabase.IsValidFolder(destinationPath);
            
            foreach (var source in sources)
            {
                var sourceIsMain = AssetDatabase.IsMainAsset(source);
                if (sourceIsMain && destinationIsFolder) continue;

                var sourcePath = AssetDatabase.GetAssetPath(source);
                var sourceAssets = new List<UnityEngine.Object>() { source };
                if (sourceIsMain)
                    sourceAssets.AddRange(AssetDatabase.LoadAllAssetRepresentationsAtPath(sourcePath));

                // Peform move assets from source file to destination
                foreach (var asset in sourceAssets)
                {
                    AssetDatabase.RemoveObjectFromAsset(asset);

                    if (destinationIsFolder)
                    {
                        var assetName = asset.name + "." + GetFileExtention(asset);
                        var assetPath = Path.Combine(destinationPath, assetName);
                        AssetDatabase.CreateAsset(asset, assetPath);
                    }
                    else
                    {
                        AssetDatabase.AddObjectToAsset(asset, destinationPath);
                    }
                }

                // Remove asset file if it is empty now
                if (sourceIsMain)
                    AssetDatabase.DeleteAsset(sourcePath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// Thanks to mob-sakai (https://github.com/mob-sakai/SubAssetEditor) for full mapping list
        private static string GetFileExtention(UnityEngine.Object obj)
        {
            if (obj is AnimationClip)
                return "anim";
            else if (obj is UnityEditor.Animations.AnimatorController)
                return "controller";
            else if (obj is AnimatorOverrideController)
                return "overrideController";
            else if (obj is Material)
                return "mat";
            else if (obj is Texture)
                return "png";
            else if (obj is ComputeShader)
                return "compute";
            else if (obj is Shader)
                return "shader";
            else if (obj is Cubemap)
                return "cubemap";
            else if (obj is Flare)
                return "flare";
            else if (obj is ShaderVariantCollection)
                return "shadervariants";
            else if (obj is LightmapParameters)
                return "giparams";
            else if (obj is GUISkin)
                return "guiskin";
            else if (obj is PhysicMaterial)
                return "physicMaterial";
            else if (obj is UnityEngine.Audio.AudioMixer)
                return "mixer";
            else if (obj is TextAsset)
                return "txt";
            else if (obj is GameObject)
                return "prefab";
            else if (obj is ScriptableObject)
                return "asset";
            return "";
        }
    }
}