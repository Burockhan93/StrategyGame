using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Shared.DataTypes;
using Shared.Structures;
using Shared.Game;
using System.Text;
using System.Diagnostics;
using System;
using Shared.HexGrid;

namespace Shared.HexGrid
{
  public class Pathfinding {
    public static int getShortestPathAStar(HexGrid grid, HexCell startnode, HexCell targetnode, Tribe tribe) {
      int result = -1;
      HexCell[] cells = grid.cells;
      HexCoordinates targetcoord = targetnode.coordinates;
      foreach (HexCell cell in cells) {
        cell.StraightLineDistanceToTarget = HexCoordinates.calcDistance(cell.coordinates, targetcoord) + Math.Abs(cell.GetElevationDifference(targetnode));
        cell.MinCostToStart = 0;
        cell.Visited = false;
        cell.EnemyTerritoryCounter = 0;
      }
      result = AStarSearch(cells, startnode, targetnode, tribe);
      return result;
    }
    private static int AStarSearch (HexCell[] cells, HexCell startnode, HexCell targetnode, Tribe tribe) {
      int result = -1;
      var prioQueue = new List<HexCell>();
      int length = cells.Length;
      startnode.MinCostToStart = 0;
      startnode.EnemyTerritoryCounter = 1;
      prioQueue.Add(startnode);
      do {
        prioQueue = prioQueue.OrderBy(x => x.MinCostToStart + x.StraightLineDistanceToTarget).ToList();
        HexCell node = prioQueue.First();
        prioQueue.Remove(node);
        if (node.EnemyTerritoryCounter >= 7)
        {
          continue;
        }
        foreach (HexCell cnn in node.neighbors){
          if (cnn == null){
            continue;
          }
          if (cnn.GetCurrentTribe() != tribe.Id && cnn.GetCurrentTribe() != 256) {
            if(node.EnemyTerritoryCounter < cnn.EnemyTerritoryCounter || cnn.EnemyTerritoryCounter == 0) {
              cnn.EnemyTerritoryCounter = (byte)((int)node.EnemyTerritoryCounter + 1);
            }
          }
          if (cnn.EnemyTerritoryCounter >= 7) {//if this neighbor node would be 6 deep into enemy territory then we dont add it now.)
            continue;
          }
          if (cnn.Visited) {
            continue;
          }
          if(cnn.MinCostToStart == 0 || node.MinCostToStart + 1 + Math.Abs(node.GetElevationDifference(cnn)) < cnn.MinCostToStart) {
            cnn.MinCostToStart = node.MinCostToStart + 1 + Math.Abs(node.GetElevationDifference(cnn));
            if(!prioQueue.Contains(cnn)){
              if (cnn == targetnode) 
              {
              }
              prioQueue.Add(cnn);
            }
          }
        }
        node.Visited = true;
        if (node == targetnode){
          return node.MinCostToStart;
        }
      } while (prioQueue.Any());
      return result;
    }

  }
}