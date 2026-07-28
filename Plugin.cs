using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace HsNoPets
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.reqvam.hsnopets";
        public const string PluginName = "No Pets";
        public const string PluginVersion = "1.0.0";

        private void Awake()
        {
            PetPatches.Log = Logger;

            // Config file name must match the assembly name for Firestone's Mod Manager
            ConfigFile fsConfig = new ConfigFile(
                Path.Combine(Paths.ConfigPath, "HsNoPets.cfg"), true);

            fsConfig.Bind("General", "Name", PluginName);
            fsConfig.Bind("General", "Guid", PluginGuid);
            fsConfig.Bind("General", "Version", PluginVersion);
            fsConfig.Bind("General", "DownloadLink", "https://github.com/reqvamhs/No-Pets");
            fsConfig.Bind("General", "Description",
                "Hides opponent battlefield pets during games.");

            PetPatches.Enabled = fsConfig.Bind("Features", "HidePets", true,
                "Master toggle for the plugin.");
            PetPatches.HideOpponentPet = fsConfig.Bind("Features", "HideOpponentPet", true,
                "Hide the opponent pet and its corner platform during games.");
            PetPatches.HideOwnPet = fsConfig.Bind("Features", "HideOwnPet", false,
                "Also hide your own pet and its corner platform during games.");

            new Harmony(PluginGuid).PatchAll();
            PetPatches.Log.LogInfo($"{PluginName} {PluginVersion} loaded.");
        }
    }

    /// <summary>
    /// In-game pets: PetControllerBoard.CreatePetObject honors the game's own creation
    /// blocker (IsCreationBlocked) as its first check, so forcing the getter to true makes
    /// every creation attempt a clean no-op; the corner platform is a separate corner-spell
    /// replacement driven by per-side contexts, zeroed before application.
    /// </summary>
    public static class PetPatches
    {
        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<bool> HideOpponentPet;
        internal static ConfigEntry<bool> HideOwnPet;

        internal static bool On(ConfigEntry<bool> e) => e != null && e.Value;

        internal static bool ShouldHide(int controllerPlayerId)
        {
            if (!On(Enabled))
                return false;
            bool hideOpp = On(HideOpponentPet);
            bool hideOwn = On(HideOwnPet);
            if (!hideOpp && !hideOwn)
                return false;
            if (hideOpp && hideOwn)
                return true;
            try
            {
                Player friendly = GameState.Get()?.GetFriendlySidePlayer();
                if (friendly != null)
                {
                    bool isOwn = friendly.GetPlayerId() == controllerPlayerId;
                    return isOwn ? hideOwn : hideOpp;
                }
            }
            catch (System.Exception e)
            {
                Log?.LogError($"Pet side check failed: {e}");
            }
            return false;
        }

        [HarmonyPatch(typeof(PetControllerBoard), "get_IsCreationBlocked")]
        public static class BlockPetCreationPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(PetControllerBoard __instance, ref bool __result)
            {
                try
                {
                    // Gameplay only: Board-family controllers also serve collection previews.
                    if (SceneMgr.Get() == null || SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY)
                        return true;
                    if (ShouldHide(__instance.PlayerId))
                    {
                        __result = true;
                        return false;
                    }
                }
                catch (System.Exception e)
                {
                    Log?.LogError($"BlockPetCreation prefix failed: {e}");
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(CornerSpellReplacementManager), "UpdateCornerSpellReplacements")]
        public static class BlockPetCornerPatch
        {
            [HarmonyPrefix]
            public static void Prefix(
                ref CornerReplacementContext cornerReplacementContextFriendly,
                ref CornerReplacementContext cornerReplacementContextOpposing)
            {
                try
                {
                    if (!On(Enabled))
                        return;
                    // Gameplay only: the same manager also builds collection previews.
                    if (SceneMgr.Get() == null || SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY)
                        return;
                    if (On(HideOwnPet))
                        cornerReplacementContextFriendly.cornerReplacementPetType = CornerReplacementPetType.NONE;
                    if (On(HideOpponentPet))
                        cornerReplacementContextOpposing.cornerReplacementPetType = CornerReplacementPetType.NONE;
                }
                catch (System.Exception e)
                {
                    Log?.LogError($"BlockPetCorner prefix failed: {e}");
                }
            }
        }
    }
}
