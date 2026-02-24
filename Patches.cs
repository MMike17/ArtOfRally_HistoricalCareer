using System;
using System.Collections;
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

    // TODO : This is doing some list matching bullshit (should I replace the whole thing ?)
    //CareerManager.SetSeasonInProgress(Season TheSeason)

    [HarmonyPatch(typeof(PanelManager))]
    static class PanelPatcher
    {
        //const string DIFFICULTY_PANEL = "CareerDifficultySettings";
        const string CAR_PANEL = "Choose Car";
        const string GROUP_PANEL_FORMAT = "Group";
        const string PILOT_NAME_TAG = "Year";
        const string RESTARTS_TAG = "Restarts";
        const string LOCATION_YEAR_TAG = "AiSkill";
        const string RALLY_TAG = "SeasonInfo";

        public static string submitUIString { get; private set; }
        public static Player playerInput { get; private set; }
        public static RallySettings currentRally { get; private set; }

        private static CareerUI careerUI;
        private static CarClass currentGroup;
        private static bool inCareer;

        [HarmonyPatch(nameof(PanelManager.AddPanelAddToHistory), new[] { typeof(Panel) })]
        static void Postfix(Panel panel)
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

            if (panel.name.Contains(GROUP_PANEL_FORMAT))
            {
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

                GameObject model = layout.transform.GetChild(0).gameObject;
                List<RallySettings> settings = RallyManager.GetSettingsForClass(currentGroup);
                settings.ForEach(setting =>
                {
                    GameObject seasonButton = GameObject.Instantiate(model, layout.transform);
                    SetupSeasonButton(seasonButton, setting);
                });

                //add custom UI
                CarrouselUI carrousel = layout.GetComponent<CarrouselUI>();

                if (carrousel == null)
                    carrousel = layout.gameObject.AddComponent<CarrouselUI>();

                carrousel.Reset(settings);
                inCareer = true;
            }
            else if (panel.name == CAR_PANEL && inCareer)
            {
                //Main.Log("Test");

                //// TODO : UI doesn't show up at all
                //if (careerUI == null)
                //    careerUI = Main.SpawnUI(panel.transform.parent);

                //Main.Log(careerUI.transform.parent.name);

                //panel.Hide();
                //careerUI.Set(currentRally, rally =>
                //{
                //    RallyManager.AppyRallySettings(rally);
                //    panel.GetComponent<CarChooserHelper>().BeginEvent();
                //});
            }
        }

        static IEnumerator WaitAndActivate(float duration, Action callback)
        {
            yield return new WaitForSeconds(duration);
            callback?.Invoke();
        }

        [HarmonyPatch(nameof(PanelManager.GoBack))]
        static void Postfix() => inCareer = false;

        static void SetupSeasonButton(GameObject button, RallySettings settings)
        {
            CustomButtonSeason seasonButton = button.GetComponent<CustomButtonSeason>();
            seasonButton.transform.Find("ClassImage").GetComponent<Image>().sprite = settings.pilotPicture;

            foreach (Text text in seasonButton.GetComponentsInChildren<Text>())
            {
                switch (text.name)
                {
                    case PILOT_NAME_TAG:
                        text.text = settings.pilotName;
                        break;

                    case RESTARTS_TAG:
                        text.enabled = false;
                        break;

                    case LOCATION_YEAR_TAG:
                        text.text = settings.season.Rallies[0].CurrentArea + " (" + settings.season.Year + ")";
                        break;

                    case RALLY_TAG:
                        text.text = settings.rallyName;
                        break;
                }
            }

            button.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(CareerManager), nameof(CareerManager.SetSeasonInProgress))]
    static class CareerPatcher
    {
        static void Postfix(CareerManager __instance, Season TheSeason)
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
                TheSeason.RemoveDLCCar(); // TODO : Will have to check if seasons are using DLC and if the player has the DLC

                Main.SetField(__instance, "CurrentSeasonInProcess", BindingFlags.Instance, TheSeason);
            }
        }
    }
}
