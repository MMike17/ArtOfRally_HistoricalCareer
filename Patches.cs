using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
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

    // TODO : Inject custom rally data
    [HarmonyPatch(typeof(PanelManager))]
    static class PanelPatcher
    {
        const string DIFFICULTY_PANEL = "CareerDifficultySettings";
        const string CAR_PANEL = "Choose Car";

        static bool inCareer;

        [HarmonyPatch(nameof(PanelManager.AddPanelAddToHistory), new[] { typeof(Panel) })]
        static void Prefix(Panel panel)
        {
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

                        //panel.GetComponent<CarChooserHelper>().BeginEvent();
                        //UIManager.Instance.PanelManager.AddPanelAddToHistory();

                        // TODO : How do I force the game to start ?
                        // TODO : Is it starting the game when I press "A" on one of the custom buttons ?
                    }
                    break;
            }

            if (panel.name == DIFFICULTY_PANEL)
                inCareer = true;
        }

        [HarmonyPatch(nameof(PanelManager.GoBack))]
        static void Postfix()
        {
            inCareer = false;
        }
    }
}
