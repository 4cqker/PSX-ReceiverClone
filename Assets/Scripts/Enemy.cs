using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Transform[] movePositions;
    [SerializeField] private float minDistance = 0.1f;

    private NavMeshAgent agent;
    private Transform moveTarget;

    private void Start()
    {
        if (!TryGetComponent(out agent)) Debug.LogError("No agent on the enemy!");

        SetMoveTarget(movePositions[Random.Range(0, movePositions.Length)]);
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, moveTarget.position) < minDistance)
        {
            Debug.Log("Change target");
            SetMoveTarget(movePositions[Random.Range(0, movePositions.Length)]);
        }
    }

    private void SetMoveTarget(Transform transform)
    {
        moveTarget = transform;
        agent.SetDestination(moveTarget.position);
    }
}
