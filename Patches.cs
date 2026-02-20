using System;
using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

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
        //const string DIFFICULTY_PANEL = "CareerDifficultySettings";
        const string CAR_PANEL = "Choose Car";
        const string GROUP_PANEL_FORMAT = "Group";

        static bool inCareer;

        [HarmonyPatch(nameof(PanelManager.AddPanelAddToHistory), new[] { typeof(Panel) })]
        static void Postfix(Panel panel)
        {
            if (!Main.enabled)
                return;

            Main.Log("Switch to panel " + panel.name);

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
                Car.CarClass group = Car.CarClass.COUNT;

                if (!Enum.TryParse(panel.name.Replace(GROUP_PANEL_FORMAT, "GROUP_"), out group))
                {
                    Main.Error("Couldn't find corresponding group for panel " + panel.name + " (this will crash the mod).");
                    return;
                }

                GameObject model = layout.transform.GetChild(0).gameObject;
                RallyManager.GetSettingsForClass(group).ForEach(settings =>
                {
                    GameObject seasonButton = GameObject.Instantiate(model, layout.transform);
                    SetupSeasonButton(seasonButton, settings);
                });

                // add custom UI
                CarrouselUI carrousel = layout.GetComponent<CarrouselUI>();

                if (carrousel == null)
                    carrousel = layout.gameObject.AddComponent<CarrouselUI>();

                carrousel.Reset();
                inCareer = true;
            }
            else if (panel.name == CAR_PANEL && inCareer)
            {
                // TODO : No need to detect by year anymore
                //int year = GameModeManager.CareerManager.GetCurrentSeason().Year;
                //Main.Log("Starting season " + year);
                //RallyManager.AppyRallySettings(year);

                // TODO : Show rally and driver details above car selection (disable OG UI)
                //panel.StartCoroutine(WaitAndActivate(0.01f, () => panel.GetComponent<CarChooserHelper>().BeginEvent()));
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
            // TODO : Setup button with custom data and season
            button.SetActive(true);
        }
    }
}
