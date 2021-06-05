using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SensorToolkit;
using System;
using Random = UnityEngine.Random;

public class CreatureAI : MonoBehaviour
{

    #region Inspector Variables
    [SerializeField] private TriggerSensor m_TriggerSensor;
    public NavMeshAgent navMeshAgent;

    [Tooltip("Used to calculate the next roaming position when free roaming.")]
    [SerializeField] private float roamingNextPointDistance;
    [Tooltip("Minimum ammount of seconds that the creature will wait after reaching a roaming position to start roaming again.")]
    [SerializeField] private int roamWaitMin = 4;
    [Tooltip("Maximum ammount of seconds that the creature will wait after reaching a roaming position to start roaming again.")]
    [SerializeField] private int roamWaitMax = 8;
    [Tooltip("The roaming speed of the creature")]
    [SerializeField] private float roamSpeed = 1;
    [Tooltip("The speed of the creature when it is fleeing")]
    [SerializeField] private float fleeSpeed = 3;
    [Tooltip("The speed of the creature when it is hunting a player")]
    [SerializeField] private float huntSpeed = 3;

    #endregion

    #region Other Variables

    /// <summary>
    /// Holds the current behavioural state of this creature.
    /// </summary>
    [SerializeField][Header("Debug Serialization")]
    private BehaviourState _currentState = BehaviourState.Roam;
    /// <summary>
    /// Property to set the _currentState, that also calls back the onBehaviourChangeCallback when it is updated.
    /// </summary>
    public BehaviourState CurrentState
    {
        get
        {
            return _currentState;
        }
        set
        {
            if(value != _currentState)
            {
                if (onBehaviourChangeCallback != null)
                {
                    onBehaviourChangeCallback(this, _currentState, value);
                }
                _currentState = value;
            }
        }
    }

    /// <summary>
    /// Callback that is called when the behaviour state changes on this CreatureAI instance. The first BehaviourState parameter 
    /// </summary>
    private Action<CreatureAI, BehaviourState, BehaviourState> onBehaviourChangeCallback = null;

    /// <summary>
    /// Caches the coroutine that runs the behaviour execution for this creature.
    /// </summary>
    private Coroutine behaviourCoroutine;

    /// <summary>
    /// Holds the initial spawn position.
    /// </summary>
    private Vector3 spawnPosition;

    /// <summary>
    /// Caches the Player transfomr that this creature is hunting.
    /// </summary>
    [SerializeField][Header("Debug Serialization")]
    private Transform playerHuntingTransform;

    /// <summary>
    /// Holds the current lightDmg taken, which ticks down when not in a light and ticks up if a ligth is being casted on the creature.
    /// </summary>
    private float lightDmg;

    /// <summary>
    /// Defines the max light dmg that the creatures needs to take to flee.
    /// </summary>
    private float maxLightDmg;
    #endregion

    #region enums

    /// <summary>
    /// Defines the behaviour states in which the creature can be in.
    /// </summary>
    public enum BehaviourState
    {
        //Behaviour of the creature roaming through the map.
        Roam,
        //Behaviour of the creature fleeing from the players.
        Flee,
        //Behaviour of the creature when it is hunting a player.
        Hunt
    }

    #endregion

    #region Methods

    /// <summary>
    /// Sets the on change behaviour callback to be called
    /// </summary>
    public void SetOnBehaviourChangeCallback(Action<CreatureAI, BehaviourState, BehaviourState> callback)
    {
        onBehaviourChangeCallback = callback;
    }

    /// <summary>
    /// Informs this Creature AI about it's Spawning position.
    /// </summary>
    /// <param name="spawnPosition"></param>
    public void SetSpawnPosition(Vector3 spawnPosition)
    {
        this.spawnPosition = spawnPosition;
    }

    public void Roam()
    {
        CurrentState = BehaviourState.Roam;
        m_TriggerSensor.gameObject.SetActive(true);

        navMeshAgent.speed = roamSpeed;

        if (behaviourCoroutine != null)
            StopCoroutine(behaviourCoroutine);

        behaviourCoroutine = StartCoroutine(RoamingCoroutine());
    }

    public void Flee()
    {
        CurrentState = BehaviourState.Flee;
        m_TriggerSensor.gameObject.SetActive(false);

        navMeshAgent.speed = fleeSpeed;

        if (behaviourCoroutine != null)
            StopCoroutine(behaviourCoroutine);

        behaviourCoroutine = StartCoroutine(FleeingCoroutine());
    }

    public void HuntPlayer(Transform player)
    {
        CurrentState = BehaviourState.Hunt;
        m_TriggerSensor.gameObject.SetActive(false);
        playerHuntingTransform = player;

        navMeshAgent.speed = huntSpeed;

        if (behaviourCoroutine != null)
            StopCoroutine(behaviourCoroutine);

        behaviourCoroutine = StartCoroutine(HuntingCoroutine());
    }


    public void ApplyLightDamage(float dmg)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets a random position on the navmesh that is contained within a radius from the current position. Or if it is being hard to find a new valid position to roam to this way, just goes back to the initial spawn position.
    /// </summary>
    private Vector3 GetRandomNavMeshPosition()
    {
        bool valid = false;
        NavMeshHit hit = default;
        int tries = 10;
        int counter = 0;
        while (!valid)
        {
            Vector3 roamPoint = Random.onUnitSphere * roamingNextPointDistance;
            roamPoint += transform.position;

            if (NavMesh.SamplePosition(roamPoint, out hit, roamingNextPointDistance, NavMesh.AllAreas))
            {
                valid = true;
            }

            ++counter;
            if(counter >= tries)
            {
                return spawnPosition;
            }
        }
        return hit.position;
    }    
    #endregion

    #region Coroutines
    /// <summary>
    /// Implementation of the Roaming Behaviour
    /// </summary>
    private IEnumerator RoamingCoroutine()
    {
        while(true)
        {
            Vector3 roamPosition = GetRandomNavMeshPosition();
            //DEBUG:::
            debugRoamingTarget = roamPosition;
            navMeshAgent.SetDestination(roamPosition);

            while(!navMeshAgent.PathComplete())
            {
                yield return null;
            }
            //int randomWait = Random.Range(roamWaitMin, roamWaitMax + 1);

            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Implementation of the Hunting Behaviour
    /// </summary>
    private IEnumerator HuntingCoroutine()
    {        
        while(true)
        {
            navMeshAgent.SetDestination(playerHuntingTransform.position);
            yield return null;
        }
    }

    /// <summary>
    /// Implementation for the Fleeing Behaviour
    /// </summary>
    private IEnumerator FleeingCoroutine()
    {
        NavMeshPath fleePath = CreaturesAIManager.Instance.GetFleePath(this);
        navMeshAgent.SetPath(fleePath);
        while(!navMeshAgent.PathComplete())
        {
            yield return null;
        }

        Roam();
    }

    #endregion

    #region Events

    /// <summary>
    /// Callback when the sensor detects a new go in front of it.
    /// </summary>
    private void OnSenseSomething(GameObject go, Sensor sensor)
    {
        if(go.CompareTag("Player"))
        {
            HuntPlayer(go.transform);
        }
    }

    #endregion

    #region Unity Events

    private void Start()
    {
        m_TriggerSensor.OnDetected.AddListener(OnSenseSomething);
        CreaturesAIManager.Instance.RegisterCreatureAI(this);
        Roam();

        //DEBUG::: For Testing
        SetSpawnPosition(transform.position);
    }


    Vector3? debugRoamingTarget = null;
    private void OnDrawGizmos()
    {
        if(debugRoamingTarget != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(debugRoamingTarget.Value, 1f);
        }
    }

    #endregion
}
