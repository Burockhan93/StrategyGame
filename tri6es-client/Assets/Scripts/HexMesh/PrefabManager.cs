using System.Collections.Generic;
using UnityEngine;
using Shared.Structures;
using System;
using Tree = Shared.Structures.Tree;

public class PrefabManager : MonoBehaviour
{
    public GameObject[] Ressources;
    public static Dictionary<Type, int> RessourceIndizes = new Dictionary<Type, int>
    {
        { typeof(Tree), 0 },
        { typeof(Rock), 1 },
        { typeof(Scrub), 2 },
        { typeof(Grass), 3 },
        { typeof(Fish), 4 },
        { typeof(CoalOre), 6 },
        { typeof(Wheat), 7 },
        { typeof(Cow), 8 },
    }; 

    private Dictionary<Type, GameObject[]> BuildingPrefabs;

    public GameObject[] Woodcutters;

    public GameObject[] Storages;

    public GameObject[] HQs;

    public GameObject[] Quarries;

    public GameObject[] Mines;

    public GameObject[] Smelters;

    public GameObject[] Roads;

    public GameObject[] Markets;

    public GameObject[] Bakeries;

    public GameObject[] WheatFarms;

    public GameObject[] CowFarms;

    public GameObject[] Butchers;

    public GameObject[] Tanners;

    public GameObject[] Barracks;

    public GameObject[] Fishers;

    public GameObject[] Bridges;

    public GameObject[] Towers;

    public GameObject[] Smiths;

    public GameObject[] Workshops;

    public GameObject[] AssemblyPoints;

    public GameObject[] Libraries;

    public GameObject[] GuildHouses;

    public GameObject Ruin;

    public GameObject Empty;

    public GameObject BuildingParticles;

    public GameObject PlayerPrefab;

    public GameObject SpriteOnScene;

    public GameObject AvatarSelection;

    public GameObject TruhenPrefab;


    private void Awake()
    {
        // The array entries correspond to the building's level, eg. if HQ's max level is 3, its array should have 
        // 3 prefabs, one per level.
        // If you check the PrefabManager in Unity you may find that some/many buildings with a max level of 1
        // still have 3 prefabs. I'm not sure why that is the case, perhaps in case the max level changes in the future?
        BuildingPrefabs = new Dictionary<Type, GameObject[]>
        {
            { typeof(Woodcutter), Woodcutters },
            { typeof(Storage), Storages },
            { typeof(Headquarter), HQs },
            { typeof(Quarry), Quarries },
            { typeof(CoalMine), Mines },
            { typeof(Smelter), Smelters },
            { typeof(LandRoad), Roads },
            { typeof(Bridge), Bridges },
            { typeof(Market), Markets },
            { typeof(Bakery), Bakeries },
            { typeof(WheatFarm), WheatFarms },
            { typeof(CowFarm), CowFarms },
            { typeof(Butcher), Butchers },
            { typeof(Tanner), Tanners },
            { typeof(Barracks), Barracks },
            { typeof(Fisher), Fishers },
            { typeof(Tower), Towers },
            { typeof(WeaponSmith), Smiths },
            { typeof(ArmorSmith), Smiths },
            { typeof(Workshop), Workshops },
            { typeof(IronMine), Mines},
            { typeof(AssemblyPoint), AssemblyPoints },
            { typeof(Library), Libraries },
            { typeof(GuildHouse), GuildHouses },
        };
    }

    /// <summary>Returns the prefab for the given <see cref="Structure"/>.</summary>
    public GameObject GetPrefab(Structure structure)
    {
        if (structure == null)
            return Empty;

        Type type = structure.GetType();
        if (typeof(Ruin).IsAssignableFrom(type))
            return Ruin;
        if (typeof(Ressource).IsAssignableFrom(type))
            return GetRessourcePrefab(type);
        if (typeof(Collectable).IsAssignableFrom(type))
            return TruhenPrefab;
        else if (typeof(Building).IsAssignableFrom(type))
            return GetBuildingPrefab(type, ((Building)structure).Level);
        else
            return Empty;
    }

    /// <summary>Returns the prefab for the given <see cref="Ressource"/> type.</summary>
    public GameObject GetRessourcePrefab(Type type)
    {
        return Ressources[RessourceIndizes[type]];
    }


    /// <summary>Returns the prefab for the given <see cref="Building"/> type and <paramref name="level"/>.</summary>
    public GameObject GetBuildingPrefab(Type type, int level)
    {
        return BuildingPrefabs[type][level - 1];
    }

}
