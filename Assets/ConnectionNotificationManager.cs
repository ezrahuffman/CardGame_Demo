using System;
using UnityEngine;
using Unity.Netcode;

public enum ConnectionStatus
{
    Connected,
    Disconnected
}
public class ConnectionNotificationManager : MonoBehaviour
{
    public static ConnectionNotificationManager Singleton { get; internal set; }


    /// <summary>
    /// This action is invoked whenever a client connects or disconnects from the game.
    ///   The first parameter is the ID of the client (ulong).
    ///   The second parameter is whether that client is connecting or disconnecting.
    /// </summary>
    public event Action<ulong, ConnectionStatus> OnClientConnectionNotification;

    private void Awake()
    {
        if (Singleton != null)
        {
            // As long as you aren't creating multiple NetworkManager instances, throw an exception.
            // (***the current position of the callstack will stop here***)
            throw new Exception($"Detected more than one instance of {nameof(ConnectionNotificationManager)}! " +
                $"Do you have more than one component attached to a {nameof(GameObject)}");
        }
        Singleton = this;
    }

    private void Start()
    {
        if (Singleton != this)
        {
            return; // so things don't get even more broken if this is a duplicate >:(
        }

        if (NetworkManager.Singleton == null)
        {
            // Can't listen to something that doesn't exist >:(
            throw new Exception($"There is no {nameof(NetworkManager)} for the {nameof(ConnectionNotificationManager)} to do stuff with! " +
                $"Please add a {nameof(NetworkManager)} to the scene.");
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    private void OnDestroy()
    {
        // Since the NetworkManager can potentially be destroyed before this component, only 
        // remove the subscriptions if that singleton still exists.
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        OnClientConnectionNotification?.Invoke(clientId, ConnectionStatus.Connected);
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        OnClientConnectionNotification?.Invoke(clientId, ConnectionStatus.Disconnected);
    }
}
