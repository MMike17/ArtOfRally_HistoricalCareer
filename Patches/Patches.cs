using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

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
                    __instance.DriverList = new List<Driver>();

                    SaveManager.SaveSeasonData(__instance);
                    Main.Log("Completed season : " + RallyManager.GetSeasonCode(__instance));
                    __instance.SelectedCar = null;
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
            {
                if (season.SelectedCar != null)
                    SaveManager.SaveSeasonData(season);
                else if (PanelPatcher.currentRally != null)
                    SaveManager.SaveSeasonData(PanelPatcher.currentRally);
            }
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

            Main.Try(nameof(SaveDriverList), () => SeasonPatcher.SaveIfCareer(GameModeManager.CareerManager.GetCurrentSeason()));
        }
    }

    [HarmonyPatch(typeof(GroupTitle), nameof(GroupTitle.ConstructStringUsingCareerData))]
    static class TitlePatcher
    {
        static string lastTitle;

        [HarmonyPrefix]
        static bool FixCareerTitle(GroupTitle __instance)
        {
            if (Main.enabled && GameModeManager.GameMode == GameModeManager.GAME_MODES.CAREER)
            {
                Main.Try(nameof(FixCareerTitle), () =>
                {
                    string divider = Main.GetField<string, GroupTitle>(__instance, "divider", BindingFlags.Instance);
                    string panelName = __instance.GetComponentInParent<Panel>().name;

                    if (Enum.TryParse(panelName.Insert(panelName.Length - 1, "_").ToUpper(), out Car.CarClass carClass))
                    {
                        string group = Main.InvokeMethod<GroupTitle, string>(
                            __instance,
                            "GetStringFromClass",
                            BindingFlags.Instance,
                            new object[] { carClass }
                        );

                        string[] frags = CarrouselUI.GetSelectedInfo();
                        string suffix = string.Concat(new[] { frags[0], " ", divider, " ", frags[1] });
                        Main.SetField(__instance, "suffix", BindingFlags.Instance, suffix);
                        __instance.SetString(lastTitle = string.Concat(new[] { group, " ", divider, " ", suffix }));
                    }
                    else
                        __instance.SetString(lastTitle);
                });

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(StageIntroCinematic), "DoCinematic")]
    static class RallyIntroPatcher
    {
        public static StageIntroCinematic instance;

        static void Prefix(StageIntroCinematic __instance)
        {
            if (Main.enabled)
                instance = __instance;
        }
    }

    [HarmonyPatch(typeof(LeanTween), nameof(LeanTween.alphaText), new[] { typeof(RectTransform), typeof(float), typeof(float) })]
    static class RallyIntroFixer
    {
        [HarmonyPostfix]
        static void FixRallyIntro()
        {
            if (Main.enabled && RallyIntroPatcher.instance != null)
            {
                Main.Try(nameof(FixRallyIntro), () =>
                {
                    bool checkText = RallyIntroPatcher.instance.FirstText.text ==
                        AreaManager.GetAreaStringLocalized(GameModeManager.GetRallyDataCurrentGameMode().CurrentArea).ToLower();

                    if (checkText)
                    {
                        RallySettings settings = RallyManager.GetSettingsFromSeason(
                            GameModeManager.GetSeasonDataCurrentGameMode()
                        );

                        RallyIntroPatcher.instance.FirstText.text = settings.areaName;
                        RallyIntroPatcher.instance.SecondText.text = settings.rallyName;
                        Main.Log("Fixing rally title");
                    }
                });
            }
        }
    }

    [HarmonyPatch(typeof(LoadingScreen), "ConstructCareerSuffix")]
    static class LoadingPatcher
    {
        static void Postfix(LoadingScreen __instance, int year, ref string __result)
        {
            __result = year + " | " + RallyManager.GetSettingsFromSeason(GameModeManager.GetSeasonDataCurrentGameMode()).rallyName;
        }
    }

    [HarmonyPatch(typeof(PreStageScreen), "BuildHeading")]
    static class PreStagePatcher
    {
        static void Postfix(PreStageScreen __instance)
        {
            string area = AreaManager.GetAreaStringLocalized(GameModeManager.GetRallyDataCurrentGameMode().CurrentArea).ToLower();
            RallySettings settings = RallyManager.GetSettingsFromSeason(GameModeManager.GetSeasonDataCurrentGameMode());
            __instance.panelTitleControl.heading_small.fontSize -= 5;

            __instance.panelTitleControl.SetHeading(
                __instance.panelTitleControl.heading_small.text.Replace(area, settings.rallyName)
            );
        }
    }
}