using UnityEngine;
using Shared.HexGrid;
using Shared.DataTypes;

public static class MeshMetrics
{
    public const float solidFactor = 0.85f;
    public const float blendFactor = 1f - solidFactor;


    public const float roadSolidFactor = 0.3f;
    public const float roadBlendFactor = 1f - roadSolidFactor;
    public const float roadHeight = 1f;

    public const float roadSideFactor = 0.2f;

    public const float elevationStep = .4f * 8;

    public static Texture2D noiseSource;

    public const float noiseScale = 0.003f;
    public const float cellPerturbStrength = 1.5f;
    public const float elevationPerturbStrength = 1.5f;

    #region HexMesh
    public static Vector3 GetFirstCorner(HexDirection direction)
    {
        return HexMetrics.corners[(int)direction];
    }

    public static Vector3 GetSecondCorner(HexDirection direction)
    {
        return HexMetrics.corners[((int)direction + 1) % 6];
    }

    public static Vector3 GetFirstSolidRoadCorner(HexDirection direction)
    {
        return HexMetrics.corners[(int)direction] * roadSolidFactor;
    }

    public static Vector3 GetSecondSolidRoadCorner(HexDirection direction)
    {
        return HexMetrics.corners[((int)direction + 1) % 6] * roadSolidFactor;
    }
    public static Vector3 GetFirstSolidCorner(HexDirection direction)
    {
        return HexMetrics.corners[(int)direction] * solidFactor;
    }

    public static Vector3 GetSecondSolidCorner(HexDirection direction)
    {
        return HexMetrics.corners[((int)direction + 1) % 6] * solidFactor;
    }

    public static Vector3 GetBridge(HexDirection direction)
    {
        return (HexMetrics.corners[(int)direction] + HexMetrics.corners[((int)direction + 1) % 6]) * blendFactor;
    }
    #endregion

    #region RoadMesh
    public static Vector3 GetRoadBridge(HexDirection direction)
    {
        return (HexMetrics.corners[(int)direction] + HexMetrics.corners[((int)direction + 1) % 6]) * roadBlendFactor;
    }

    public static Vector3 GetRoadEnd(HexDirection direction)
    {
        return HexMetrics.GetFirstCorner(direction) * 0.4f - new Vector3(0, 3, 0);
    }

    public static Vector3 GetRoadGroundOffset(HexDirection direction)
    {
        return HexMetrics.GetFirstCorner(direction) * 0.2f - new Vector3(0, 3, 0);
    }

    public static Vector3 GetBridgeGroundOffset(HexDirection direction)
    {
        return HexMetrics.GetFirstCorner(direction) * 0.1f - new Vector3(0, 1.5f, 0);
    }

    public static Vector3 GetOrthogonalOffset(HexDirection direction)
    {
        Vector3 v = Vector3.Lerp(HexMetrics.GetFirstCorner(direction), HexMetrics.GetSecondCorner(direction), 0.5f);
        return Vector3.Normalize(v) * HexMetrics.outerRadius * 0.2f - new Vector3(0, 3 , 0);
    }

    public static Vector3 GetOrthogonalBridgeOffset(HexDirection direction)
    {
        Vector3 v = Vector3.Lerp(HexMetrics.GetFirstCorner(direction), HexMetrics.GetSecondCorner(direction), 0.5f);
        return Vector3.Normalize(v) * HexMetrics.outerRadius * 0.1f - new Vector3(0, 1.5f, 0);
    }

    public static Vector4 SampleNoise(Vector3 position)
    {
        return noiseSource.GetPixelBilinear(position.x * noiseScale, position.z * noiseScale);
    }
    #endregion

    /// <summary>Converts a tribe id to a hue float.</summary>
    public static float TribeToHue(byte tribe)
    {
        return (float) tribe / 16f;
    }

    /// <summary>Converts a tribe id to a color with the default saturation and brightness.</summary>
    public static Color TribeToColor(byte tribe)
    {
        return TribeToColor(tribe, 0.7f, 0.95f);
    }

    /// <summary>Converts a tribe id to a color with a custom saturation and brightness.</summary>
    public static Color TribeToColor(byte tribe, float saturation, float brightness)
    {
        float hue = TribeToHue(tribe);
        return Color.HSVToRGB(hue, saturation, brightness);
        
    }

    public static Color ToColor(this RessourceType ressourceType)
    {
        switch (ressourceType)
        {
            case RessourceType.WOOD:
                return new Color32(153, 105, 43, 255);
            case RessourceType.STONE:
                return new Color32(130, 130, 130, 255);
            case RessourceType.COAL:
                return new Color32(28, 28, 28, 255);
            case RessourceType.IRON:
                return new Color32(201, 201, 201, 255);
            case RessourceType.WHEAT:
                return new Color32(250, 194, 62, 255);
            case RessourceType.FOOD:
                return new Color32(36, 171, 209, 255);
            default:
                return new Color32(255, 255, 255, 255);
        }
    }

    public static Color ToColor(this HexCellBiome biome)
    {
        switch (biome)
        {
            case HexCellBiome.FOREST:
                return new Color32(83, 130, 36, 255);
            case HexCellBiome.GRASS:
                return new Color32(93, 148, 38, 255);
            case HexCellBiome.CROP:
                return new Color32(135, 92, 35, 255);
            case HexCellBiome.ROCK:
                return new Color32(82, 82, 82, 255);
            case HexCellBiome.SNOW:
                return new Color32(252, 252, 252, 255);
            case HexCellBiome.CITY:
                return new Color32(40, 40, 40, 255);
            case HexCellBiome.WATER:
                return new Color32(158, 122, 68, 255);
            case HexCellBiome.SCRUB:
                return new Color32(89, 145, 47, 255);
            case HexCellBiome.COAL:
                return new Color32(12, 12, 12, 255);
            default:
                return Color.white;
        }
    }

    public static Color GetRoadColor(int Level)
    {
        switch(Level)
        {
            case 1: return new Color32(145, 100, 60, 255);
            case 2: return new Color32(102, 69, 44, 255);
            default: return new Color32(74, 73, 72, 255);

        }

    }
}
