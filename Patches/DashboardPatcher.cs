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
    [HarmonyPatch(typeof(SeasonDashboardUI))]
    static class DashboardPatcher
    {
        public static bool skipSetText;

        [HarmonyPatch("ShowNextSeasonInDashboardAnim")]
        [HarmonyPrefix]
        static bool NextSeasonAnimOverride(Season Season, SeasonDashboardUI __instance)
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
            CustomButtonSeason currentSeasonButton = null;
            PanelManager panelManager = null;

            Main.Try(nameof(CustomNextSeasonAnim) + "_1", () =>
            {
                panelManager = UIManager.Instance.PanelManager;
                panelManager.PopAllPanels();
                panelManager.AddPanelAddToHistory(panelManager.MainPanel, false);
                panelManager.AddPanelAddToHistory(panelManager.CareerClassesDashboardPanel, false);

                instance.seasonCompleteProgressUI.UnfocusAllCircles();
                instance.seasonCompleteProgressUI.SetCanvasGroupAlpha(0f);

                panelManager.MoveCameraToCareer();
                panelManager.AddCareerDashboardPanel(season.CarClass, false);

                PanelPatcher.SetupSeasonPanel(instance.transform.Find(season.CarClass.ToString().Replace("GROUP_", "Group")).GetComponent<Panel>());

                currentSeasonButton = PanelPatcher.GetButtonForSeason();
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(currentSeasonButton.gameObject);

                PanelPatcher.SetCarouselState(false);

                PanelPatcher.ShowSeasonButton(currentSeasonButton);
                PanelPatcher.SelectCurrentSeason();
            });

            skipSetText = true;
            yield return currentSeasonButton.SeasonCompleteCoroutine(season);
            PanelPatcher.SetSeasonButtonsState(currentSeasonButton, true);
            PanelPatcher.SetCarouselState(true);

            yield return new WaitForSeconds(0.5f);

            // replaces SeasonDashboardUI.UnlockNewClassSequence
            if (RallyManager.CheckUnlockNextGroup(season))
            {
                CarClass unlockedClass = 0;

                Main.Try(nameof(CustomNextSeasonAnim) + "_2", () =>
                {
                    unlockedClass = season.CarClass + 1;
                    Main.Log("Playing unlock anim for group " + unlockedClass);

                    panelManager.PopPanel();
                    PanelPatcher.SetCarouselState(false);
                    panelManager.SetBackButtonActive(false);
                });

                yield return instance.StartCoroutine(panelManager.CarTrailersPlayer.PlayVideoCoroutine(unlockedClass, false));
                CustomButtonCareerClass classButton = null;

                Main.Try(nameof(CustomNextSeasonAnim) + "_3", () =>
                {
                    if (panelManager.Peek() == panelManager.VideoPlayerPanel)
                        panelManager.GoBack();

                    // replaces SeasonDashboardUI.DoNewClassButtonUnlockAnimation
                    instance.isShowingAnimation = true;
                    classButton = Main.InvokeMethod<SeasonDashboardUI, CustomButtonCareerClass>(
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
                });

                PanelPatcher.SetupGroupPanel(instance.transform.parent.GetChild(1).GetComponent<Panel>());
                yield return instance.StartCoroutine(classButton.ClassUnlockedSequence());
                yield return new WaitForSecondsRealtime(0.5f);

                Main.Try(nameof(CustomNextSeasonAnim) + "_4", () =>
                {
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

                    PanelPatcher.SetCarouselState(true);
                    instance.isShowingAnimation = false;
                });
            }

            panelManager.SetBackButtonActive(true);
        }

        [HarmonyPatch("ShouldShowNewGroupVideo")]
        [HarmonyPostfix]
        static void NewGroupVideoCheck(ref bool __result, Season currentSeason)
        {
            if (Main.enabled)
            {
                bool temp = __result;
                Main.Try(nameof(NewGroupVideoCheck), () => temp = RallyManager.CheckUnlockNextGroup(currentSeason));
                __result = temp;
            }
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
            if (!Main.enabled)
                return;

            Main.Try(nameof(ContinueFix), () =>
            {
                RallyManager.ApplyRallySettings(GameModeManager.CareerManager.GetCurrentSeason());
                SaveManager.LoadSeasonData(GameModeManager.CareerManager.GetCurrentSeason());
            });
        }

        [HarmonyPatch("DisplayUnlocksAndDashboardSequence")]
        [HarmonyPrefix]
        static bool UnlocksFixer(SeasonDashboardUI __instance, Season Season, List<CustomButtonSeason> ButtonsForSeason)
        {
            if (Main.enabled)
                __instance.StartCoroutine(CustomUnlocksAnim(__instance, Season, ButtonsForSeason));

            return !Main.enabled;
        }

        static IEnumerator CustomUnlocksAnim(SeasonDashboardUI instance, Season Season, List<CustomButtonSeason> ButtonsForSeason)
        {
            Main.Log("Start " + nameof(CustomUnlocksAnim));

            CarChooserManager carChooserManager = GameObject.Find("CarChooser").GetComponent<CarChooserManager>();
            Car selectedCar = Season.SelectedCar;
            Car car = CarManager.UnlockCar(Season.Year);
            List<Car> BonusUnlockCars = new List<Car>();

            Main.Try(nameof(CustomUnlocksAnim) + "_1", () =>
            {
                Main.InvokeMethod(instance, "SetSeasonComplete", BindingFlags.Instance, null);
                instance.isShowingAnimation = true;
                GameObject.Find("Dioramas").GetComponent<DioramaManager>();

                if (Season.RestartsRemaining > 0)
                {
                    BonusUnlockCars = CarManager.UnlockBonusForCar(Season.Year);
                    Main.InvokeMethod(instance, "DoAllUnlocksCompleteAchievementCheck", BindingFlags.Instance, null);
                }
            });

            if (car != null)
            {
                Main.Try(nameof(CustomUnlocksAnim) + "_2", () =>
                {
                    UIManager.Instance.PanelManager.DioramaManager.SetCarUnlockDiorama();
                    UIManager.Instance.PanelManager.AddCarUnlockedPanel();
                });

                yield return instance.carUnlockScreen.ShowCarUnlockedAnimation(
                    car,
                    carChooserManager,
                    BonusUnlockCars.Count,
                    false
                );

                instance.seasonCompleteProgressUI.FocusNextCircle();
            }

            if (Season.RestartsRemaining > 0 && BonusUnlockCars.Count > 0)
            {
                Main.Try(nameof(CustomUnlocksAnim) + "_2", () => UIManager.Instance.PanelManager.AddBonusUnlockedPanel());

                yield return instance.bonusUnlockScreen.BonusUnlockAnimation(
                    Season.RestartsRemaining,
                    Season.InitialRestarts,
                    BonusUnlockCars,
                    carChooserManager,
                    Season.CarClass,
                    false
                );
            }

            List<RallySettings> groupASettings = RallyManager.GetSettingsForClass(CarClass.GROUP_A);
            RallySettings lastSettings = groupASettings[groupASettings.Count - 1];

            if (RallyManager.GetSeasonCode(Season) == RallyManager.GetSeasonCode(lastSettings))
            {
                Main.Try(nameof(CustomUnlocksAnim) + "_2", () =>
                {
                    SaveGame.SetInt(SaveConstants.PLAY_COMPLETE_CUTSCENE, 1);
                    SaveGame.SetInt(SaveConstants.GAME_COMPLETE, 1);
                    SaveGame.Save();
                    GameCompleteDataSetup.GoToGameCompleteCutscene(selectedCar);
                });
            }
            else
            {
                yield return new WaitForSecondsRealtime(0.5f);
                SceneLoader.FadeAndHoldForFun(0.3f, 0.5f, 0.75f, true);

                yield return new WaitForSecondsRealtime(0.5f);
                IEnumerator routine = Main.InvokeMethod<SeasonDashboardUI, IEnumerator>(
                    instance,
                    "ShowNextSeasonInDashboardAnim",
                    BindingFlags.Instance,
                    new object[] { Season, ButtonsForSeason }
                );

                yield return instance.StartCoroutine(routine);
            }

            instance.isShowingAnimation = false;
            yield break;
        }
    }
}
