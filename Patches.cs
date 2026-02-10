using System.Collections.Generic;
using HarmonyLib;

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
                        Main.Log("Entry !");
                        // TODO : How do I force the panel to get popped ?
                        // TODO : How do I get the current year of career ?

                        //UIManager.Instance.PanelManager.AddPanelAddToHistory();
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
