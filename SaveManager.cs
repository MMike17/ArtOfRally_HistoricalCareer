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

        public static void SaveSeasonData(RallySettings settings)
        {
            string seasonCode = RallyManager.GetSeasonCode(settings);
            SeasonData newData = new SeasonData(settings.season);
            SeasonData data = JsonUtility.FromJson<SeasonData>(
                PlayerPrefs.GetString(seasonCode, newData.ToString()
            ));

            if (newData.partsCondition == null && data.partsCondition != null)
                newData.partsCondition = data.partsCondition;

            PlayerPrefs.SetString(seasonCode, newData.ToString());
            PlayerPrefs.Save();
        }

        public static void SaveSeasonData(Season season) => SaveSeasonData(RallyManager.GetSettingsFromSeason(season));

        public static void LoadSeasonData(RallySettings settings)
        {
            SeasonData data = JsonUtility.FromJson<SeasonData>(
                PlayerPrefs.GetString(
                    RallyManager.GetSeasonCode(settings),
                    new SeasonData(settings.season).ToString()
                )
            );

            settings.season.Status = data.status;
            settings.season.StageWins = data.stageWins;
            settings.season.OverallStandingPlayer = data.standingPlayer;
            settings.season.Rallies[0].CurrentStageIndex = data.currentStageIndex;
            settings.season.DriverList = data.driverList;

            if (settings.season.SelectedCar != null && data.partsCondition != null)
                settings.season.SelectedCar.performancePartsCondition = data.partsCondition;
        }

        public static void LoadSeasonData(Season season) => LoadSeasonData(RallyManager.GetSettingsFromSeason(season));

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