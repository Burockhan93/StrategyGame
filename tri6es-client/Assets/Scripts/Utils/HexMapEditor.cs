using UnityEngine;
using Shared.HexGrid;
using Shared.Structures;
using System;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    public HexGrid hexGrid;

    private HexMeshGrid meshGrid;

    private Type[] buildingTypes = {
        typeof(Woodcutter),
        typeof(Storage),
        typeof(Road),
        typeof(Headquarter),
        typeof(Quarry),
    };
    private Int32 selectedBuildingType = 0;

    private void Awake()
    {
        meshGrid = FindObjectOfType<HexMeshGrid>();

    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        if (Input.GetMouseButtonDown(0))
        {
            // HandleInput();
        }

        if (Input.GetMouseButtonDown(1))
        {
            // HandleInput1();
        }
    }


    void HandleInput()
    {
        if(hexGrid == null)
        {
            return;
        }
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(inputRay, out hit))
        {
            HexCell cell = hexGrid.GetCell(hit.point);
            ClientSend.RequestPlaceBuilding(cell.coordinates, buildingTypes[(int)selectedBuildingType]);
        }
    }

    void HandleInput1()
    {
        if (hexGrid == null)
        {
            return;
        }
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            HexCell cell = hexGrid.GetCell(hit.point);
            //ClientSend.UpgradeBuilding(cell.coordinates);
        }
    }
}
