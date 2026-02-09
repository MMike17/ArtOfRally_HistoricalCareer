using System.Collections.Generic;

namespace HistoricalCareer
{
    public class RallySettings
    {
        public RallyData rallyData;
        public int carIndex; // car order per car class is static
        public int liveryIndex; // LiveryManager.GetValidLiveries(<car name>(GameModeManager.GetSeasonDataCurrentGameMode().SelectedCar.prefabName), false);

        // TODO : How do I generate the rallies that I want ?
        // pick area from enum
        // pick stages from list
        // pick weathers

        public RallySettings(AreaManager.Areas area, List<int> stagesIndeces, List<ConditionTypes.Weather> weathers)
        {
            // TODO : What are the rally making rules ?
            // No double stage (same stage, same direction) in a rally
            // stages list is static

            rallyData = new RallyData();
            rallyData.SetArea((int)area);
            rallyData.SetStageCount(stagesIndeces.Count);

            //AreaManager.GetStageByIndex(ref int index, area)
            //AreaManager.AreaDictionary[area].stageList
            //AreaManager.AreaDictionary[area].weatherList

            for (int i = 0; i < stagesIndeces.Count; i++)
            {
                int index = stagesIndeces[i];
                Stage stage = AreaManager.GetStageByIndex(ref index, area);
                rallyData.SetStage(i, stage);
                rallyData.SetWeatherForStage(i, weathers[i]);
            }
        }
    }
}
