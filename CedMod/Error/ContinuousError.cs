using UnityEngine;

namespace CedMod.Error
{
    public class ContinuousError : MonoBehaviour
    {
        private const int TimeBetween = 60;

        private float _remaining = TimeBetween;

        private void Update()
        {
            _remaining -= Time.unscaledDeltaTime;

            if (_remaining > 0) return;
            _remaining = TimeBetween;
            ErrorCollector.SendErrors();
        }
    }
}