using System.Linq;
using System.Collections.Generic;
using Shared.Game;
using Shared.Structures;
using System;

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
        /// <summary>
        /// Finds path from given start point to end point. Returns an empty list if the path couldn't be found.
        /// </summary>
        /// <param name="startPoint">Start HexCell.</param>
        /// <param name="endPoint">Destination HexCell.</param>
        public static List<HexCell> FindPath(HexCell startPoint, HexCell endPoint,bool walk)
        {
            List<HexCell> openPathHexCells = new List<HexCell>();
            List<HexCell> closedPathHexCells = new List<HexCell>();

            // Prepare the start HexCell.
            HexCell currentHexCell = startPoint;

            currentHexCell.g = 0;
            currentHexCell.h = GetEstimatedPathCost(startPoint, endPoint);

            // Add the start HexCell to the open list.
            openPathHexCells.Add(currentHexCell);

            while (openPathHexCells.Count != 0)
            {
                // Sorting the open list to get the HexCell with the lowest F.
                openPathHexCells = openPathHexCells.OrderBy(x => x.f).ThenByDescending(x => x.g).ToList();
                currentHexCell = openPathHexCells[0];

                // Removing the current HexCell from the open list and adding it to the closed list.
                openPathHexCells.Remove(currentHexCell);
                closedPathHexCells.Add(currentHexCell);

                int g = currentHexCell.g + 1;

                // If there is a target HexCell in the closed list, we have found a path.
                if (closedPathHexCells.Contains(endPoint))
                {
                    break;
                }

                // Investigating each adjacent HexCell of the current HexCell.
                foreach (HexCell adjacentHexCell in currentHexCell.GetNeighbors(1))
                {
                   
                    // Ignore the HexCell if it's already in the closed list.
                    if (closedPathHexCells.Contains(adjacentHexCell))
                    {
                        continue;
                    }
                    //buildings will not be returned if we use the path for road building
                   

                    //if adjacentCell has a building on it, dont use it unless its HQ
                    if (!walk && (adjacentHexCell.Structure is Building) )
                    {
                        if(adjacentHexCell.Structure is Headquarter) { }
                            else { Console.WriteLine("+"); continue; }
                       
                    }
                   

                    // If it's not in the open list - add it and compute G and H.
                    if (!(openPathHexCells.Contains(adjacentHexCell)))
                    {
                        adjacentHexCell.g = g;
                        adjacentHexCell.h = GetEstimatedPathCost(adjacentHexCell, endPoint);
                        adjacentHexCell.f = adjacentHexCell.g+adjacentHexCell.h;
                        openPathHexCells.Add(adjacentHexCell);
                    }
                    // Otherwise check if using current G we can get a lower value of F, if so update it's value.
                    else if (adjacentHexCell.f > g + adjacentHexCell.h)
                    {
                        adjacentHexCell.g = g;
                    }
                }
            }

            List<HexCell> finalPathHexCells = new List<HexCell>();

            // Backtracking - setting the final path.
            if (closedPathHexCells.Contains(endPoint))
            {
                currentHexCell = endPoint;
                finalPathHexCells.Add(currentHexCell);

                for (int i = endPoint.g - 1; i >= 0; i--)
                {
                    currentHexCell = closedPathHexCells.Find(x => x.g == i && currentHexCell.GetNeighbors(1).Contains(x));
                    finalPathHexCells.Add(currentHexCell);
                }

                finalPathHexCells.Reverse();
            }

            return finalPathHexCells;
        }

        /// <summary>
        /// Returns estimated path cost from given start position to target position of hex HexCell using Manhattan distance.
        /// </summary>
        /// <param name="startPosition">Start position.</param>
        /// <param name="targetPosition">Destination position.</param>
        protected static int GetEstimatedPathCost(HexCell startPosition, HexCell targetPosition)
        {
            return HexCoordinates.calcDistance(startPosition.coordinates, targetPosition.coordinates); //+ Math.Abs(startPosition.GetElevationDifference(targetPosition));
           
        }


    }
}