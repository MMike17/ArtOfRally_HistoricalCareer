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
    [HarmonyPatch(typeof(PanelManager))]
    internal static class PanelPatcher
    {
        const string MAIN_PANEL = "Race";
        const string CAR_PANEL = "Choose Car";
        const string GROUP_PANEL_FORMAT = "Group";
        const string STANDING_TAG = "OverallStanding";
        const string PILOT_NAME_TAG = "Year";
        const string RESTARTS_TAG = "Restarts";
        const string LOCATION_YEAR_TAG = "AiSkill";
        const string RALLY_TAG = "SeasonInfo";
        const float LOCK_SIZE = 0.7f;

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
            Main.InvokeMethod(seasonButton, "AssignProperties", BindingFlags.Instance, null);
            Image wreathIcon = Main.GetField<Image, CustomButtonSeason>(seasonButton, "WreathImage", BindingFlags.Instance);
            Image stageWins = Main.GetField<Image, CustomButtonSeason>(seasonButton, "StageWinsFill", BindingFlags.Instance);
            Image lockedIcon = Main.GetField<Image, CustomButtonSeason>(seasonButton, "LockedIcon", BindingFlags.Instance);

            wreathIcon.enabled = settings.season.Status == Season.STATUS.COMPLETED;
            stageWins.fillAmount = (float)settings.season.GetSeasonStageWinsPercentage() / 100f; // TODO : Do I need to save this separately ?
            lockedIcon.enabled = settings.season.Status == Season.STATUS.LOCKED;

            Image pilotImage = seasonButton.transform.Find("ClassImage").GetComponent<Image>();
            pilotImage.sprite = settings.pilotPicture;
            RectTransform buttonSkin = seasonButton.transform.GetChild(0).GetComponent<RectTransform>();

            if (settings.season.Status == Season.STATUS.LOCKED)
            {
                lockedIcon.preserveAspect = true;
                lockedIcon.transform.SetParent(pilotImage.transform);

                lockedIcon.rectTransform.anchorMin = Vector2.one * (1 - LOCK_SIZE);
                lockedIcon.rectTransform.anchorMax = Vector2.one * LOCK_SIZE;
                lockedIcon.rectTransform.offsetMin = lockedIcon.rectTransform.offsetMax = Vector2.zero;

                lockedIcon.transform.localScale = Vector3.one;
                lockedIcon.transform.localPosition = Vector3.zero;
            }

            foreach (Text text in seasonButton.GetComponentsInChildren<Text>())
            {
                switch (text.name)
                {
                    case STANDING_TAG:
                        text.text = settings.season.Status == Season.STATUS.COMPLETED ?
                            settings.season.GetFormattedOverallPlayerStanding() : string.Empty; // TODO : Do I need to save this separately ?
                        break;

                    case PILOT_NAME_TAG:
                        text.text = settings.pilotName;
                        text.fontStyle = FontStyle.Bold;
                        Main.DelayCall(() => text.fontSize = StyleConstants.Text.Standard.GetFontSize(StyleManager.Instance().UIScale));
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
}
