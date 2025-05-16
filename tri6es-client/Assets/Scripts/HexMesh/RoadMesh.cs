using System.Collections.Generic;
using UnityEngine;
using Shared.HexGrid;
using Shared.Structures;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoadMesh : MonoBehaviour
{
    Mesh hexMesh;


    static List<Vector3> vertices = new List<Vector3>();
    static List<Color> colors = new List<Color>();
    static List<int> triangles = new List<int>();

    MeshCollider meshCollider;

    private HexGridChunk chunk;

    private WaterMesh waterMesh;

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        hexMesh.name = "RoadMesh";
        enabled = false;
    }

    private void LateUpdate()
    {
        Triangulate();
        enabled = false;
    }

    public void SetChunk(HexGridChunk chunk)
    {
        this.chunk = chunk;
        Refresh();
    }

    public void Refresh()
    {
        enabled = true;
    }

    public void Triangulate()
    {
        hexMesh.Clear();
        vertices.Clear();
        colors.Clear();
        triangles.Clear();
        foreach (HexCell cell in chunk.cells)
        {
            Triangulate(cell);
        }
        hexMesh.vertices = vertices.ToArray();
        hexMesh.colors = colors.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();

        meshCollider.sharedMesh = hexMesh;
    }

    public void Triangulate(HexCell cell)
    {
        if (cell.Structure != null && cell.Structure is Road)
        {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                Triangulate(d, cell);
            }
        }

    }

    void Triangulate(HexDirection direction, HexCell cell)
    {
        Road road = (Road)cell.Structure;
        if (road is Bridge)
        {
            TriangulateBridge(direction, cell);
            return;
        }

        Vector3 center = cell.Position + new Vector3(0, road.GetElevation() * MeshMetrics.elevationStep + MeshMetrics.roadHeight);

        Vector3 v1 = center + MeshMetrics.GetFirstSolidRoadCorner(direction);
        Vector3 v2 = center + MeshMetrics.GetSecondSolidRoadCorner(direction);

        Color roadColor = MeshMetrics.GetRoadColor(road.Level);

        if (road is Bridge)
            roadColor = Color.grey;

        if (road.HasBuilding(direction))
        {
            AddTriangle(center, v1, v2);
            AddTriangleColor(roadColor);

            if (road.HasRoad(direction))
            {
                if (direction <= HexDirection.SE)
                {
                    TriangulateConnection(direction, cell, v1, v2);
                }
            }
            else
                TriangulateEnding(direction, cell, v1, v2);

            if (road.IsEmpty(direction.Next()))
            {
                AddQuad(center, center + MeshMetrics.GetOrthogonalOffset(direction.Next().Next()), v2, v2 + MeshMetrics.GetRoadGroundOffset(direction.Next()));
                AddTriangleColor(roadColor);
                AddTriangleColor(roadColor);
            }
            if (road.IsEmpty(direction.Previous()))
            {
                AddQuad(center, v1, center + MeshMetrics.GetOrthogonalOffset(direction.Previous().Previous()), v1 + MeshMetrics.GetRoadGroundOffset(direction));
                AddTriangleColor(roadColor);
                AddTriangleColor(roadColor);
            }
        }
        else if (road.HasStraightLine(direction.Previous()))
        {
            Vector3 v3 = center + MeshMetrics.GetSecondSolidRoadCorner(direction.Next());
            AddTriangle(center, v1, v3);
            AddTriangleColor(roadColor);

            TriangulateSide(v1, v3, direction, direction.Next().Next());
            AddTriangleColor(roadColor);
            AddTriangleColor(roadColor);
        }
        else if (road.IsSmoothCorner(direction))
        {
            Vector3 v3 = center + MeshMetrics.GetSecondSolidRoadCorner(direction.Next());
            AddTriangle(center, v1, v2);
            AddTriangleColor(roadColor);

            TriangulateSide(v1, v2, direction, direction.Next());
            AddTriangleColor(roadColor);
            AddTriangleColor(roadColor);
        }
        else if (!road.HasAnyConnection())
        {
            AddTriangle(center, v1, v2);
            AddTriangleColor(roadColor);

            TriangulateSide(v1, v2, direction, direction.Next());
            AddTriangleColor(roadColor);
            AddTriangleColor(roadColor);
        }
        else if (road.IsEmpty(direction.Next()) && road.IsEmpty(direction.Previous()))
        {
            AddTriangle(center, center + MeshMetrics.GetOrthogonalOffset(direction), center + MeshMetrics.GetOrthogonalOffset(direction.Next()));
            AddTriangleColor(roadColor);
        }
    }

    void TriangulateBridge(HexDirection direction, HexCell cell)
    {
        Road road = (Road)cell.Structure;
        Vector3 center = cell.Position + new Vector3(0, road.GetElevation() * MeshMetrics.elevationStep + MeshMetrics.roadHeight);

        Vector3 v1 = center + MeshMetrics.GetFirstSolidRoadCorner(direction);
        Vector3 v2 = center + MeshMetrics.GetSecondSolidRoadCorner(direction);

        Color roadColor = Color.grey;


        if (road.HasBuilding(direction))
        {
            AddTriangle(center, v1, v2);
            AddTriangleColor(roadColor);

            if (road.HasRoad(direction))
            {
                if (direction <= HexDirection.SE)
                {
                    TriangulateConnection(direction, cell, v1, v2);
                }
            }
            else
                TriangulateEnding(direction, cell, v1, v2);

            if (road.IsEmpty(direction.Next()))
            {
                if(road is Bridge)
                    AddQuad(center, center + MeshMetrics.GetOrthogonalBridgeOffset(direction.Next().Next()), v2, v2 + MeshMetrics.GetBridgeGroundOffset(direction.Next()));
                else
                    AddQuad(center, center + MeshMetrics.GetOrthogonalOffset(direction.Next().Next()), v2, v2 + MeshMetrics.GetRoadGroundOffset(direction.Next()));
                AddTriangleColor(roadColor);
                AddTriangleColor(roadColor);
            }
            if (road.IsEmpty(direction.Previous()))
            {
                if (road is Bridge)
                    AddQuad(center, v1, center + MeshMetrics.GetOrthogonalBridgeOffset(direction.Previous().Previous()), v1 + MeshMetrics.GetBridgeGroundOffset(direction));
                else
                    AddQuad(center, v1, center + MeshMetrics.GetOrthogonalOffset(direction.Previous().Previous()), v1 + MeshMetrics.GetRoadGroundOffset(direction));
               
                AddTriangleColor(roadColor);
                AddTriangleColor(roadColor);
            }
        }
        else if (road.HasStraightLine(direction.Previous()))
        {
            Vector3 v3 = center + MeshMetrics.GetSecondSolidRoadCorner(direction.Next());
            AddTriangle(center, v1, v3);
            AddTriangleColor(roadColor);

            TriangulateBridgeSide(v1, v3, direction, direction.Next().Next());
            AddTriangleColor(roadColor);
            AddTriangleColor(roadColor);
        }
        else if (road.IsSmoothCorner(direction))
        {
            Vector3 v3 = center + MeshMetrics.GetSecondSolidRoadCorner(direction.Next());
            AddTriangle(center, v1, v2);
            AddTriangleColor(roadColor);

            TriangulateBridgeSide(v1, v2, direction, direction.Next());
            AddTriangleColor(roadColor);
            AddTriangleColor(roadColor);
        }
        else if (!road.HasAnyConnection())
        {
            AddTriangle(center, v1, v2);
            AddTriangleColor(roadColor);

            TriangulateBridgeSide(v1, v2, direction, direction.Next());
            AddTriangleColor(roadColor);
            AddTriangleColor(roadColor);
        }
        else if (road.IsEmpty(direction.Next()) && road.IsEmpty(direction.Previous()))
        {
            AddTriangle(center, center + MeshMetrics.GetOrthogonalOffset(direction), center + MeshMetrics.GetOrthogonalOffset(direction.Next()));
            AddTriangleColor(roadColor);
        }
    }

    void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());

        Road neighborRoad = (Road)neighbor.Structure;

        if (neighbor == null)
            return;

        

        Color roadColor = MeshMetrics.GetRoadColor(((Road)cell.Structure).Level);
        Color neighborColor = MeshMetrics.GetRoadColor(((Road)neighbor.Structure).Level);

        

        Vector3 v3 = v1 + MeshMetrics.GetRoadBridge(direction);
        Vector3 v4 = v2 + MeshMetrics.GetRoadBridge(direction);

        v3.y = v4.y = neighborRoad.GetElevation() * MeshMetrics.elevationStep + MeshMetrics.roadHeight;



        Vector3 v5 = v1 + 0.5f * (v3 - v1);
        Vector3 v6 = v2 + 0.5f * (v4 - v2);

        TriangulateConnectionSides(cell, v1, v2, v3, v4, direction);
    }

    void TriangulateConnectionSides(HexCell cell, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, HexDirection direction)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());

        Road neighborRoad = (Road)neighbor.Structure;

        if (neighbor == null)
            return;

        Vector3 v5 = v1 + 0.5f * (v3 - v1);
        Vector3 v6 = v2 + 0.5f * (v4 - v2);

        Color roadColor = MeshMetrics.GetRoadColor(((Road)cell.Structure).Level);
        Color neighborColor = MeshMetrics.GetRoadColor(((Road)neighbor.Structure).Level);

        if (cell.Structure is Bridge || neighbor.Structure is Bridge)
        {
            roadColor = Color.grey;
            neighborColor = Color.grey;
        }

        v5.y = v6.y += 0.1f * 8 * Mathf.Abs(((Road)cell.Structure).GetElevationDifference(direction));

        AddQuad(v1, v2, v5, v6);

        AddTriangleColor(roadColor);
        AddTriangleColor(roadColor);

        AddQuad(v5, v6, v3, v4);

        AddTriangleColor(neighborColor);
        AddTriangleColor(neighborColor);

        Vector3 c1 = v1 + ((cell.Structure is Bridge || neighbor.Structure is Bridge) ? MeshMetrics.GetBridgeGroundOffset(direction) : MeshMetrics.GetRoadGroundOffset(direction));
        Vector3 c3 = v3 + ((cell.Structure is Bridge || neighbor.Structure is Bridge) ? MeshMetrics.GetBridgeGroundOffset(direction.Opposite().Next()) : MeshMetrics.GetRoadGroundOffset(direction.Opposite().Next()));

        AddQuad(v1, v5, c1, c1 + 0.5f * (c3 - c1));
        AddTriangleColor(roadColor);
        AddTriangleColor(roadColor);

        AddQuad(v5, v3, c1 + 0.5f * (c3 - c1), c3);
        AddTriangleColor(neighborColor);
        AddTriangleColor(neighborColor);

        Vector3 c2 = v2 + ((cell.Structure is Bridge || neighbor.Structure is Bridge) ? MeshMetrics.GetBridgeGroundOffset(direction.Next()) : MeshMetrics.GetRoadGroundOffset(direction.Next()));
        Vector3 c4 = v4 + ((cell.Structure is Bridge || neighbor.Structure is Bridge) ? MeshMetrics.GetBridgeGroundOffset(direction.Opposite()) : MeshMetrics.GetRoadGroundOffset(direction.Opposite()));

        AddQuad(v6, v2, c2 + 0.5f * (c4 - c2), c2);
        AddTriangleColor(roadColor);
        AddTriangleColor(roadColor);

        AddQuad(v4, v6, c4, c2 + 0.5f * (c4 - c2));
        AddTriangleColor(neighborColor);
        AddTriangleColor(neighborColor);
    }

    void TriangulateEnding(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());

        if (neighbor == null)
            return;

        Color roadColor = MeshMetrics.GetRoadColor(((Road)cell.Structure).Level);

        Vector3 v3 = v1 + MeshMetrics.GetRoadBridge(direction);
        Vector3 v4 = v2 + MeshMetrics.GetRoadBridge(direction);

        v3.y = v4.y -= 3;

        Vector3 v5 = v1 + 0.4f * (v3 - v1);
        Vector3 v6 = v2 + 0.4f * (v4 - v2);

        AddQuad(v1, v2, v5, v6);

        AddTriangleColor(roadColor);
        AddTriangleColor(roadColor);

        Vector3 c1 = v1 + MeshMetrics.GetRoadGroundOffset(direction);
        Vector3 c3 = v3 + MeshMetrics.GetRoadGroundOffset(direction.Opposite().Next());

        AddQuad(v1, v5, c1, c1 + 0.5f * (c3 - c1));
        AddTriangleColor(roadColor);
        AddTriangleColor(roadColor);

        Vector3 c2 = v2 + MeshMetrics.GetRoadGroundOffset(direction.Next());
        Vector3 c4 = v4 + MeshMetrics.GetRoadGroundOffset(direction.Opposite());

        AddQuad(v6, v2, c2 + 0.5f * (c4 - c2), c2);
        AddTriangleColor(roadColor);
        AddTriangleColor(roadColor);
    }

    void TriangulateSide(Vector3 v1, Vector3 v2, HexDirection d1, HexDirection d2) {
        AddQuad(v1, v2, v1 + MeshMetrics.GetRoadGroundOffset(d1), v2 + MeshMetrics.GetRoadGroundOffset(d2));
    }

    void TriangulateBridgeSide(Vector3 v1, Vector3 v2, HexDirection d1, HexDirection d2)
    {
        AddQuad(v1, v2, v1 + MeshMetrics.GetBridgeGroundOffset(d1), v2 + MeshMetrics.GetBridgeGroundOffset(d2));
    }

    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        AddTriangle(v1, v3, v2);
        AddTriangle(v2, v3, v4);
    }

    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(Perturb(v1));
        vertices.Add(Perturb(v2));
        vertices.Add(Perturb(v3));
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    void AddTriangleColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }

    Vector3 Perturb(Vector3 position)
    {
        Vector4 sample = MeshMetrics.SampleNoise(position);
        position.x += (sample.x * 2f - 1f) * MeshMetrics.cellPerturbStrength;
        position.y += (sample.y * 2f - 1f) * MeshMetrics.elevationPerturbStrength;
        position.z += (sample.z * 2f - 1f) * MeshMetrics.cellPerturbStrength;

        return position;
    }
}
