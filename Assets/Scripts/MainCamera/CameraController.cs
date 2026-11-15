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

    [SerializeField] private Vector3 m_cameraBias;

    private void Update()
    {
      MoveToPlayerInstant();
    }

    private void MoveToPlayerInstant()
    {
      Vector3 playerPosition = m_playerObject.transform.position;

      transform.position = new Vector3(
            playerPosition.x + m_cameraBias.x,
            transform.position.y,
            playerPosition.z + m_cameraBias.z
          );
    }
  }
}
