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

            GenerateGroup2Seasons(assembly, pilotPicturePath);
            GenerateGroup3Seasons(assembly, pilotPicturePath);
            GenerateGroup4Seasons(assembly, pilotPicturePath);
            GenerateGroupBSeasons(assembly, pilotPicturePath);
            GenerateGroupSSeasons(assembly, pilotPicturePath);
            GenerateGroupASeasons(assembly, pilotPicturePath);

            UnlockFirstSeason();

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

        private static void UnlockFirstSeason()
        {
            Season firstSeason = rallySettings[0][0].season;

            if (firstSeason.Status == Season.STATUS.LOCKED)
                firstSeason.Status = Season.STATUS.UNLOCKED;

            SaveManager.SaveSeasonData(firstSeason);
        }

        private static void GenerateGroup2Seasons(Assembly assembly, string pilotPicturePath)
        {
            CarClass group = CarClass.GROUP_2;

            AddCustomRally(
                group, 1966, Areas.FINLAND, "Finland", "1000 Lakes rally", "Timo Mäkinen",
                assembly, pilotPicturePath, 1966, 1, 0, 2,
                new int[] { 0, 6 }, new Weather[] { Weather.Morning, Weather.Afternoon },
                "Following their 1965 win, <b>Timo Mäkinen</b> and his copilot <b>Pekka Keskitalo</b> took advantage of their Mini's front-wheel-drive and light body to excell on their home country's gravel jumps, becoming the first \"flying finns\" with <b>Simo Lampinen</b> and <b>Rauno Aaltonen</b>, who placed 3rd on the same rally."
            );
            AddCustomRally(
                group, 1967, Areas.SARDINIA, "Italy", "Rally dei Fiori", "Jean-François Piot",
                assembly, pilotPicturePath, 1967, 5, 1, 0,
                new int[] { 2, 6, 0 }, new Weather[] { Weather.Fog, Weather.Afternoon, Weather.Sunset },
                "After winning the <b>Tour de Corse</b> and the <b>Coupe de Alpes</b> the previous year, <b>Jean-François Piot</b> joined by copilot <b>Nicolas Roure</b> armed with a prototype Renault 8 Gordini 1440 dominated tarmac-gravel mixed stages, beating its competition in the mediterranean conditions."
            );
            AddCustomRally(
                group, 1968, Areas.GERMANY, "West Germany", "Wiesbaden German Rally", "Pauli Toivonen",
                assembly, pilotPicturePath, 1969, 4, 5, 0,
                new int[] { 0, 5, 3, 11 }, new Weather[] { Weather.Rain, Weather.Sunset, Weather.Fog, Weather.Afternoon },
                "For his debut in a Porsche, <b>Pauli Toivonen</b> couldn't secure a <b>Monte-carlo</b> win, being overtaken by <b>Vic Elford</b>. But with the help of <b>Martti Kolari</b> as copilot, they managed a win in the <b>West German rally</b>. Pauli continued winning that same year in Austria and Swizerland with different copilots."
            );
            AddCustomRally(
                group, 1969, Areas.SARDINIA, "Italy", "Rally Sanremo", "Harry Källström",
                assembly, pilotPicturePath, 1970, 6, 3, 0,
                new int[] { 2, 6, 10, 0 }, new Weather[] { Weather.Sunset, Weather.Afternoon, Weather.Sunset, Weather.Afternoon },
                "Nicknamed \"Sputnik\" because of how fast his career took off, driver and later actorn <b>Harry Källström</b> took the wheel of a Lancia Fulvia with his copilot <b>Häggbom Gunnar</b> to win the <b>Sanremo Rally</b> as well as the <b>Spanish RACE Rally</b> and <b>British RAC Rally</b> that year, earning their first European champions title."
            );
            AddCustomRally(
                group, 1970, Areas.FINLAND, "Finland", "1000 Lakes rally", "Hannu Mikkola",
                assembly, pilotPicturePath, 1966, 0, 1, 2,
                new int[] { 0, 5, 6, 8, 2 }, new Weather[] { Weather.Morning, Weather.Afternoon, Weather.Morning, Weather.Sunset, Weather.Rain },
                "After their win in the grueling <b>London-Mexico World Cup Rally</b>, <b>Hannu Mikkola</b> and the exceptional navigator <b>Gunnar Palm</b> brought their Ford Escort twin cam to Finland. With its engine originally developped for the Lotus Elan and its lights body shell the Escort proved to be a solid contender for the rougher rallies."
            );
            AddCustomRally(
                group, 1970, Areas.GERMANY, "Austria", "Rally Munich-Vienne-Budapest", "Jean-Claude Andruet",
                assembly, pilotPicturePath, 1977, 2, 5, 6,
                new int[] { 1, 8, 10, 5, 7 }, new Weather[] { Weather.Rain, Weather.Morning, Weather.Rain, Weather.Sunset, Weather.Fog },
                "After dominating in the french rallies, <b>Jean-Claude Andruet</b> wildly swung his <b>Alpine A110</b> with <b>Michèle Veron</b> as copilot in this rally spanning between Germany, Austria and Hungary. The battle with Fords and Porsches was fierce but they won the French and European rally championships titles."
            );

            // TODO : Finish designing rallies for group 2
        }

        private static void GenerateGroup3Seasons(Assembly assembly, string pilotPicturePath)
        {
            CarClass group = CarClass.GROUP_3;

            AddCustomRally( // TEST
                group, 1971, Areas.FINLAND, "Test zone", "Test rally", "Test driver",
                assembly, pilotPicturePath, 1970, 9, 0, 2, new int[] { 0 }, new Weather[] { Weather.Afternoon },
                "This is a test lore text to check if I can unlock next season correctly"
            );

            // TODO : Finish designing rallies for group 3
        }

        private static void GenerateGroup4Seasons(Assembly assembly, string pilotPicturePath)
        {
            CarClass group = CarClass.GROUP_4;

            // TODO : Finish designing rallies for group 4
        }

        private static void GenerateGroupBSeasons(Assembly assembly, string pilotPicturePath)
        {
            CarClass group = CarClass.GROUP_B;

            // TODO : Finish designing rallies for group B
        }

        private static void GenerateGroupSSeasons(Assembly assembly, string pilotPicturePath)
        {
            CarClass group = CarClass.GROUP_S;

            // TODO : Finish designing rallies for group S
        }

        private static void GenerateGroupASeasons(Assembly assembly, string pilotPicturePath)
        {
            CarClass group = CarClass.GROUP_A;

            // TODO : Finish designing rallies for group A
        }

        private static Sprite LoadPilotPicture(Assembly assembly, string rootPath, CarClass carClass, int year, Areas area)
        {
            string path = rootPath + GetSeasonCode(carClass, year, area) + ".jpg";

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

        public static string GetSeasonCode(CarClass carClass, int year, Areas area) => carClass + "_" + year + "_" + area;

        public static string GetSeasonCode(Season season)
        {
            if (season.Rallies.Count <= 0)
                Main.Log("Rallies are not setup for this (" + season.CarClass + " " + season.Year + ")");

            return GetSeasonCode(season.CarClass, season.Year, season.Rallies[0].CurrentArea);
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

        public static void ApplyRallySettings(RallySettings settings)
        {
            CarManager.SetChosenClass(settings.carClass);
            CarManager.SetChosenCar(settings.carIndex);
            CarManager.SetChosenLivery(settings.livery);
            // TODO : Liveries are broken (maybe)

            Main.Log("Applied rally settings");
        }

        public static void ApplyRallySettings(Season season)
        {
            ApplyRallySettings(rallySettings[season.CarClass].Find(item => GetSeasonCode(item.season) == GetSeasonCode(season)));
        }

        public static Sprite GetCarSprite(CarClass carClass, int carIndex)
        {
            string carName = CarManager.GetCurrentCarsListForClass(carClass)[carIndex].prefabName;
            Sprite result = carSprites.Find(item => item.name == carName);

            if (result == null)
                Main.Error("Couldn't find sprite for car \"" + carName + "\"");

            return result;
        }

        // TODO : How do we detect that it's the end of the game ?
        // SeasonDashboardUI.DisplayUnlocksAndDashboardSequence
        //      (line 251 : Season.Year == GameModeManager.CareerManager.GroupASeason[GameModeManager.CareerManager.GroupASeason.Count - 1].Year)
        // override GameCompleteDataSetup.GoToGameCompleteCutscene

        public static void UnlockNextSeason(Season season)
        {
            CareerData save = Main.GetField<CareerData, CareerManager>(GameModeManager.CareerManager, "CareerData", BindingFlags.Instance);
            List<RallySettings> currentList = rallySettings[season.CarClass];
            RallySettings currentSettings = currentList.Find(item => string.Equals(GetSeasonCode(item.season), GetSeasonCode(season)));
            int index = currentList.IndexOf(currentSettings);
            Season selected = null;

            if (index + 1 <= currentList.Count - 1)
                selected = currentList[index + 1].season;
            else if (season.CarClass != CarClass.GROUP_A)
            {
                CarClass newClass = season.CarClass + 1;
                selected = rallySettings[newClass][0].season;

                switch (newClass)
                {
                    case CarClass.GROUP_3:
                        save.Group3CarClass.isLocked = false;
                        break;

                    case CarClass.GROUP_4:
                        save.Group4CarClass.isLocked = false;
                        break;

                    case CarClass.GROUP_B:
                        save.GroupBCarClass.isLocked = false;
                        break;

                    case CarClass.GROUP_S:
                        save.GroupSCarClass.isLocked = false;
                        break;

                    case CarClass.GROUP_A:
                        save.GroupACarClass.isLocked = false;
                        break;
                }
            }

            if (selected != null)
            {
                if (selected.Status == Season.STATUS.LOCKED)
                {
                    selected.Status = Season.STATUS.UNLOCKED;
                    SaveManager.SaveSeasonData(selected);
                }

                Main.Log("Next season : " + GetSeasonCode(selected));
            }
            else
                Main.Log("No next season detected");
        }

        public static bool CheckUnlockNextGroup(Season season)
        {
            List<RallySettings> currentList = rallySettings[season.CarClass];
            RallySettings currentSettings = currentList.Find(item => string.Equals(GetSeasonCode(item.season), GetSeasonCode(season)));
            return currentList.IndexOf(currentSettings) == currentList.Count - 1;
        }

        public static void ResetRallySaves()
        {
            foreach (KeyValuePair<CarClass, List<RallySettings>> pair in rallySettings)
            {
                foreach (RallySettings settings in pair.Value)
                {
                    settings.season.Status = Season.STATUS.LOCKED;
                    settings.season.OverallStandingPlayer = 0;
                    settings.season.StageWins = 0;
                    SaveManager.SaveSeasonData(settings.season);
                }
            }

            UnlockFirstSeason();
            Main.SetField(GameModeManager.CareerManager, "CurrentSeasonInProcess", BindingFlags.Instance, null);
        }

        public static RallySettings GetRallyInProgress()
        {
            RallySettings result = null;
            int count = 0;

            foreach (CarClass carClass in Enum.GetValues(typeof(CarClass)))
            {
                if (!rallySettings.ContainsKey(carClass))
                    continue;

                RallySettings settings = rallySettings[carClass].Find(item => item.season.Status == Season.STATUS.IN_PROGRESS);

                if (settings != null)
                {
                    result = settings;
                    count++;
                }
            }

            if (count > 1)
                Main.Error("Found more than one season in progress (this shouldn't be happening");

            if (result != null)
            {
                Main.Log("Loaded custom season in progress : " + GetSeasonCode(result.season));
                return result;
            }
            else
                return null;
        }

        public static void ResetRallyInProgress()
        {
            int count = 0;

            foreach (CarClass carClass in Enum.GetValues(typeof(CarClass)))
            {
                if (!rallySettings.ContainsKey(carClass))
                    continue;

                RallySettings settings = rallySettings[carClass].Find(item => item.season.Status == Season.STATUS.IN_PROGRESS);

                if (settings != null)
                {
                    settings.season.Status = Season.STATUS.UNLOCKED;
                    SaveManager.SaveSeasonData(settings.season);
                    count++;
                }
            }

            if (count > 1)
                Main.Error("Found more than one season in progress (this shouldn't be happening");

            Main.Log("Reset rally in progress");
        }
    }
}
