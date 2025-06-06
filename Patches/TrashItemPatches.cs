using HarmonyLib;
using MelonLoader;
using UnityEngine;
using System;
#if MONO
using ScheduleOne;
using ScheduleOne.Trash;
#else
using Il2CppScheduleOne;
using Il2CppScheduleOne.Trash;
#endif

namespace HS_SFW.Patches
{
    public class TrashItemPatches
    {
        private static readonly Color bodyColor = new Color(0.6039f, 0.651f, 0.6314f, 1f);

#if MONO
        [HarmonyPatch(typeof(TrashItem), "Awake")]
#else
        [HarmonyPatch(typeof(Il2CppScheduleOne.Trash.TrashItem), "Awake")]
#endif
        public static class TrashItem_Awake_Patch
        {
            public static void Postfix(TrashItem __instance)
            {
                try
                {
                    if (__instance.ID != "chemicaljug") return;
                    
                    var meshRenderers = __instance.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (var renderer in meshRenderers)
                    {
                        if (renderer.gameObject.name != "Body") continue;
                        renderer.material.color = bodyColor;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error in TrashItem.Awake patch: {ex.Message}");
                    MelonLogger.Error(ex.StackTrace);
                }
            }
        }

#if MONO
        [HarmonyPatch(typeof(TrashItem), "Start")]
#else
        [HarmonyPatch(typeof(Il2CppScheduleOne.Trash.TrashItem), "Start")]
#endif
        public static class TrashItem_Start_Patch
        {
            public static void Postfix(TrashItem __instance)
            {
                try
                {
                    if (__instance.ID != "chemicaljug") return;
                    
                    var meshRenderers = __instance.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (var renderer in meshRenderers)
                    {
                        if (renderer.gameObject.name != "Body") continue;
                        renderer.material.color = bodyColor;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error in TrashItem.Start patch: {ex.Message}");
                    MelonLogger.Error(ex.StackTrace);
                }
            }
        }
    }
} 