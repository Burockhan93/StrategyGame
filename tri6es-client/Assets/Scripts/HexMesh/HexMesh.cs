using System.Collections.Generic;
using UnityEngine;
using Shared.HexGrid;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    /// <summary>How much of the tribe color is mixed into a cell border color.</summary>
    const float TRIBE_MIX = 0.3f;

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
        hexMesh.name = "Hex Mesh";

        waterMesh = GetComponentInChildren<WaterMesh>();
    }

    public void SetChunk(HexGridChunk chunk)
    {
        this.chunk = chunk;
        Triangulate();

        waterMesh.SetChunk(chunk);
    }

    void ResetMeshData()
    {
        hexMesh.Clear();
        vertices.Clear();
        colors.Clear();
        triangles.Clear();
    }

    void ApplyMeshData()
    {
        hexMesh.SetVertices(vertices.ToArray());
        hexMesh.SetColors(colors.ToArray());
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();

        meshCollider.sharedMesh = hexMesh;
    }

    public void Refresh()
    {
        Triangulate();
        waterMesh.Triangulate();
    }

    public void Triangulate()
    {
        ResetMeshData();
        foreach (HexCell cell in chunk.cells)
        {
            TriangulateCell(cell);
        }
        ApplyMeshData();
    }

    /// <summary>Triangulate all cells normally except those given in @param cells which are highlighted.</summary>
    public void TriangulateHighlight(List<HexCell> cells)
    {
        ResetMeshData();
        foreach (HexCell cell in chunk.cells)
        {
            if (!cells.Contains(cell))
                TriangulateCell(cell);
            else
                TriangulateCell(cell, Color.green);
        }
        ApplyMeshData();
    }

    /// <summary>Triangulate the cell with the default color.</summary>
    public void TriangulateCell(HexCell cell)
    {
        TriangulateCell(cell, GetCellColor(cell));
    }

    /// <summary>Triangulate the cell with a given color.</summary>
    public void TriangulateCell(HexCell cell, Color color)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            TriangulateCellPart(d, cell, color);
        }
    }

    /// <summary>Triangulate cell parts with a given color.</summary>
    void TriangulateCellPart(HexDirection direction, HexCell cell, Color color)
    {
        Vector3 center = cell.Position + new Vector3(0, cell.Elevation * MeshMetrics.elevationStep, 0);
        Vector3 corner1 = center + MeshMetrics.GetFirstSolidCorner(direction);
        Vector3 corner2 = center + MeshMetrics.GetSecondSolidCorner(direction);
        AddTriangle(center, corner1, corner2, color);

        TriangulateBorder(direction, cell, corner1, corner2);
    }

    void TriangulateBorder(HexDirection direction, HexCell cell, Vector3 corner1, Vector3 corner2)
{
        // get neighbors
        HexDirection prev = direction.Previous();
        HexDirection next = direction.Next();
        HexCell neighbor = cell.GetNeighbor(direction);
        HexCell prevNeighbor = cell.GetNeighbor(prev);
        HexCell nextNeighbor = cell.GetNeighbor(next);

        // compute position differences
        Vector3 diff = MeshMetrics.GetBridge(direction) - new Vector3(0, cell.GetElevationDifference(direction) * MeshMetrics.elevationStep, 0);
        Vector3 prevDiff = MeshMetrics.GetBridge(prev) - new Vector3(0, cell.GetElevationDifference(prev) * MeshMetrics.elevationStep, 0);
        Vector3 nextDiff = MeshMetrics.GetBridge(next) - new Vector3(0, cell.GetElevationDifference(next) * MeshMetrics.elevationStep, 0);

        // draw border
        Vector3 middle1 = corner1 + diff / 2;
        Vector3 middle2 = corner2 + diff / 2;
        AddQuad(corner1, corner2, middle1, middle2, GetBorderColor(cell, neighbor));

        // draw previous corner
        Vector3 prevMiddle = corner1 + prevDiff / 2;
        Vector3 prevCornerCenter = (3 * corner1 + diff + prevDiff) / 3;
        AddTriangle(corner1, prevMiddle, prevCornerCenter, GetBorderCornerColor(cell, neighbor, prevNeighbor));

        // draw next corner
        Vector3 nextMiddle = corner2 + nextDiff / 2;
        Vector3 nextCornerCenter = (3 * corner2 + diff + nextDiff) / 3;
        AddTriangle(corner2, nextCornerCenter, nextMiddle, GetBorderCornerColor(cell, neighbor, nextNeighbor));
    }

    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color color)
    {
        AddTriangle(v1, v3, v2, color);
        AddTriangle(v2, v3, v4, color);
    }

    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color color)
    {
        // add indices
        int index = vertices.Count;
        vertices.Add(Perturb(v1));
        vertices.Add(Perturb(v2));
        vertices.Add(Perturb(v3));

        // add vertices
        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);

        // add color
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

    /// <summary>Returns the color for the cell.</summary>
    Color GetCellColor(HexCell cell)
    {
        return cell.Data.Biome.ToColor();
    }

    /// <summary>Returns the color of the tribe.</summary>
    Color GetTribeColor(byte tribe)
    {
        return MeshMetrics.TribeToColor(tribe, 1.0f, 0.95f);
    }

    /// <summary>Returns the border color for a cell and its neighbor.</summary>
    Color GetBorderColor(HexCell cell, HexCell neighbor)
    {
        // get cell color or use higher neighbor color
        Color color;
        if (cell.GetElevationDifference(neighbor) < 0)
            color = GetCellColor(neighbor);
        else
            color = GetCellColor(cell);

        // check for owning tribe
        if (cell.isProtected())
        {
            // use or mix tribe color
            Color tribe = GetTribeColor((byte) cell.GetCurrentTribe());
            bool border = neighbor == null || cell.GetCurrentTribe() != neighbor.GetCurrentTribe();
            return border ? tribe : Color.Lerp(color, tribe, TRIBE_MIX);
        }
        else
        {
            return color;
        }
    }

    /// <summary>Returns the border corner color for a cell and its two neighbors.</summary>
    Color GetBorderCornerColor(HexCell cell, HexCell neighbor1, HexCell neighbor2)
    {
        // get cell color or use higher neighbor color
        Color color;
        if (cell.GetElevationDifference(neighbor2) < 0)
        {
            int diff = neighbor2.GetElevationDifference(neighbor1);
            if (diff < 0)
            {
                // neighbor1 is highest
                color = GetCellColor(neighbor1);
            }
            else
            {
                // default to neighbor2
                color = GetCellColor(neighbor2);
            }
        }
        else if (cell.GetElevationDifference(neighbor1) < 0)
        {
            // neighbor1 is highest
            color = GetCellColor(neighbor1);
        }
        else
        {
            // cell is highest or all are same
            color = GetCellColor(cell);
        }

        // check for owning tribe
        if (cell.isProtected())
        {
            // use or mix tribe color
            Color tribe = GetTribeColor((byte) cell.GetCurrentTribe());
            bool border = (
                neighbor1 == null
                || cell.GetCurrentTribe() != neighbor1.GetCurrentTribe()
                || neighbor2 == null
                || cell.GetCurrentTribe() != neighbor2.GetCurrentTribe()
            );
            return border ? tribe : Color.Lerp(color, tribe, TRIBE_MIX);
        }
        else
        {
            return color;
        }
    }
}
