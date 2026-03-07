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
        const string PILOT_PATH = ".Data.Pictures.Pilots.";
        const string CAR_SPRITES_PATH = ".Data.Pictures.Cars.";

        private static Dictionary<CarClass, List<RallySettings>> rallySettings;
        private static List<Sprite> carSprites;

        public RallyManager(string modFolderName)
        {
            // creation of rallies
            Assembly assembly = Assembly.GetExecutingAssembly();
            rallySettings = new Dictionary<CarClass, List<RallySettings>>();
            string pilotPicturePath = modFolderName + PILOT_PATH;

            // Group 2
            CarClass group2 = CarClass.GROUP_2;

            AddCustomRally(
                group2, 1966, Areas.FINLAND, "Finland", "1000 Lakes rally", "Timo Mäkinen",
                assembly, pilotPicturePath, 1966, 1, 0, 2,
                new int[] { 0, 6 }, new Weather[] { Weather.Morning, Weather.Afternoon },
                "Following their 1965 win, <b>Timo Mäkinen</b> and his copilot <b>Pekka Keskitalo</b> took advantage of their Mini's front-wheel-drive and light body to excell on their home country's gravel jumps, becoming the first \"flying finns\" with <b>Simo Lampinen</b> and <b>Rauno Aaltonen</b>, who placed 3rd on the same rally."
            );
            AddCustomRally(
                group2, 1967, Areas.SARDINIA, "Italy", "Rally dei Fiori", "Jean-François Piot",
                assembly, pilotPicturePath, 1967, 5, 1, 0,
                new int[] { 2, 6, 0 }, new Weather[] { Weather.Fog, Weather.Afternoon, Weather.Sunset },
                "After winning the <b>Tour de Corse</b> and the <b>Coupe de Alpes</b> the previous year, <b>Jean-François Piot</b> joined by copilot <b>Nicolas Roure</b> armed with a prototype Renault 8 Gordini 1440 dominated tarmac-gravel mixed stages, beating its competition in the mediterranean conditions."
            );
            AddCustomRally(
                group2, 1968, Areas.GERMANY, "West Germany", "Wiesbaden German Rally", "Pauli Toivonen",
                assembly, pilotPicturePath, 1969, 4, 5, 0,
                new int[] { 0, 5, 3, 11 }, new Weather[] { Weather.Rain, Weather.Sunset, Weather.Fog, Weather.Afternoon },
                "For his debut in a Porsche, <b>Pauli Toivonen</b> couldn't secure a <b>Monte-carlo</b> win, being overtaken by <b>Vic Elford</b>. But with the help of <b>Martti Kolari</b> as copilot, they managed a win in the <b>West German rally</b>. Pauli continued winning that same year in Austria and Swizerland with different copilots."
            );
            AddCustomRally(
                group2, 1970, Areas.FINLAND, "Finland", "1000 Lakes rally", "Hannu Mikkola",
                assembly, pilotPicturePath, 1966, 0, 1, 2,
                new int[] { 0, 5, 6, 8 }, new Weather[] { Weather.Morning, Weather.Rain, Weather.Afternoon, Weather.Morning },
                "After their win in the grueling <b>London-Mexico World Cup Rally</b>, <b>Hannu Mikkola</b> and the exceptional navigator <b>Gunnar Palm</b> brought their Ford Escort twin cam to Finland. With its engine originally developped for the Lotus Elan and its lights body shell the Escort proved to be a solid contender for the rougher rallies."
            );
            // TODO : Finish designing rallies for group 2

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
            string areaName,
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
            AddCustomRally(
                carClass,
                year,
                area,
                areaName,
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
            );
        }

        /// <summary>This method is used to add custom rallies to the mod</summary>
        /// <param name="pilotPicture">Square sprite of the pilot's picture (if it's not square it might have weird stretching)</param>
        /// <param name="pilotPictureYear">The year the pilot picture was taken in</param>
        /// <param name="carIndex">Index of the car in its era list (check CarManager.Init)</param>
        /// <param name="locationPictureIndex">Index of the stage in its area, stages go by pairs (0-1, 2-3, etc / check the "custom rally" menu in game)</param>
        /// <param name="stagesIndeces">No double stages, stages go by pairs (0-1, 2-3, etc...)</param>
        /// <param name="weathers">Use AreaManager.GetWeatherForCurrentArea to get valid weathers</param>
        public static void AddCustomRally(
            CarClass carClass,
            int year,
            Areas area,
            string areaName,
            string rallyName,
            string pilotName,
            Sprite pilotPicture,
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
                areaName,
                rallyName,
                pilotName,
                pilotPicture,
                pilotPictureYear,
                carIndex,
                liveryIndex,
                locationPictureIndex,
                stagesIndeces,
                weathers,
                loreText
            ));
        }

        /// <summary>This method is used to get custom rallies for a given Car.CarClass</summary>
        public static List<RallySettings> GetSettingsForClass(CarClass group)
        {
            if (!rallySettings.ContainsKey(group))
            {
                Main.Error("Couldn't find settings for group " + group + " (this will crash the mod)");
                return null;
            }

            // check for DLC
            if (Platform.Get().IsDLCInstalled(Platform.DLCName.Australia))
                return rallySettings[group];
            else
                return rallySettings[group].FindAll(item => !item.needsDLC);
        }

        /// <summary>This method is used to remove all custom rallies for a given Car.CarClass</summary>
        public static void ClearSettingsForClass(CarClass group)
        {
            if (!rallySettings.ContainsKey(group))
            {
                Main.Error("Couldn't find settings for group " + group);
                return;
            }

            rallySettings[group].Clear();
        }

        public static void AppyRallySettings(RallySettings settings)
        {
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
