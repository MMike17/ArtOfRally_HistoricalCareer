using System.Collections.Generic;
using UnityEngine;

using static Car;

namespace HistoricalCareer
{
    public class RallySettings
    {
        public CarClass carClass;
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

        /// <param name="pilotPictureYear">The year the pilot picture was taken in</param>
        /// <param name="carIndex">Index of the car in its era list (check CarManager.Init)</param>
        /// <param name="locationPictureIndex">Index of the stage in its area, stages go by pairs (0-1, 2-3, etc...)</param>
        /// <param name="stagesIndeces">No double stages, stages go by pairs (0-1, 2-3, etc...)</param>
        /// <param name="weathers">Use AreaManager.GetWeatherForCurrentArea to get valid weathers</param>

        public RallySettings(
            CarClass carClass,
            int year,
            AreaManager.Areas area,
            string rallyName,
            string pilotName,
            Sprite pilotPicture,
            int pilotPictureYear,
            int carIndex,
            int liveryIndex,
            int locationPictureIndex,
            int[] stagesIndeces,
            ConditionTypes.Weather[] weathers,
            string loreText
        )
        {
            this.carClass = carClass;
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
            int restarts = 0; // TODO : What's the OG number of restarts for that season ? Do I need to change that ?
            Season.STATUS status = Season.STATUS.UNLOCKED; // TODO : Manage season state from the save system

            season = new Season(
                year,
                carClass,
                1,
                stagesIndeces.Length,
                restarts,
                "UNLOCKABLE_" + year + "_BONUS",
                true,
                AIDriverSkillTables.AI_Skill.EASY,
                status
            );

            // setup rally
            season.Rallies.Add(new RallyData());
            season.Rallies[0].SetArea((int)area);
            season.Rallies[0].SetStageCount(stagesIndeces.Length);

            for (int i = 0; i < stagesIndeces.Length; i++)
            {
                int index = stagesIndeces[i];
                Stage stage = AreaManager.GetStageByIndex(ref index, area);
                season.Rallies[0].SetStage(i, stage);
                season.Rallies[0].SetWeatherForStage(i, weathers[i]);
            }

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
    }
}
