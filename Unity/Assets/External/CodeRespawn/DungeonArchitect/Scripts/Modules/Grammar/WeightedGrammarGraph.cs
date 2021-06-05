//$ Copyright 2015-21, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//
using UnityEngine;

namespace DungeonArchitect.Grammar
{
    [System.Serializable]
    public class WeightedGrammarGraph : ScriptableObject
    {
        [SerializeField]
        public float weight;

        [SerializeField]
        [HideInInspector]
        public GrammarGraph graph;
    }
}
