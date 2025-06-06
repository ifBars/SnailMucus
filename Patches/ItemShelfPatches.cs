using HarmonyLib;
using MelonLoader;
using UnityEngine;
using System.Reflection;
using System.IO;
using System;
#if MONO
using ScheduleOne;
using ScheduleOne.Storage;
using ScheduleOne.ItemFramework;
#else
using Il2CppScheduleOne;
using Il2CppScheduleOne.Storage;
using Il2CppScheduleOne.ItemFramework;
#endif

namespace HS_SFW.Patches
{
    public class ItemShelfPatches
    {
        private static Material cleanLabelMaterial = null;
        private static readonly Color bodyColor = new Color(0.6039f, 0.651f, 0.6314f, 1f);

        public static Material GetLabelMaterial()
        {
            return cleanLabelMaterial;
        }

        public static void InitializeCustomLabelMaterial()
        {
            try
            {
                if (EquippableViewmodelPatches.GetLabelMaterial() != null)
                {
                    cleanLabelMaterial = EquippableViewmodelPatches.GetLabelMaterial();
                    return;
                }

                var assembly = Assembly.GetExecutingAssembly();
                using Stream stream = assembly.GetManifestResourceStream("HS_SFW.Assets.HorseSemenLabel_icon.png");
                if (stream == null)
                {
                    MelonLogger.Error("[ItemShelfPatches] Failed to find embedded resource: HorseSemenLabel_icon.png");
                    return;
                }

                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(bytes))
                {
                    cleanLabelMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                        {
                            mainTexture = texture
                        };
                }
                else
                {
                    MelonLogger.Error("[ItemShelfPatches] Failed to load custom horsesemen label texture");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[ItemShelfPatches] Error initializing custom horsesemen label material for shelf items: {ex.Message}");
                MelonLogger.Error(ex.StackTrace);
            }
        }

#if MONO
        [HarmonyPatch(typeof(StoredItem), "InitializeStoredItem")]
#else
        [HarmonyPatch(typeof(Il2CppScheduleOne.Storage.StoredItem), "InitializeStoredItem")]
#endif
        public static class StoredItem_InitializeStoredItem_Patch
        {
            public static void Postfix(StoredItem __instance, StorableItemInstance _item)
            {
                try
                {
                    if (_item is not { ID: "horsesemen" }) return;
                    
                    var meshRenderers = __instance.GetComponentsInChildren<MeshRenderer>(true);
                    
                    // Apply custom material to Label
                    bool labelFound = false;
                    foreach (var renderer in meshRenderers)
                    {
                        if (renderer.gameObject.name != "Label") continue;
                        
                        labelFound = true;
                        
                        if (cleanLabelMaterial == null) continue;
                        
                        Material oldMaterial = renderer.material;
                        renderer.material = cleanLabelMaterial;
                        
                        break;
                    }
                    
                    if (!labelFound)
                    {
                        MelonLogger.Warning("[ItemShelfPatches] No Label object found in mesh renderers");
                    }
                    
                    // Change Body color
                    bool bodyFound = false;
                    foreach (var renderer in meshRenderers)
                    {
                        if (renderer.gameObject.name != "Body") continue;
                        
                        bodyFound = true;
                        Color oldColor = renderer.material.color;
                        renderer.material.color = bodyColor;
                        
                        break;
                    }
                    
                    if (!bodyFound)
                    {
                        MelonLogger.Warning("[ItemShelfPatches] No Body object found in mesh renderers");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[ItemShelfPatches] Error in StoredItem.InitializeStoredItem patch: {ex.Message}");
                    MelonLogger.Error(ex.StackTrace);
                }
            }
        }

#if MONO
        [HarmonyPatch(typeof(StoredItem), "CreateGhostModel")]
#else
        [HarmonyPatch(typeof(Il2CppScheduleOne.Storage.StoredItem), "CreateGhostModel")]
#endif
        public static class StoredItem_CreateGhostModel_Patch
        {
            public static void Postfix(ref GameObject __result, ItemInstance _item)
            {
                try
                {
                    if (_item is not { ID: "horsesemen" } || __result == null) return;
                    
                    var meshRenderers = __result.GetComponentsInChildren<MeshRenderer>(true);
                    
                    // Apply custom material to Label
                    bool labelFound = false;
                    foreach (var renderer in meshRenderers)
                    {
                        if (renderer.gameObject.name != "Label") continue;
                        
                        labelFound = true;
                        
                        if (cleanLabelMaterial == null) continue;
                        
                        Material oldMaterial = renderer.material;
                        renderer.material = cleanLabelMaterial;
                        
                        break;
                    }
                    
                    if (!labelFound)
                    {
                        MelonLogger.Warning("[ItemShelfPatches] No Label object found in ghost mesh renderers");
                    }
                    
                    // Change Body color
                    bool bodyFound = false;
                    foreach (var renderer in meshRenderers)
                    {
                        if (renderer.gameObject.name != "Body") continue;
                        
                        bodyFound = true;
                        Color oldColor = renderer.material.color;
                        renderer.material.color = bodyColor;
                        
                        break;
                    }
                    
                    if (!bodyFound)
                    {
                        MelonLogger.Warning("[ItemShelfPatches] No Body object found in ghost mesh renderers");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[ItemShelfPatches] Error in StoredItem.CreateGhostModel patch: {ex.Message}");
                    MelonLogger.Error(ex.StackTrace);
                }
            }
        }

#if !MONO
        [HarmonyPatch(typeof(Il2CppScheduleOne.Storage.StoredItem), "InitializeStoredItem")]
        public static class StoredItem_InitializeStoredItem_Il2CppPatch
        {
            public static void Postfix(Il2CppScheduleOne.Storage.StoredItem __instance, Il2CppScheduleOne.Storage.StorableItemInstance _item)
            {
                try
                {
                    // Try alternative item ID check approach
                    bool isHorseSemen = false;
                    
                    try 
                    {
                        // Using direct property access instead of casting
                        var idProp = _item?.GetType().GetProperty("ID");
                        if (idProp != null)
                        {
                            string id = idProp.GetValue(_item, null) as string;
                            isHorseSemen = id == "horsesemen";
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"[ItemShelfPatches] Error accessing ID property: {ex.Message}");
                    }
                    
                    if (!isHorseSemen) return;
                    
                    // Create material from scratch for this instance
                    Material newMaterial = CreateCustomMaterial();
                    
                    if (newMaterial == null)
                    {
                        MelonLogger.Error("[ItemShelfPatches] Failed to create new material");
                        return;
                    }
                    
                    // Try to find the Label object directly by name in the hierarchy
                    Transform[] allChildren = __instance.gameObject.GetComponentsInChildren<Transform>(true);
                    
                    // Find and handle Label
                    Transform labelTransform = null;
                    foreach (var child in allChildren)
                    {
                        if (child.gameObject.name == "Label")
                        {
                            labelTransform = child;
                            break;
                        }
                    }
                    
                    if (labelTransform != null)
                    {
                        MeshRenderer labelRenderer = labelTransform.GetComponent<MeshRenderer>();
                        if (labelRenderer != null)
                        {
                            try
                            {
                                // Direct material assignment
                                labelRenderer.material = newMaterial;
                            }
                            catch (Exception ex)
                            {
                                MelonLogger.Error($"[ItemShelfPatches] Error assigning material: {ex.Message}");
                            }
                        }
                        else
                        {
                            MelonLogger.Warning("[ItemShelfPatches] Label transform doesn't have a MeshRenderer component");
                        }
                    }
                    else
                    {
                        MelonLogger.Warning("[ItemShelfPatches] Couldn't find Label transform in hierarchy");
                    }
                    
                    // Find and handle Body
                    Transform bodyTransform = null;
                    foreach (var child in allChildren)
                    {
                        if (child.gameObject.name == "Body")
                        {
                            bodyTransform = child;
                            break;
                        }
                    }
                    
                    if (bodyTransform != null)
                    {
                        MeshRenderer bodyRenderer = bodyTransform.GetComponent<MeshRenderer>();
                        if (bodyRenderer != null)
                        {
                            try
                            {
                                Color oldColor = bodyRenderer.material.color;
                                bodyRenderer.material.color = bodyColor;
                            }
                            catch (Exception ex)
                            {
                                MelonLogger.Error($"[ItemShelfPatches] Error changing body color: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[ItemShelfPatches] Error in IL2CPP-specific InitializeStoredItem patch: {ex.Message}");
                    MelonLogger.Error(ex.StackTrace);
                }
            }
        }

        [HarmonyPatch(typeof(Il2CppScheduleOne.Storage.StoredItem), "CreateGhostModel")]
        public static class StoredItem_CreateGhostModel_Il2CppPatch
        {
            public static void Postfix(ref GameObject __result, Il2CppScheduleOne.ItemFramework.ItemInstance _item)
            {
                try
                {
                    if (__result == null)
                    {
                        MelonLogger.Warning("[ItemShelfPatches] Ghost model result is null");
                        return;
                    }
                    
                    // Try alternative item ID check approach
                    bool isHorseSemen = false;
                    
                    try 
                    {
                        // Using direct property access instead of casting
                        var idProp = _item?.GetType().GetProperty("ID");
                        if (idProp != null)
                        {
                            string id = idProp.GetValue(_item, null) as string;
                            isHorseSemen = id == "horsesemen";
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"[ItemShelfPatches] Ghost - Error accessing ID property: {ex.Message}");
                    }
                    
                    if (!isHorseSemen) return;
                    
                    // Create material from scratch for this instance
                    Material newMaterial = CreateCustomMaterial();
                    
                    if (newMaterial == null)
                    {
                        MelonLogger.Error("[ItemShelfPatches] Failed to create new material for ghost");
                        return;
                    }
                    
                    // Try to find the Label object directly by name in the hierarchy
                    Transform[] allChildren = __result.GetComponentsInChildren<Transform>(true);
                    
                    // Find and handle Label
                    Transform labelTransform = null;
                    foreach (var child in allChildren)
                    {
                        if (child.gameObject.name == "Label")
                        {
                            labelTransform = child;
                            break;
                        }
                    }
                    
                    if (labelTransform != null)
                    {
                        MeshRenderer labelRenderer = labelTransform.GetComponent<MeshRenderer>();
                        if (labelRenderer != null)
                        {
                            try
                            {
                                // Direct material assignment
                                labelRenderer.material = newMaterial;
                            }
                            catch (Exception ex)
                            {
                                MelonLogger.Error($"[ItemShelfPatches] Error assigning material to ghost: {ex.Message}");
                            }
                        }
                        else
                        {
                            MelonLogger.Warning("[ItemShelfPatches] Label transform in ghost doesn't have a MeshRenderer component");
                        }
                    }
                    else
                    {
                        MelonLogger.Warning("[ItemShelfPatches] Couldn't find Label transform in ghost hierarchy");
                    }
                    
                    // Find and handle Body
                    Transform bodyTransform = null;
                    foreach (var child in allChildren)
                    {
                        if (child.gameObject.name == "Body")
                        {
                            bodyTransform = child;
                            break;
                        }
                    }
                    
                    if (bodyTransform != null)
                    {
                        MeshRenderer bodyRenderer = bodyTransform.GetComponent<MeshRenderer>();
                        if (bodyRenderer != null)
                        {
                            try
                            {
                                Color oldColor = bodyRenderer.material.color;
                                bodyRenderer.material.color = bodyColor;
                            }
                            catch (Exception ex)
                            {
                                MelonLogger.Error($"[ItemShelfPatches] Error changing ghost body color: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[ItemShelfPatches] Error in IL2CPP-specific CreateGhostModel patch: {ex.Message}");
                    MelonLogger.Error(ex.StackTrace);
                }
            }
        }
        
        private static Material CreateCustomMaterial()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using Stream stream = assembly.GetManifestResourceStream("HS_SFW.Assets.HorseSemenLabel_icon.png");
                if (stream == null)
                {
                    MelonLogger.Error("[ItemShelfPatches] Failed to find embedded resource for new material");
                    return null;
                }

                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                bool loadSuccess = texture.LoadImage(bytes);
                
                if (!loadSuccess)
                {
                    MelonLogger.Error("[ItemShelfPatches] Failed to load texture for new material");
                    return null;
                }
                
                Shader shader = null;
                string[] shaderNames = {
                    "Universal Render Pipeline/Lit",
                    "Standard",
                    "Unlit/Texture",
                    "Diffuse",
                    "Legacy Shaders/Diffuse"
                };
                
                foreach (string shaderName in shaderNames)
                {
                    shader = Shader.Find(shaderName);
                    if (shader != null) break;
                }
                
                if (shader == null)
                {
                    MelonLogger.Error("[ItemShelfPatches] No suitable shader found for new material");
                    return null;
                }
                
                Material material = new Material(shader);
                material.name = "CustomHorseSemenLabel_Shelf_" + DateTime.Now.Ticks;
                material.mainTexture = texture;
                
                try
                {
                    int mainTexID = Shader.PropertyToID("_MainTex");
                    material.SetTexture(mainTexID, texture);
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"[ItemShelfPatches] Couldn't set texture with property ID: {ex.Message}");
                }
                
                return material;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[ItemShelfPatches] Error creating custom material: {ex.Message}");
                return null;
            }
        }
#endif
        
        private static bool IlCppBuild()
        {
#if MONO
            return false;
#else
            return true;
#endif
        }
    }
}
