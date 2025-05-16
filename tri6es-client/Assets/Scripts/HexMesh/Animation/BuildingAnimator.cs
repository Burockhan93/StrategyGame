using UnityEngine;
using Shared.Structures;

public class BuildingAnimator : StructureAnimator<Building>
{
    public MeshRenderer TeamColor;

    private void Start()
    {
        if (TeamColor == null)
        {
            return;
        }
        
        TeamColor.material.SetColor("_BaseColor", MeshMetrics.TribeToColor(structure.Tribe));
    }

    public override void Refresh()
    {
        //Do something with depending on health
    }
}
