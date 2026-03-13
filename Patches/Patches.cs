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
        static bool MarkSeasonAsComplete_Prefix(Season __instance)
        {
            if (__instance != null)
            {
                // skipping ResetValues (breaks rallies)
                Main.Try("MarkSeasonAsComplete Postfix", () =>
                {
                    __instance.Status = Season.STATUS.COMPLETED;
                    __instance.SelectedCar = null;
                    __instance.DriverList = new List<Driver>();

                    SaveManager.SaveSeasonData(__instance);
                    Main.Log("Complete season " + RallyManager.GetSeasonCode(__instance));
                });

                return false;
            }
            else
                return false;
        }

        [HarmonyPatch(nameof(Season.ResetInProgressSeason))]
        [HarmonyPostfix]
        static void ResetInProgressSeason_Postfix(Season __instance) => Main.Try("ResetInProgressSeason Postfix", () => SaveIfCareer(__instance));

        [HarmonyPatch(nameof(Season.ResetValuesForStartingNewSeason))]
        [HarmonyPostfix]
        static void ResetValuesForStartingNewSeason_Postfix(Season __instance) => Main.Try("ResetValuesForStartingNewSeason Postfix", () => SaveIfCareer(__instance)); // TODO : This is creating errors

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
        static bool Prefix() => !PanelPatcher.inCareer;
    }

    [HarmonyPatch(typeof(CarChooserManager), nameof(CarChooserManager.InitForClassChooser))]
    static class ChooserPatcher
    {
        static bool Prefix() => !PanelPatcher.inCareer;
    }

    [HarmonyPatch(typeof(SeasonDashboardUI))]
    static class DashboardFixer
    {
        [HarmonyPatch("HideButtons", new Type[] { typeof(List<CustomButtonSeason>) })]
        [HarmonyPrefix]
        static bool HideButtons_Prefix(List<CustomButtonSeason> Buttons)
        {
            return (Main.enabled && Buttons != null) || !Main.enabled;
        }

        [HarmonyPatch("GetButtonForSeason")]
        [HarmonyPrefix]
        static bool GetButtonForSeason_Prefix(Season Season, SeasonDashboardUI __instance)
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


}