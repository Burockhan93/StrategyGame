using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Shared.Game;

public class FeedBackManager : MonoBehaviour
{
    HexMeshGrid meshGrid;
    
    public TMP_Text FeedbackMessage;
    public TMP_Text battleLog_Left;
    public TMP_Text battleLog_Right;
    // Start is called before the first frame update
    void Start()
    {
        //FeedbackMessage = transform.Find("Message").GetComponent<TMP_Text>();
        //battleLog_Left = transform.Find("BattleLog-Left").GetComponent<TMP_Text>();
        //battleLog_Right = transform.Find("BattleLog-Right").GetComponent<TMP_Text>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void takeFeedback(Feedback feedback)
    {
        meshGrid = FindObjectOfType<HexMeshGrid>();
        FeedbackMessage.text = "";
        FeedbackMessage.enableAutoSizing = true;
        battleLog_Left.text = ""; ;
        battleLog_Right.text = "";


        switch (feedback.feedbackStyle)
        {
            case Feedback.FeedbackStyle.harvest:
                HarvestFeedback(feedback);
                break;
            case Feedback.FeedbackStyle.build:
                BuildFeedback(feedback);
                break;
            case Feedback.FeedbackStyle.upgrade:
                UpgradeFeedback(feedback);
                break;
            case Feedback.FeedbackStyle.destroy:
                DestroyFeedback(feedback);
                break;
            case Feedback.FeedbackStyle.ui:
                FeedbackMessage.text = feedback.message;
                break;
            case Feedback.FeedbackStyle.battle:
                StartCoroutine(BattleFeedback(feedback));
                if (feedback.successfull)
                {
                    //gameObject.SetActive(false);
                    foreach (KeyValuePair<Shared.DataTypes.TroopType, int> kvp in feedback.battleLog.attackerTroops)
                    {
                        Debug.Log("troop: " + kvp.Key + " sayi : " + kvp.Value);
                    }
                }
                break;
            case Feedback.FeedbackStyle.plainMessage:
                FeedbackMessage.text = feedback.message;
                break;
            default:
               
                break;
        }
    }
    void HarvestFeedback(Feedback feedback)
    {
        if (!feedback.successfull)
        {
            FeedbackMessage.text = $"Failed to collect {feedback.resource} ";   
        }
        else
        {
            //Substituting textual with an imagial feedback for now
            gameObject.SetActive(false);

            /*else if (feedback.quantity == 1)
            {
                FeedbackMessage.text = $"Succesfully collected a {feedback.resource} ";

            }
            else if (feedback.quantity >= 1)
            {
                FeedbackMessage.text = $"Succesfully collected {feedback.quantity} {feedback.resource}s ";
            }*/


        }
        meshGrid.harvestRessourceCallback(feedback.coordinates, feedback.successfull, feedback.resource, feedback.quantity);
        
 
    }
    void UpgradeFeedback(Feedback feedback)
    {
        if (!feedback.successfull)
        {
            FeedbackMessage.text = feedback.message + $"to up ugrade {feedback.type.Name}";
        }
        else
        {
            FeedbackMessage.text = $"Succesfully upgraded the {feedback.type.Name}";
        }

        meshGrid.UpgradeBuildingCallback(feedback.coordinates, feedback.successfull);

    }
    void BuildFeedback(Feedback feedback)
    {
        if (!feedback.successfull)
        {
            FeedbackMessage.text = feedback.message;
        }
        else
        {
            FeedbackMessage.text = $"Succesfully built the {feedback.type.Name}";
        }

        meshGrid.UpgradeBuildingCallback(feedback.coordinates, feedback.successfull);

    }
    void DestroyFeedback(Feedback feedback)
    {
        if (feedback.successfull)
        {
            FeedbackMessage.text = $"Succesfully destroyed the {feedback.type.Name}";
            if(feedback.quantity==0)
            {
                FeedbackMessage.text += $"No carriage is in the process destroyed";
            }else if (feedback.quantity == 1)
            {
                FeedbackMessage.text += $"\n 1 carriage is in the process destroyed";
            }
            else
            {
                FeedbackMessage.text += $"\n {feedback.quantity} carriages are in the process destroyed";
            }
            
        } 
        else 
        {
            FeedbackMessage.text = $"Couldn't destroy the {feedback.type.Name}";
        }
        
    }
    IEnumerator BattleFeedback(Feedback feedback)
    {
        if (feedback.successfull)
        {
            FeedbackMessage.text = "Waiting...";
            if (feedback.playername == GameLogic.GetPlayer(feedback.playername).Name)
            {
                yield return new WaitForSeconds(1.5f);
                gameObject.GetComponent<RectTransform>().offsetMin += new Vector2(0, -1750);
                gameObject.GetComponent<RectTransform>().offsetMax -= new Vector2(0, 450);

                string msg_our = "Our Forces:" + "\n";
                msg_our += string.Join("\n", feedback.battleLog.attackerTroops);
                battleLog_Left.text = msg_our;

                string msg_enemy = "Enemy Forces:" + "\n";
                msg_enemy += string.Join("\n", feedback.battleLog.defenderTroops);
                battleLog_Right.text = msg_enemy;

                FeedbackMessage.enableAutoSizing = false;
                FeedbackMessage.fontSize = 200;
                FeedbackMessage.ForceMeshUpdate();
                FeedbackMessage.enableAutoSizing = true;
                FeedbackMessage.text = $"{feedback.message}";
            }
            else if (GameLogic.GetPlayer(feedback.playername).Tribe == Client.instance.Player.Tribe)
            {
               FeedbackMessage.text = $"{feedback.playername} from our tribe attacked the {feedback.type.Name} of {feedback.TribeId}.Tribe!";
            }
            else if (feedback.TribeId == Client.instance.Player.Tribe.Id)
            {
                FeedbackMessage.text = $"{feedback.TribeId}.Tribe has attacked our {feedback.type.Name} at {feedback.coordinates}";
            }
        }
        else
        {
           FeedbackMessage.text = $"{feedback.message} ";
        }
        yield return null;
    }

    public void ressourceharvest() { }
    public  void resosourcefailedharvest() { }
}
