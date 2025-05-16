using UnityEngine;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public TMP_InputField ipInput;
    public TMP_InputField nameInput;

    public void Connect()
    {
        string ip = ipInput.text.Substring(0, ipInput.text.IndexOf(':'));
        string port = ipInput.text.Substring(ipInput.text.IndexOf(':') + 1);

        string name = nameInput.text;

        if(ip != "" && port != "")
        {
            PlayerPrefs.SetString("IP", ip);
            PlayerPrefs.SetString("Port", port);

            PlayerPrefs.SetString("PlayerName", name);
        }
        else
        {
            Debug.Log("Pls enter the ip and port of the gameserver like: \"ip-adress : port\"");
        }

        LoadManager.instance.LoadGame();
        
    }

    public void Start()
    {
        if(PlayerPrefs.HasKey("IP") && PlayerPrefs.HasKey("PlayerName"))
        {
            ipInput.text = PlayerPrefs.GetString("IP") + ":" + PlayerPrefs.GetString("Port");
            nameInput.text = PlayerPrefs.GetString("PlayerName");
        }
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape pressed");
            Application.Quit();
        }

    }
}
