using System.Collections.Generic;
using UnityEngine;

namespace CedMod
{
    /// <summary>
    /// The rainbow light component. Attached to lights.
    /// </summary>
    public class RainbowLight : MonoBehaviour
    {
        public static readonly List<RainbowLight> Instances = new List<RainbowLight>();
        public float saturation = 1f;
        public float hueShiftSpeed = 0.2f;
        public float value = 1f;

        private FlickerableLightController _light;
        /// <summary>
        /// Gets the <see cref="FlickerableLightController"/> component of this <see cref="GameObject"/>.
        /// </summary>
        public FlickerableLightController Light
        {
            get
            {
                if (_light == null)
                    _light = GetComponent<FlickerableLightController>();
                return _light;
            }
        }

        /// <summary>
        /// Called when the component is added.
        /// </summary>
        private void Awake()
        {
            Instances.Add(this);
            Light.Network_warheadLightOverride = true;
        }

        /// <summary>
        /// Called when the component is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Instances.Remove(this);
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        private void Update()
        {
            float amountToShift = hueShiftSpeed * Time.deltaTime;
            Color newColor = ShiftHueBy(Light.Network_warheadLightColor, amountToShift);
            Light.Network_warheadLightColor = newColor;
        }

        /// <summary>
        /// Shits the hue of the <see cref="Color"/> by the specified amount.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to shift.</param>
        /// <param name="amount">The amount to shift by.</param>
        /// <returns>The shifted <see cref="Color"/>.</returns>
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
