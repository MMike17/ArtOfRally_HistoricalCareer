using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Rewired;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    [HarmonyPatch(typeof(PanelManager))]
    static class PanelPatcher
    {
        const string MAIN_PANEL = "Race";
        const string CAR_PANEL = "Choose Car";
        const string GROUP_PANEL_FORMAT = "Group";
        const string PILOT_NAME_TAG = "Year";
        const string RESTARTS_TAG = "Restarts";
        const string LOCATION_YEAR_TAG = "AiSkill";
        const string RALLY_TAG = "SeasonInfo";

        public static Player playerInput { get; private set; }
        public static Font titleFont { get; private set; }
        public static Font bodyFont { get; private set; }
        public static string submitUIString { get; private set; }
        public static bool inCareer { get; private set; }

        private static RallySettings currentRally;

        private static CareerUI careerUI;
        private static CarClass currentGroup;

        [HarmonyPatch(nameof(PanelManager.AddPanelAddToHistory), new[] { typeof(Panel) })]
        static void Postfix(Panel panel)
        {
            Main.Try("AddPanelAddToHistory Postfix", () =>
            {
                if (!Main.enabled)
                    return;

                Main.Log("Switch to panel " + panel.name);

                if (string.IsNullOrEmpty(submitUIString))
                {
                    BaseInputModule inputModule = EventSystem.current.currentInputModule;
                    submitUIString = Main.GetField<string, RewiredStandaloneInputModule>(
                        inputModule as RewiredStandaloneInputModule,
                        "m_SubmitButton",
                        BindingFlags.Instance
                    );
                    int playerID = Main.GetField<int[], RewiredStandaloneInputModule>(
                        inputModule as RewiredStandaloneInputModule,
                        "playerIds",
                        BindingFlags.Instance
                    )[0];
                    playerInput = ReInput.players.GetPlayer(playerID);
                }

                // main panel
                if (panel.name == MAIN_PANEL)
                {
                    titleFont = panel.transform.GetChild(0).GetChild(0).GetComponentInChildren<Text>().font;
                    bodyFont = panel.GetComponentInChildren<VersionText>().GetComponent<Text>().font;
                }
                else if (panel.name.Contains(GROUP_PANEL_FORMAT)) // group selection panel
                {
                    SeasonDashboardUI ui = panel.transform.parent.GetComponent<SeasonDashboardUI>();
                    HorizontalLayoutGroup layout = panel.GetComponentInChildren<HorizontalLayoutGroup>();
                    ContentSizeFitter fitter = layout.GetComponent<ContentSizeFitter>();

                    // should cut config short
                    if (fitter != null)
                        return;

                    layout.spacing = -10;
                    layout.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

                    // hide buttons
                    foreach (Transform child in layout.transform)
                        child.gameObject.SetActive(false);

                    // generate buttons
                    currentGroup = CarClass.COUNT;

                    if (!Enum.TryParse(panel.name.Replace(GROUP_PANEL_FORMAT, "GROUP_"), out currentGroup))
                    {
                        Main.Error("Couldn't find corresponding group for panel " + panel.name + " (this will crash the mod).");
                        return;
                    }

                    List<CustomButtonSeason> buttons = Main.GetField<List<CustomButtonSeason>, SeasonDashboardUI>(
                        ui,
                        "AllSeasonButtons",
                        BindingFlags.Instance
                    );

                    CustomButtonSeason model = layout.transform.GetChild(0).gameObject.GetComponent<CustomButtonSeason>();
                    List<RallySettings> settings = RallyManager.GetSettingsForClass(currentGroup);
                    settings.ForEach(setting =>
                    {
                        CustomButtonSeason seasonButton = GameObject.Instantiate(model, layout.transform);
                        SetupSeasonButton(seasonButton, setting);

                        buttons.Add(seasonButton);
                    });

                    Main.SetField(ui, "AllSeasonButtons", BindingFlags.Instance, buttons);

                    //add custom UI
                    CarrouselUI carrousel = layout.GetComponent<CarrouselUI>();

                    if (carrousel == null)
                        carrousel = layout.gameObject.AddComponent<CarrouselUI>();

                    carrousel.Reset(settings);
                    inCareer = true;
                }
                else if (panel.name == CAR_PANEL && inCareer) // car selection panel
                {
                    if (careerUI == null)
                        careerUI = Main.SpawnUI(panel.transform.parent);

                    panel.Hide();
                    GameObject diorama = GameObject.Find("Dioramas");
                    diorama.transform.Find("CarChooserDiorama").gameObject.SetActive(false);
                    Main.SetField(
                        GameObject.FindObjectOfType<PanelManager>(),
                        "carChooserManager",
                        BindingFlags.Instance,
                        diorama.GetComponentInChildren<CarChooserManager>()
                    );

                    careerUI.Set(currentRally, rally =>
                    {
                        // have to prepare before applying settings...yeah I know...
                        CarChooserHelper helper = panel.GetComponent<CarChooserHelper>();
                        helper.CarButton.index = currentRally.carIndex;
                        helper.LiveryButton.index = currentRally.liveryIndex;

                        RallyManager.AppyRallySettings(rally);
                        panel.GetComponent<CarChooserHelper>().BeginEvent();

                        // TODO : CarChooserHelper.BeginEvent calls LiveryButton.Save which might be causing the livery glitch
                    });
                }
            });
        }

        [HarmonyPatch(nameof(PanelManager.GoBack))]
        static void GoBack_Postfix() => inCareer = false;

        static void SetupSeasonButton(CustomButtonSeason seasonButton, RallySettings settings)
        {
            // TODO : Setup visuals for locked seasons
            // TODO : Setup visuals for completed seasons
            seasonButton.transform.Find("ClassImage").GetComponent<Image>().sprite = settings.pilotPicture;

            foreach (Text text in seasonButton.GetComponentsInChildren<Text>())
            {
                switch (text.name)
                {
                    case PILOT_NAME_TAG:

                        text.text = settings.pilotName;
                        Main.DelayCall(() => text.fontSize = StyleConstants.Text.Header1.GetFontSize(StyleManager.Instance().UIScale));
                        break;

                    case RESTARTS_TAG:
                        text.enabled = false;
                        break;

                    case LOCATION_YEAR_TAG:
                        text.text = settings.areaName + " (" + settings.season.Year + ")";
                        break;

                    case RALLY_TAG:
                        text.text = settings.rallyName;
                        break;
                }
            }

            seasonButton.gameObject.SetActive(true);
        }

        public static void SelectRally(RallySettings settings) => currentRally = settings;
    }

    // TODO : In progress seasons are not detected by the game

    [HarmonyPatch(typeof(CareerManager))]
    static class CareerPatcher
    {
        [HarmonyPatch(nameof(CareerManager.SetSeasonInProgress))]
        [HarmonyPostfix]
        static void SetSeasonInProgress_Postfix(CareerManager __instance, Season TheSeason)
        {
            Main.Try("SetSeasonInProgress Postfix", () =>
            {
                if (TheSeason != null)
                {
                    // let's just assume the season is OK since it's generated by the mod
                    TheSeason.ResetValuesForStartingNewSeason();
                    TheSeason.Status = Season.STATUS.IN_PROGRESS;

                    // checks in the og code are super weird
                    if (TheSeason != null && TheSeason.SelectedCar != null && TheSeason.SelectedCar.performancePartsCondition != null)
                        TheSeason.SelectedCar.performancePartsCondition.ClampValues();

                    TheSeason.ResetStageInfo();
                    TheSeason.ResetRoadSurface();
                    TheSeason.RemoveDLCCar();

                    Main.SetField(__instance, "CurrentSeasonInProcess", BindingFlags.Instance, TheSeason);
                    SaveManager.SetSeasonStatus(TheSeason, Season.STATUS.IN_PROGRESS);
                    Main.Log("Set season in progress");
                }
            });
        }

        [HarmonyPatch("CheckIfContainsSeasonAndUnlockNextOnes")]
        [HarmonyPostfix]
        static void CheckIfContainsSeasonAndUnlockNextOnes_Postfix(Season season)
        {
            Main.Try("CheckIfContainsSeasonAndUnlockNextOnes Postfix", () =>
            {
                if (season != null)
                {
                    RallyManager.UnlockNextSeason(season);
                    Main.Log("Unlock next season " + RallyManager.GetSeasonCode(season));
                }
            });
        }

        [HarmonyPatch("UnlockSeasonClasses")]
        [HarmonyPostfix]
        static void UnlockSeasonClasses_Postfix()
        {
            // TODO : Do I need to do something there ?
            Main.Log("Unlock next group");
        }

        [HarmonyPatch(nameof(CareerManager.ResetAllInProgressSeasons))]
        [HarmonyPostfix]
        static void ResetAllInProgressSeasons_Postfix()
        {
            // TODO : Do I need this ?
            Main.Log("Reset all in progress seasons");
        }
    }

    [HarmonyPatch(typeof(Season), nameof(Season.MarkSeasonAsComplete))]
    static class SeasonPatcher
    {
        static bool Prefix(Season __instance)
        {
            if (__instance != null)
            {
                // skipping ResetValues (breaks rallies)
                Main.Try("MarkSeasonAsComplete Postfix", () =>
                {
                    __instance.Status = Season.STATUS.COMPLETED;
                    __instance.SelectedCar = null;
                    __instance.DriverList = new List<Driver>();

                    SaveManager.SetSeasonStatus(__instance, Season.STATUS.COMPLETED);
                    Main.Log("Complete season");
                });

                return false;
            }
            else
                return false;
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

    [HarmonyPatch(typeof(SeasonDashboardUI), "HideButtons", new Type[] { typeof(List<CustomButtonSeason>) })]
    static class DashboardFixer
    {
        static bool Prefix(List<CustomButtonSeason> Buttons)
        {
            return (Main.enabled && Buttons != null) || !Main.enabled;
        }
    }
}