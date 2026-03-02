using System;
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
        const string RALLIES_PATH = ".Data.Rallies.csv";
        const string PILOT_PATH = ".Data.Pictures.Pilots.";
        const string CAR_SPRITES_PATH = ".Data.Pictures.Cars.";

        private static Dictionary<CarClass, List<RallySettings>> rallySettings;
        private static List<Sprite> carSprites;

        public RallyManager(string modFolderName)
        {
            // TEST
            //rallySettings = new Dictionary<CarClass, List<RallySettings>>();
            //rallySettings.Add(CarClass.GROUP_2, new List<RallySettings>());
            //rallySettings[CarClass.GROUP_2].Add(new RallySettings(1966, "Stig", null, 1967, CarClass.GROUP_2, 1, 0, Areas.FINLAND, 1, "1000 tests rally", new[] { 0, 2 }, new[] { Weather.Morning, Weather.Afternoon }, "This is test lore for later"));
            // TEST

            // load resources
            rallySettings = new Dictionary<CarClass, List<RallySettings>>();
            carSprites = new List<Sprite>();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourcesPaths = assembly.GetManifestResourceNames();
            string ralliesFilePath = modFolderName + RALLIES_PATH;
            string carsRootPath = modFolderName + CAR_SPRITES_PATH;
            int carsCount = 0;

            foreach (string path in resourcesPaths)
            {
                // load rallies data
                if (path == ralliesFilePath)
                    ParseRallies(modFolderName, ralliesFilePath, assembly);

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

        private void ParseRallies(string roonFolderName, string path, Assembly assembly)
        {
            int ralliesCount = 0;

            // how can I parse this mess ?
            using (Stream stream = assembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                {
                    Main.Error("Couldn't read local file at path : " + path + ". Make sure the files have been included in the build.");
                    return;
                }

                bool isFirstLine = true;

                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        if (!isFirstLine)
                        {
                            Main.Try("ParsingRally", () =>
                            {
                                string[] cells = reader.ReadLine().Split(';');
                                CarClass carClass = (CarClass)Enum.Parse(typeof(CarClass), cells[3].Replace(" ", "_").ToUpper());

                                if (!rallySettings.ContainsKey(carClass))
                                    rallySettings.Add(carClass, new List<RallySettings>());

                                // stages
                                string[] stagesStrings = cells[9].Split(new[] { ", " }, StringSplitOptions.None);
                                int[] stages = new int[stagesStrings.Length];

                                for (int i = 0; i < stages.Length; i++)
                                    stages[i] = int.Parse(stagesStrings[i]);

                                // weather
                                string[] weatherStrings = cells[10].Split(new[] { ", " }, StringSplitOptions.None);
                                Weather[] weathers = new Weather[weatherStrings.Length];

                                for (int i = 0; i < weathers.Length; i++)
                                    weathers[i] = (Weather)Enum.Parse(typeof(Weather), weatherStrings[i]);

                                rallySettings[carClass].Add(new RallySettings(
                                    int.Parse(cells[0]),
                                    cells[1],
                                    LoadPilotSprite(assembly, roonFolderName, PILOT_PATH + cells[1] + " " + cells[2] + ".jpg"),
                                    int.Parse(cells[2]),
                                    carClass,
                                    int.Parse(cells[4]),
                                    int.Parse(cells[5]),
                                    (Areas)Enum.Parse(typeof(Areas), cells[6].ToUpper()),
                                    int.Parse(cells[7]),
                                    cells[8],
                                    stages,
                                    weathers,
                                    cells[11]
                                ));

                                ralliesCount++;
                            });
                        }
                        else // skip labels
                        {
                            isFirstLine = false;
                            reader.ReadLine();
                        }
                    }
                }
            }

            Main.Log("Loaded " + ralliesCount + " rallies from file");
        }

        private Sprite LoadPilotSprite(Assembly assembly, string rootPath, string path)
        {
            // TODO : This doesn't seem to work
            path = rootPath + path;

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
