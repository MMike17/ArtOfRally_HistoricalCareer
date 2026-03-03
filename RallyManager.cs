using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

using static AreaManager;
using static Car;
using static ConditionTypes;

namespace HistoricalCareer
{
    public class RallyManager
    {
        const string PILOT_PATH = ".Data.Pictures.Pilots.";
        const string CAR_SPRITES_PATH = ".Data.Pictures.Cars.";

        private static Dictionary<CarClass, List<RallySettings>> rallySettings;
        private static List<Sprite> carSprites;

        public RallyManager(string modFolderName)
        {
            // creation of rallies
            Assembly assembly = Assembly.GetExecutingAssembly();
            rallySettings = new Dictionary<CarClass, List<RallySettings>>();

            // Group 2
            AddCustomRally(
                CarClass.GROUP_2, 1966, Areas.FINLAND, "1000 tests rally", "Stig",
                assembly, modFolderName + PILOT_PATH, 1966, 1, 0, 1,
                new int[] { 0, 2 }, new Weather[] { Weather.Morning, Weather.Afternoon },
                "This is test lore for later"
            );

            // checks
            int count = 0;

            foreach (KeyValuePair<CarClass, List<RallySettings>> pair in rallySettings)
                count += pair.Value.Count;

            Main.Log("Loaded " + count + " rallies");

            // load resources
            carSprites = new List<Sprite>();
            string[] resourcesPaths = assembly.GetManifestResourceNames();
            string carsRootPath = modFolderName + CAR_SPRITES_PATH;
            int carsCount = 0;

            foreach (string path in resourcesPaths)
            {
                // load car sprites
                if (!path.Contains(CAR_SPRITES_PATH)) // skip non car paths
                    continue;

                carsCount++;
                LoadCarSprite(assembly, path, carsRootPath);
            }

            Main.Log("Loaded " + carsCount + " cars sprites");

            Main.OnToggle += state =>
            {
                // TODO : The fix for car class being wrong on some years will probably here
            };
        }

        private static Sprite LoadPilotPicture(Assembly assembly, string rootPath, CarClass carClass, int year, Areas area)
        {
            string path = rootPath + carClass + "_" + year + "_" + area + ".jpg";

            using (Stream stream = assembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                {
                    Main.Error("Couldn't read local file at path : " + path + ". Make sure the files have been included in the build.");
                    return null;
                }

                byte[] data;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    data = memoryStream.ToArray();
                }

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.LoadImage(data);

                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2, 100);
                sprite.name = Path.GetFileNameWithoutExtension(path.Replace(rootPath, ""));
                return sprite;
            }
        }

        private void LoadCarSprite(Assembly assembly, string path, string carsRootPath)
        {
            using (Stream stream = assembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                {
                    Main.Error("Couldn't read local file at path : " + path + ". Make sure the files have been included in the build.");
                    return;
                }

                byte[] data;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    data = memoryStream.ToArray();
                }

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.LoadImage(data);

                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2, 100);
                sprite.name = Path.GetFileNameWithoutExtension(path.Replace(carsRootPath, ""));
                carSprites.Add(sprite);
            }
        }

        private static void AddCustomRally(
            CarClass carClass,
            int year,
            Areas area,
            string rallyName,
            string pilotName,
            Assembly assembly,
            string rootPath,
            int pilotPictureYear,
            int carIndex,
            int liveryIndex,
            int locationPictureIndex,
            int[] stagesIndeces,
            Weather[] weathers,
            string loreText
        )
        {
            if (!rallySettings.ContainsKey(carClass))
                rallySettings.Add(carClass, new List<RallySettings>());

            // check stages
            List<int> indexCount = new List<int>();
            string error = string.Empty;

            foreach (int index in stagesIndeces)
            {
                int adjustedIndex = (index - index % 2 / 2);

                if (indexCount.Contains(adjustedIndex))
                    error += index + ", ";
                else
                    indexCount.Add(adjustedIndex);
            }

            if (!string.IsNullOrEmpty(error))
            {
                Main.Error(
                    "Indeces " + error.TrimEnd(' ', ',') +
                    " have been found twice in the list, the rally " +
                    rallyName + " (" + area + " " + carClass + " " + year + ")"
                );
                return;
            }

            // check weathers
            List<Weather> authorizedWeathers = AreaManager.AreaDictionary[area].weatherList;
            error = string.Empty;

            foreach (Weather weather in weathers)
            {
                if (!authorizedWeathers.Contains(weather))
                    error += weather + ", ";
            }

            if (!string.IsNullOrEmpty(error))
            {
                Main.Error("Weathers " + error.TrimEnd(' ', ',') + " are invalid for the area " + area);
                return;
            }

            // create rally
            rallySettings[carClass].Add(new RallySettings(
                carClass,
                year,
                area,
                rallyName,
                pilotName,
                LoadPilotPicture(assembly, rootPath, carClass, year, area),
                pilotPictureYear,
                carIndex,
                liveryIndex,
                locationPictureIndex,
                stagesIndeces,
                weathers,
                loreText
            ));
        }

        // TODO : Make variation for public API (+ documented / transfer from RallySettings constructor)
        public static void AddCustomRally()
        {
            //
        }

        public static List<RallySettings> GetSettingsForClass(CarClass group)
        {
            if (!rallySettings.ContainsKey(group))
            {
                Main.Error("Couldn't find settings for group " + group + " (this will crash the mod).");
                return null;
            }

            // check for DLC
            if (Platform.Get().IsDLCInstalled(Platform.DLCName.Australia))
                return rallySettings[group];
            else
                return rallySettings[group].FindAll(item => !item.needsDLC);
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
