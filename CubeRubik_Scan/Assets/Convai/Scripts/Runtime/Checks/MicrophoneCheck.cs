using System.Collections;
using Convai.Scripts.LoggerSystem;
using Convai.Scripts.NotificationSystem;
using Convai.Scripts.Services;
using UnityEngine;

namespace Convai.Scripts.Checks
{
    public static class MicrophoneCheck
    {
        // Duration for microphone input check.
        private const float INPUT_CHECK_DURATION = 3f;

        // Microphone sensitivity, adjust as needed.
        private const float SENSITIVITY = 10f;

        // Threshold level to detect microphone issues.
        private const float THRESHOLD = 0.1f;


        // Coroutine to check the microphone device after a specified duration.
        public static IEnumerator CheckMicrophoneDevice(AudioClip audioClip)
        {
            // Check if the provided AudioClip is null.
            if (audioClip == null)
            {
                // Log an error and abort the microphone check.
                ConvaiUnityLogger.Error("AudioClip is null!", LogCategory.Character);
                yield break;
            }


            // Wait for the specified duration before analyzing microphone input.
            yield return new WaitForSeconds(INPUT_CHECK_DURATION);


            // Calculate the range of audio samples to analyze based on the duration.
            int sampleEnd = (int)(INPUT_CHECK_DURATION * audioClip.frequency * audioClip.channels);

            // Initialize an array to store audio samples.
            float[] samples = new float[sampleEnd];
            int samplesLength = samples.Length;

            // Attempt to retrieve audio data from the AudioClip.
            if (!audioClip.GetData(samples, 0))
            {
                ConvaiUnityLogger.Error("Failed to get audio data!", LogCategory.Character);
                yield break;
            }

            // Initialize a variable to store the total absolute level of audio samples.
            float level = 0;

            // Calculate the total absolute level of audio samples.
            for (int i = 0; i < samplesLength; i++)
            {
                level += Mathf.Abs(samples[i] * SENSITIVITY);
            }

            // Normalize the calculated level by dividing it by the number of samples and then multiply by sensitivity.
            level = level / samplesLength * SENSITIVITY;

            // Check if the microphone level is below the threshold, indicating a potential issue.
            if (level < THRESHOLD)
            {
                ConvaiUnityLogger.Warn("Microphone Issue Detected!", LogCategory.Character);
                yield break;
            }

            // Log that the microphone is working fine.
            ConvaiUnityLogger.Info("Microphone is working fine.", LogCategory.Character);
            ConvaiServices.NotificationService.RequestNotification(NotificationType.MICROPHONE_ISSUE);
        }
    }
}
