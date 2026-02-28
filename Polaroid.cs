using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

using static StyleText;
using static UnityEngine.RectTransform;

namespace HistoricalCareer
{
    internal class Polaroid : MonoBehaviour
    {
        const float PICTURE_RATIO = 2.4f / 2.8f;
        const float TEXT_RATIO = 0.4f / 3.4f;

        private RectTransform polaroid;
        private RectTransform pictureFrame;
        private Image picture;
        private Text caption;

        public void SetPicture(Sprite picture, string text)
        {
            if (polaroid == null)
            {
                polaroid = GetComponent<RectTransform>();
                pictureFrame = transform.GetChild(0).GetComponent<RectTransform>();
                this.picture = pictureFrame.GetChild(0).GetComponent<Image>();

                caption = transform.GetComponentInChildren<Text>();
                caption.resizeTextForBestFit = false;

                // I don't really know why I'm doing this since it doesn't really work...
                StyleText captionStyle = caption.gameObject.AddComponent<StyleText>();
                Main.SetField(captionStyle, CareerUI.COLOR_PROP_NAME, BindingFlags.Instance, DarkType.Custom);
                Color color = Color.black;

                if (ColorUtility.TryParseHtmlString("#232524", out color))
                {
                    Main.SetField(captionStyle, CareerUI.LIGHT_PROP_NAME, BindingFlags.Instance, color);
                    Main.SetField(captionStyle, CareerUI.DARK_PROP_NAME, BindingFlags.Instance, color);
                    caption.color = color;
                }

                Main.SetField(captionStyle, CareerUI.STYLE_PROP_NAME, BindingFlags.Instance, TextType.Title);
                caption.fontSize = StyleConstants.Text.Header1.GetFontSize(StyleManager.Instance().UIScale);

                gameObject.AddComponent<StyleImageColour>();
            }

            float polaroidWidth = polaroid.rect.width;
            float polaroidHeight = polaroid.rect.height;
            float frameWidth = polaroidWidth * PICTURE_RATIO;
            float frameSpacing = (polaroidWidth - frameWidth) / 2;
            pictureFrame.SetSizeWithCurrentAnchors(Axis.Horizontal, frameWidth);
            pictureFrame.SetSizeWithCurrentAnchors(Axis.Vertical, frameWidth);
            pictureFrame.localPosition = new Vector3(0, polaroidHeight / 2 - frameWidth / 2 - frameSpacing, 0);

            float textHeight = TEXT_RATIO * polaroidHeight;
            caption.rectTransform.SetSizeWithCurrentAnchors(Axis.Horizontal, frameWidth * 0.9f);
            caption.rectTransform.SetSizeWithCurrentAnchors(Axis.Vertical, textHeight);
            caption.transform.localPosition = new Vector3(0, -polaroidHeight / 2 + textHeight / 2 + frameSpacing, 0);

            this.picture.sprite = picture;
            caption.text = text;
        }
    }
}
