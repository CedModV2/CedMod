using System.Collections.Generic;
using UnityEngine;

namespace CedMod
{
    public class RainbowLight : MonoBehaviour
    {
        public static readonly List<RainbowLight> Instances = new List<RainbowLight>();
        public float saturation = 1f;
        public float hueShiftSpeed = 0.2f;
        public float value = 1f;

        private FlickerableLightController _light;
        public FlickerableLightController Light
        {
            get
            {
                if (_light == null)
                    _light = GetComponent<FlickerableLightController>();
                return _light;
            }
        }

        private void Awake()
        {
            Instances.Add(this);
            Light.Network_warheadLightOverride = true;
        }

        private void OnDestroy()
        {
            Instances.Remove(this);
        }

        private void Update()
        {
            float amountToShift = hueShiftSpeed * Time.deltaTime;
            Color newColor = ShiftHueBy(Light.Network_warheadLightColor, amountToShift);
            Light.Network_warheadLightColor = newColor;
        }

        private Color ShiftHueBy(Color color, float amount)
        {
            // convert from RGB to HSV
            Color.RGBToHSV(color, out float hue, out float sat, out float val);

            // shift hue by amount
            hue += amount;
            sat = saturation;
            val = value;

            // convert back to RGB and return the color
            return Color.HSVToRGB(hue, sat, val);
        }
    }
}
