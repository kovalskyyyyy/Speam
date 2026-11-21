using System.Collections.Generic;
using UnityEngine;

namespace Speam.Core
{
    /// <summary>
    /// Маркер точки появления игрока. Положите объект в игровую сцену
    /// и задайте id, если точек несколько.
    /// </summary>
    public class PlayerSpawnPoint : MonoBehaviour
    {
        public string id = "Start";

        static readonly List<PlayerSpawnPoint> s_all = new List<PlayerSpawnPoint>();

        void OnEnable()
        {
            if (!s_all.Contains(this)) s_all.Add(this);
        }

        void OnDisable()
        {
            s_all.Remove(this);
        }

        public static PlayerSpawnPoint Find(string spawnId)
        {
            foreach (var point in s_all)
            {
                if (point != null && point.id == spawnId) return point;
            }
            return null;
        }
    }
}
