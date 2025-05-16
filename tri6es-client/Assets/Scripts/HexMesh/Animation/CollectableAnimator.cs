using UnityEngine;
using Shared.Structures;

public class CollectableAnimator : StructureAnimator<Collectable>
{
    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = 7 * Vector3.one;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void Refresh() 
    {
        transform.localScale = 7 * structure.da* Vector3.one;
    }
}
