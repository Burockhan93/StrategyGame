using UnityEngine;
using Shared.Structures;
using Shared.HexGrid;
public class RoadAnimator : StructureAnimator<Road>
{
    public GameObject cartPrefab;

    private GameObject cart;

    private Vector3 startPos;

    private Vector3 endPos;

    private CartAnimator cartAnimator;

    private void Awake()
    {
        cart = Instantiate(cartPrefab, this.transform.position + new Vector3(0, MeshMetrics.roadHeight), Quaternion.identity, this.transform);
        cart.SetActive(false);
        this.cartAnimator = cart.GetComponent<CartAnimator>();
    }

    public override void Refresh()
    {
        //Do something with depending on number of carts
        if (structure.Carts.Count > 0)
        {
            

            Cart animatedCart = structure.Carts[0];
            HexDirection dir = structure.connectedStorages[animatedCart.Origin.Tribe][animatedCart.Destination].Item1;

            Color c = animatedCart.Inventory.IsEmpty() ? new Color(0, 0, 0, 0) : animatedCart.Inventory.GetMainRessource().ToColor();

            Structure neighbor = this.structure.Cell.GetNeighbor(dir).Structure;

            if(neighbor is Road)
                cartAnimator.SetValues(
                    this.structure.Cell.Position + new Vector3(0, this.structure.GetElevation() * MeshMetrics.elevationStep + MeshMetrics.roadHeight),
                    this.structure.Cell.GetNeighbor(dir).Position + new Vector3(0, ((Road)this.structure.Cell.GetNeighbor(dir).Structure).GetElevation() * MeshMetrics.elevationStep + MeshMetrics.roadHeight),
                    dir,
                    animatedCart.Origin.Tribe,
                    c
                );
            else if(neighbor is InventoryBuilding)
                cartAnimator.SetValues(
                    this.structure.Cell.Position + new Vector3(0, this.structure.GetElevation() * MeshMetrics.elevationStep + MeshMetrics.roadHeight),
                    this.structure.Cell.GetNeighbor(dir).Position + new Vector3(0, this.structure.Cell.GetNeighbor(dir).Elevation * MeshMetrics.elevationStep + MeshMetrics.roadHeight),
                    dir,
                    animatedCart.Origin.Tribe,
                    c
                );
            cart.SetActive(true);
        } 
        else 
        {
            cart.SetActive(false);
        }
    }
}
    
