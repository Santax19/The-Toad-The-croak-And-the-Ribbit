using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Spawner : NetworkBehaviour, INetworkRunnerCallbacks
{
    [Header("Prefab del Jugador")]
    [SerializeField] private NetworkObject _playerPrefab;

    [Header("Puntos de Spawn")]
    [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
    private HashSet<PlayerRef> _spawnedPlayers = new HashSet<PlayerRef>(); // Control para evitar spawnear dos veces al mismo jugador si hay lag

    public override void Spawned()
    {
        Debug.Log("¡SPAWNER ACTIVADO EN LA RED!");
        Runner.AddCallbacks(this);
        if (Runner.IsServer)
        {
            foreach (var player in Runner.ActivePlayers)
            {
                SpawnPlayer(player);
            }
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        runner.RemoveCallbacks(this);
        _spawnedPlayers.Clear();
    }

    // 2. Este es el método que nos importa (La interfaz pide que se llame OnPlayerJoined)
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (Runner.IsServer)
        {
            SpawnPlayer(player);
        }
    }
    private void SpawnPlayer(PlayerRef player)
    {
        // Evitamos doble spawn por seguridad
        if (_spawnedPlayers.Contains(player)) return;

        Debug.Log($"Spawneando lógica para Jugador {player.PlayerId}");

        if (_playerPrefab == null || _spawnPoints.Count == 0)
        {
            Debug.LogError("Faltan referencias en el Spawner");
            return;
        }

        int spawnIndex = player.PlayerId % _spawnPoints.Count;
        Transform point = _spawnPoints[spawnIndex];

        NetworkObject playerObj = Runner.Spawn(
            _playerPrefab,
            point.position,
            point.rotation,
            player // Input Authority
        );

        // Marcamos que ya lo spawneamos
        _spawnedPlayers.Add(player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // Opcional: Limpiar de la lista si se va
        if (_spawnedPlayers.Contains(player))
        {
            // Si el objeto sigue vivo, lo borramos de la red
            if (runner.TryGetPlayerObject(player, out NetworkObject networkObject) && networkObject != null && runner.IsServer)
            {
                runner.Despawn(networkObject);
            }

            // Lo sacamos de la lista
            _spawnedPlayers.Remove(player);
        }
    }
    // -------------------------------------------------------------------------
    // 3. STUBS OBLIGATORIOS
    // -------------------------------------------------------------------------
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // Creamos la estructura
        var data = new NetworkInputData();

        // Llenamos con datos de Unity (Input local de la PC)
        data.moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Botones (Bool)
        data.buttons.Set(MyButtons.Fire1, Input.GetButton("Fire1"));
        data.buttons.Set(MyButtons.Fire2, Input.GetButton("Fire2"));
        data.buttons.Set(MyButtons.Crouch, Input.GetKey(KeyCode.LeftShift));
        data.buttons.Set(MyButtons.Reload, Input.GetKey(KeyCode.R));
        data.buttons.Set(MyButtons.Grapple, Input.GetKey(KeyCode.Space));

        // Acciones de un solo frame (Reload, Cambio de arma)
        data.buttons.Set(MyButtons.Alpha1, Input.GetKey(KeyCode.Alpha1));
        data.buttons.Set(MyButtons.Alpha2, Input.GetKey(KeyCode.Alpha2));
        data.buttons.Set(MyButtons.Alpha3, Input.GetKey(KeyCode.Alpha3));
        data.buttons.Set(MyButtons.Alpha4, Input.GetKey(KeyCode.Alpha4));

        data.mouseX = Input.GetAxis("Mouse X");
        if (Camera.main != null)
        {
            data.aimDirection = Camera.main.transform.forward; // Envía a dónde miro
        }

        input.Set(data);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
