using System;
using System.Threading.Tasks;
using Convai.Scripts.LoggerSystem;
using UnityEngine;

namespace Convai.Scripts.Checks
{
    public static class NetworkCheck
    {
        public static async Task<bool> CheckAsync()
        {
            // Variable to store the debug text for network reachability status
            string networkStatusDebugText = "";
            bool isNetworkAvailable = false;

            try
            {
                // Create a ping instance to google.com
                Ping ping = new("8.8.8.8");

                // Wait for up to 2 seconds for the ping response
                float startTime = Time.time;
                while (!ping.isDone)
                {
                    if (Time.time - startTime > 0.5f)
                    {
                        networkStatusDebugText = "Ping Timeout";
                        break;
                    }

                    await Task.Delay(100); // Wait 100ms between checks
                }

                if (ping.isDone && ping.time >= 0)
                {
                    networkStatusDebugText = $"Connected (Ping: {ping.time}ms)";
                    isNetworkAvailable = true;
                }
            }
            catch (Exception e)
            {
                networkStatusDebugText = "Ping Failed: " + e.Message;
                isNetworkAvailable = false;
            }

            // If ping failed, fall back to Unity's basic network check
            if (!isNetworkAvailable)
            {
                switch (Application.internetReachability)
                {
                    case NetworkReachability.NotReachable:
                        networkStatusDebugText += " | Not Reachable";
                        break;
                    case NetworkReachability.ReachableViaCarrierDataNetwork:
                        networkStatusDebugText += " | Reachable via Carrier Data Network";
                        isNetworkAvailable = true;
                        break;
                    case NetworkReachability.ReachableViaLocalAreaNetwork:
                        networkStatusDebugText += " | Reachable via Local Area Network";
                        isNetworkAvailable = true;
                        break;
                }
            }

            // Log the network reachability status for debugging
            ConvaiUnityLogger.Info("Network Reachability: " + networkStatusDebugText, LogCategory.Player);

            return isNetworkAvailable;
        }
    }
}
