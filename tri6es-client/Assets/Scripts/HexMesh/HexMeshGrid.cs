using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Shared.HexGrid;
using Shared.Structures;
using Shared.Game;
using Shared.DataTypes;
using UnityEngine.UI;
using TMPro;
public class HexMeshGrid : MonoBehaviour
{
    public GameObject chunkPrefab;


    private GameObject[] chunks;

    private HexMesh[] meshes;

    private StructureManager[] structureManagers;

    private RoadMesh[] roadMeshes;


    private Dictionary<Player, (GameObject, GameObject)> players;

    public Texture2D noisePattern;

    private PrefabManager prefabManager;
    private WeatherManager weatherManager;

    public HexGrid grid;


    private List<int> activeChunks;

    private void Awake()
    {
        MeshMetrics.noiseSource = noisePattern;
        prefabManager = FindObjectOfType<PrefabManager>();
        weatherManager = FindObjectOfType<WeatherManager>();
        players = new Dictionary<Player, (GameObject, GameObject)>();

        activeChunks = new List<int>();
    }

    public void SetGrid(HexGrid grid)
    {
        this.grid = grid;
        chunks = new GameObject[grid.chunkCountX * grid.chunkCountZ];
        meshes = new HexMesh[grid.chunkCountX * grid.chunkCountZ];
        structureManagers = new StructureManager[grid.chunkCountX * grid.chunkCountZ];
        roadMeshes = new RoadMesh[grid.chunkCountX * grid.chunkCountZ];
        for (int i = 0; i < grid.chunkCountX * grid.chunkCountZ; i++)
        {
            InitChunk(i);
        }

        HexMapEditor editor = FindObjectOfType<HexMapEditor>();
        editor.hexGrid = grid;
    }

    private void InitChunk(int i)
    {
        GameObject hexChunk = Instantiate(chunkPrefab);

        hexChunk.transform.parent = this.transform;
        chunks[i] = hexChunk;

        meshes[i] = hexChunk.GetComponentInChildren<HexMesh>();
        meshes[i].SetChunk(grid.chunks[i]);

        structureManagers[i] = hexChunk.GetComponentInChildren<StructureManager>();
        structureManagers[i].SetChunk(grid.chunks[i]);

        roadMeshes[i] = hexChunk.GetComponentInChildren<RoadMesh>();
        roadMeshes[i].SetChunk(grid.chunks[i]);

        hexChunk.SetActive(false);
    }

    public void UpdateActiveChunks(HexCoordinates coordinates)
    {
        foreach (int chunk in activeChunks)
        {
            this.chunks[chunk].SetActive(false);
        }
        activeChunks.Clear();
        int index = ToChunkIndex(coordinates);

        for(int i = -2; i <= 2; i++)
        {
            for(int j = -2; j <= 2; j++)
            {
                int chunkIndex = index + i * grid.chunkCountX + j;
                if (chunkIndex >= 0 && chunkIndex < this.chunks.Length)
                {
                    this.chunks[chunkIndex].SetActive(true);
                    this.activeChunks.Add(chunkIndex);
                }
            }
        }
    }

    public void UpdateCell(HexCoordinates coordinates)
    {
        UpdateCell(GameLogic.grid.GetCell(coordinates));
    }

    public void UpdateCell(HexCell cell)
    {
        int index = cell.coordinates.ToChunkX() + cell.coordinates.ToChunkZ() * grid.chunkCountX;
        structureManagers[index].UpdateStructures(cell);

        UpdateRoads(cell);
    }

    public void BuildBuilding(HexCoordinates coords)
    {
        BuildBuilding(GameLogic.grid.GetCell(coords));
    }

    public void BuildBuilding(HexCell cell)
    {
        int index = cell.coordinates.ToChunkX() + cell.coordinates.ToChunkZ() * grid.chunkCountX;
        structureManagers[index].BuildBuilding(cell);

        UpdateRoads(cell);

        // check for protected building
        if (cell.Structure is ProtectedBuilding)
        {
            ProtectedBuilding building = (ProtectedBuilding) cell.Structure;
            RefreshCellChunks(building.GetProtectedCells());
        }
    }

    private void UpdateRoads(HexCell cell)
    {
        int index = ToChunkIndex(cell.coordinates);
        roadMeshes[index].Refresh();
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            HexCell neighbor = cell.GetNeighbor(d);
            if (neighbor != null)
            {
                index = neighbor.coordinates.ToChunkX() + neighbor.coordinates.ToChunkZ() * grid.chunkCountX;
                roadMeshes[index].Refresh();
            }
        }
    }

    /// <summary>Returns the chunk index of the given cell.</summary>
    private int ToChunkIndex(HexCoordinates coordinates)
    {
        return coordinates.ToChunkX() + coordinates.ToChunkZ() * grid.chunkCountX;
    }

    /// <summary>Returns the chunk meshes containing the given cells.</summary>
    private HashSet<HexMesh> GetChunkMeshes(List<HexCell> cells)
    {
        return new HashSet<HexMesh>(cells.Select(cell => meshes[ToChunkIndex(cell.coordinates)]));
    }

    /// <summary>Refreshes the chunk meshes for the given cells.</summary>
    public void RefreshCellChunks(List<HexCell> cells)
    {
        // force update the chunks
        foreach (HexMesh mesh in GetChunkMeshes(cells))
        {
            mesh.Triangulate();
        }
    }

    public void UpdatePlayerPosition(Player player)
    {
        int index = ToChunkIndex(player.Position);
        if (!players.ContainsKey(player))
        {
            GameObject playerPrefab = Instantiate(prefabManager.PlayerPrefab, Vector3.zero, Quaternion.identity);

            // check for own player
            if (player == Client.instance.Player)
            {
                HexCell cell = grid.GetCell(player.Position);
                HexMapCamera camera = FindObjectOfType<HexMapCamera>();
                camera.MoveCamera(cell);
                playerPrefab.transform.GetChild(0).gameObject.SetActive(true);
            }

            players.Add(player, (playerPrefab, null));
        }
        GameObject playerObject = players[player].Item1;

        Color _white = Color.white;
        

        if (player.Tribe != null)
        {
            _white.a = 1;
            //Set Avatars and its child material to tribe color
            playerObject.transform.GetChild(1).GetComponent<MeshRenderer>().material.SetColor("_BaseColor", MeshMetrics.TribeToColor(player.Tribe.Id));
            playerObject.transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_BaseColor", MeshMetrics.TribeToColor(player.Tribe.Id));
            playerObject.transform.GetChild(2).GetChild(1).GetComponent<TMP_Text>().text = $"{player.Name}";
                    
            if (player.Avatar < 255)
            {
                playerObject.transform.GetChild(2).GetChild(0).GetComponent<Image>().sprite = FindObjectOfType<UIManager>().Avatars[player.Avatar];
            }
            else
            {
                _white.a = 0;
            }

            playerObject.transform.GetChild(2).GetChild(0).GetComponent<Image>().color = _white;

            //dont show the name of own player.
            if (player == Client.instance.Player) playerObject.transform.GetChild(2).GetChild(1).GetComponent<TMP_Text>().text ="";
        }

        else
        {
            //Set Avatars and its child material to white when there is no tribe found
            playerObject.transform.GetChild(1).GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.white);
            playerObject.transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.white);
            playerObject.transform.GetChild(2).GetChild(1).GetComponent<TMP_Text>().text = $"{player.Name}";
            _white.a = 0;
            playerObject.transform.GetChild(2).GetChild(0).GetComponent<Image>().color = _white;
        }
        
        GameObject playerPosition = players[player].Item2;
        if (playerPosition != null)
        {
            playerPosition.SetActive(false);
        }
        playerPosition = structureManagers[index].SetPlayerPosition(player.Position, playerObject);
        players[player] = (playerObject, playerPosition);
    }


    public void Refresh()
    {
        //weatherManager.Update();
        foreach (int chunkIndex in activeChunks)
        {
            structureManagers[chunkIndex].Refresh();
        }
    }

    public void ResetCellHighlighting(List<HexCell> highlightedCells)
    {
        foreach (HexMesh mesh in GetChunkMeshes(highlightedCells))
        {
            mesh.Triangulate();
        }
    }

    public void HighlightCells(List<HexCell> placableCells)
    {
        foreach (HexMesh mesh in GetChunkMeshes(placableCells))
        {
            mesh.TriangulateHighlight(placableCells);
        }
    }
    public void aboveInformation(HexCoordinates coords)
    {
       List<HexCell> cell = GameLogic.grid.GetCell(coords).GetNeighbors(1);
       
       int index = coords.ToChunkX() + coords.ToChunkZ() * grid.chunkCountX;

        structureManagers[index].aboveInfo(cell);
    }

    /// <summary>
    /// plays a little animation for a visual feedback
    /// </summary>
    /// <param name="coords"></param>
    /// <param name="success"></param>
    public void harvestRessourceCallback(HexCoordinates coords, bool success, RessourceType ressource, int quantity)
    {
        //Get Cell
        HexCell cell = GameLogic.grid.GetCell(coords);
        //Get relevant Chunk
        int index = coords.ToChunkX() + coords.ToChunkZ() * grid.chunkCountX;
        //sprite 
        int res = UIManager.ressourceIndizes[ressource];
        Sprite sprt = FindObjectOfType<UIManager>().ressourceImages[res];
        //Show Feedback
        structureManagers[index].animateRessource(cell, success, sprt, quantity);

    }
    /// <summary>
    /// plays a little animation for a visual feedback
    /// </summary>
    /// <param name="coords"></param>
    /// <param name="success"></param>
    public void UpgradeBuildingCallback(HexCoordinates coords, bool success)
    {
        //Get Cell
        HexCell cell = GameLogic.grid.GetCell(coords);
        //Get relevant Chunk
        int index = coords.ToChunkX() + coords.ToChunkZ() * grid.chunkCountX;
        //Show Feedback
        structureManagers[index].animateBuilding(cell, success);
    }
    public void DeclareBattle(HexCoordinates coords)
    {
        HexCell cell = GameLogic.grid.GetCell(coords);
        //Get relevant Chunk
        int index = coords.ToChunkX() + coords.ToChunkZ() * grid.chunkCountX;
        //Show Feedback
        structureManagers[index].animateBattleLogo(cell);
    }

    public void UpdateFireColor(HexCell cell, Color color)
    {
        HexCoordinates coords = cell.coordinates;
        int index = coords.ToChunkX() + coords.ToChunkZ() * grid.chunkCountX;
        structureManagers[index].UpdateFireColors(cell, color);
    } 
    
    public void IncreasePlayerFov(Player player)
    {
        if (players.ContainsKey(player))
        {
            GameObject playerGo = players[player].Item1;
            Transform cloudTransform = playerGo.transform.Find("Clouds");
            cloudTransform.localScale = new Vector3(60f, 40f, 60f);   // Store constant somewhere more appropriate
        }
    }

    public void ResetPlayerFov(Player player)
    {
        if (players.ContainsKey(player))
        {
            GameObject playerGo = players[player].Item1;
            Transform cloudTransform = playerGo.transform.Find("Clouds");
            cloudTransform.localScale = new Vector3(30f, 20f, 30f);   // Store constant somewhere more appropriate
        }
    }
}
