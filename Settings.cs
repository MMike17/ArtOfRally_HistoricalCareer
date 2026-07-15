using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace HistoricalCareer
{
    public class Settings : ModSettings, IDrawable
    {
        // [Draw(DrawType.)]

        [Header("UI")]
        [Draw(DrawType.Slider, Min = 0.5f, Max = 3)]
        public float carrouselAnimSpeed = 2;

        [Header("Debug")]
        [Draw(DrawType.Toggle)]
        public bool disableInfoLogs = true;
        [Draw(DrawType.Toggle)]
        public bool shortRallies = false;

        public override void Save(ModEntry modEntry) => Save(this, modEntry);

        public void OnChange()
        {
            // SnapValue(, 0.1f);
        }

        internal void OnGUI()
        {
            // custom GUI here

            if (GUILayout.Button("Reset settings", GUILayout.Width(200)))
            {
                carrouselAnimSpeed = 2;
            }

            if (GUILayout.Button("Reset saves", GUILayout.Width(150)))
                RallyManager.ResetRallySaves();

            if (GUILayout.Button("Unlock all rallies", GUILayout.Width(250)))
            {
                RallyManager.UnlockAllRallies();
            }
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
