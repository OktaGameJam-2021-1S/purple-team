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

    [SerializeField] private TriggerSensor triggerSensor;
    [SerializeField] private Animator animator;

    [SerializeField] private string idleName = "Skull_Idle";
    [SerializeField] private string skullReactionName = "Skull_Reaction";


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

    [Tooltip("The recovery speed of the light dmg that the creature can take")]
    [SerializeField] private float recoverySpeed = 3;

    [Tooltip("The max light dmg that this creature can take to start fleeing.")]
    [SerializeField] private float maxLightDmg = 10;

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
    private float lightDmg = 0;

    /// <summary>
    /// Holds whether this creature is currently taking dmg or not.
    /// </summary>
    private bool takingDmg = false;
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

    /// <summary>
    /// Starts the Roam behaviour for this creature
    /// </summary>
    public void Roam()
    {
        print("Roam called");
        animator.Play(idleName);
        CurrentState = BehaviourState.Roam;
        triggerSensor.gameObject.SetActive(true);

        navMeshAgent.speed = roamSpeed;

        if (behaviourCoroutine != null)
            StopCoroutine(behaviourCoroutine);

        behaviourCoroutine = StartCoroutine(RoamingCoroutine());
    }

    /// <summary>
    /// Starts the Fleeing behaviour for this creature
    /// </summary>
    public void Flee()
    {
        print("Flee called");
        animator.Play(idleName);
        CurrentState = BehaviourState.Flee;
        triggerSensor.gameObject.SetActive(false);

        navMeshAgent.speed = fleeSpeed;

        if (behaviourCoroutine != null)
            StopCoroutine(behaviourCoroutine);

        behaviourCoroutine = StartCoroutine(FleeingCoroutine());
    }

    /// <summary>
    /// Starts the hunting of a player for this creature
    /// </summary>
    public void HuntPlayer(Transform player)
    {
        print("Hunt called");
        animator.Play(idleName);
        CurrentState = BehaviourState.Hunt;
        triggerSensor.gameObject.SetActive(false);
        playerHuntingTransform = player;

        navMeshAgent.speed = huntSpeed;

        if (behaviourCoroutine != null)
            StopCoroutine(behaviourCoroutine);

        behaviourCoroutine = StartCoroutine(HuntingCoroutine());
    }

    /// <summary>
    /// Apply a dmg value to this creature.
    /// </summary>
    public void ApplyLightDamage(float dmg)
    {
        //Can only apply dmg to creature if it is not fleeing.
        if (CurrentState == BehaviourState.Flee)
            return;

        takingDmg = true;
        animator.Play(skullReactionName);
        lightDmg += dmg;
        print("Applying dmg: "+dmg.ToString()+", current lightDmg value: "+lightDmg+", behaviourState: " + CurrentState.ToString());
        if (lightDmg >= maxLightDmg)
        {
            Flee();
            lightDmg = 0;
        }
        else
        {
            navMeshAgent.isStopped = true;

            if (behaviourCoroutine != null)
                StopCoroutine(behaviourCoroutine);

            behaviourCoroutine = StartCoroutine(TakingDmgCoroutine());
        }
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
    /// Implementation for the taking dmg from light.
    /// </summary>
    /// <returns></returns>
    private IEnumerator TakingDmgCoroutine()
    {
        //Waiting a frame to not recover the dmg on the same frame that the dmg is being taken.
        yield return null;

        while(lightDmg > 0)
        {
            print("Recovering");
            lightDmg -= recoverySpeed * Time.deltaTime;
            yield return null;
        }

        takingDmg = false;

        if (CurrentState == BehaviourState.Hunt)
        {
            print("Back To hunting");
            HuntPlayer(playerHuntingTransform);
        }
        else if (CurrentState == BehaviourState.Roam)
        {
            print("Back to Roaming");
            Roam();
        }
    }

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
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(roamPosition);

            while(!navMeshAgent.PathComplete())
            {
                print("Roaming");
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
            print("Hunting");
            navMeshAgent.isStopped = false;
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
        navMeshAgent.isStopped = false;
        navMeshAgent.SetPath(fleePath);
        while(!navMeshAgent.PathComplete())
        {
            print("Fleeing");
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
            if (CurrentState != BehaviourState.Flee && !takingDmg)
            {
                HuntPlayer(go.transform);
            }
        }
    }

    #endregion

    #region Unity Events

    private void Start()
    {
        triggerSensor.OnDetected.AddListener(OnSenseSomething);
        CreaturesAIManager.Instance.RegisterCreatureAI(this);
        Roam();
        lightDmg = 0;

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
