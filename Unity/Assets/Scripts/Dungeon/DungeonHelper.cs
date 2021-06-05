using DungeonArchitect;
using DungeonArchitect.Builders.GridFlow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonHelper : MonoBehaviour
{
    private const string DUNGEON_SCENE_NAME = "Dungeon";

    private static DungeonHelper m_Instance;
    public static DungeonHelper Instance
    {
        get
        {
            if (m_Instance == null)
               new GameObject("DungeonHelper").AddComponent<DungeonHelper>();
            return m_Instance;
        }
    }

    private Dungeon m_Dungeon;

    private List<Transform> m_PlayerSpawn;
    private List<Transform> m_EnemySpawn;
    private List<Transform> m_RefillPoint;
    private List<Transform> m_ObjectivePoint;

    private void Awake()
    {
        m_Instance = this;
        DontDestroyOnLoad(m_Instance.gameObject);

        Reset();
    }

    private void Reset()
    {
        m_PlayerSpawn = new List<Transform>();
        m_EnemySpawn = new List<Transform>();
        m_RefillPoint = new List<Transform>();
        m_ObjectivePoint = new List<Transform>();
    }

    public void Bind(DungeonItemTag instance)
    {
        switch(instance.itemTag)
        {
            case DungeonItemTag.DungeonItemTagType.DUNGEON:
                m_Dungeon = instance.GetComponent<Dungeon>();
                return;
            case DungeonItemTag.DungeonItemTagType.PLAYER_SPAWN:
                m_PlayerSpawn.Add(instance.transform);
                return;
            case DungeonItemTag.DungeonItemTagType.ENEMY_SPAWN:
                m_EnemySpawn.Add(instance.transform);
                return;
            case DungeonItemTag.DungeonItemTagType.REFILL_POINT:
                m_RefillPoint.Add(instance.transform);
                return;
            case DungeonItemTag.DungeonItemTagType.OBJECTIVE_POINT:
                m_ObjectivePoint.Add(instance.transform);
                return;
        }
    }

    public Transform GetPlayerSpawnPoint() => m_PlayerSpawn[0];

    public Transform GetObjectivePoint() => m_ObjectivePoint[0];

    public List<Transform> GetEnemySpawnPoints() => new List<Transform>(m_EnemySpawn);
    
    public List<Transform> GetRefillPoints() => new List<Transform>(m_RefillPoint);


    public void BuildDungeon(uint seed, System.Action callback)
    {
        StopAllCoroutines();
        StartCoroutine(Co_Build(seed, callback));
    }

    private IEnumerator Co_Build(uint seed, System.Action callback)
    {
        //if (m_Dungeon == null)
        //    yield return SceneManager.LoadSceneAsync(DUNGEON_SCENE_NAME, LoadSceneMode.Additive);

        //while (m_Dungeon == null)
        //    yield return null;

        m_Dungeon.Config.Seed = seed;
        m_Dungeon.RequestRebuild();

        //var builder = m_Dungeon.GetComponent<GridFlowDungeonBuilder>();
        //builder.asyncBuild = false;
        //builder.BuildDungeon(m_Dungeon.Config, m_Dungeon.ActiveModel);

        yield return null;
        callback?.Invoke();
    }

    [ContextMenu("Build dungeon")]
    public void test_build()
    {
        var seed = (uint)(new System.Random()).Next(1 << 30);
        BuildDungeon(seed, null);
    }
}
