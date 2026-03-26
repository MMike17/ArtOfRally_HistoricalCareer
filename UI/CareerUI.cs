using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

using static StyleText;

namespace HistoricalCareer
{
    public class CareerUI : MonoBehaviour
    {
        public const string STYLE_PROP_NAME = "_scaleType";
        public const string COLOR_PROP_NAME = "_darkType";
        public const string LIGHT_PROP_NAME = "_customDarkModeOff";
        public const string DARK_PROP_NAME = "_customDarkModeOn";

        public static UIScale uiScale { get; private set; }

        private CanvasGroup panelGroup;
        private Panel panel;
        private Text seasonDate;
        private Text rallyName;
        private Polaroid environmentPolaroid;
        private Polaroid pilotPolaroid;
        private Image carPicture;
        private Text contextText;
        private Action StartEvent;

        public void Set(RallySettings settings, Action<RallySettings> startEvent)
        {
            StartEvent = () => startEvent?.Invoke(settings);

            if (seasonDate == null)
            {
                panelGroup = GetComponent<CanvasGroup>();
                panel = gameObject.AddComponent<Panel>();

                Transform titleHolder = transform.GetChild(0);
                uiScale = StyleManager.Instance().UIScale;

                seasonDate = titleHolder.GetChild(0).GetComponent<Text>();
                seasonDate.resizeTextForBestFit = true;
                seasonDate.font = PanelPatcher.titleFont;
                StyleText seasonStyle = seasonDate.gameObject.AddComponent<StyleText>();
                Main.SetField(seasonStyle, STYLE_PROP_NAME, BindingFlags.Instance, TextType.StageTitle);
                Main.SetField(seasonStyle, COLOR_PROP_NAME, BindingFlags.Instance, DarkType.Custom);
                Main.SetField(
                    seasonStyle,
                    LIGHT_PROP_NAME,
                    BindingFlags.Instance,
                    ColorsConstants.HueColourValue(ColorsConstants.HueColorNames.Selected_Color)
                );
                Main.SetField(
                    seasonStyle,
                    DARK_PROP_NAME,
                    BindingFlags.Instance,
                    ColorsConstants.HueColourValue(ColorsConstants.HueColorNames.Selected_Color)
                );
                seasonDate.color = Main.InvokeMethod<StyleText, Color>(
                    seasonStyle,
                    "GetCustomColour",
                    BindingFlags.Instance,
                    new object[] { StyleManager.Instance().DarkMode }
                );

                rallyName = titleHolder.GetChild(1).GetComponent<Text>();
                rallyName.font = PanelPatcher.titleFont;
                StyleText rallyStyle = rallyName.gameObject.AddComponent<StyleText>();
                Main.SetField(rallyStyle, STYLE_PROP_NAME, BindingFlags.Instance, TextType.StageTitle);
                rallyName.fontSize = StyleConstants.Text.StageTitle.GetFontSize(uiScale);

                Transform displayHolder = transform.GetChild(1);
                environmentPolaroid = displayHolder.GetChild(0).gameObject.AddComponent<Polaroid>();
                pilotPolaroid = displayHolder.GetChild(1).gameObject.AddComponent<Polaroid>();
                carPicture = displayHolder.GetChild(2).GetChild(0).GetComponent<Image>();

                contextText = transform.GetChild(2).GetComponent<Text>();
                contextText.font = PanelPatcher.bodyFont;
                StyleText contextStyle = contextText.gameObject.AddComponent<StyleText>();
                Main.SetField(contextStyle, STYLE_PROP_NAME, BindingFlags.Instance, TextType.Header1);
                contextText.fontSize = StyleConstants.Text.Header1.GetFontSize(uiScale);

                panel.Show();
            }

            seasonDate.text = settings.season.Year.ToString();
            rallyName.text = settings.rallyName;
            environmentPolaroid.SetPicture(settings.locationPicture, settings.areaName);
            pilotPolaroid.SetPicture(settings.pilotPicture, settings.pilotName + " (" + settings.pilotPictureYear + ")");
            carPicture.sprite = RallyManager.GetCarSprite(settings.carClass, settings.carIndex);
            contextText.text = settings.loreText;

            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (panelGroup.alpha < 0.1f)
                return;

            if (PanelPatcher.playerInput.GetButtonDown(PanelPatcher.submitUIString))
            {
                StartEvent?.Invoke();
                StartEvent = null;
            }

            if (PanelPatcher.playerInput.GetButtonDown(PanelPatcher.cancelUIString))
                panel.Hide();
        }
    }
}
