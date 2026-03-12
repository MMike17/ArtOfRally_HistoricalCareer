using UnityEngine;

namespace HistoricalCareer
{
    // This is replacing the game's save system
    internal class SaveManager
    {
        public static bool HasSaves(Season season) => PlayerPrefs.HasKey(RallyManager.GetSeasonCode(season));

        public static void SetSeasonStatus(Season season, Season.STATUS status)
        {
            PlayerPrefs.SetInt(RallyManager.GetSeasonCode(season), (int)status);
            PlayerPrefs.Save();
        }

        public static Season.STATUS GetSeasonStatus(Season season) => (Season.STATUS)PlayerPrefs.GetInt(RallyManager.GetSeasonCode(season), 0);

        public static Season.STATUS GetSeasonStatus(Car.CarClass carClass, int year, AreaManager.Areas area)
        {
            return (Season.STATUS)PlayerPrefs.GetInt(RallyManager.GetSeasonCode(carClass, year, area));
        }
    }
}