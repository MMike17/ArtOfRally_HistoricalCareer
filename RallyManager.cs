using System.Collections.Generic;

using static AreaManager;
using static Car;
using static ConditionTypes;

namespace HistoricalCareer
{
    public class RallyManager
    {
        private static Dictionary<CarClass, List<RallySettings>> rallySettings;

        public RallyManager()
        {
            // TODO : Generate custom rallies here
            rallySettings = new Dictionary<CarClass, List<RallySettings>>();

            //    Main.OnToggle += state =>
            //    {
            //        // TODO : The fix for car class being wrong on some years will probably here
            //    };
        }

        private void CreateRally(int year, CarClass carClass, int carIndex, int liveryIndex, Areas area, int[] stages, Weather[] weathers)
        {
            if (!rallySettings.ContainsKey(carClass))
                rallySettings.Add(carClass, new List<RallySettings>());

            Car car = CarManager.GetCurrentCarsListForClass(carClass)[carIndex];
            rallySettings[carClass].Add(new RallySettings(
                year,
                car,
                RallySettings.GetCarLiveries(car.prefabName)[liveryIndex],
                area,
                stages,
                weathers
            ));

            Main.Log("Created rally for " + year + " (class : " + carClass + ")");
        }

        public static void AppyRallySettings(CarClass group)
        {
            RallySettings settings;

            if (rallySettings.ContainsKey(group))
                settings = rallySettings[group][CarrouselUI.selectedIndex];
            else
            {
                Main.Error("Couldn't find rally settings for group " + group + ", this will crash the mod.");
                return;
            }

            // TEST
            //CarManager.SetChosenClass(CarClass.GROUP_S);
            //return;

            // car
            CarManager.SetChosenClass(settings.carClass);

            List<Car> cars = CarManager.GetCurrentCarsListForClass(settings.carClass);
            CarManager.SetChosenCar(cars.IndexOf(settings.car));
            Main.Log("test car : " + (cars.IndexOf(settings.car) != -1));

            List<Livery> liveries = RallySettings.GetCarLiveries(settings.car.prefabName);
            CarManager.SetChosenLivery(settings.livery);
            // TODO : Liveries are broken (this is kinda random and doesn't happen every time)

            // rally
            RallyData currentRally = GameModeManager.GetRallyDataCurrentGameMode();
            currentRally.SetArea((int)settings.season.Rallies[0].CurrentArea);

            int stageCount = settings.season.Rallies[0].StageCount;
            currentRally.SetStageCount(stageCount);

            for (int i = 0; i < stageCount; i++)
            {
                Stage stage = settings.season.Rallies[0].StageList[i];
                currentRally.SetStage(i, stage);
                currentRally.SetWeatherForStage(i, stage.Weather);
            }

            Main.Log(
                "Applied rally settings for " + group + " (" +
                settings.car.name + " " +
                settings.carClass + ")\n(" +
                settings.season.Rallies[0].CurrentArea + " " +
                stageCount + ")"
            );
        }
    }
}
