using System.Collections.Generic;

namespace HistoricalCareer
{
    public class RallySettings
    {
        public Car.CarClass carClass; // TODO : do we really need this ?
        public Car car;
        public Livery livery;
        public Season season;

        /// <param name="car">Use CarManager.GetCurrentCarsListForClass to get cars</param>
        /// <param name="livery">Use RallySettings.GetCarLiveries (with car.prefabName) to get car liveries</param>
        /// <param name="stagesIndeces">No double stages, stages go by pairs (0-1, 2-3, etc...)</param>
        /// <param name="weathers">Use AreaManager.GetWeatherForCurrentArea to get valid weathers</param>
        public RallySettings(int year, Car car, Livery livery, AreaManager.Areas area, int[] stagesIndeces, ConditionTypes.Weather[] weathers)
        {
            carClass = car.carClass;
            this.car = car;
            this.livery = livery;

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
            string bonusSaveConstant = null; // TODO : What the hell is supposed to be the BonusSaveConstant ?
            Season.STATUS status = Season.STATUS.UNLOCKED; // TODO : Manage season state from the save system

            season = new Season(
                year,
                carClass,
                1, // TODO : Not sure if I need to be able to go above 1 rally
                stagesIndeces.Length,
                restarts,
                bonusSaveConstant,
                true,
                AIDriverSkillTables.AI_Skill.EASY,
                status
            );

            // setup rally
            season.Rallies[0] = new RallyData();
            season.Rallies[0].SetArea((int)area);
            season.Rallies[0].SetStageCount(stagesIndeces.Length);

            for (int i = 0; i < stagesIndeces.Length; i++)
            {
                int index = stagesIndeces[i];
                Stage stage = AreaManager.GetStageByIndex(ref index, area);
                season.Rallies[0].SetStage(i, stage);
                season.Rallies[0].SetWeatherForStage(i, weathers[i]);
            }
        }

        // TODO : I'm not sure this thing is working correctly...
        public static List<Livery> GetCarLiveries(string carName)
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
