using System;
using System.Collections.Generic;
using UnityEngine;

using static Car;

namespace HistoricalCareer
{
    // This is replacing the game's save system
    internal class SaveManager
    {
        public static bool HasSaves(Season season) => PlayerPrefs.HasKey(RallyManager.GetSeasonCode(season));

        public static void SaveSeasonData(Season season)
        {
            SeasonData newData = new SeasonData(season);
            SeasonData data = JsonUtility.FromJson<SeasonData>(
                PlayerPrefs.GetString(RallyManager.GetSeasonCode(season), newData.ToString()
            ));

            if (newData.partsCondition == null && data.partsCondition != null)
                newData.partsCondition = data.partsCondition;

            PlayerPrefs.SetString(RallyManager.GetSeasonCode(season), newData.ToString());
            PlayerPrefs.Save();
        }

        public static void LoadSeasonData(Season season)
        {
            SeasonData data = JsonUtility.FromJson<SeasonData>(
                PlayerPrefs.GetString(RallyManager.GetSeasonCode(season), new SeasonData(season).ToString()
            ));

            season.Status = data.status;
            season.StageWins = data.stageWins;
            season.OverallStandingPlayer = data.standingPlayer;
            season.Rallies[0].CurrentStageIndex = data.currentStageIndex;
            season.DriverList = data.driverList;

            if (season.SelectedCar != null && data.partsCondition != null)
                season.SelectedCar.performancePartsCondition = data.partsCondition;
        }

        [Serializable]
        public class SeasonData
        {
            public Season.STATUS status;
            public int stageWins;
            public int standingPlayer;
            public int currentStageIndex;
            public List<Driver> driverList;
            public PerformancePartsCondition partsCondition;

            public SeasonData(Season season)
            {
                status = season.Status;
                stageWins = season.StageWins;
                standingPlayer = season.OverallStandingPlayer;
                currentStageIndex = season.Rallies[0].CurrentStageIndex;
                driverList = season.DriverList;

                if (season.SelectedCar != null)
                    partsCondition = season.SelectedCar.performancePartsCondition;
            }

            public override string ToString() => JsonUtility.ToJson(this);
        }
    }
}