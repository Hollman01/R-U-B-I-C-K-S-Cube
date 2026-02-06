using System.Threading.Tasks;
using UnityEngine;

namespace Convai.Scripts.Checks
{
    public static class TalkDurationCheck
    {
        private const float DURATION_TO_CHECK = 0.5f;
        private static float _currTime;
        private static bool _isChecking;


        public static async Task<bool> Check()
        {
            _isChecking = true;
            _currTime = 0f;
            while (_currTime < DURATION_TO_CHECK)
            {
                _currTime += Time.deltaTime;
                await Task.Delay(Mathf.FloorToInt(Time.deltaTime * 1000));
            }

            return _isChecking;
        }

        public static void StopCheck() => _isChecking = false;
    }
}
