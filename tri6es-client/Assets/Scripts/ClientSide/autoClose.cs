using System.Collections;
using UnityEngine;

public class autoClose : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private int timer = 5;
   void OnEnable(){
       StartCoroutine(closer());
   }
    private void OnDisable()
    {
        gameObject.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        gameObject.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
    }

    IEnumerator closer()
   {
       yield return new WaitForSeconds(timer);
        gameObject.SetActive(false);

   }
}