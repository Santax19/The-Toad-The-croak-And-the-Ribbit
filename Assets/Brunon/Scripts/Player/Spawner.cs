using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class Spawner : SimulationBehaviour
{
    [Header("Prefab del Jugador")]
    [SerializeField] private NetworkObject _playerPrefab;
    [Header("Puntos de Spawn")]
    [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
    public void SpawnLocalPlayer(NetworkRunner runner)
    {
        // Esta función es llamada por Fusion en el host
        // cada vez que un nuevo jugador (incluído él mismo) se une.

        if (_playerPrefab == null)
        {
            Debug.LogError("¡No hay _playerPrefab asignado en el PlayerSpawner!");
            return;
        }
        if (_spawnPoints.Count == 0)
        {
            Debug.LogError("¡No hay _spawnPoints asignados en el PlayerSpawner!");
            return;
        }
        // Spawnea el prefab del jugador
        // Le damos InputAuthority al jugador que acaba de entrar
        PlayerRef player = runner.LocalPlayer;
        int playerIndex = player.PlayerId;
        int spawnIndex = playerIndex % _spawnPoints.Count;
        Transform spawnPoint = _spawnPoints[spawnIndex];
        NetworkObject playerNetworkObject = runner.Spawn(
            _playerPrefab,
            spawnPoint.position,    // Usamos la posición del spawn point
            spawnPoint.rotation,    // Usamos la rotación del spawn point
            player                  // Le damos autoridad a este jugador
        );
        Debug.Log($"Jugador {player.PlayerId} spawneado.");
    }
}
