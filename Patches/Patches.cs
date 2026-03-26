using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

using static Car;

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

    [HarmonyPatch(typeof(CustomButtonCars), "SaveLiveryToCarManager")]
    static class CustomButtonPatcher
    {
        [HarmonyPrefix]
        static bool Prefix() // we did that manually
        {
            return !PanelPatcher.inCareer;
        }
    }

    [HarmonyPatch(typeof(CarChooserManager), nameof(CarChooserManager.InitForClassChooser))]
    static class ChooserPatcher
    {
        [HarmonyPrefix]
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
        static bool NextSeaconAnimOverride(Season Season, SeasonDashboardUI __instance)
        {
            if (!Main.enabled)
                return true;

            __instance.StartCoroutine(CustomNextSeasonAnim(__instance, Season));
            return false;
        }

        // replaces SeasonDashboardUI.ShowNextSeasonInDashboardAnim
        private static IEnumerator CustomNextSeasonAnim(SeasonDashboardUI instance, Season season)
        {
            Main.Log("Starting custom next season anim");

            PanelManager panelManager = UIManager.Instance.PanelManager;
            panelManager.PopAllPanels();
            panelManager.AddPanelAddToHistory(panelManager.MainPanel, false);
            panelManager.AddPanelAddToHistory(panelManager.CareerClassesDashboardPanel, false);

            instance.seasonCompleteProgressUI.UnfocusAllCircles();
            instance.seasonCompleteProgressUI.SetCanvasGroupAlpha(0f);

            panelManager.MoveCameraToCareer();
            panelManager.AddCareerDashboardPanel(season.CarClass, false);

            PanelPatcher.SetupSeasonPanel(instance.transform.Find(season.CarClass.ToString().Replace("GROUP_", "Group")).GetComponent<Panel>());

            CustomButtonSeason currentSeasonButton = PanelPatcher.GetButtonForSeason(season);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(currentSeasonButton.gameObject);

            PanelPatcher.SetCarouselState(false);

            //Navigation nav = currentSeasonButton.navigation;
            //nav.mode = Navigation.Mode.None;
            //currentSeasonButton.navigation = nav;

            PanelPatcher.ShowSeasonButton(currentSeasonButton);
            PanelPatcher.SetCarouselSelection(season);
            yield return currentSeasonButton.SeasonCompleteCoroutine(season);
            PanelPatcher.SetSeasonButtonsState(currentSeasonButton, true);

            //nav.mode = Navigation.Mode.Explicit;
            PanelPatcher.SetCarouselState(true);

            yield return new WaitForSeconds(0.5f);

            // replaces SeasonDashboardUI.UnlockNewClassSequence
            if (RallyManager.CheckUnlockNextGroup(season))
            {
                CarClass unlockedClass = season.CarClass + 1;
                Main.Log("Playing unlock anim for group " + unlockedClass);

                panelManager.PopPanel();
                PanelPatcher.SetCarouselState(false);
                panelManager.SetBackButtonActive(false);

                yield return instance.StartCoroutine(panelManager.CarTrailersPlayer.PlayVideoCoroutine(unlockedClass, false));

                if (panelManager.Peek() == panelManager.VideoPlayerPanel)
                    panelManager.GoBack();

                // replaces SeasonDashboardUI.DoNewClassButtonUnlockAnimation
                instance.isShowingAnimation = true;
                CustomButtonCareerClass classButton = Main.InvokeMethod<SeasonDashboardUI, CustomButtonCareerClass>(
                    instance,
                    "GetClassButtonFromEnum",
                    BindingFlags.Instance,
                    new object[] { unlockedClass }
                );
                Main.InvokeMethod(instance, "RefreshButtons", BindingFlags.Instance, null);
                EventSystem.current.SetSelectedGameObject(null);

                PanelPatcher.SetSeasonButtonsState(null, false);
                PanelPatcher.SetCarouselState(false);
                panelManager.SetBackButtonActive(false);

                //nav = classButton.navigation;
                //nav.mode = Navigation.Mode.None;
                //classButton.navigation = nav;

                yield return instance.StartCoroutine(classButton.ClassUnlockedSequence());
                yield return new WaitForSecondsRealtime(0.5f);

                Main.InvokeMethod(
                    instance,
                    "ShowClassButtons",
                    BindingFlags.Instance,
                    new object[] {
                        Main.GetField<List<CustomButtonCareerClass>, SeasonDashboardUI>(instance, "ClassButtons", BindingFlags.Instance),
                        unlockedClass
                    }
                );

                panelManager.SetBackButtonActive(true);
                classButton.interactable = true;
                EventSystem.current.SetSelectedGameObject(classButton.gameObject);

                //nav.mode = Navigation.Mode.Explicit;
                //classButton.navigation = nav;
                PanelPatcher.SetCarouselState(true);

                instance.isShowingAnimation = false;
            }

            panelManager.SetBackButtonActive(true);
        }

        [HarmonyPatch("ShouldShowNewGroupVideo")]
        [HarmonyPostfix]
        static void NewGroupVideoCheck(ref bool __result, Season currentSeason)
        {
            if (Main.enabled)
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
            return !Main.enabled;
        }

        [HarmonyPatch(nameof(SeasonDashboardUI.ContinueSeason))]
        [HarmonyPrefix]
        static void ContinueFix()
        {
            if (Main.enabled)
                RallyManager.ApplyRallySettings(GameModeManager.CareerManager.GetCurrentSeason());
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

            Main.Try(nameof(SaveStageRank), () =>
            {
                if (GameModeManager.GameMode == GameModeManager.GAME_MODES.CAREER)
                    SeasonPatcher.SaveIfCareer(GameModeManager.CareerManager.GetCurrentSeason());
            });
        }

        [HarmonyPatch(nameof(DriverManager.ResetStageData))]
        [HarmonyPostfix]
        static void SaveResetStage()
        {
            if (!Main.enabled)
                return;

            Main.Try(nameof(SaveResetStage), () =>
            {
                if (GameModeManager.GameMode == GameModeManager.GAME_MODES.CAREER)
                    SeasonPatcher.SaveIfCareer(GameModeManager.CareerManager.GetCurrentSeason());
            });
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