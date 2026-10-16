using Unity.Cinemachine;
using UnityEngine;

namespace RoomGame
{
    /// <summary>Spawn points and camera hookup for the local player.</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public Transform[] spawnPoints;
        public CinemachineCamera followCamera;

        void Awake() => Instance = this;

        public Vector3 GetSpawnPosition()
        {
            if (spawnPoints == null || spawnPoints.Length == 0) return Vector3.zero;
            return spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        }

        public void OnLocalPlayerSpawned(PlayerMovement player)
        {
            if (followCamera != null) followCamera.Follow = player.transform;
        }
    }
}
