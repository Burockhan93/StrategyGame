using System.Collections;
using System.Collections.Generic;

namespace Shared.HexGrid
{
    public class HexGridChunk
    {
        public HexCell[] cells;

        public HexGridChunk()
        {
            cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
        }

        public void AddCell(int index, HexCell cell)
        {
            cells[index] = cell;
            cell.chunk = this;
        }

        public HexCell GetCell(HexCoordinates coordinates)
        {
            return cells[coordinates.ToOffsetX() % HexMetrics.chunkSizeX + (coordinates.ToOffsetZ() % HexMetrics.chunkSizeZ) * HexMetrics.chunkSizeX];
        }
    }
}
