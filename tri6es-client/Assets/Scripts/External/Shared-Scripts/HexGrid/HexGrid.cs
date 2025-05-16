using Shared.DataTypes;
using UnityEngine;
using System.Globalization;
using System.Xml;
using System;

namespace Shared.HexGrid
{
    public enum Precipitation
    {
        NONE, DRIZZLE, RAIN, SNOW 
    }
    public enum Obscuration
    {
        NONE, FOG
    }

    public enum Descriptor
    {
        NONE, THUNDERSTORM
    }
    
    public enum Wind
    {
        NONE, WIND, STORM
    }
    public enum Cloudy
    {
        NONE, FEW, SCATTERD, BROKEN
    }
    public enum Sunny
    {
        NONE, SUN
    }
    public class HexGrid
    {
        public float cornerLon, cornerLat;
        public float deltaLon, deltaLat;

        public int chunkCountX, chunkCountZ;
        public int cellCountX, cellCountZ;

        public int weather_id;
        public float wind_speed;
        public Precipitation precipitation;
        public Obscuration obscuration;
        public Descriptor descriptor;
        public Wind wind;
        public Cloudy cloudy;
        public Sunny sunny;

        // Display the forecast.
        public void ParseForecast(string weather)
        {

            // Load the response into an XML document.
            XmlDocument xml_doc = new XmlDocument();

            Console.WriteLine("Test: \n" + weather);
            try
            {
                xml_doc.LoadXml(weather);


                // Get the city, country, latitude, and longitude.
                XmlNode loc_node = xml_doc.SelectSingleNode("current/city");
                String txtCity = loc_node.Attributes["name"].Value;
                Console.WriteLine($"textCity: {txtCity}");
                String txtId = loc_node.Attributes["id"].Value;
                Console.WriteLine($"txtId: {txtId}");
                String txtCountry = loc_node.SelectSingleNode("country").InnerText;
                Console.WriteLine($"txtCountry: {txtCountry}");
                XmlNode geo_node = loc_node.SelectSingleNode("coord");
                String txtLat = geo_node.Attributes["lat"].Value;
                Console.WriteLine($"txtLat: {txtLat}");
                String txtLong = geo_node.Attributes["lon"].Value;
                Console.WriteLine($"txtLong: {txtLong}");

                char degrees = (char)176;

                XmlNode time_node = xml_doc.SelectSingleNode("current/lastupdate");
                // Get the time in UTC.
                DateTime time = DateTime.Parse(time_node.Attributes["value"].Value, null, DateTimeStyles.AssumeUniversal);
                DateTime dateToday = DateTime.Today;

                // Get the temperature.
                XmlNode temp_node = xml_doc.SelectSingleNode("current/temperature");
                string temp = temp_node.Attributes["value"].Value;

                // Get the weather.
                XmlNode weather_node = xml_doc.SelectSingleNode("current/weather");
                string weather_value = weather_node.Attributes["value"].Value;
                weather_id = int.Parse(weather_node.Attributes["number"].Value);

                // Get percipitation
                XmlNode precipitation_node = xml_doc.SelectSingleNode("current/precipitation");
                string precipitation_string = precipitation_node.Attributes["mode"].Value;

                // Get the wind.
                XmlNode wind_node = xml_doc.SelectSingleNode("current/wind");
               // Debug.Log($"Wind_speed unparsed: {wind_node.SelectSingleNode("speed").Attributes["value"].Value}");
                CultureInfo provider = new CultureInfo("en-US");
                wind_speed = float.Parse(wind_node.SelectSingleNode("speed").Attributes["value"].Value, provider);

                //Debug.Log($"Wind_speed: {wind_speed}");



                // Doku: https://openweathermap.org/weather-conditions


                precipitation = GetPrecipitation(weather_id);
                obscuration = GetObscuration(weather_id);
                descriptor = GetDescriptor(weather_id);
                cloudy = GetCloudy(weather_id);
                sunny = GetSunny(weather_id);

                float wind_kmh = wind_speed * 3.6f;
                wind = GetWind(wind_kmh);
                //Debug.Log($"Wind_speed set: {wind}");

            }
            catch (XmlException e)
            {
                Console.WriteLine("XML error: " + e.Message);
            }

        }

        public Wind GetWind(float wind_kmh)
        {
            //Debug.Log($"Wind Speed: {wind_kmh}");
            if (29 < wind_kmh && wind_kmh <= 65)
            {
                return Wind.WIND;
            }
            else if (65 < wind_kmh)
            {
                return Wind.STORM;
            }
            return Wind.NONE;
        }
        public Descriptor GetDescriptor(int weather_id)
        {
            if (200 <= weather_id && weather_id < 300)
            {
                return Descriptor.THUNDERSTORM;
            }

            return Descriptor.NONE;
            
        }
        public Obscuration GetObscuration(int weather_id)
        {

            if (weather_id == 741 || weather_id == 701)
            {
                return Obscuration.FOG;
            }

            return Obscuration.NONE;
            
        }
        public Precipitation GetPrecipitation(int weather_id)
        {
            if (300 <= weather_id && weather_id < 400 || 230 <= weather_id && weather_id <= 232)
            {
                return Precipitation.DRIZZLE;
            }
            else if (500 <= weather_id && weather_id < 600 || 200 <= weather_id && weather_id <= 202)
            {
                return Precipitation.RAIN;
            }
            else if (600 <= weather_id && weather_id < 700)
            {
                return Precipitation.SNOW;
                //if(weather_id == 615)
                //{
                //    precipitation = Precipitation.BOTH;
                //}
            }
            
             return Precipitation.NONE;
        }
        public Cloudy GetCloudy(int weather_id)
        {
            if(weather_id == 801)
            {
                return Cloudy.FEW;
            }
            else if (weather_id == 802)
            {
                return Cloudy.SCATTERD;
            }
            else if (weather_id == 803 || weather_id == 804)
            {
                return Cloudy.BROKEN;
            }
            return Cloudy.NONE;
        }
        public Sunny GetSunny(int weather_id)
        {
            if(weather_id == 800)
            {
                return Sunny.SUN;
            }
            return Sunny.NONE;
        }
        public float Width { get { return HexMetrics.innerRadius + 2f * HexMetrics.innerRadius * (float)cellCountX; } }
        public float Height { get { return 0.5f * HexMetrics.outerRadius + 1.5f * HexMetrics.outerRadius * (float)cellCountZ; } }

        public HexCell[] cells;

        public HexGridChunk chunkPrefab;

        public HexGridChunk[] chunks;
        
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
                    /*HexGridChunk chunk = */chunks[i++] = new HexGridChunk();
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


