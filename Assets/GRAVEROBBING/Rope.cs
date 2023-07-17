using UnityEngine;

public class Rope : MonoBehaviour, IInteractable
{
    public Transform top;
    public Transform bottom;

    public void Interact()
    {
        DungeonCameraController.Instance.isRappelling = true;
        DungeonCameraController.Instance.currentRope = this;
        DungeonCameraController.Instance.controller.enabled = false;

        Transform playerTransform = DungeonCameraController.Instance.transform;

        Vector3 playerPos = new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);
        Vector3 ropePos = new Vector3(transform.position.x, 0f, transform.position.z);

        Vector3 targetPos = Vector3.Normalize(playerPos - ropePos) * DungeonCameraController.Instance.rappelHoldDistance + transform.position;
        targetPos.y = Mathf.Clamp(playerTransform.position.y, bottom.position.y, top.position.y);
        playerTransform.position = targetPos;

        DungeonCameraController.Instance.controller.enabled = true;
    }
}
