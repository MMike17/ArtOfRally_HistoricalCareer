using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using static AreaManager;
using static Car;
using static ConditionTypes;

namespace HistoricalCareer
{
    public class RallySettings
    {
        public CarClass carClass;
        public string areaName;
        public string rallyName;
        public string pilotName;
        public Sprite pilotPicture;
        public int pilotPictureYear;
        public int carIndex;
        public int liveryIndex;
        public int locationPictureIndex;
        public string loreText;

        public Livery livery;
        public Sprite locationPicture;
        public Season season;
        public bool needsDLC;

        public RallySettings(
            CarClass carClass,
            int year,
            AreaManager.Areas area,
            string areaName,
            string rallyName,
            string pilotName,
            Sprite pilotPicture,
            int pilotPictureYear,
            int carIndex,
            int liveryIndex,
            int locationPictureIndex,
            int[] stagesIndeces,
            ConditionTypes.Weather[] weathers,
            int restarts,
            string loreText
        )
        {
            this.carClass = carClass;
            this.areaName = areaName;
            this.rallyName = rallyName;
            this.pilotName = pilotName;
            this.pilotPicture = pilotPicture;
            this.pilotPictureYear = pilotPictureYear;
            this.carIndex = carIndex;
            this.liveryIndex = liveryIndex;
            this.loreText = loreText;

            livery = GetCarLiveries(CarManager.GetCurrentCarsListForClass(carClass)[carIndex].prefabName)[liveryIndex];

            string stagePrefix = AreaManager.AreaDictionary[area].stageList[locationPictureIndex].LeaderboardStagePrefixString;
            string locationPicturePath = "Sprites/TrackBackgrounds/" + area.ToString().ToLower() + "_" + stagePrefix[stagePrefix.Length - 1];
            locationPicture = Resources.Load<Sprite>(locationPicturePath);

            // check stages
            List<int> correctedStages = new List<int>();
            int maxIndex = AreaManager.AreaDictionary[area].stageList.Count;

            foreach (int index in stagesIndeces)
            {
                if (index >= maxIndex)
                    Main.Error("Stage index " + index + " is out of bounds (" + maxIndex + "), the stage will be ignored.");
                else
                    correctedStages.Add(index);
            }

            stagesIndeces = correctedStages.ToArray();

            // check weathers
            List<ConditionTypes.Weather> areaWeathers = AreaManager.GetWeatherForCurrentArea(area);
            List<ConditionTypes.Weather> correctedWeathers = new List<ConditionTypes.Weather>();

            foreach (ConditionTypes.Weather weather in weathers)
            {
                if (!areaWeathers.Contains(weather))
                    Main.Error("Weather " + weather + " is invalid for this area (" + area + "), the weather will be ignored.");
                else
                    correctedWeathers.Add(weather);
            }

            weathers = correctedWeathers.ToArray();

            // setup season
            season = new Season(
                year,
                carClass,
                1,
                Main.settings.shortRallies ? 1 : stagesIndeces.Length,
                restarts,
                "UNLOCKABLE_" + year + "_BONUS",
                true,
                AIDriverSkillTables.AI_Skill.EASY
            );

            // setup rally
            season.Rallies.Add(new RallyData());
            season.Rallies[0].SetArea((int)area);
            season.Rallies[0].SetStageCount(Main.settings.shortRallies ? 1 : stagesIndeces.Length);

            for (int i = 0; i < (Main.settings.shortRallies ? 1 : stagesIndeces.Length); i++)
            {
                int index = stagesIndeces[i];
                Stage stage = AreaManager.GetStageByIndex(ref index, area);
                season.Rallies[0].SetStage(i, stage);
                season.Rallies[0].SetWeatherForStage(i, weathers[i]);
            }

            SaveManager.LoadSeasonData(season);

            // checks for DLC use
            needsDLC = area == AreaManager.Areas.AUSTRALIA;

            if (!needsDLC)
            {
                switch (carClass)
                {
                    case CarClass.GROUP_2:
                        needsDLC = carIndex == 7;
                        break;

                    case CarClass.GROUP_4:
                        needsDLC = carIndex == 10;
                        break;

                    case CarClass.GROUP_B:
                        needsDLC = carIndex == 16;
                        break;

                    case CarClass.GROUP_A:
                        needsDLC = carIndex == 5;
                        break;
                }
            }
        }

        private static List<Livery> GetCarLiveries(string carName)
        {
            List<Livery> liveries = new List<Livery>();

            foreach (string textureName in LiveryManager.GenericTextures)
            {
                liveries.Add(new Livery(
                    textureName,
                    LiveryManager.GetStandardLiveryPath(carName, textureName),
                    Livery.LiveryType.Standard,
                    carName,
                    false
                ));
            }

            return liveries;
        }

        public static void GenerateGroup2Seasons(Assembly assembly, string pilotPicturePath, Func<float, int, int> ComputeRestarts)
        {
            CarClass group = CarClass.GROUP_2;

            RallyManager.AddCustomRally(
                group, 1966, Areas.FINLAND, "Finland", "1000 Lakes rally", "Timo Mäkinen",
                assembly, pilotPicturePath, 1966, 1, 0, 2,
                new int[] { 0, 6 }, new Weather[] { Weather.Morning, Weather.Afternoon }, ComputeRestarts(0 / 5f, 2),
                "Following their 1965 win, <b>Timo Mäkinen</b> and his copilot <b>Pekka Keskitalo</b> took advantage of their Mini's front-wheel-drive and light body to excell on their home country's gravel jumps, becoming the first \"flying finns\" with <b>Simo Lampinen</b> and <b>Rauno Aaltonen</b>, who placed 3rd on the same rally."
            );
            RallyManager.AddCustomRally(
                group, 1967, Areas.SARDINIA, "Italy", "Rally dei Fiori", "Jean-François Piot",
                assembly, pilotPicturePath, 1967, 5, 1, 0,
                new int[] { 2, 6, 0 }, new Weather[] { Weather.Fog, Weather.Afternoon, Weather.Sunset }, ComputeRestarts(1 / 5f, 3),
                "After winning the <b>Tour de Corse</b> and the <b>Coupe de Alpes</b> the previous year, <b>Jean-François Piot</b> joined by copilot <b>Nicolas Roure</b> armed with a prototype Renault 8 Gordini 1440 dominated tarmac-gravel mixed stages, beating its competition in the mediterranean conditions."
            );
            RallyManager.AddCustomRally(
                group, 1968, Areas.GERMANY, "West Germany", "Wiesbaden German Rally", "Pauli Toivonen",
                assembly, pilotPicturePath, 1969, 4, 5, 0, new int[] { 0, 5, 3, 11 },
                new Weather[] { Weather.Rain, Weather.Sunset, Weather.Fog, Weather.Afternoon }, ComputeRestarts(2 / 5f, 4),
                "For his debut in a Porsche, <b>Pauli Toivonen</b> couldn't secure a <b>Monte-carlo</b> win, being overtaken by <b>Vic Elford</b>. But with the help of <b>Martti Kolari</b> as copilot, they managed a win in the <b>West German rally</b>. Pauli continued winning that same year in Austria and Swizerland with different copilots."
            );
            RallyManager.AddCustomRally(
                group, 1969, Areas.SARDINIA, "Italy", "Rally Sanremo", "Harry Källström",
                assembly, pilotPicturePath, 1970, 6, 3, 0, new int[] { 2, 6, 10, 0 },
                new Weather[] { Weather.Sunset, Weather.Afternoon, Weather.Sunset, Weather.Afternoon }, ComputeRestarts(3 / 5f, 4),
                "Nicknamed \"Sputnik\" because of how fast his career took off, driver and later actorn <b>Harry Källström</b> took the wheel of a Lancia Fulvia with his copilot <b>Häggbom Gunnar</b> to win the <b>Sanremo Rally</b> as well as the <b>Spanish RACE Rally</b> and <b>British RAC Rally</b> that year, earning their first European champions title."
            );
            RallyManager.AddCustomRally(
                group, 1970, Areas.FINLAND, "Finland", "1000 Lakes rally", "Hannu Mikkola",
                assembly, pilotPicturePath, 1966, 0, 1, 2, new int[] { 0, 5, 6, 8, 2 },
                new Weather[] { Weather.Morning, Weather.Afternoon, Weather.Morning, Weather.Sunset, Weather.Rain }, ComputeRestarts(4 / 5f, 5),
                "After their win in the grueling <b>London-Mexico World Cup Rally</b>, <b>Hannu Mikkola</b> and the exceptional navigator <b>Gunnar Palm</b> brought their Ford Escort twin cam to Finland. With its engine originally developped for the Lotus Elan and its lights body shell the Escort proved to be a solid contender for the rougher rallies."
            );
            RallyManager.AddCustomRally(
                group, 1970, Areas.GERMANY, "Austria", "Rally Munich-Vienne-Budapest", "Jean-Claude Andruet",
                assembly, pilotPicturePath, 1977, 2, 5, 6, new int[] { 1, 8, 10, 5, 7 },
                new Weather[] { Weather.Rain, Weather.Morning, Weather.Rain, Weather.Sunset, Weather.Fog }, ComputeRestarts(5 / 5f, 5),
                "After dominating in the french rallies, <b>Jean-Claude Andruet</b> wildly swung his <b>Alpine A110</b> with <b>Michèle Veron</b> as copilot in this rally spanning between Germany, Austria and Hungary. The battle with Fords and Porsches was fierce but they won the French and European rally championships titles."
            );
        }

        public static void GenerateGroup3Seasons(Assembly assembly, string pilotPicturePath, Func<float, int, int> ComputeRestarts)
        {
            CarClass group = CarClass.GROUP_3;

            RallyManager.AddCustomRally(
                group, 1971, Areas.KENYA, "Kenya", "Safari rally", "Edgar Herrmann",
                assembly, pilotPicturePath, 1970, 9, 0, 4, new int[] { 4, 6 },
                new Weather[] { Weather.Sunset, Weather.Rain }, ComputeRestarts(0 / 9f, 2),
                "German born pilot Edgar Hermann shelved his <b>Porsche 911</b> for a <b>Datsun 1600 SSS</b>, landing him a series of wins in <b>1970</b> before winning a second <b>Safari rally</b> behind the wheel of a <b>Datsun 240Z</b>, guided by <b>Hans Schuller</b>. Hard-eyed, confident and said to be trailed by good-looking women, Edgar ended up falling in love with Kenya from which he was eventually naturalized."
            );

            // TODO : Finish designing rallies for group 3
        }

        public static void GenerateGroup4Seasons(Assembly assembly, string pilotPicturePath, Func<float, int, int> ComputeRestarts)
        {
            CarClass group = CarClass.GROUP_4;

            // TODO : Finish designing rallies for group 4
        }

        public static void GenerateGroupBSeasons(Assembly assembly, string pilotPicturePath, Func<float, int, int> ComputeRestarts)
        {
            CarClass group = CarClass.GROUP_B;

            // TODO : Finish designing rallies for group B
        }

        public static void GenerateGroupSSeasons(Assembly assembly, string pilotPicturePath, Func<float, int, int> ComputeRestarts)
        {
            CarClass group = CarClass.GROUP_S;

            // TODO : Finish designing rallies for group S
        }

        public static void GenerateGroupASeasons(Assembly assembly, string pilotPicturePath, Func<float, int, int> ComputeRestarts)
        {
            CarClass group = CarClass.GROUP_A;

            // TODO : Finish designing rallies for group A
        }
    }
}
