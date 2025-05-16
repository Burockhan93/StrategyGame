using UnityEngine;
using Shared.DataTypes;
using Shared.Structures;
using TMPro;

public class ArmySlot : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject CurrentSupply;
    public TroopType type;
    private Headquarter hq;
    void Start()
    {
        hq = Client.instance.Player.Tribe.HQ;
    }

    // Update is called once per frame
    void Update()
    {
        CurrentSupply.GetComponent<TMP_Text>().text = hq.TroopInventory.Troops[type].ToString() + "/" + hq.TroopInventory.TroopLimit.ToString();
    }
}
