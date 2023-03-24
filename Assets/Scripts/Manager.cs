using Photon.Pun;
using UnityEngine;

namespace Com.Kawaiisun.SimpleHostile
{
    public class Manager : MonoBehaviour
    {
        public string player_Prefab;
        public Transform[] spawn_points;

        private void Start()
        {
            Spawn();
        }

        public void Spawn()
        {
            Transform t_spawn = spawn_points[Random.Range(0, spawn_points.Length)];
            PhotonNetwork.Instantiate(player_Prefab, t_spawn.position, t_spawn.rotation);
        }
    }
}