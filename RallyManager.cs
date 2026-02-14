using System.Collections.Generic;
using static AreaManager;
using static Car;
using static ConditionTypes;

namespace HistoricalCareer
{
    public class RallyManager
    {
        private static Dictionary<int, RallySettings> rallySettings;

        public RallyManager()
        {
            // TODO : Generate custom rallies here
            rallySettings = new Dictionary<int, RallySettings>();

            CreateRally(1991, CarClass.GROUP_2, 0, 0, Areas.FINLAND, new[] { 0, 2 }, new[] { Weather.Morning, Weather.Afternoon });

            Main.OnToggle += state =>
            {
                // TODO : The fix for car class being wrong on some years will probably here
            };
        }

        private void CreateRally(int year, CarClass carClass, int carIndex, int liveryIndex, Areas area, int[] stages, Weather[] weathers)
        {
            Car car = CarManager.GetCurrentCarsListForClass(carClass)[carIndex];
            rallySettings.Add(year, new RallySettings(car, RallySettings.GetCarLiveries(car.prefabName)[liveryIndex], area, stages, weathers));
            Main.Log("Created rally for " + year + " (class : " + carClass + ")");
        }

        public static void AppyRallySettings(int year)
        {
            RallySettings settings;

            if (rallySettings.ContainsKey(year))
                settings = rallySettings[year];
            else
            {
                Main.Error("Couldn't find rally settings for year " + year + ", this will crash the mod.");
                return;
            }

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
            currentRally.SetArea((int)settings.rallyData.CurrentArea);
            currentRally.SetStageCount(settings.rallyData.StageCount);

            for (int i = 0; i < settings.rallyData.StageCount; i++)
            {
                Stage stage = settings.rallyData.StageList[i];
                currentRally.SetStage(i, stage);
                currentRally.SetWeatherForStage(i, stage.Weather);
            }

            Main.Log(
                "Applied rally settings for " + year + " (" +
                settings.car.name + " " +
                settings.carClass + ")\n(" +
                settings.rallyData.CurrentArea + " " +
                settings.rallyData.StageCount + ")"
            );
        }
    }
}
