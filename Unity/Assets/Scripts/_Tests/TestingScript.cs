using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestingScript : MonoBehaviour
{
    public Renderer renderer;
    public Color c = Color.yellow;
    public float intensity;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            print("Setting intensity");
            var m = renderer.materials[1];
            m.SetColor("_EmissionColor", c * intensity);
        }
    }
}
