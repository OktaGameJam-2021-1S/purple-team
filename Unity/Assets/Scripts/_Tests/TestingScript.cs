using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestingScript : MonoBehaviour
{
    public CreatureAI creature;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            print("Making Creature Flee");
            creature.Flee();
        }
    }
}
