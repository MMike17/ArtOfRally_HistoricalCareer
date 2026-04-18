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
        const string CAREER_PANEL_FORMAT = "ClassesDashboard";
        const string GROUP_PANEL_FORMAT = "Group";
        const string CONTINUE_PANEL = "ContinueSeasonScreen";
        const string RESULTS_PANEL = "ResultsScreen";
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
        public static string cancelUIString { get; private set; }
        public static RallySettings currentRally { get; private set; }

        private static Dictionary<string, CustomButtonSeason> seasonButtons;
        private static CarrouselUI carrousel;
        private static CareerUI careerUI;

        [HarmonyPatch(nameof(PanelManager.AddPanelAddToHistory), new[] { typeof(Panel) })]
        [HarmonyPostfix]
        static void CheckPanel(Panel panel)
        {
            if (!Main.enabled || GameModeManager.GameMode != GameModeManager.GAME_MODES.CAREER)
                return;

            Main.Try(nameof(CheckPanel), () =>
            {
                if (string.IsNullOrEmpty(submitUIString))
                {
                    BaseInputModule inputModule = EventSystem.current.currentInputModule;

                    if (inputModule != null)
                    {
                        submitUIString = Main.GetField<string, RewiredStandaloneInputModule>(
                            inputModule as RewiredStandaloneInputModule,
                            "m_SubmitButton",
                            BindingFlags.Instance
                        );
                        cancelUIString = Main.GetField<string, RewiredStandaloneInputModule>(
                            inputModule as RewiredStandaloneInputModule,
                            "m_CancelButton",
                            BindingFlags.Instance
                        );
                        int playerID = Main.GetField<int[], RewiredStandaloneInputModule>(
                            inputModule as RewiredStandaloneInputModule,
                            "playerIds",
                            BindingFlags.Instance
                        )[0];
                        playerInput = ReInput.players.GetPlayer(playerID);
                    }
                }

                // main panel
                if (panel.name == MAIN_PANEL)
                {
                    titleFont = panel.transform.GetChild(0).GetChild(0).GetComponentInChildren<Text>().font;
                    bodyFont = panel.GetComponentInChildren<VersionText>().GetComponent<Text>().font;
                }
                else if (panel.name == CAREER_PANEL_FORMAT) // group selection panel
                {
                    CustomButtonCareerClass[] classButtons = panel.GetComponentsInChildren<CustomButtonCareerClass>();
                    int lastValid = -1;

                    foreach (CustomButtonCareerClass classButton in classButtons)
                    {
                        bool hasSettings = RallyManager.GetSettingsForClass(classButton.CarClass) != null;
                        classButton.gameObject.SetActive(hasSettings);

                        if (hasSettings)
                            lastValid++;
                    }

                    if (lastValid > -1 && lastValid < classButtons.Length)
                    {
                        CustomButtonCareerClass classButton = classButtons[lastValid + 1];
                        classButton.enabled = false;
                        classButton.gameObject.SetActive(true);

                        classButton.transform.Find("Class").GetComponent<Text>().text = "Coming soon";
                        classButton.transform.Find("SeasonInProgress").GetComponent<Text>().enabled = false;
                        classButton.transform.Find("SeasonFill").GetComponent<Image>().enabled = false;

                        Main.DelayCall(() => classButton.transform.Find("LockedIcon").GetComponent<Image>().enabled = false);
                    }
                }
                else if (panel.name.Contains(GROUP_PANEL_FORMAT)) // season selection panel
                    SetupSeasonPanel(panel);
                else if (panel.name == CAR_PANEL && GameModeManager.GameMode == GameModeManager.GAME_MODES.CAREER) // car selection panel
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

                        RallyManager.ApplyRallySettings(rally);
                        SaveManager.SaveSeasonData(rally);
                        panel.GetComponent<CarChooserHelper>().BeginEvent();
                    });
                }
                else if (panel.name == CONTINUE_PANEL)
                {
                    RallySettings settings = RallyManager.GetRallyInProgress();
                    panel.GetComponent<ContinueSeasonScreen>().SeasonText.text = settings.rallyName + " (" + settings.season.Year + ")";
                }
                else if (panel.name == RESULTS_PANEL)
                {
                    Main.DelayCall(() =>
                    {
                        RallySettings settings = RallyManager.GetRallyInProgress();
                        GroupTitle title = panel.GetComponentInChildren<GroupTitle>();
                        title.SetString(title.Text.text.Replace("rally 1/1", settings.rallyName));
                        panel.transform.Find("TextSubtitleStage").GetComponent<Text>().text = "Rally results";
                    });
                }
            });
        }

        public static void SetupSeasonPanel(Panel panel)
        {
            SeasonDashboardUI ui = panel.transform.parent.GetComponent<SeasonDashboardUI>();
            HorizontalLayoutGroup layout = panel.GetComponentInChildren<HorizontalLayoutGroup>();
            ContentSizeFitter fitter = layout.GetComponent<ContentSizeFitter>();
            carrousel = layout.GetComponent<CarrouselUI>();

            // should cut config short
            if (carrousel != null)
                return;

            layout.spacing = -10;
            layout.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            // hide buttons
            foreach (Transform child in layout.transform)
                child.gameObject.SetActive(false);

            // generate buttons
            CarClass currentGroup = CarClass.COUNT;

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
            seasonButtons = new Dictionary<string, CustomButtonSeason>();

            settings.ForEach(setting =>
            {
                CustomButtonSeason seasonButton = GameObject.Instantiate(model, layout.transform);
                SetupSeasonButton(seasonButton, setting);

                buttons.Add(seasonButton);
                seasonButtons.Add(RallyManager.GetSeasonCode(setting), seasonButton);
            });

            Main.SetField(ui, "AllSeasonButtons", BindingFlags.Instance, buttons);

            // add custom UI
            carrousel = layout.gameObject.AddComponent<CarrouselUI>();
            carrousel.Reset(settings);
        }

        public static CustomButtonSeason GetButtonForSeason(Season season)
        {
            string seasonCode = RallyManager.GetSeasonCode(season);

            if (seasonButtons == null)
            {
                Main.Error("Season buttons dictionary not initialized, panel was not setup");
                return null;
            }

            if (!seasonButtons.ContainsKey(seasonCode))
            {
                Main.Error("Season code not found in dictionary (" + RallyManager.GetSeasonCode(season) + ")");
                return null;
            }

            if (seasonButtons[seasonCode] == null)
                Main.Error("Couldn't find button for provided season (" + RallyManager.GetSeasonCode(season) + ")");

            return seasonButtons[seasonCode];
        }

        public static void ShowSeasonButton(CustomButtonSeason seasonButton)
        {
            SetSeasonButtonsState(seasonButton, false);
            seasonButton.EnableCanvas();
        }

        public static void SetSeasonButtonsState(CustomButtonSeason seasonButton, bool status)
        {
            foreach (KeyValuePair<string, CustomButtonSeason> pair in seasonButtons)
            {
                if (pair.Value != seasonButton)
                {
                    if (status)
                    {
                        pair.Value.DisableCanvas();
                        pair.Value.interactable = true;
                    }
                    else
                    {
                        pair.Value.HideCanvas();
                        pair.Value.interactable = false;
                    }
                }
            }
        }

        public static void SetCarouselState(bool state) => carrousel.SetInputState(state);

        public static void SetCarouselSelection(Season season)
        {
            // we assume season is valid
            int index = 0;

            foreach (string key in seasonButtons.Keys)
            {
                if (key == RallyManager.GetSeasonCode(season))
                    break;
                else
                    index++;
            }

            carrousel.ForceSelection(index);
        }

        static void SetupSeasonButton(CustomButtonSeason seasonButton, RallySettings settings)
        {
            Main.InvokeMethod(seasonButton, "AssignProperties", BindingFlags.Instance, null);
            Image wreathIcon = Main.GetField<Image, CustomButtonSeason>(seasonButton, "WreathImage", BindingFlags.Instance);
            Image stageWins = Main.GetField<Image, CustomButtonSeason>(seasonButton, "StageWinsFill", BindingFlags.Instance);
            Image lockedIcon = Main.GetField<Image, CustomButtonSeason>(seasonButton, "LockedIcon", BindingFlags.Instance);

            wreathIcon.enabled = settings.season.Status == Season.STATUS.COMPLETED;
            stageWins.fillAmount = (float)settings.season.GetSeasonStageWinsPercentage() / 100f;
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
                            settings.season.GetFormattedOverallPlayerStanding() : string.Empty;
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

        public static void ResetCareerUIs()
        {
            foreach (CarrouselUI carrousel in GameObject.FindObjectsOfType<CarrouselUI>())
                GameObject.Destroy(carrousel);
        }
    }
}
