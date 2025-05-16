
namespace Shared.DataTypes
{
    public enum HexCellBiome
    {
        FOREST, SCRUB, GRASS, CROP, ROCK, SNOW, CITY, BUILDINGS, WATER, COAL
    }


    public struct HexCellData
    {
        public int Elevation { get; set; }
        public HexCellBiome Biome { get; set; }        
        public byte WaterDepth { get; set; }      

        public HexCellData(int elevation, HexCellBiome biome, byte waterDepth)
        {
            Elevation = elevation;
            Biome = biome;
            WaterDepth = waterDepth;
        }

        public void SetElevation(int elevation)
        {
            this.Elevation = elevation;
        }

        public void SetBiome(HexCellBiome biome)
        {
            this.Biome = biome;
        }

        public void SetWaterDepth(byte waterDepth)
        {
            this.WaterDepth = waterDepth;
        }

        public string toString()
        {
            return  this.Elevation + ", " + this.Biome + ", " + this.WaterDepth;
        }
    }
}
