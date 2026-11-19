using UnityEngine;
namespace DialogueSystem.Runtime
{
    /// <summary>Данные говорящего: создаются через Assets > Create > Dialogue > Speaker.</summary>
    [CreateAssetMenu(menuName = "Dialogue/Speaker", fileName = "NewSpeaker")]
    public class SpeakerData : ScriptableObject
    {
        public string displayName;
        public Color nameColor = Color.white;
        public Sprite sprite1;
        [Tooltip("Скорость печати текста, символов в секунду.")]
        public float textSpeed = 30f;
    }
}
