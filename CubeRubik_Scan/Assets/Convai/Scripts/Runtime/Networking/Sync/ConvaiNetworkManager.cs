using System;
using System.Threading;
using UnityEngine;

public class ConvaiNetworkManager
{
    private static ConvaiNetworkManager _instance;

    private bool _lastConnectionStatus;
    private Timer _networkCheckTimer;

    private ConvaiNetworkManager()
    {
        _lastConnectionStatus = CheckInternetConnection();
        IsConnected = _lastConnectionStatus;

        // Timer will execute CheckNetworkStatus every 2 seconds (2000 ms)
        _networkCheckTimer = new Timer(CheckNetworkStatus, null, 0, 2000);
    }

    public static ConvaiNetworkManager Instance => _instance ??= new ConvaiNetworkManager();

    public bool IsConnected { get; private set; }

    public event Action<bool> OnNetworkStatusChanged;

    private void CheckNetworkStatus(object state)
    {
        bool currentStatus = CheckInternetConnection();
        if (currentStatus != _lastConnectionStatus)
        {
            _lastConnectionStatus = currentStatus;
            IsConnected = currentStatus;
            OnNetworkStatusChanged?.Invoke(IsConnected);
        }
    }

    private bool CheckInternetConnection() => Application.internetReachability != NetworkReachability.NotReachable;

    // Optional: Dispose the timer when no longer needed
    public void StopNetworkMonitoring()
    {
        _networkCheckTimer?.Dispose();
        _networkCheckTimer = null;
    }
}
