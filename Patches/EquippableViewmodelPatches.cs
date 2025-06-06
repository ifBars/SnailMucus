using HarmonyLib;
using MelonLoader;
using UnityEngine;
using System.Reflection;
using System.IO;
using System;
#if MONO
using ScheduleOne.Equipping;
using ScheduleOne.Storage;
using ScheduleOne.ItemFramework;
#else
using Il2CppScheduleOne.Equipping;
using Il2CppScheduleOne.Storage;
using Il2CppScheduleOne.ItemFramework;
using Il2CppInterop.Runtime;
#endif

namespace HS_SFW.Patches
{
    public class EquippableViewmodelPatches
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
                var assembly = Assembly.GetExecutingAssembly();
                using Stream stream = assembly.GetManifestResourceStream("HS_SFW.Assets.HorseSemenLabel_icon.png");
                if (stream == null)
                {
                    MelonLogger.Error("[EquippableViewmodelPatches] Failed to find embedded resource: HorseSemenLabel_icon.png");
                    return;
                }

                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                bool loadSuccess = texture.LoadImage(bytes);
                
                if (loadSuccess)
                {
                    Material newMaterial = null;
                    
                    var shader1 = Shader.Find("Universal Render Pipeline/Lit");
                    if (shader1 != null)
                    {
                        newMaterial = new Material(shader1);
                    }
                    
                    if (newMaterial == null)
                    {
                        var shader2 = Shader.Find("Standard");
                        if (shader2 != null)
                        {
                            newMaterial = new Material(shader2);
                        }
                    }
                    
                    if (newMaterial == null)
                    {
                        var shader3 = Shader.Find("Unlit/Texture");
                        if (shader3 != null)
                        {
                            newMaterial = new Material(shader3);
                        }
                    }
                    
                    if (newMaterial == null)
                    {
                        var shader4 = Shader.Find("Default-Diffuse");
                        if (shader4 != null)
                        {
                            newMaterial = new Material(shader4);
                        }
                    }
                    
                    if (newMaterial == null)
                    {
                        newMaterial = new Material(Shader.Find("Diffuse"));
                    }
                    
                    if (newMaterial != null)
                    {
                        newMaterial.mainTexture = texture;
                        
                        int mainTexID = Shader.PropertyToID("_MainTex");
                        newMaterial.SetTexture(mainTexID, texture);
                        
                        cleanLabelMaterial = newMaterial;
                    }
                    else
                    {
                        MelonLogger.Error("[EquippableViewmodelPatches] Failed to create material - no suitable shader found");
                    }
                }
                else
                {
                    MelonLogger.Error("[EquippableViewmodelPatches] Failed to load custom horsesemen label texture");
                }
                
                if (cleanLabelMaterial == null)
                {
                    MelonLogger.Error("[EquippableViewmodelPatches] Material is still null after initialization!");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[EquippableViewmodelPatches] Error initializing custom horsesemen label material: {ex.Message}");
                MelonLogger.Error(ex.StackTrace);
            }
        }

        [HarmonyPatch]
        public static class EquippableViewmodelParameterCountPatch
        {
            public static MethodBase TargetMethod()
            {
#if MONO
                var type = typeof(Equippable_Viewmodel);
#else
                var type = typeof(Il2CppScheduleOne.Equipping.Equippable_Viewmodel);
#endif
                foreach (var method in type.GetMethods())
                {
                    if (method.Name == "Equip" && method.GetParameters().Length == 1)
                    {
                        return method;
                    }
                }
                
                MelonLogger.Error("[EquippableViewmodelPatches] Could not find method");
                return null;
            }
            
            public static void Postfix(Component __instance, object __0)
            {
                try
                {
                    
                    PropertyInfo idProperty = __0?.GetType().GetProperty("ID");
                    if (idProperty == null) return;
                    string id = idProperty.GetValue(__0, null) as string;
                    if (id != "horsesemen") return;
                    var meshRenderers = __instance.GetComponentsInChildren<MeshRenderer>();
                    
                    bool labelFound = false;
                    foreach (var renderer in meshRenderers)
                    {
                        if (renderer.gameObject.name != "Label") continue;
                        labelFound = true;
                        if (cleanLabelMaterial == null) continue;
                        renderer.material = cleanLabelMaterial;
                        break;
                    }
                    
                    if (!labelFound) MelonLogger.Warning("[EquippableViewmodelPatches] No Label object found in mesh renderers");
                    bool bodyFound = false;
                    foreach (var renderer in meshRenderers)
                    {
                        if (renderer.gameObject.name != "Body") continue;
                        
                        bodyFound = true;
                        renderer.material.color = bodyColor;
                        break;
                    }
                    
                    if (!bodyFound) MelonLogger.Warning("[EquippableViewmodelPatches] No Body object found in mesh renderers");
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[EquippableViewmodelPatches] Error in parameter count patch: {ex.Message}");
                    MelonLogger.Error(ex.StackTrace);
                }
            }
        }

        [HarmonyPatch]
        public static class EquippableViewmodelGenericPatch
        {
            public static MethodBase TargetMethod()
            {
#if MONO
                var type = typeof(Equippable_Viewmodel);
#else
                var type = typeof(Il2CppScheduleOne.Equipping.Equippable_Viewmodel);
#endif
                foreach (var method in type.GetMethods())
                {
                    if (method.Name == "Equip" && method.GetParameters().Length == 1)
                    {
                        return method;
                    }
                }
                
                MelonLogger.Error("[EquippableViewmodelPatches] Could not find Equip method with one parameter");
                return null;
            }
            
            public static void Postfix(object __instance, object __0)
            {
                try
                {
                    MelonLogger.Msg($"[EquippableViewmodelPatches] GenericPatch called, IL2CPP: {IlCppBuild()}");
                    
                    var component = __instance as Component;
                    if (component == null)
                    {
                        MelonLogger.Error("[EquippableViewmodelPatches] Instance is not a Component");
                        return;
                    }
                    
                    string itemId = null;
                    try
                    {
                        var idProp = __0?.GetType().GetProperty("ID");
                        if (idProp != null)
                        {
                            itemId = idProp.GetValue(__0, null) as string;
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"[EquippableViewmodelPatches] Error getting ID: {ex.Message}");
                    }
                    
                    if (itemId != "horsesemen") return;

                    var meshRenderers = component.GetComponentsInChildren<MeshRenderer>();
                    
                    bool labelFound = false;
                    foreach (var renderer in meshRenderers)
                    {
                        if (renderer.gameObject.name != "Label") continue;
                        labelFound = true;
                        if (cleanLabelMaterial != null) renderer.material = cleanLabelMaterial;
                        break;
                    }
                    
                    if (!labelFound)
                    {
                        MelonLogger.Warning("[EquippableViewmodelPatches] No Label object found in mesh renderers");
                    }
                    
                    bool bodyFound = false;
                    foreach (var renderer in meshRenderers)
                    {
                        if (renderer.gameObject.name != "Body") continue;
                        
                        bodyFound = true;
                        renderer.material.color = bodyColor;
                        break;
                    }
                    
                    if (!bodyFound)
                    {
                        MelonLogger.Warning("[EquippableViewmodelPatches] No Body object found in mesh renderers");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[EquippableViewmodelPatches] Error in generic patch: {ex.Message}");
                    MelonLogger.Error(ex.StackTrace);
                }
            }
        }

#if MONO
        [HarmonyPatch(typeof(Equippable_Viewmodel), "Equip")]
#else
        [HarmonyPatch(typeof(Il2CppScheduleOne.Equipping.Equippable_Viewmodel), "Equip")]
#endif
        public static class EquippableViewmodelEquipPatch
        {
#if MONO
            public static void Postfix(Equippable_Viewmodel __instance, ItemInstance item)
#else
            public static void Postfix(Il2CppScheduleOne.Equipping.Equippable_Viewmodel __instance, Il2CppScheduleOne.ItemFramework.ItemInstance item)
#endif
            {
                try
                {
                    StorableItemInstance storableItemInstance = item as StorableItemInstance;
                    if (storableItemInstance?.ID != "horsesemen") return;
                    
                    var meshRenderers = __instance.gameObject.GetComponentsInChildren<MeshRenderer>();
                    bool labelFound = false;
                    foreach (var renderer in meshRenderers)
                    {
                        if (renderer.gameObject.name != "Label") continue;
                        labelFound = true;
                        if (cleanLabelMaterial == null) continue;
                        renderer.material = cleanLabelMaterial;
                    }
                    
                    if (!labelFound) MelonLogger.Warning("[EquippableViewmodelPatches] No Label object found in mesh renderers");
                    
                    bool bodyFound = false;
                    foreach (var renderer in meshRenderers)
                    {
                        if (renderer.gameObject.name != "Body") continue;
                        
                        bodyFound = true;
                        renderer.material.color = bodyColor;
                        break;
                    }
                    
                    if (!bodyFound)
                    {
                        MelonLogger.Warning("[EquippableViewmodelPatches] No Body object found in mesh renderers");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[EquippableViewmodelPatches] Error in equip patch: {ex.Message}");
                    MelonLogger.Error(ex.StackTrace);
                }
            }
        }

#if !MONO
        [HarmonyPatch(typeof(Il2CppScheduleOne.Equipping.Equippable_Viewmodel), "Equip")]
        public static class EquippableViewmodelEquipIl2CppPatch
        {
            public static void Postfix(Il2CppScheduleOne.Equipping.Equippable_Viewmodel __instance, Il2CppScheduleOne.ItemFramework.ItemInstance item)
            {
                try
                {
                    bool isHorseSemen = false;
                    try 
                    {
                        var idProp = item?.GetType().GetProperty("ID");
                        if (idProp != null)
                        {
                            string id = idProp.GetValue(item, null) as string;
                            isHorseSemen = id == "horsesemen";
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"[EquippableViewmodelPatches] Error accessing ID property: {ex.Message}");
                    }
                    
                    if (!isHorseSemen) return;
                    Material newMaterial = CreateCustomMaterial();
                    
                    if (newMaterial == null)
                    {
                        MelonLogger.Error("[EquippableViewmodelPatches] Failed to create new material");
                        return;
                    }
                    
                    Transform labelTransform = null;
                    Transform[] allChildren = __instance.gameObject.GetComponentsInChildren<Transform>(true);
                    
                    foreach (var child in allChildren)
                    {
                        if (child.gameObject.name != "Label") continue;
                        labelTransform = child;
                        break;
                    }
                    
                    if (labelTransform != null)
                    {
                        MeshRenderer labelRenderer = labelTransform.GetComponent<MeshRenderer>();
                        if (labelRenderer != null)
                        {
                            try
                            {
                                labelRenderer.material = newMaterial;
                                if (labelRenderer.material.mainTexture == null)
                                {
                                    MelonLogger.Warning("[EquippableViewmodelPatches] Material has no texture after assignment");
                                }
                            }
                            catch (Exception ex)
                            {
                                MelonLogger.Error($"[EquippableViewmodelPatches] Error assigning material: {ex.Message}");
                            }
                        }
                        else
                        {
                            MelonLogger.Warning("[EquippableViewmodelPatches] Label transform doesn't have a MeshRenderer component");
                        }
                    }
                    else
                    {
                        MelonLogger.Warning("[EquippableViewmodelPatches] Couldn't find Label transform in hierarchy");
                    }
                    
                    Transform bodyTransform = null;
                    foreach (var child in allChildren)
                    {
                        if (child.gameObject.name != "Body") continue;
                        bodyTransform = child;
                        break;
                    }
                    
                    if (bodyTransform != null)
                    {
                        MeshRenderer bodyRenderer = bodyTransform.GetComponent<MeshRenderer>();
                        if (bodyRenderer == null) return;
                        try
                        {
                            bodyRenderer.material.color = bodyColor;
                        }
                        catch (Exception ex)
                        {
                            MelonLogger.Error($"[EquippableViewmodelPatches] Error changing body color: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[EquippableViewmodelPatches] Error in IL2CPP-specific patch: {ex.Message}");
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
                    MelonLogger.Error("[EquippableViewmodelPatches] Failed to find embedded resource for new material");
                    return null;
                }

                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                bool loadSuccess = texture.LoadImage(bytes);
                
                if (!loadSuccess)
                {
                    MelonLogger.Error("[EquippableViewmodelPatches] Failed to load texture for new material");
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
                    MelonLogger.Error("[EquippableViewmodelPatches] No suitable shader found for new material");
                    return null;
                }
                
                Material material = new Material(shader);
                material.name = "CustomHorseSemenLabel_" + DateTime.Now.Ticks;
                material.mainTexture = texture;
                
                try
                {
                    int mainTexID = Shader.PropertyToID("_MainTex");
                    material.SetTexture(mainTexID, texture);
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"[EquippableViewmodelPatches] Couldn't set texture with property ID: {ex.Message}");
                }
                
                return material;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[EquippableViewmodelPatches] Error creating custom material: {ex.Message}");
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
