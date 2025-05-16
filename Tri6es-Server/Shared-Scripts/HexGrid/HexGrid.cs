using Shared.DataTypes;
using UnityEngine;

namespace Shared.HexGrid
{
    public class HexGrid
    {
        public float cornerLon, cornerLat;
        public float deltaLon, deltaLat;

        public int chunkCountX, chunkCountZ;
        public int cellCountX, cellCountZ;

        public float Width { get { return HexMetrics.innerRadius + 2f * HexMetrics.innerRadius * (float)cellCountX; } }
        public float Height { get { return 0.5f * HexMetrics.outerRadius + 1.5f * HexMetrics.outerRadius * (float)cellCountZ; } }

        public HexCell[] cells;

        public HexGridChunk chunkPrefab;

        public HexGridChunk[] chunks;

        public string weather;
        public string snow;
        public string rain;
        public string sun;

        public HexGrid(int chunkCountX, int chunkCountZ)
        {
            this.chunkCountX = chunkCountX;
            this.chunkCountZ = chunkCountZ;

            cellCountX = chunkCountX * HexMetrics.chunkSizeX;
            cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

            CreateChunks();
            CreateCells();
        }

        public HexGrid(int chunkCountX, int chunkCountZ, float cornerLon, float cornerLat, float deltaLon, float deltaLat)
        {
            this.chunkCountX = chunkCountX;
            this.chunkCountZ = chunkCountZ;

            this.cornerLon = cornerLon;
            this.cornerLat = cornerLat;

            this.deltaLon = deltaLon;
            this.deltaLat = deltaLat;

            cellCountX = chunkCountX * HexMetrics.chunkSizeX;
            cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

            CreateChunks();
            CreateCells();
        }

        void CreateChunks()
        {
            chunks = new HexGridChunk[chunkCountX * chunkCountZ];

            for (int z = 0, i = 0; z < chunkCountZ; z++)
            {
                for (int x = 0; x < chunkCountX; x++)
                {
                    HexGridChunk chunk = chunks[i++] = new HexGridChunk();
                }
            }
        }

        void CreateCells()
        {
            cells = new HexCell[cellCountX * cellCountZ];
            for (int z = 0, i = 0; z < cellCountZ; z++)
            {
                for (int x = 0; x < cellCountX; x++)
                {
                    CreateCell(x, z, i++);
                }
            }
        }

        void CreateCell(int x, int z, int i)
        {
            HexCell cell = cells[i] = new HexCell();
            cell.Data = new HexCellData();
            cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);           

            UpdateNeighbors(x, z, i);

            AddCellToChunk(x, z, cell);
        }

        public void UpdateNeighbors(int x, int z, int i) 
        {
            HexCell cell = cells[i];
            if (x > 0)
            {
                cell.setNeighbor(HexDirection.W, cells[i - 1]);
            }

            if (z > 0)
            {
                if ((z & 1) == 0)
                {
                    cell.setNeighbor(HexDirection.SE, cells[i - cellCountX]);
                    if (x > 0)
                    {
                        cell.setNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                    }
                }
                else
                {
                    cell.setNeighbor(HexDirection.SW, cells[i - cellCountX]);
                    if (x < cellCountX - 1)
                    {
                        cell.setNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
                    }
                }
            }
        }

        public void AddCellToChunk(int x, int z, HexCell cell)
        {
            int chunkX = x / HexMetrics.chunkSizeX;
            int chunkZ = z / HexMetrics.chunkSizeZ;

            HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

            int localX = x - chunkX * HexMetrics.chunkSizeX;
            int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
            chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
        }

        public HexCell GetCell(int x, int z)
        {
            return GetCell(HexCoordinates.FromOffsetCoordinates(x, z));
        }

        public HexCell GetCell(HexCoordinates coordinates)
        {
            int z = coordinates.Z;
            int x = coordinates.X + z / 2;
            if (x + z * cellCountX < 0 | x + z * cellCountX > cellCountX * cellCountZ - 1)
                return null;
            return cells[x + z * cellCountX];
        }

        public HexCell GetCell(Vector3 position)
        {
            HexCoordinates coordinates = HexCoordinates.FromPosition(position);
            return GetCell(coordinates);
        }

        public HexCell GetCell(float lon, float lat)
        {
            float dx = (lon - cornerLon) / deltaLon;
            float dz = (lat - cornerLat) / deltaLat;

            return GetCell(new Vector3(Width * dx, 0, Height * dz));
        }

        public Vector3 GetPosition(HexCoordinates coordinates)
        {
            /*coordinates.X = Width * dx;
            coordinates.Z = Height * dz;

            float dx = (lon - cornerLon) / deltaLon;
            float dz = (lat - cornerLat) / deltaLat;*/

            float dx = coordinates.X / Width;
            float dz = coordinates.Y / Height;

            float lon = dx * deltaLon + cornerLon;
            float lat = dz * deltaLat + cornerLat;

            return new Vector3(lon, lat);
        }

    }
}


