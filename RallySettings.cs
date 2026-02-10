using System.Collections.Generic;

namespace HistoricalCareer
{
    public class RallySettings
    {
        public Car.CarClass carClass;
        // TODO : Not sure if I should store the car and the livery or their index...
        public Car car;
        public Livery livery;
        public RallyData rallyData;

        /// <param name="car">Use CarManager.GetCurrentCarsListForClass to get cars</param>
        /// <param name="livery">Use RallySettings.GetCarLiveries (with car.prefabName) to get car liveries</param>
        /// <param name="weathers">Use AreaManager.GetWeatherForCurrentArea to get valid weathers</param>
        public RallySettings(Car car, Livery livery, AreaManager.Areas area, int[] stagesIndeces, ConditionTypes.Weather[] weathers)
        {
            carClass = car.carClass;
            this.car = car;
            this.livery = livery;

            // TODO : What are the rally making rules ?
            // No double stage (same stage, same direction) in a rally
            // check if stages indeces are in bounds (stages list is static)
            // check if weathers are in bounds (weather list is static)

            rallyData = new RallyData();
            rallyData.SetArea((int)area);
            rallyData.SetStageCount(stagesIndeces.Length);

            //AreaManager.AreaDictionary[area].stageList.Count

            for (int i = 0; i < stagesIndeces.Length; i++)
            {
                int index = stagesIndeces[i];
                Stage stage = AreaManager.GetStageByIndex(ref index, area);
                rallyData.SetStage(i, stage);
                rallyData.SetWeatherForStage(i, weathers[i]);
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
