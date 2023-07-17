using UnityEngine;

public class Rope : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        DungeonCameraController.Instance.isRappelling = true;
    }
}
