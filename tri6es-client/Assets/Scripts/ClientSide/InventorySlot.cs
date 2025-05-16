using UnityEngine;
using Shared.DataTypes;
using Shared.Structures;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public GameObject CurrentText;
    public RessourceType type;
    int current, limit;
    float income, expenditure, net;
    TribeInventory tribeInventory;

    // Start is called before the first frame update
    void Start()
    {
        tribeInventory = Client.instance.Player.Tribe.tribeInventory;
    }

    // Update is called once per frame
    void Update()
    {

        CurrentText.GetComponent<TMP_Text>().text = $"{tribeInventory.GetRessourceAmount(type)}/{tribeInventory.GetLimit(type)}";

    }
}
