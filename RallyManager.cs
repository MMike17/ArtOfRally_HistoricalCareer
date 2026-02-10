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

            CreateRally(1987, CarClass.GROUP_2, 0, 0, Areas.FINLAND, new[] { 0, 2 }, new[] { Weather.Morning, Weather.Afternoon });
        }

        private void CreateRally(int year, CarClass carClass, int carIndex, int liveryIndex, Areas area, int[] stages, Weather[] weathers)
        {
            Car car = CarManager.GetCurrentCarsListForClass(carClass)[carIndex];
            rallySettings.Add(year, new RallySettings(car, LiveryManager.GetValidLiveries(car.prefabName)[liveryIndex], area, stages, weathers));
        }

        public static RallySettings GetSettingsForYear(int year)
        {
            if (rallySettings.ContainsKey(year))
                return rallySettings[year];

            Main.Error("Couldn't find rally settings for year " + year + ", this will crash the mod.");
            return null;
        }
    }
}
