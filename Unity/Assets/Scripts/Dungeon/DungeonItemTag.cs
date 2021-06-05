using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonItemTag : MonoBehaviour
{
    public enum DungeonItemTagType
    {
        DUNGEON,
        PLAYER_SPAWN,
        ENEMY_SPAWN,
        OBJECTIVE_POINT,
        REFILL_POINT
    }

    [SerializeField] private DungeonItemTagType m_Tag;

    public DungeonItemTagType itemTag => m_Tag;

    private void Start()
    {
        DungeonHelper.Instance.Bind(this);
    }
}
