using System;
using UnityEngine;
using UnityEngine.UI;

namespace HistoricalCareer
{
    public class CareerUI : MonoBehaviour
    {
        private Text seasonDate;
        private Text rallyName;
        private Polaroid environmentPolaroid;
        private Polaroid pilotPolaroid;
        private Image carPicture;
        private Text contextText;
        private Action StartEvent;

        // TODO : Add StyleImageColour to polaroids when spawning window
        // off color = #FFFFFFFF
        // on color = #D3D3D3FF

        public void Set(RallySettings settings, Action<RallySettings> startEvent)
        {
            StartEvent = () => startEvent?.Invoke(settings);

            if (seasonDate == null)
            {
                Transform titleHolder = transform.GetChild(0);
                seasonDate = titleHolder.GetChild(0).GetComponent<Text>();
                rallyName = titleHolder.GetChild(1).GetComponent<Text>();

                Transform displayHolder = transform.GetChild(1);
                environmentPolaroid = displayHolder.GetChild(0).gameObject.AddComponent<Polaroid>();
                pilotPolaroid = displayHolder.GetChild(1).gameObject.AddComponent<Polaroid>();
                carPicture = displayHolder.GetChild(2).GetComponent<Image>();

                contextText = transform.GetChild(2).GetComponent<Text>();
            }

            seasonDate.text = settings.season.Year.ToString();
            rallyName.text = settings.rallyName;
            environmentPolaroid.SetPicture(null, settings.season.Rallies[0].CurrentArea.ToString()); // TODO : How do I source the country pictures ?
            pilotPolaroid.SetPicture(settings.pilotPicture, settings.pilotName + "(" + settings.pilotPictureYear + ")");
            //carPicture.sprite = ; // TODO : How do I get the car picture ?
            contextText.text = settings.loreText;

            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (PanelPatcher.playerInput.GetButtonDown(PanelPatcher.submitUIString))
            {
                StartEvent?.Invoke();
                StartEvent = null;
            }
        }
    }
}
