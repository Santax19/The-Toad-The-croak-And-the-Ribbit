using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ConnectionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Referencias")]
    [SerializeField] private NetworkRunner _runnerPrefab; // Arrastra el prefab del Runner aquí    [Header("UI Paneles")]
    [SerializeField] private GameObject _mainPanel;      // Donde están los botones Host/Join
    [SerializeField] private GameObject _browserPanel;
    [Header("UI Browser")]
    [SerializeField] private Transform _sessionListContent; // El "Content" del ScrollView
    [SerializeField] private SessionItems _sessionEntryPrefab;

    private NetworkRunner _runnerInstance;

    // Se llama desde el botón "Crear Partida (Host)"
    public void OnHostGame()
    {
        // Generamos un código random de 6 dígitos
        string roomCode = UnityEngine.Random.Range(100000, 999999).ToString();
        Debug.Log($"Creando sala con código: {roomCode}");

        StartGame(GameMode.Host, roomCode);
    }

    // Se llama desde el botón "Unirse (Client)"
    private void JoinSession(string sessionName)
    {
        // Ya estamos conectados al lobby, ahora iniciamos el juego como Cliente
        StartGame(GameMode.Client, sessionName);
    }

    // --------------------------------------------------------
    // BOTÓN: "BUSCAR PARTIDAS" (Abre el Browser)
    // --------------------------------------------------------
    public void OnOpenBrowser()
    {
        _mainPanel.SetActive(false);
        _browserPanel.SetActive(true);

        // Iniciamos el Runner solo para conectar al Lobby
        if (_runnerInstance == null) _runnerInstance = Instantiate(_runnerPrefab);

        // Nos registramos para recibir callbacks (como la lista de sesiones)
        _runnerInstance.AddCallbacks(this);

        // Nos unimos al Lobby por defecto
        var result = _runnerInstance.JoinSessionLobby(SessionLobby.ClientServer);
    }
    private async void StartGame(GameMode mode, string sessionName)
    {
        if (_runnerInstance == null) _runnerInstance = Instantiate(_runnerPrefab);

        // Necesario para cargar escenas
        if (!_runnerInstance.TryGetComponent<NetworkSceneManagerDefault>(out var sceneManager))
        {
            sceneManager = _runnerInstance.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }
        Debug.Log($"Iniciando {mode} en Sala {sessionName}, intentando cargar escena índice 1...");

        await _runnerInstance.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(1), // Índice de tu escena de juego
            SceneManager = sceneManager,
            PlayerCount = 4
        });
    }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        // 1. Limpiar la lista visual anterior
        foreach (Transform child in _sessionListContent)
        {
            Destroy(child.gameObject);
        }

        // 2. Crear un botón por cada sesión encontrada
        foreach (SessionInfo session in sessionList)
        {
            // Solo mostramos salas que estén abiertas y visibles
            if (session.IsVisible && session.IsOpen)
            {
                SessionItems newItem = Instantiate(_sessionEntryPrefab, _sessionListContent);

                // Le pasamos los datos y la función 'JoinSession' para que la llame al clickear
                newItem.SetInfo(session.Name, session.PlayerCount, session.MaxPlayers, JoinSession);
            }
        }

        if (sessionList.Count == 0)
        {
            Debug.Log("No hay partidas disponibles...");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
