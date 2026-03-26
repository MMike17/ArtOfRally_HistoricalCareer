using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace HistoricalCareer
{
    public class Settings : ModSettings, IDrawable
    {
        // [Draw(DrawType.)]

        [Header("UI")]
        [Draw(DrawType.Slider, Min = 0.5f, Max = 2)]
        public float carrouselAnimSpeed = 2;

        [Header("Debug")]
        [Draw(DrawType.Toggle)]
        public bool showMarkers;
        [Draw(DrawType.Toggle)]
        public bool disableInfoLogs = false;
        //public bool disableInfoLogs = true;
        [Draw(DrawType.Toggle)]
        public bool shortRallies = true;
        //public bool shortRallies = false;

        public override void Save(ModEntry modEntry) => Save(this, modEntry);

        public void OnChange()
        {
            Main.SetMarkers(showMarkers);

            // SnapValue(, 0.1f);
        }

        internal void OnGUI()
        {
            // custom GUI here

            if (GUILayout.Button("Reset settings", GUILayout.Width(150)))
            {
                carrouselAnimSpeed = 2;
            }

            if (GUILayout.Button("Reset saves", GUILayout.Width(150)))
                RallyManager.ResetRallySaves();
        }

        private float SnapValue(float value, float snapValue, float range, float snapPercent)
        {
            float snapDiff = range * snapPercent;
            float minTarget = snapValue - snapDiff / 2;
            float maxTarget = snapValue + snapDiff / 2;
            return value <= maxTarget && value >= minTarget ? snapValue : value;
        }
    }
}
