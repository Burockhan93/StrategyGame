using System.Collections.Generic;
using UnityEngine;
using Shared.HexGrid;
using Shared.Structures;
using UnityEngine.UI;
using TMPro;

public class StructureManager : MonoBehaviour
{
    public HexGridChunk chunk;

    private EmptyAnimator[] structures;

    public PrefabManager prefabManager;

    private void Awake()
    {
        structures = new EmptyAnimator[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
        prefabManager = FindObjectOfType<PrefabManager>();
    }


    public void SetChunk(HexGridChunk chunk)
    {
        this.chunk = chunk;
        Init();
    }

    private void Init()
    {
        UpdateStructures();
    }

    public void Update() 
    {
       UpdateUnloadedStructures();
    }

    public void UpdateUnloadedStructures()
    {
        foreach (HexCell cell in chunk.cells)
        {
            if (cell.Structure != null)
            {
                if (cell.notloaded)
                {
                    Random.InitState(cell.coordinates.X * cell.coordinates.Z);
                    int index = toChunkIndex(cell.coordinates);
                    Vector3 position = cell.Position + MeshMetrics.elevationStep * new Vector3(0, cell.Data.Elevation);
                    GameObject structurePrefab = prefabManager.GetPrefab(cell.Structure);
                    if (structurePrefab != null)
                    {
                        if (cell.Structure is Ressource)
                        {
                            GameObject g = Instantiate(structurePrefab, position, Quaternion.Euler(0, 60 * Random.Range(0, 6), 0), this.transform);
                            structures[index] = g.GetComponent<RessourceAnimator>();
                            structures[index].GetComponent<RessourceAnimator>().structure = (Ressource)cell.Structure;

                        }
                        else if (cell.Structure is Collectable)
                        {
                            GameObject g = Instantiate(structurePrefab, position, structurePrefab.transform.localRotation, this.transform);
                            structures[index] = g.GetComponent<CollectableAnimator>();
                            structures[index].GetComponent<CollectableAnimator>().structure = (Collectable)cell.Structure;
                        }
                        cell.notloaded = false;
                    }
                }
            }
            else 
            {
                cell.notloaded = true;
            }
        }
    }

    public void UpdateStructures()
    {
        foreach (HexCell cell in chunk.cells)
        {
            UpdateStructures(cell);
        }
    }

   
    public void UpdateStructures(HexCell cell)
    {
        Random.InitState(cell.coordinates.X * cell.coordinates.Z);
        int index = toChunkIndex(cell.coordinates);
        if(structures[index] != null)
        {
            PlayBuildingPlacedParticles(cell);
            Destroy(structures[index].gameObject,0.9f);
        }

        Vector3 position = cell.Position + MeshMetrics.elevationStep * new Vector3(0, cell.Data.Elevation);

        GameObject structurePrefab = prefabManager.GetPrefab(cell.Structure);

        if (structurePrefab != null)
        {
            if (cell.Structure is Ressource)
            {
                GameObject g = Instantiate(structurePrefab, position, Quaternion.Euler(0, 60 * Random.Range(0, 6), 0), this.transform);
                structures[index] = g.GetComponent<RessourceAnimator>();
                structures[index].GetComponent<RessourceAnimator>().structure = (Ressource)cell.Structure;

            }
            else if (cell.Structure is Collectable)
            {
                GameObject g = Instantiate(structurePrefab, position, structurePrefab.transform.localRotation, this.transform);
                structures[index] = g.GetComponent<CollectableAnimator>();
                structures[index].GetComponent<CollectableAnimator>().structure = (Collectable)cell.Structure;
            }
            else if (cell.Structure is Building)
            {
                GameObject g;
                if (cell.Structure is Fisher)
                {
                    HexDirection dir = HexDirection.NE;
                    for (dir = HexDirection.NE; dir <= HexDirection.NW; dir++)
                    {
                        HexCell neighbor = cell.GetNeighbor(dir);
                        if (neighbor != null && cell.GetNeighbor(dir).Structure is Fish)
                            break;
                    }
                    g = Instantiate(structurePrefab, position, Quaternion.Euler(0, 60 * ((int)dir - 1), 0), this.transform);
                }
                else
                {
                    g = Instantiate(structurePrefab, position, structurePrefab.transform.localRotation, this.transform);
                }

                if (cell.Structure is Road)
                {
                    structures[index] = g.GetComponent<RoadAnimator>();
                    structures[index].GetComponent<RoadAnimator>().structure = (Road)cell.Structure;
                }
                else
                {
                    structures[index] = g.GetComponent<BuildingAnimator>();
                    structures[index].GetComponent<BuildingAnimator>().structure = (Building)cell.Structure;
                }
            }
            else
            {
                GameObject g = Instantiate(structurePrefab, position, Quaternion.identity, this.transform);
                structures[index] = g.GetComponent<EmptyAnimator>();
            }
            cell.notloaded = false;
        }
        else 
        {
            cell.notloaded = true;
        }
    }

    /// <summary>
    /// Sets the 'startColor' attribute of the 'FX_Fire_HQ' ParticleSystem component located at the given HexCell.
    /// </summary>
    public void UpdateFireColors(HexCell cell, Color color)
    {
        int index = toChunkIndex(cell.coordinates);
        if (structures.Length <= index)
        {
            // To be safe...
            Debug.Log("Fire color: returning because index out of bounds.");
            return;
        }
        
        GameObject building = structures[index].gameObject;
        if (building != null && cell.Structure is Headquarter)
        {
            Transform fireFxTransform = building.transform.Find("FX_Fire_HQ");
            if (fireFxTransform == null)
            {
                Debug.Log("Fire color: returning because no child transform with the name 'FX_Fire_HQ' found.");
                return;
            } 
            
            ParticleSystem fireFx = fireFxTransform.GetComponent<ParticleSystem>();
            if (fireFx != null)
            {
                ParticleSystem.MainModule main = fireFx.main;
                main.startColor = new ParticleSystem.MinMaxGradient(color);
            }
        }
    }

    public void BuildBuilding(HexCell cell)
    {
        PlayBuildingPlacedParticles(cell);
        UpdateStructures(cell);
        Refresh();
    }

    private void PlayBuildingPlacedParticles(HexCell cell)
    {
        Vector3 position = cell.Position + MeshMetrics.elevationStep * new Vector3(0, cell.Data.Elevation);
        GameObject particlesGO = Instantiate(prefabManager.BuildingParticles, position, Quaternion.Euler(-90, 0, 0), transform);
        ParticleSystem particles = particlesGO.GetComponent<ParticleSystem>();
        if (particles != null)
        {
            // Call Play() manually rather than using "Play On Awake" to ensure that the particle system is not 
            // replayed when the HexChunk it's located on is re-activated in HexMeshGrid::UpdateActiveChunks.
            // (The update of chunks happens for example when the player position has changed.)
            particles.Play();
        }
    }

    public GameObject SetPlayerPosition(HexCoordinates coords, GameObject prefab)
    {
        int index = toChunkIndex(coords);
        // Anna Workaround 
        GameObject position = null;
        if (structures[index] != null)
        { 
            position = structures[index].GetNextPlayerPos();
            if (position != null)
            {
                prefab.transform.position = position.transform.position;
            }
        }
        return position;
    }


    public void Refresh()
    {
        foreach(EmptyAnimator animator in structures)
        {
            // Anna workaround
            if (animator != null)
                animator.Refresh();
            else
                Debug.Log(structures);
        }
    }

    internal int toChunkIndex(HexCoordinates coordinate)
    {
        return (coordinate.ToOffsetX() % HexMetrics.chunkSizeX) + (coordinate.ToOffsetZ() % HexMetrics.chunkSizeZ) * HexMetrics.chunkSizeX;
    }
    #region FeedBack Animations
    public void aboveInfo(List<HexCell> hexCells)
    {
        foreach (HexCell cell in hexCells)
        {            
            if(cell.Structure is Building && !(cell.Structure is Road))
            {
                Vector3 position = cell.Position + MeshMetrics.elevationStep * new Vector3(0, cell.Data.Elevation) + 10*Vector3.up;
                GameObject Image = Instantiate(prefabManager.SpriteOnScene, position, Quaternion.Euler(-30, 180, 0), transform);
                
                Image.transform.GetChild(0).gameObject.SetActive(false);
                Image.GetComponentInChildren<TMP_Text>().text = $"{cell.Structure.GetType().Name}";
                //Image.GetComponentInChildren<TMP_Text>().color = new Color32(0, 0, 0, 255);
                //Image.GetComponentInChildren<TMP_Text>().faceColor = new Color32(0, 0, 0, 255);
                //Image.GetComponentInChildren<TMP_Text>().outlineWidth = 0.4f;
                //Image.GetComponentInChildren<TMP_Text>().outlineColor= new Color32(255, 0, 0, 255);
                Destroy(Image, 1.1f);
            }

            //int index = toChunkIndex(cell.coordinates);
            //if (structures[index].gameObject.GetComponentInChildren<Player>() != null)
            //{
            //    Debug.Log($"{structures[index].gameObject.GetComponentInChildren<Player>().Name}");
            //}

        }
        
    }
   
    public void animateRessource(HexCell cell, bool success, Sprite spr=null, int quantity=1)
    {
        int index = toChunkIndex(cell.coordinates);
        var seq = LeanTween.sequence();
        if (success)
        {
            seq.append(LeanTween.scale(structures[index].gameObject, structures[index].gameObject.transform.localScale * 2, 0.15f));
            seq.append(LeanTween.scale(structures[index].gameObject, new Vector3(0, 0, 0), 0.15f));

            if (spr)
            {
                Vector3 position = cell.Position + MeshMetrics.elevationStep * new Vector3(0, cell.Data.Elevation);
                GameObject Image = Instantiate(prefabManager.SpriteOnScene, position,Quaternion.Euler(-30,180,0),transform);
                
                Image.GetComponentInChildren<Image>().sprite = spr;
                Image.GetComponentInChildren<TMP_Text>().text = $"+{quantity} {spr.name}";
                var seq1 = LeanTween.sequence();
                Debug.Log(Image.GetComponent<RectTransform>().rect.y);
                seq1.append(LeanTween.moveY(Image.GetComponent<RectTransform>(), Image.GetComponent<RectTransform>().transform.localPosition.y+15, 1).setEase(LeanTweenType.easeOutBounce) );
                
                
                Destroy(Image, 1.1f);
            }
        }
        else
        {
            seq.append(LeanTween.moveLocal(structures[index].gameObject.transform.GetChild(0).gameObject, 2 * Vector3.right, 0.1f));
            seq.append(LeanTween.moveLocal(structures[index].gameObject.transform.GetChild(0).gameObject, 4 * Vector3.left, 0.1f));
            seq.append(LeanTween.moveLocal(structures[index].gameObject.transform.GetChild(0).gameObject, 4 * Vector3.right, 0.1f));
            seq.append(LeanTween.moveLocal(structures[index].gameObject.transform.GetChild(0).gameObject, 4 * Vector3.left, 0.1f));
            seq.append(LeanTween.moveLocal(structures[index].gameObject.transform.GetChild(0).gameObject, 4 * Vector3.right, 0.1f));
            seq.append(LeanTween.moveLocal(structures[index].gameObject.transform.GetChild(0).gameObject, 2 * Vector3.left, 0.1f));


        }

    }
    public void animateBuilding(HexCell cell, bool success)
    {
        int index = toChunkIndex(cell.coordinates);
        var seq = LeanTween.sequence();
        Vector3 scale = structures[index].gameObject.transform.localScale;
        //if successfull it will be replaced a by its upgraded version and particle effects will be played. thats why we dont need to add an another visual effect
        if (!success)
        {
            seq.append(LeanTween.scale(structures[index].gameObject, structures[index].gameObject.transform.localScale * 2, 0.15f));
            seq.append(LeanTween.scale(structures[index].gameObject, scale, 0.15f));
        }

    }
    public void animateBattleLogo(HexCell cell)
    {
        int index = toChunkIndex(cell.coordinates);

        Vector3 position = cell.Position + MeshMetrics.elevationStep * new Vector3(0, cell.Data.Elevation);
        GameObject Image = Instantiate(prefabManager.SpriteOnScene, position, Quaternion.Euler(-30, 180, 0), transform);
        Image.transform.localScale *= 1.5f;

        Sprite spr =  FindObjectOfType<UIManager>().BattleDeclare;

        Image.GetComponentInChildren<Image>().sprite = spr;
        //Image.GetComponentInChildren<TMP_Text>().text = $"You started a Tribe Battle";
        Image.GetComponentInChildren<TMP_Text>().text = "";
        var seq1 = LeanTween.sequence();
        
        seq1.append(LeanTween.moveY(Image.GetComponent<RectTransform>(), Image.GetComponent<RectTransform>().transform.localPosition.y + 15, 1).setEase(LeanTweenType.easeOutBounce));


        Destroy(Image, 1.5f);
    }
    #endregion
}
