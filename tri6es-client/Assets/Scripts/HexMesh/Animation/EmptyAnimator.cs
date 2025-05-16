using UnityEngine;

public class EmptyAnimator : MonoBehaviour
{
    [SerializeField]
    private GameObject[] PlayerPositions;

    public GameObject GetNextPlayerPos()
    {
        foreach (GameObject gameObject in PlayerPositions)
        {
            // Note: if you model/create a new building, make sure that the PlayerPosition objects in the
            //       building prefab are deactivated
            //      (activated means that spot is taken -> all activated no spot left for the player to step on)
            if (gameObject.activeSelf == false)
            {
                gameObject.SetActive(true);
                return gameObject;
            }
        }
        return null;
    }

    public void FreePlayerPos(GameObject playerPos)
    {
        foreach (GameObject gameObject in PlayerPositions)
        {
            if (gameObject.Equals(playerPos))
            {
                gameObject.SetActive(false);
            }
        }
    }

    public virtual void Refresh()
    {

    }
}
