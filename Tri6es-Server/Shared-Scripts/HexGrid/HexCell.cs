using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Shared.DataTypes;
using Shared.Structures;
using Shared.Game;

namespace Shared.HexGrid
{
    public class HexCell
    {
        public HexCoordinates coordinates;

        public HexGridChunk chunk;

        public HexCell[] neighbors { get; }

        public bool Visited { get; set; }

        public int StraightLineDistanceToTarget { get; set; }

        public int MinCostToStart { get; set; }

        public HexCellData Data { get; set; }

        public Structure Structure { get; set; }

        public bool notloaded;

        public byte EnemyTerritoryCounter { get; set; }
        /// <summary>
        /// Current Movement Cost
        /// </summary>
        public int g  { get; set; } = 0;
        /// <summary>
        /// Sum of g,h
        /// </summary>
        public int f { get; set; } = 0;
        /// <summary>
        /// Estimated Cost
        /// </summary>
        public int h { get; set; } = 0;

        /// <summary>Locations of the buildings protecting this cell.</summary>
        public List<HexCoordinates> Protectors = new List<HexCoordinates>();

        public int Elevation
        {
            get
            {
                return Data.Elevation - (int)Data.WaterDepth;
            }
        }

        public Vector3 Position
        {
            get
            {
                Vector3 position;
                position.x = (coordinates.X + coordinates.Z * 0.5f) * (HexMetrics.innerRadius * 2f);
                position.y = 0;
                position.z = coordinates.Z * (HexMetrics.outerRadius * 1.5f);

                return position;
            }
        }

        public HexCell()
        {
            neighbors = new HexCell[6];
        }

        public HexCell(HexCoordinates coordinates, HexCellData data, List<HexCoordinates> protectors)
        {
            neighbors = new HexCell[6];
            this.coordinates = coordinates;
            Data = data;
            this.Protectors = protectors;
        }

        public int GetElevationDifference(HexDirection direction)
        {
            HexCell neighbor = GetNeighbor(direction);
            int difference = 0;
            if(neighbor != null)
            {
                difference = Elevation - GetNeighbor(direction).Elevation;
            }

            return difference;
        }

        public int GetElevationDifference(HexCell neighbor)
        {
            int difference = 0;
            if (neighbor != null)
            {
                difference = Elevation - neighbor.Elevation;
            }

            return difference;
        }

        public HexCell GetNeighbor(HexDirection direction)
        {
            return neighbors[(int)direction];
        }

        public void setNeighbor(HexDirection direction, HexCell cell)
        {
            neighbors[(int)direction] = cell;
            cell.neighbors[(int)direction.Opposite()] = this;
        }

        public List<T> GetNeighborStructures<T>(int depth) where T : Structure
        {
            return this.GetNeighbors(depth).FindAll(elem => elem.Structure is T).ConvertAll<T>(elem => (T)elem.Structure);
        }

        public List<HexCell> GetNeighbors(int depth)
        {
            return GetNeighbors(depth, new List<HexCell>());
        }

        public List<HexCell> GetNeighbors(int depth, List<HexCell> cells)
        {
            if (!cells.Contains(this))
                cells.Add(this);
            if (depth <= 0)
                return cells;

            for(HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = this.GetNeighbor(d);
                if (neighbor == null)
                    continue;
                neighbor.GetNeighbors(depth - 1, cells);
            }
            return cells;
        }

        /// <summary>Checks whether the cell is claimed by any tribe.</summary>.
        public bool isProtected()
        {
            return Protectors.Count != 0;
        }

        /// <summary>Returns the building currently claiming this cell for its tribe.</summary>
        public ProtectedBuilding GetCurrentProtector()
        {
            if (!isProtected())
                return null;

            HexCell cell = GameLogic.grid.GetCell(Protectors[0]);
            return (ProtectedBuilding) cell.Structure;
        }

        /// <summary>
        /// Returns all buildings in range to claim this cell.
        /// The first element is the one currently claiming this cell for its tribe.
        /// </summary>
        public IEnumerable<ProtectedBuilding> GetAllProtectors()
        {
            return Protectors.Select(coords => (ProtectedBuilding) GameLogic.grid.GetCell(coords).Structure);
        }

        /// <summary>Returns the current tribe owning the cell or 256 otherwise.</summary>.
        public int GetCurrentTribe()
        {
            ProtectedBuilding protector = GetCurrentProtector();
            return protector != null ? protector.Tribe : 256;
        }
    }
}
