/// Makes camera follow the player
/// Maybe we have to get the cinemachine

using UnityEngine;

namespace MainCamera
{

  [RequireComponent(typeof(Camera))] // TODO: If we use cinemachine, then make other dependency
  public class CameraController : MonoBehaviour
  {
    [SerializeField] private Camera m_camera;
    [SerializeField] private GameObject m_playerObject;

    [Header("Bias (Obsolete)")]
    [SerializeField] private bool m_useCameraBias = false;
    [SerializeField] private Vector3 m_cameraBias;

    [Header("Target Based")]
    [SerializeField] private Vector3 m_playerOrigin = new Vector3(0f, 1f, 0f);
    [SerializeField] private float m_distance = 9f;
    [SerializeField] private float m_smoothTime = 0.25f;
    [SerializeField] private Vector3 angles = new Vector3(30f, 45f, 0f);


    private Vector3 velocity;

    private void LateUpdate()
    {
      MoveToPlayerInstant();
    }

    private void MoveToPlayerInstant()
    {
      Vector3 playerPosition = m_playerObject.transform.position;

      if (m_useCameraBias) {
        transform.position = new Vector3(
          playerPosition.x + m_cameraBias.x,
          transform.position.y,
          playerPosition.z + m_cameraBias.z
        );
      } 
      else
      {
        Quaternion desired_rotation = Quaternion.Euler(angles);
        Vector3 target_positon = playerPosition + m_playerOrigin;
        Vector3 desired_position = target_positon - desired_rotation * Vector3.forward * m_distance;

        transform.rotation = desired_rotation;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desired_position,
            ref velocity,
            m_smoothTime
            );
      }
    }
  }
}
