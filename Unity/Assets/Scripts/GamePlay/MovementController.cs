using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    [SerializeField] private float _speed = 10;
    Rigidbody _rigidbody;
    PhotonView photonView;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            Vector3 axis = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;

            _rigidbody.velocity = axis * _speed + new Vector3(0, _rigidbody.velocity.y, 0);

            if (axis.magnitude > 0)
            {
                transform.LookAt(transform.position + axis);
            }
        }
    }
}
