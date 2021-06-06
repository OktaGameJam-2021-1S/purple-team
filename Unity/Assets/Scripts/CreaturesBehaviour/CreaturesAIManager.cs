using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using GamePlay;

/// <summary>
/// Manager for the creatures and the general behaviour of the creatures on the game. Also tries to keep up with the players progress to trigger additional obstacles like more aggressiveness 
/// </summary>
public class CreaturesAIManager : MonoBehaviour
{
    #region Inspector Variables

    public float killDistance = 0.5f;
    public float minFleeDistance = 10;

    public List<Vector3> lCreaturesSpawnPoints = new List<Vector3>();

    #endregion

    #region Other Variables

    /// <summary>
    /// Holds all the creatures registered.
    /// </summary>
    List<CreatureAI> lCreatures = new List<CreatureAI>();

    //Lists for creatures on specific states. 
    List<CreatureAI> roamCreatures = new List<CreatureAI>();
    List<CreatureAI> huntCreatures = new List<CreatureAI>();
    List<CreatureAI> fleeCreatures = new List<CreatureAI>();

    List<PlayerController> players = new List<PlayerController>();

    private GameController _gameController;

    #endregion

    #region Methods

    public void Initialize(GameController gameController, List<CreatureAI> creatures, List<PlayerController> players)
    {
        _gameController = gameController;

        this.players = players;

        foreach (CreatureAI creature in creatures)
        {
            RegisterCreatureAI(creature);
        }
    }

    public void GotPlayer(CreatureAI creature, PlayerController player)
    {
        _gameController.PlayerDied(player);
    }

    /// <summary>
    /// Gets a fleeing point for the given creature considering its position.
    /// </summary>
    public NavMeshPath GetFleePath(CreatureAI creature)
    {        
        //Gets path based on the spawning points.

        NavMeshPath cachedFirstPath = null;
        NavMeshPath path;        

        Vector3 playersAveragePosition = Vector3.zero;
        for(int i = 0;
            i < players.Count;
            ++i)
        {
            playersAveragePosition += players[i].transform.position;
        }

        playersAveragePosition = playersAveragePosition / players.Count;
        Vector3 directionAwayFromPlayers = creature.transform.position - playersAveragePosition;
        directionAwayFromPlayers.y = 0;
        directionAwayFromPlayers.Normalize();

        path = new NavMeshPath();

        if (creature.navMeshAgent.CalculatePath(creature.transform.position + directionAwayFromPlayers * minFleeDistance, path))
        {
            print("Getting path to random point");
            return path;
        }

        for (int i = 0;
            i < lCreaturesSpawnPoints.Count;
            ++i)
        {
            path = new NavMeshPath();
            if (cachedFirstPath == null)
            {
                cachedFirstPath = path;
            }
            if (creature.navMeshAgent.CalculatePath(lCreaturesSpawnPoints[i], path))
            {
                if(path.GetPathLength() > minFleeDistance)
                {
                    print("Found min ditance flee position");
                    return path;
                }
            }
        }
        return cachedFirstPath;
    }

    /// <summary>
    /// Used by creatures to register themselves.
    /// </summary>
    /// <param name="creature"></param>
    private void RegisterCreatureAI(CreatureAI creature)
    {
        creature.Initialize(this, killDistance);

        lCreaturesSpawnPoints.Add(creature.transform.position);

        creature.SetOnBehaviourChangeCallback(CreatureChangeBehaviourCallback);
        lCreatures.Add(creature);
        if (creature.CurrentState == CreatureAI.BehaviourState.Roam)
            roamCreatures.Add(creature);
        else if (creature.CurrentState == CreatureAI.BehaviourState.Hunt)
            huntCreatures.Add(creature);
        else if (creature.CurrentState == CreatureAI.BehaviourState.Flee)
            fleeCreatures.Add(creature);
    }

    #endregion

    #region Callbacks

    public void CreatureChangeBehaviourCallback(CreatureAI creature, CreatureAI.BehaviourState pBState, CreatureAI.BehaviourState cBState)
    {
        //TODO::: Implement balancing here.
        //Something like, if number of creatures hunting is 3, then this creature can't go hunting.

        List<CreatureAI> creatures = null;
        if (pBState == CreatureAI.BehaviourState.Roam)
        {
            creatures = roamCreatures;
        }
        else if (pBState == CreatureAI.BehaviourState.Hunt)
        {
            creatures = huntCreatures;
        }
        else if (pBState == CreatureAI.BehaviourState.Flee)
        {
            creatures = fleeCreatures;
        }
        else
        {
            Debug.LogError("There is something wrong, because the creature wasn't on the previous list");
        }
        creatures.Remove(creature);

        if (cBState == CreatureAI.BehaviourState.Roam)
        {
            creatures = roamCreatures;
        }
        else if (cBState == CreatureAI.BehaviourState.Hunt)
        {
            creatures = huntCreatures;
        }
        else if (cBState == CreatureAI.BehaviourState.Flee)
        {
            creatures = fleeCreatures;
        }
        else
        {
            Debug.LogError("There is something wrong, because the creature wasn't on the previous list");
        }

        creatures.Add(creature);
    }

    #endregion

    #region Unity
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        roamCreatures = new List<CreatureAI>();
        huntCreatures = new List<CreatureAI>();
        fleeCreatures = new List<CreatureAI>();      
    }    
    #endregion
}
