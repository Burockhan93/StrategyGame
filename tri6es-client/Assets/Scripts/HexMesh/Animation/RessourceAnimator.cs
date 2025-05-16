using UnityEngine;
using Shared.Structures;

public class RessourceAnimator : StructureAnimator<Ressource> 
{
    private void Start()
    {
        if (structure is Fish)
        {
            GetComponent<Animator>().SetFloat("Offset", Random.Range(0.0f, 1.0f));
        }
    }
    private void Update()
    {
        // transform.localScale = ((float)(structure.Progress + 5) / (float)(structure.MaxProgress + 5)) * Vector3.one;
    }

    public override void Refresh()
    {
        transform.localScale = ((float)(structure.Progress + 5) / (float)(structure.MaxProgress + 5)) * Vector3.one;
    }
}
