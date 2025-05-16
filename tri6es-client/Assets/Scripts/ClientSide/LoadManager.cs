using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadManager : MonoBehaviour
{
    public static LoadManager instance;

    public GameObject LoadingScreen;
    public GameObject ProgressBar;
    public TMP_Text loadingText;
    public GameObject EventSystem;

    List<AsyncOperation> scenesLoading = new List<AsyncOperation>();
    float totalSceneProgress;
    float totalInitProgress;

    public enum SceneIndexes
    {
        MANAGER = 0,
        TITLE_SCREEN = 1,
        IN_GAME = 2
    }

    private void Awake()
    {
        //Singleton Stuff
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists ,destroying obj!");
            Destroy(this);
        }
        DontDestroyOnLoad(EventSystem);
        DontDestroyOnLoad(LoadingScreen);
        //Load Title Screen
        SceneManager.LoadSceneAsync((int)SceneIndexes.TITLE_SCREEN, LoadSceneMode.Additive);
    }

    public void LoadTitleScreen()
    {
        scenesLoading.Add(SceneManager.UnloadSceneAsync((int)SceneIndexes.IN_GAME));
        scenesLoading.Add(SceneManager.LoadSceneAsync((int)SceneIndexes.TITLE_SCREEN, LoadSceneMode.Additive));

        scenesLoading[1].allowSceneActivation = true;
    }

    /**
     * Loads the actual Game 
    */
    public void LoadGame()
    {
        LoadingScreen.SetActive(true);

        scenesLoading.Add(SceneManager.UnloadSceneAsync((int)SceneIndexes.TITLE_SCREEN));
        scenesLoading.Add(SceneManager.LoadSceneAsync((int)SceneIndexes.IN_GAME, LoadSceneMode.Additive));

        scenesLoading[1].allowSceneActivation = true;

        StartCoroutine(GetSceneLoadProgress());
        StartCoroutine(GetTotalLoadProgress());
    }
    public void ReturnToTitleScreen()
    {
        SceneManager.LoadScene(0);
    }

    public IEnumerator GetSceneLoadProgress()
    {
        for(int i = 0; i < scenesLoading.Count; i++)
        {
            while (!scenesLoading[i].isDone)
            {
                totalSceneProgress = 0;

                foreach(AsyncOperation op in scenesLoading)
                {
                    totalSceneProgress += op.progress;
                }

                totalSceneProgress = (totalSceneProgress / scenesLoading.Count) * 100f;

                Debug.Log("Scene: " + i + " , " + totalSceneProgress);

                loadingText.text = string.Format("Connecting: {0}%", totalSceneProgress);

                yield return null;
            }

        }

        totalSceneProgress = 100f;


    }

    public IEnumerator GetTotalLoadProgress()
    {
        float totalProgress = 0f;
        LoadingScreen.SetActive(true);
        while (!ClientHandle.isDone)
        {

            totalInitProgress = Mathf.RoundToInt(ClientHandle.loadingProgress * 100f);

            loadingText.text = string.Format("Loading Map: {0}%", totalProgress);


            totalProgress = Mathf.Round((totalSceneProgress + totalInitProgress) / 2) ;
            ProgressBar.GetComponent<Image>().fillAmount = Mathf.RoundToInt(totalProgress) / 100f;

            yield return null;
        }

        ProgressBar.GetComponent<Image>().fillAmount = 1;
        loadingText.text = string.Format("Loading Map: {0}%", 100);
        yield return new WaitForSecondsRealtime(1f);

        LoadingScreen.SetActive(false);
        scenesLoading.Clear();

    }
}
