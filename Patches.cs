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

    [HarmonyPatch(typeof(CustomButtonCars), "SaveCarToCarManager")]
    static class RallySettingsTester
    {
        // TEST
        public static RallySettings testRally;
        // TEST

        static void Postfix()
        {
            // TEST
            Main.Try("test", () =>
            {
                Car selectedCar = CarManager.GetCurrentCarsListForClass(Car.CarClass.GROUP_2)[0];
                List<ConditionTypes.Weather> weathers = AreaManager.GetWeatherForCurrentArea(AreaManager.Areas.FINLAND);
                testRally = new RallySettings(
                    selectedCar,
                    LiveryManager.GetValidLiveries(selectedCar.prefabName)[0],
                    AreaManager.Areas.FINLAND,
                    new[] { 0, 2 },
                    new[] { weathers[0], weathers[1] }
                );

                string test = "Rally settings : \n" + testRally.carClass + " " + testRally.car.prefabName + " " + testRally.livery.Name + "\n" + testRally.rallyData.CurrentArea + " :";

                foreach (Stage stage in testRally.rallyData.StageList)
                    test += "\n" + stage.Name + " " + stage.GetWeatherString();

                Main.Log(test);
            });
            // TEST
        }
    }

    // TODO : Find when we select a year in career mode (entry point) => inject own data
}
