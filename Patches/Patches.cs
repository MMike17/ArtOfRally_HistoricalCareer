using System;
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

    // TODO : In progress seasons are not always detected by the game

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