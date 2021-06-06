using SensorToolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSensorToolkit : MonoBehaviour
{
    [SerializeField] private TriggerSensor m_TriggerSensor;

    void Start()
    {
        if (m_TriggerSensor == null)
            m_TriggerSensor = GetComponentInChildren<TriggerSensor>();

        m_TriggerSensor.OnDetected.AddListener((go, sensor) =>
        {
            //Debug.Log($"<color=green>object detected: {go.name}</color>");
        });

        m_TriggerSensor.OnLostDetection.AddListener((go, sensor) =>
        {
            //Debug.Log($"<color=red>lost detection of: {go.name}</color>");
        });
    }
}
