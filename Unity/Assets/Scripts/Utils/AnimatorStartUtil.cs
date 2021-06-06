using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorStartUtil : MonoBehaviour
{
    public string m_StartBooleanKey;
    public bool m_StartBooleanValue;

    void Awake()
    {
        var anim = GetComponent<Animator>();

        if (!string.IsNullOrEmpty(m_StartBooleanKey))
            anim.SetBool(m_StartBooleanKey, m_StartBooleanValue);
    }
}
