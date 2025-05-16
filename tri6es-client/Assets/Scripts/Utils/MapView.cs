using UnityEngine;
using Shared.HexGrid;
using Shared.Game;
using System;
using System.Collections.Generic;

public static class MapView
{
    /// <summary>
    ///  Canvas's limits : from -70 to 70 both in x and y(z) axis
    ///  Texture2D has 70*70 pixels.
    ///  This will return us an approximate Value
    /// </summary>
    /// <returns>A list of Tupples that holds locations of Players with an Avatar</returns>
    public static List<Tuple<int,int>> PlayerCoordinates()
    {
        List<Tuple<int, int>> Coordinates = new List<Tuple<int, int>>();

        foreach (Player player in GameLogic.Players)
        {
                HexCell playerCell = GameLogic.grid.GetCell(player.Position);
                int x = playerCell.coordinates.ToOffsetX();
                int z = playerCell.coordinates.ToOffsetZ();

               
                int x1 = -70 + 2 * x;
                int z1 = -70 + 2 * z;

                Coordinates.Add(new Tuple<int, int>(x1, z1));
            
        }

        return Coordinates;
    }
    /// <summary>
    /// This Metod will return a pixelated MapView as 2D Texture if the given ID=0
    /// if ID=1 then will return Tribe Terrains with a transparent overlay. 
    /// <param name="id">should either 1 or 0. Could be extended for further uses</param>
    /// </summary>
    public static Texture2D getMapView(HexGrid grid,int id =99)
    {
        Texture2D mapView;
       
        mapView = new Texture2D(grid.cellCountX, grid.cellCountZ);
        
        mapView.filterMode = FilterMode.Point;

        Color transparent;

        for (int i = 0; i < grid.cellCountX; i++)
        {
            for (int j = 0; j < grid.cellCountZ; j++)
            {
                HexCell cell = grid.GetCell(i, j);

                if (id == 0) mapView.SetPixel(i, j, cell.Data.Biome.ToColor());
                if (id == 1)
                {
                    if (cell.isProtected())
                    {
                        //Get a transparent color for your tribe
                        transparent = MeshMetrics.TribeToColor((byte)cell.GetCurrentTribe());
                        transparent.a = 0.3f;
                        mapView.SetPixel(i, j, transparent);
                    }
                    else
                    {
                        mapView.SetPixel(i, j, Color.clear);
                    }
                    
                }

            }
        }

        //Set Player Location
        //if(id == 1)
        //{
        //    int count=0;
        //    foreach(Player player in GameLogic.Players)
        //    {
                
        //        HexCell playerCell = grid.GetCell(player.Position);
        //        int x = playerCell.coordinates.ToOffsetX();
        //        int z = playerCell.coordinates.ToOffsetZ();

        //        if (player.Tribe != null)
        //        {

        //            mapView.SetPixel(x, z, MeshMetrics.TribeToColor(player.Tribe.Id));
                    
        //        }
        //        else
        //        {
        //            mapView.SetPixel(x, z, Color.white);
        //        }
        //    }
           

        //}
        

        mapView.Apply();

        return mapView;
    }
}
