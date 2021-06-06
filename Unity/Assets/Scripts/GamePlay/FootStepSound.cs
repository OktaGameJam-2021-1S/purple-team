using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStepSound : MonoBehaviour
{
    [SerializeField] private AudioSource _footStep;

    public void FootStep()
    {
        _footStep.Play();
    }
}
