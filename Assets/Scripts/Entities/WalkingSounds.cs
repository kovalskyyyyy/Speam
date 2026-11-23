using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    public class WalkingSounds : MonoBehaviour
    {
        [SerializeField] private AudioSource m_audioSource;
        [SerializeField] private List<AudioClip> m_walkingSounds;

        [Header("Pitch Settings")]
        [SerializeField] private float m_minPitch = 0.9f;
        [SerializeField] private float m_maxPitch = 1.1f;

        public void MakeWalkingSound()
        {
            if (m_walkingSounds == null || m_walkingSounds.Count == 0 || m_audioSource == null)
                return;

            int index = GetRandomIndex();
            AudioClip clip = m_walkingSounds[index];

            m_audioSource.pitch = Random.Range(m_minPitch, m_maxPitch);
            m_audioSource.PlayOneShot(clip);

            Debug.Log("[WalkingSounds] walked!");
        }

        // private int m_lastSoundIndex = -1;

        private int GetRandomIndex()
        {
            if (m_walkingSounds.Count == 1)
                return 0;

            // int index;
            // do
            // {
            int index = Random.Range(0, m_walkingSounds.Count);
            // }
            // while (index == m_lastSoundIndex);

            // m_lastSoundIndex = index;
            return index;
        }
    }
}
