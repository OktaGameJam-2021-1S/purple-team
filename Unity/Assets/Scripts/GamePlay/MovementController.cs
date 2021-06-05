using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

public class MovementController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private Animator _animator;
    [SerializeField] float _maxSpeed = 5;
    [SerializeField] float _angularSpeed = 20;

    private Vector3 _velocity;
    public Vector3 Velocity
    {
        get
        {
            return _velocity;
        }
    }

    public Vector3 Forward { get; private set; }

    public void Initialize()
    {
        TeleportToPosition(transform.position);
        _navMeshAgent.speed = _maxSpeed;
    }

    public void UpdateAxis(Vector2 axis)
    {
        Vector3 moveDirection = new Vector3(axis.x, 0, axis.y);

        Forward = moveDirection;
        _velocity = Forward * _maxSpeed;
        _navMeshAgent.angularSpeed = 0;

        UpdateAnimation();
    }

    public void UpdatePosition(float deltaTime)
    {
        _navMeshAgent.Move(Velocity * deltaTime);

        Vector3 forward = Forward;
        if (forward.magnitude <= 0)
        {
            forward = Velocity.normalized;
        }

        if (forward.magnitude > 0)
        {
            Vector3 newForward = Vector3.RotateTowards(_navMeshAgent.transform.forward, forward, _angularSpeed * deltaTime, 0);
            _navMeshAgent.transform.forward = newForward;
        }
    }

    public void TeleportToPosition(Vector3 position)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 500, 1))
        {
            _navMeshAgent.transform.position = hit.position;
            _navMeshAgent.Warp(hit.position);
        }
        else
        {
            _navMeshAgent.Warp(position);
        }
    }

    public void RotateToAngle(float rotation)
    {
        _navMeshAgent.transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

    private void UpdateAnimation()
    {
        _animator.SetFloat("Speed", Velocity.magnitude / _maxSpeed);
    }
}
