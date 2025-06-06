using HarmonyLib;
using MelonLoader;
using UnityEngine;
using System.Reflection;
using System.IO;
using System.Linq;
#if MONO
using ScheduleOne;
using ScheduleOne.Equipping;
using ScheduleOne.Storage;
using ScheduleOne.ItemFramework;
using ScheduleOne.UI.Items;
using ScheduleOne.UI.Shop;
#else
using Il2CppScheduleOne.Equipping;
using Il2CppScheduleOne.Storage;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.UI.Items;
using Il2CppScheduleOne.UI.Shop;
#endif

namespace HS_SFW.Patches
{
    public class ItemUIPatches
    {
        private static Sprite cleanHorseSemenIcon = null;

        public static void InitializeCustomIcon()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using Stream stream = assembly.GetManifestResourceStream("HS_SFW.Assets.HorseSemen_Icon.png");
                if (stream == null)
                {
                    MelonLogger.Error("Failed to find embedded resource: HorseSemen_Icon.png");
                    return;
                }

                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(bytes))
                {
                    cleanHorseSemenIcon = Sprite.Create(
                        texture, 
                        new Rect(0, 0, texture.width, texture.height), 
                        new Vector2(0.5f, 0.5f));
                }
                else
                {
                    MelonLogger.Error("Failed to load custom snail mucus icon texture");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error initializing custom snail mucus icon: {ex.Message}");
                MelonLogger.Error(ex.StackTrace);
            }
        }

#if MONO
        [HarmonyPatch(typeof(ItemUI), "UpdateUI")]
#else
        [HarmonyPatch(typeof(Il2CppScheduleOne.UI.Items.ItemUI), "UpdateUI")]
#endif
        public static class ItemUI_UpdateUI_Patch
        {
            public static void Postfix(ItemUI __instance)
            {
                try
                {
                    if (__instance.itemInstance is not { ID: "horsesemen" } ||
                        cleanHorseSemenIcon == null) return;
                    __instance.IconImg.sprite = cleanHorseSemenIcon;
                    __instance.IconImg.overrideSprite = cleanHorseSemenIcon;
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error in ItemUI.UpdateUI patch: {ex.Message}");
                }
            }
        }

#if MONO
        [HarmonyPatch(typeof(ItemInstance), "get_Name")]
#else
        [HarmonyPatch(typeof(Il2CppScheduleOne.ItemFramework.ItemInstance), "get_Name")]
#endif
        public static class ItemInstance_Name_Patch
        {
            public static bool Prefix(ItemInstance __instance, ref string __result)
            {
                try
                {
                    if (__instance.ID == "horsesemen")
                    {
                        __result = "Snail Mucus";
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error in ItemInstance.get_Name patch: {ex.Message}");
                }
                return true;
            }
        }

#if MONO
        [HarmonyPatch(typeof(ItemInstance), "get_Description")]
#else
        [HarmonyPatch(typeof(Il2CppScheduleOne.ItemFramework.ItemInstance), "get_Description")]
#endif
        public static class ItemInstance_Description_Patch
        {
            public static bool Prefix(ItemInstance __instance, ref string __result)
            {
                try
                {
                    if (__instance.ID == "horsesemen")
                    {
                        __result = "A viscous secretion from a rare snail species. NOT RECOMMENDED FOR CONSUMPTION";
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error in ItemInstance.get_Description patch: {ex.Message}");
                }
                return true;
            }
        }

#if MONO
        [HarmonyPatch(typeof(ItemInfoContent), "Initialize", new[] { typeof(ItemInstance) })]
#else
        [HarmonyPatch(typeof(Il2CppScheduleOne.UI.Items.ItemInfoContent), "Initialize", new[] { typeof(Il2CppScheduleOne.ItemFramework.ItemInstance) })]
#endif
        public static class ItemInfoContent_Initialize_Patch
        {
            public static void Postfix(ItemInfoContent __instance, ItemInstance instance)
            {
                try
                {
                    if (instance is not { ID: "horsesemen" }) return;
                    __instance.NameLabel.text = "Snail Mucus";
                    __instance.DescriptionLabel.text = "A viscous secretion from a rare snail species. NOT RECOMMENDED FOR CONSUMPTION";
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error in ItemInfoContent.Initialize postfix: {ex.Message}");
                }
            }
        }

#if MONO
        [HarmonyPatch(typeof(ListingUI), "Initialize")]
#else
        [HarmonyPatch(typeof(Il2CppScheduleOne.UI.Shop.ListingUI), "Initialize")]
#endif
        public static class ListingUI_Initialize_Patch
        {
            public static void Postfix(ListingUI __instance, ShopListing listing)
            {
                try
                {
                    if (listing == null || listing.Item == null || listing.Item.ID != "horsesemen") return;
                    if (cleanHorseSemenIcon != null)
                    {
                        __instance.Icon.sprite = cleanHorseSemenIcon;
                    }
                        
                    __instance.NameLabel.text = "Snail Mucus";
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error in ListingUI.Initialize patch: {ex.Message}");
                }
            }
        }

#if MONO
        [HarmonyPatch(typeof(ShopInterfaceDetailPanel), "Open")]
#else
        [HarmonyPatch(typeof(Il2CppScheduleOne.UI.Shop.ShopInterfaceDetailPanel), "Open")]
#endif
        public static class ShopDetailPanel_Open_Patch
        {
            public static void Postfix(ShopInterfaceDetailPanel __instance, ListingUI _listing)
            {
                try
                {
                    if (_listing == null || _listing.Listing == null || _listing.Listing.Item == null ||
                        _listing.Listing.Item.ID != "horsesemen") return;
                    if (__instance.DescriptionLabel != null)
                    {
                        __instance.DescriptionLabel.text = "A viscous secretion from a rare snail species. NOT RECOMMENDED FOR CONSUMPTION";
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error in ShopInterfaceDetailPanel.Open patch: {ex.Message}");
                }
            }
        }

#if MONO
        [HarmonyPatch(typeof(StorableItemDefinition), "GetDefaultInstance")]
#else
        [HarmonyPatch(typeof(Il2CppScheduleOne.ItemFramework.StorableItemDefinition), "GetDefaultInstance")]
#endif
        public static class ItemDefinition_GetDefaultInstance_Patch
        {
            public static void Prefix(StorableItemDefinition __instance)
            {
                try
                {
                    if (__instance == null || __instance.ID != "horsesemen") return;
                    __instance.Name = "Snail Mucus";
                    __instance.Description = "A viscous secretion from a rare snail species. NOT RECOMMENDED FOR CONSUMPTION";
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error in ItemDefinition.GetDefaultInstance patch: {ex.Message}");
                }
            }
        }

#if MONO
        [HarmonyPatch(typeof(Registry), "_GetItem")]
#else
        [HarmonyPatch(typeof(Il2CppScheduleOne.Registry), "_GetItem")]
#endif
        public static class Registry_GetItem_Patch
        {
            public static void Postfix(ref ItemDefinition __result)
            {
                try
                {
                    if (__result == null || __result.ID != "horsesemen") return;
                    __result.Name = "Snail Mucus";
                    __result.Description = "A viscous secretion from a rare snail species. NOT RECOMMENDED FOR CONSUMPTION";
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error in Registry._GetItem patch: {ex.Message}");
                }
            }
        }
    }
}