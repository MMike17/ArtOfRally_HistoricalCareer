using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

using static AreaManager;
using static Car;
using static ConditionTypes;

namespace HistoricalCareer
{
    public class RallyManager
    {
        const string CAR_SPRITES_PATH = ".Data.Pictures.Cars.";

        private static Dictionary<CarClass, List<RallySettings>> rallySettings;
        private static List<Sprite> carSprites;

        public RallyManager(string modFolderName)
        {
            // TODO : Should I load all rally data from external data or code everything here ?
            // TODO : Generate custom rallies here (load rally data and pictures from file)
            rallySettings = new Dictionary<CarClass, List<RallySettings>>();

            // How do I load data from local file ? (check real car names mod)

            // TEST
            CreateRally(1966, "Stig", null, 1967, CarClass.GROUP_2, 1, 0, Areas.FINLAND, 1, "1000 tests rally", new[] { 0, 2 }, new[] { Weather.Morning, Weather.Afternoon }, "This is test lore for later");
            // TEST

            // load resources
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourcesPaths = assembly.GetManifestResourceNames();

            // load car sprites
            carSprites = new List<Sprite>();
            string rootFolder = modFolderName + CAR_SPRITES_PATH;
            int carsCount = 0;

            foreach (string path in resourcesPaths)
            {
                // skip non car paths
                if (!path.Contains(CAR_SPRITES_PATH))
                    continue;

                carsCount++;

                using (Stream stream = assembly.GetManifestResourceStream(path))
                {
                    byte[] data;

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        data = memoryStream.ToArray();
                    }

                    Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    texture.LoadImage(data);

                    Main.Log(texture.width + " / " + texture.height);

                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2, 100);
                    sprite.name = Path.GetFileNameWithoutExtension(path.Replace(rootFolder, ""));
                    carSprites.Add(sprite);
                }
            }

            Main.Log("Loaded " + carsCount + " cars sprites");

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
            int locationPictureIndex,
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
                carClass,
                carIndex,
                liveryIndex,
                area,
                locationPictureIndex,
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
            CarManager.SetChosenClass(settings.carClass);
            CarManager.SetChosenCar(settings.carIndex);
            CarManager.SetChosenLivery(settings.livery);
            // TODO : Liveries are broken (maybe)

            Main.Log("Applied rally settings");
        }

        public static Sprite GetCarSprite(CarClass carClass, int carIndex)
        {
            string carName = CarManager.GetCurrentCarsListForClass(carClass)[carIndex].prefabName;
            Sprite result = carSprites.Find(item => item.name == carName);

            if (result == null)
                Main.Error("Couldn't find sprite for car \"" + carName + "\"");

            return result;
        }
    }
}
