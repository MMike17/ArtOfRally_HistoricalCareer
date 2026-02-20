using System.Collections.Generic;
using Rewired.Integration.UnityUI;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Reflection;

namespace HistoricalCareer
{
    // This is placed on the layout group
    internal class CarrouselUI : MonoBehaviour
    {
        const float SEMI_SELECTED_SIZE = 0.8f;
        const float SEMI_SELECTED_ALPHA = 0.7f;
        const float NON_SELECTED_SIZE = 0.6f;
        const float NON_SELECTED_ALPHA = 0.4f;
        const float MOVE_RATIO = 1000;
        const float INPUT_DELAY_THRESHOLD = 0.2f;

        public static int selectedIndex { get; private set; }

        private List<Panel> panels;
        private BaseInputModule inputModule;
        private Player playerInput;
        private string horizontalUIString;
        private string submitUIString;
        private float delay;

        private void Awake()
        {
            inputModule = EventSystem.current.currentInputModule;
            horizontalUIString = Main.GetField<string, RewiredStandaloneInputModule>(
                inputModule as RewiredStandaloneInputModule,
                "m_HorizontalAxis",
                BindingFlags.Instance
            );
            submitUIString = Main.GetField<string, RewiredStandaloneInputModule>(
                inputModule as RewiredStandaloneInputModule,
                "m_SubmitButton",
                BindingFlags.Instance
            );
            int playerID = Main.GetField<int[], RewiredStandaloneInputModule>(
                inputModule as RewiredStandaloneInputModule,
                "playerIds",
                BindingFlags.Instance
            )[0];
            playerInput = ReInput.players.GetPlayer(playerID);
        }

        public void Reset()
        {
            selectedIndex = 0;
            panels = new List<Panel>();

            // animate panels
            int under = selectedIndex - 1;
            int over = selectedIndex < panels.Count - 1 ? selectedIndex + 1 : -1;

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeInHierarchy)
                {
                    panels.Add(new Panel(
                        transform.GetChild(i),
                        i == selectedIndex ? 1 : i == under || i == over ? SEMI_SELECTED_SIZE : NON_SELECTED_SIZE,
                        i == selectedIndex ? 1 : i == under || i == over ? SEMI_SELECTED_ALPHA : NON_SELECTED_ALPHA
                    ));
                }
            }

            // move carrousel
            Transform selected = panels[selectedIndex].transform;
            float offset = transform.position.x - selected.position.x;

            Vector3 target = transform.position;
            target.x = Screen.width / 2 + offset;
            transform.position = target;
        }

        private void Update()
        {
            // move carrousel
            Transform selected = panels[selectedIndex].transform;
            float offset = transform.position.x - selected.position.x;

            Vector3 target = transform.position;
            target.x = Screen.width / 2 + offset;
            transform.position = Vector3.MoveTowards(transform.position, target, MOVE_RATIO * Main.settings.carrouselAnimSpeed * Time.deltaTime);

            // animate panels
            int under = selectedIndex - 1;
            int over = selectedIndex < panels.Count - 1 ? selectedIndex + 1 : -1;

            for (int i = 0; i < panels.Count; i++)
            {
                panels[i].Update(
                    i == selectedIndex ? 1 : i == under || i == over ? SEMI_SELECTED_SIZE : NON_SELECTED_SIZE,
                    i == selectedIndex ? 1 : i == under || i == over ? SEMI_SELECTED_ALPHA : NON_SELECTED_ALPHA,
                    (i == selectedIndex ? Main.settings.carrouselAnimSpeed * 1.5f : Main.settings.carrouselAnimSpeed) * Time.deltaTime
                );
            }

            // input
            if (delay > 0)
                delay = Mathf.Clamp01(delay - Time.deltaTime);

            if (delay == 0 && playerInput.GetAxis(horizontalUIString) > 0 && selectedIndex < panels.Count - 1)
            {
                selectedIndex++;
                delay = INPUT_DELAY_THRESHOLD;
            }

            if (delay == 0 && playerInput.GetAxis(horizontalUIString) < 0 && selectedIndex > 0)
            {
                selectedIndex--;
                delay = INPUT_DELAY_THRESHOLD;
            }

            // TODO : I feel like this is double working when I already have a career season
            if (playerInput.GetButtonDown(submitUIString))
                transform.GetComponentInParent<SeasonDashboardUI>().OnSeasonClicked(panels[selectedIndex].season);
        }

        private class Panel
        {
            public Transform transform;
            public CanvasGroup group;
            public Season season;

            public Panel(Transform transform, float startSize, float startAlpha)
            {
                this.transform = transform;
                transform.localScale = Vector3.one * startSize;

                group = transform.GetComponent<CanvasGroup>();
                group.alpha = startAlpha;

                if (group == null)
                    group = transform.gameObject.AddComponent<CanvasGroup>();

                CustomButtonSeason button = transform.GetComponent<CustomButtonSeason>();
                button.enabled = false;
                season = Main.GetField<Season, CustomButtonSeason>(button, "currentSeason", BindingFlags.Instance);
            }

            public void Update(float size, float alpha, float speed, bool immediate = false)
            {
                transform.localScale = immediate ? Vector3.one * size : Vector3.MoveTowards(transform.localScale, Vector3.one * size, speed);
                group.alpha = immediate ? alpha : Mathf.MoveTowards(group.alpha, alpha, speed);
            }
        }
    }
}
