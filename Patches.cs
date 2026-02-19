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
        const string DIFFICULTY_PANEL = "CareerDifficultySettings";
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
                HorizontalLayoutGroup group = panel.GetComponentInChildren<HorizontalLayoutGroup>();
                group.spacing = -10;
                group.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

                CarrouselUI carrousel = group.GetComponent<CarrouselUI>();

                if (carrousel == null)
                    carrousel = group.gameObject.AddComponent<CarrouselUI>();

                carrousel.Reset();

                // TODO : Destroy / hide the og panels
                // TODO : Spawn new panels with new pictures and text
                // TODO : Detect which group we are in
                // TODO : Rework data structure to have easy access by group (Season needs to be part of rally settings)
                // TODO : How do I do selection ?

                //while (group.transform.childCount != 7)
                //{
                //    Selectable previous = group.transform.GetChild(group.transform.childCount - 1).GetComponent<Selectable>();
                //    Transform newItem = GameObject.Instantiate(group.transform.GetChild(0), group.transform);

                //    newItem.GetComponent<CustomButtonSeason>().Init();
                //    newItem.GetComponent<CustomButtonSeason>().SetText(new Season(1990, Car.CarClass.GROUP_2, 1, 2, 1, string.Empty, false, AIDriverSkillTables.AI_Skill.EASY));

                //    Navigation nav = previous.navigation;
                //    nav.selectOnRight = newItem.GetComponent<Selectable>();
                //    previous.navigation = nav;

                //    newItem.GetComponent<Selectable>().navigation = new Navigation() { selectOnLeft = previous };
                //}

                //foreach (Transform item in group.transform)
                //{
                //    PropertyInfo info = typeof(Selectable).GetProperty("hasSelection", BindingFlags.NonPublic | BindingFlags.Instance);
                //    bool isSelected = (bool)info.GetValue(item.GetComponent<Selectable>());
                //    Main.Log(item.name + " selected : " + isSelected);
                //}

                //group.transform.GetChild(0).GetComponent<CustomButtonSeason>().onClick.RemoveAllListeners();
            }

            switch (panel.name)
            {
                case DIFFICULTY_PANEL:
                    inCareer = true;
                    break;

                case CAR_PANEL:
                    if (inCareer)
                    {
                        // TODO : No need to detect by year anymore => store on selection

                        int year = GameModeManager.CareerManager.GetCurrentSeason().Year;
                        Main.Log("Starting season " + year);
                        RallyManager.AppyRallySettings(year);

                        // TODO : Show rally and driver details above car selection (disable OG UI)
                        //panel.StartCoroutine(WaitAndActivate(0.01f, () => panel.GetComponent<CarChooserHelper>().BeginEvent()));
                    }
                    break;
            }

            if (panel.name == DIFFICULTY_PANEL)
                inCareer = true;
        }

        static IEnumerator WaitAndActivate(float duration, Action callback)
        {
            yield return new WaitForSeconds(duration);
            callback?.Invoke();
        }

        [HarmonyPatch(nameof(PanelManager.GoBack))]
        static void Postfix()
        {
            inCareer = false;
        }
    }
}
