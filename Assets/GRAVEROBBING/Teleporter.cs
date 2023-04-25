using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private Transform teleporterExit;

    private string[] teleportationVoicelines = new string[]
        {
            //Scout
            "Thanks for the ride!", "Hey good job there, hardhat!", "Thanks for that, tough guy!",
            //Soldier
            "Thanks.", "Thanks for the Teleporter.", "Thanks, Engine.",
            //Pyro
            "Mmphn frphha herrpha",
            //Demoman
            "Thanks fer the ride!", "Thanks, lad!",
            //Heavy
            "Thanks for ride!", "Was good trip!", "Engineer is credit to team!",
            //Engineer
            "Much obliged, pardner.", "Thanks for the ride, pardner!",
            //Medic
            "Danke, Engineer!", "Danke, mein hard-hatted friend!", "Zank you, Engineer!",
            //Sniper
            "Thanks, mate!", "Thanks!", "Thanks for that, Truckie.",
            //Spy
            "Thank you, laborer!", "Thank you, my friend.", "Cheers, Engineers!"
        };

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CharacterController controller))
        {
            Teleport(controller);
        }
    }

    private void Teleport(CharacterController controller)
    {
        controller.enabled = false;
        controller.transform.position = teleporterExit.position;
        controller.enabled = true;

        Debug.Log(teleportationVoicelines[Random.Range(0, teleportationVoicelines.Length)]);
    }
}
