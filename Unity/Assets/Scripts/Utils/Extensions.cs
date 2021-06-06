using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class Extensions
{
    /// <summary>
    /// Gets the length of a path within a Nav Mesh path
    /// </summary>
    public static float GetPathLength(this NavMeshPath path)
    {
        float lng = 0.0f;

        if (path.status != NavMeshPathStatus.PathInvalid)
        {
            for (int i = 1; i < path.corners.Length; ++i)
            {
                lng += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
        }

        return lng;
    }

    /// <summary>
    /// Gets whether the path that this creature is taking is complete. If you call SetDestination, this will already be returning false, on the same frame, until the path is completed.
    /// </summary>
    public static bool PathComplete(this NavMeshAgent navMeshAgent)
    {
        if (Vector3.Distance(navMeshAgent.destination, navMeshAgent.transform.position) <= navMeshAgent.stoppingDistance)
        {
            if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
            {
                return true;
            }
        }
        return false;
    }
}
