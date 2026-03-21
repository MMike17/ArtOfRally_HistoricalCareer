using System;
using System.Collections.Generic;
using HarmonyLib;

namespace HistoricalCareer
{
    // Patch model
    // [HarmonyPatch(typeof(), nameof())]
    // [HarmonyPatch(typeof(), MethodType.)]
    // static class type_method_Patch
    // {
    // 	static void Prefix()
    // 	{
    // 		//
    // 	}

    //	this will negate the method
    //  	static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //  	{
    //      	foreach (var instruction in instructions)
    //          	yield return new CodeInstruction(OpCodes.Ret);
    //  	}

    // 	static void Postfix()
    // 	{
    // 		//
    // 	}
    // }

    // TODO : In progress seasons are not always detected by the game

    [HarmonyPatch(typeof(Season))]
    static class SeasonPatcher
    {
        [HarmonyPatch(nameof(Season.MarkSeasonAsComplete))]
        [HarmonyPrefix]
        static bool CompleteCustomSeason(Season __instance)
        {
            if (__instance != null)
            {
                // skipping ResetValues (breaks rallies)
                Main.Try(nameof(CompleteCustomSeason), () =>
                {
                    __instance.Status = Season.STATUS.COMPLETED;
                    __instance.SelectedCar = null;
                    __instance.DriverList = new List<Driver>();

                    SaveManager.SaveSeasonData(__instance);
                    Main.Log("Completed season : " + RallyManager.GetSeasonCode(__instance));
                });

                return false;
            }
            else
                return false;
        }

        [HarmonyPatch(nameof(Season.ResetInProgressSeason))]
        [HarmonyPostfix]
        static void InitCurrentSeason(Season __instance)
        {
            Main.Try(nameof(InitCurrentSeason), () => SaveIfCareer(__instance));
        }

        [HarmonyPatch(nameof(Season.ResetValuesForStartingNewSeason))]
        [HarmonyPostfix]
        static void InitNewSeason(Season __instance)
        {
            // TODO : This is creating errors
            Main.Try(nameof(InitNewSeason), () =>
            {
                //Main.Log(Environment.StackTrace);
                SaveIfCareer(__instance);
            });
        }

        public static void SaveIfCareer(Season season)
        {
            if (GameModeManager.GameMode == GameModeManager.GAME_MODES.CAREER)
                SaveManager.SaveSeasonData(season);
        }
    }

    [HarmonyPatch(typeof(CustomButtonCars), "SaveLiveryToCarManager")]
    static class CustomButtonPatcher
    {
        // we did that manually
        static bool Prefix()
        {
            return !PanelPatcher.inCareer;
        }
    }

    [HarmonyPatch(typeof(CarChooserManager), nameof(CarChooserManager.InitForClassChooser))]
    static class ChooserPatcher
    {
        static bool Prefix()
        {
            return !PanelPatcher.inCareer;
        }
    }

    [HarmonyPatch(typeof(SeasonDashboardUI))]
    static class DashboardFixer
    {
        [HarmonyPatch("ShowNextSeasonInDashboardAnim")]
        [HarmonyPrefix]
        static void ShowNextSeasonInDashboardAnim_Prefix(Season Season, List<CustomButtonSeason> ButtonsForSeason)
        {
            // TODO : This animation isn't running at all (has to do with my custom buttons)

            ButtonsForSeason.ForEach(item =>
            {
                Main.Try("test", () =>
                {
                    Main.Log(RallyManager.GetSeasonCode(Main.GetField<Season, CustomButtonSeason>(
                        item,
                        "currentSeason",
                        System.Reflection.BindingFlags.Instance
                    )));
                });
            });
        }

        [HarmonyPatch("ShouldShowNewGroupVideo")]
        [HarmonyPostfix]
        static void NewGroupVideoCheck(ref bool __result, Season currentSeason)
        {
            __result = RallyManager.CheckUnlockNextGroup(currentSeason);
        }

        [HarmonyPatch("HideButtons", new Type[] { typeof(List<CustomButtonSeason>) })]
        [HarmonyPrefix]
        static bool HideOverride(List<CustomButtonSeason> Buttons)
        {
            return (Main.enabled && Buttons != null) || !Main.enabled;
        }

        [HarmonyPatch("GetButtonForSeason")]
        [HarmonyPrefix]
        static bool GetButtonForSeasonFix(Season Season, SeasonDashboardUI __instance)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(DriverManager))]
    static class DriverPatcher
    {
        [HarmonyPatch(nameof(DriverManager.CalculateStageRank))]
        [HarmonyPostfix]
        static void CalculateStageRank_Postfix() => Main.Try("CalculateStageRank Postfix", () => SeasonPatcher.SaveIfCareer(GameModeManager.GetSeasonDataCurrentGameMode()));

        [HarmonyPatch(nameof(DriverManager.ResetStageData))]
        [HarmonyPostfix]
        static void ResetStageData_Postfix() => Main.Try("ResetStageData Postfix", () => SeasonPatcher.SaveIfCareer(GameModeManager.GetSeasonDataCurrentGameMode()));
    }

    [HarmonyPatch(typeof(ResetCareerButton), nameof(ResetCareerButton.ResetCareerSave))]
    static class ResetCareerPatcher
    {
        [HarmonyPostfix]
        static void ResetCustomSaves()
        {
            RallyManager.ResetRallySaves();
            PanelPatcher.forceCareerUpdate = true;

            Main.Log("Reset custom rally saves");
        }
    }
}