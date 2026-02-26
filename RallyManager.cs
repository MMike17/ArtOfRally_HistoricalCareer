using System.Collections.Generic;
using UnityEngine;

using static AreaManager;
using static Car;
using static ConditionTypes;

namespace HistoricalCareer
{
    // TODO : Add access to car sprites
    // TODO : How do I access location pictures ?
    public class RallyManager
    {
        private static Dictionary<CarClass, List<RallySettings>> rallySettings;

        public RallyManager()
        {
            // TODO : Generate custom rallies here (load rally data and pictures from file)
            rallySettings = new Dictionary<CarClass, List<RallySettings>>();

            // How do I load data from local file ? (check real car names mod)

            // TEST
            CreateRally(1966, "Stig", null, 1967, CarClass.GROUP_2, 1, 0, Areas.FINLAND, "1000 tests rally", new[] { 0, 2 }, new[] { Weather.Morning, Weather.Afternoon }, "This is test lore for later");
            // TEST

            Main.OnToggle += state =>
            {
                // TODO : The fix for car class being wrong on some years will probably here
            };
        }

        private void CreateRally(
            int year,
            string pilotName,
            Sprite pilotPicture,
            int pilotPictureYear,
            CarClass carClass,
            int carIndex,
            int liveryIndex,
            Areas area,
            string rallyName,
            int[] stages,
            Weather[] weathers,
            string lore
        )
        {
            if (!rallySettings.ContainsKey(carClass))
                rallySettings.Add(carClass, new List<RallySettings>());

            Car car = CarManager.GetCurrentCarsListForClass(carClass)[carIndex];
            rallySettings[carClass].Add(new RallySettings(
                year,
                pilotName,
                pilotPicture,
                pilotPictureYear,
                carIndex,
                car,
                liveryIndex,
                RallySettings.GetCarLiveries(car.prefabName)[liveryIndex],
                area,
                rallyName,
                stages,
                weathers,
                lore
            ));

            Main.Log("Created rally for " + year + " (class : " + carClass + ")");
        }

        public static List<RallySettings> GetSettingsForClass(CarClass group)
        {
            if (!rallySettings.ContainsKey(group))
            {
                Main.Error("Couldn't find settings for group " + group + " (this will crash the mod).");
                return null;
            }

            return rallySettings[group];
        }

        public static void AppyRallySettings(RallySettings settings)
        {
            // TEST
            // this is a fix when the mod is supposed to be off
            //CarManager.SetChosenClass(CarClass.GROUP_S);
            //return;

            // car
            CarManager.SetChosenClass(settings.car.carClass);
            CarManager.SetChosenCar(settings.carIndex);
            CarManager.SetChosenLivery(settings.livery);
            // TODO : Liveries are broken (maybe)

            Main.Log("Applied car " + settings.car.name + " (" + settings.livery.Name + ")");
        }
    }
}
