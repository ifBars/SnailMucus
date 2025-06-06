using MelonLoader;
using HarmonyLib;
using HS_SFW.Patches;

[assembly: MelonInfo(typeof(HS_SFW.Core), "SnailMucus", "1.0.0", "Bars")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace HS_SFW
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
            
            LoggerInstance.Msg("Starting initialization of custom assets...");
            
            ItemUIPatches.InitializeCustomIcon();
            EquippableViewmodelPatches.InitializeCustomLabelMaterial();
            ItemShelfPatches.InitializeCustomLabelMaterial();
            var shelfMaterial = ItemShelfPatches.GetLabelMaterial();
            LoggerInstance.Msg($"- Shelf Material: {shelfMaterial != null}, Same as Viewmodel: {shelfMaterial == EquippableViewmodelPatches.GetLabelMaterial()}");
            
            ApplyPatches();
        }
        
        private void ApplyPatches()
        {
            LoggerInstance.Msg("Applying patches...");
            
            try
            {
                var harmony = new HarmonyLib.Harmony("com.bars.hs-sfw");
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                harmony.PatchAll(assembly);
                
                LoggerInstance.Msg("Snail Mucus Patches applied successfully!");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"Error applying patches: {ex.Message}");
                LoggerInstance.Error(ex.StackTrace);
            }
        }
    }
}