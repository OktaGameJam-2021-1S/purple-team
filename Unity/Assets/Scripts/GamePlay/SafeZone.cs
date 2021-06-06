using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SensorToolkit;

public class SafeZone : MonoBehaviour
{
    [SerializeField] private Sensor sensor;
    [SerializeField] private float lightDmg;

    /// <summary>
    /// Holds all creatures on the light zone that are receiving dmg
    /// </summary>
    private List<CreatureAI> creaturesApplyingDmg = new List<CreatureAI>();

    private void Awake()
    {
        sensor.OnDetected.AddListener(OnDetect);
        sensor.OnLostDetection.AddListener(OnLostDetection);
    }

    private void OnDetect(GameObject go, Sensor sensor)
    {
        if (go.transform.parent != null)
        {
            CreatureAI creature = go.transform.parent.GetComponent<CreatureAI>();
            if (creature != null)
            {
                creaturesApplyingDmg.Add(creature);
            }
        }
    }

    private void OnLostDetection(GameObject go, Sensor sensor)
    {
        if (go.transform.parent != null)
        {
            CreatureAI creature = go.transform.parent.GetComponent<CreatureAI>();
            if (creature != null)
            {
                creaturesApplyingDmg.Remove(creature);
            }
        }
    }

    private void Update()
    {
        //Checking if there is any creature on the light zone to deal dmg.
        for (int i = 0; i < creaturesApplyingDmg.Count; ++i)
        {
            creaturesApplyingDmg[i].ApplyLightDamage(lightDmg * Time.deltaTime);
        }
    }
}
