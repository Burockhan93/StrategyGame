using System.Collections.Generic;
using UnityEngine;
using Shared.HexGrid;

public class WaterMesh : MonoBehaviour
{
    Mesh hexMesh;

    static List<Vector3> vertices = new List<Vector3>();
    static List<Color> colors = new List<Color>();
    static List<int> triangles = new List<int>();

    private HexGridChunk chunk;

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();       
        hexMesh.name = "Hex Mesh";
    }

    public void SetChunk(HexGridChunk chunk)
    {
        this.chunk = chunk;
        Triangulate();
    }

    public void Triangulate()
    {
        hexMesh.Clear();
        vertices.Clear();
        //colors.Clear();
        triangles.Clear();
        foreach (HexCell cell in chunk.cells)
        {
            if(cell.Data.WaterDepth != 0)
                Triangulate(cell);
        }
        hexMesh.vertices = vertices.ToArray();
        //hexMesh.colors = colors.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();
    }

    private void Triangulate(HexCell cell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }
    }

    private void Triangulate(HexDirection direction, HexCell cell)
    {
        Vector3 center = cell.Position + new Vector3(0, cell.Data.Elevation * MeshMetrics.elevationStep -0.4f);
        Vector3 v1 = center + MeshMetrics.GetFirstCorner(direction);
        Vector3 v2 = center + MeshMetrics.GetSecondCorner(direction);
        AddTriangle(center, v1, v2);
        //AddTriangleColor(cell.Data.Biome.ToColor());
    }

    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }
}
