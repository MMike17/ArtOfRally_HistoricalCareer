using System;
using System.Collections;
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

    [HarmonyPatch(typeof(PanelManager))]
    static class PanelPatcher
    {
        const string DIFFICULTY_PANEL = "CareerDifficultySettings";
        const string CAR_PANEL = "Choose Car";

        static bool inCareer;

        [HarmonyPatch(nameof(PanelManager.AddPanelAddToHistory), new[] { typeof(Panel) })]
        static void Postfix(Panel panel)
        {
            if (!Main.enabled)
                return;

            Main.Log("Switch to panel " + panel.name);

            switch (panel.name)
            {
                case DIFFICULTY_PANEL:
                    inCareer = true;
                    break;

                case CAR_PANEL:
                    if (inCareer)
                    {
                        int year = GameModeManager.CareerManager.GetCurrentSeason().Year;
                        Main.Log("Starting season " + year);
                        RallyManager.AppyRallySettings(year);
                        panel.StartCoroutine(WaitAndActivate(0.01f, () => panel.GetComponent<CarChooserHelper>().BeginEvent()));

                        // TEST
                        //panel.GetComponent<CarChooserHelper>().BeginEvent();
                        //UIManager.Instance.PanelManager.AddPanelAddToHistory();
                        // TEST
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
