using DungeonArchitect.Flow.Domains.Tilemap;
using DungeonArchitect.Flow.Impl.GridFlow;
using DungeonArchitect.Flow.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonPlacementRule_AtCenter : ScriptableObject, ITilemapItemPlacementStrategy
{
    public bool PlaceItems(FlowItem item, FlowTilemapCell[] freeCells, TilemapItemPlacementSettings settings, TilemapItemPlacementStrategyContext context, ref int outFreeTileIndex, ref string errorMessage)
    {
        if (freeCells.Length == 0)
        {
            // Not enough free cells for placing the items
            errorMessage = "Insufficient free tiles";
            return false;
        }

        var bestCells = new List<int>();
        var bestDistance = int.MinValue;
        for (int i = 0; i < freeCells.Length; i++)
        {
            var freeCell = freeCells[i];
            var x = freeCell.TileCoord.x;
            var y = freeCell.TileCoord.y;
            var distanceCell = context.distanceField.distanceCells[x, y];
            var distance = distanceCell.DistanceFromEdge;
            if (settings.avoidPlacingNextToDoors && distanceCell.DistanceFromDoor == 1)
            {
                continue;
            }
            if (distance == bestDistance)
            {
                bestCells.Add(i);
            }
            else if (distance > bestDistance)
            {
                bestDistance = distance;
                bestCells.Clear();
                bestCells.Add(i);
            }
        }

        if (bestCells.Count == 0)
        {
            // Not enough free cells for placing the items
            errorMessage = "Insufficient free tiles";
            return false;
        }

        var bestCellIndex = context.random.Next(bestCells.Count - 1);
        outFreeTileIndex = bestCells[bestCellIndex];
        return true;
    }
}
