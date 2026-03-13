using System;
using UnityEngine;

namespace HistoricalCareer
{
    // This is replacing the game's save system
    internal class SaveManager
    {
        public static bool HasSaves(Season season) => PlayerPrefs.HasKey(RallyManager.GetSeasonCode(season));

        public static void SaveSeasonData(Season season)
        {
            PlayerPrefs.SetString(RallyManager.GetSeasonCode(season), new SeasonData(season).ToString());
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
        }

        [Serializable]
        public class SeasonData
        {
            public Season.STATUS status;
            public int stageWins;
            public int standingPlayer;

            public SeasonData(Season season)
            {
                status = season.Status;
                stageWins = season.StageWins;
                standingPlayer = season.OverallStandingPlayer;
            }

            public override string ToString() => JsonUtility.ToJson(this);
        }
    }
}