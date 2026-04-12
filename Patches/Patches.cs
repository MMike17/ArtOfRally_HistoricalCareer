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

    // TODO : Shortcircuit this to stop end of game from being triggered early
    // SeasonDashboardUI.DisplayUnlocksAndDashboardSequence
    //      (line 251 : Season.Year == GameModeManager.CareerManager.GroupASeason[GameModeManager.CareerManager.GroupASeason.Count - 1].Year)
    // override GameCompleteDataSetup.GoToGameCompleteCutscene

    [HarmonyPatch(typeof(Season))]
    static class SeasonPatcher
    {
        [HarmonyPatch(nameof(Season.MarkSeasonAsComplete))]
        [HarmonyPrefix]
        static bool CompleteCustomSeason(Season __instance)
        {
            if (!Main.enabled)
                return true;

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

            return true;
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
            Main.Try(nameof(InitNewSeason), () => SaveIfCareer(__instance));
        }

        public static void SaveIfCareer(Season season)
        {
            if (GameModeManager.GameMode == GameModeManager.GAME_MODES.CAREER)
                SaveManager.SaveSeasonData(season);
        }
    }

    [HarmonyPatch(typeof(RallyData))]
    static class RallyPatcher
    {
        [HarmonyPatch(nameof(RallyData.IncrementCurrentStageIndex))]
        [HarmonyPostfix]
        static void SaveCurrentStageIndex()
        {
            if (!Main.enabled)
                return;

            Main.Try(
                nameof(SaveCurrentStageIndex),
                () => SeasonPatcher.SaveIfCareer(GameModeManager.GetSeasonDataCurrentGameMode())
            );
        }
    }

    [HarmonyPatch(typeof(CustomButtonCars), "SaveLiveryToCarManager")]
    static class CustomButtonPatcher
    {
        [HarmonyPrefix]
        static bool Prefix() // we did that manually
        {
            return GameModeManager.GameMode != GameModeManager.GAME_MODES.CAREER;
        }
    }

    [HarmonyPatch(typeof(CarChooserManager), nameof(CarChooserManager.InitForClassChooser))]
    static class ChooserPatcher
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            return GameModeManager.GameMode != GameModeManager.GAME_MODES.CAREER;
        }
    }

    [HarmonyPatch(typeof(DriverManager))]
    static class DriverPatcher
    {
        [HarmonyPatch(nameof(DriverManager.CalculateStageRank))]
        [HarmonyPostfix]
        static void SaveStageRank()
        {
            if (!Main.enabled)
                return;

            Main.Try(nameof(SaveStageRank), () => SeasonPatcher.SaveIfCareer(GameModeManager.CareerManager.GetCurrentSeason()));
        }

        [HarmonyPatch(nameof(DriverManager.ResetStageData))]
        [HarmonyPostfix]
        static void SaveResetStage()
        {
            if (!Main.enabled)
                return;

            Main.Try(nameof(SaveResetStage), () => SeasonPatcher.SaveIfCareer(GameModeManager.CareerManager.GetCurrentSeason()));
        }
    }

    [HarmonyPatch(typeof(ResetCareerButton), nameof(ResetCareerButton.ResetCareerSave))]
    static class ResetCareerPatcher
    {
        [HarmonyPostfix]
        static void ResetCustomSaves()
        {
            if (!Main.enabled)
                return;

            Main.Try(nameof(ResetCustomSaves), () =>
            {
                RallyManager.ResetRallySaves();
                PanelPatcher.ResetCareerUIs();

                Main.Log("Reset custom rally saves");
            });
        }
    }

    [HarmonyPatch(typeof(DriverManager), nameof(DriverManager.GenerateDrivers))]
    static class DriverFixer
    {
        [HarmonyPostfix]
        static void SaveDriverList()
        {
            if (!Main.enabled)
                return;

            SeasonPatcher.SaveIfCareer(GameModeManager.CareerManager.GetCurrentSeason());
        }
    }
}