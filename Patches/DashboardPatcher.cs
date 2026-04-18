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

                currentSeasonButton = PanelPatcher.GetButtonForSeason(season);
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(currentSeasonButton.gameObject);

                PanelPatcher.SetCarouselState(false);

                PanelPatcher.ShowSeasonButton(currentSeasonButton);
                PanelPatcher.SetCarouselSelection(season);
            });

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
    }
}
