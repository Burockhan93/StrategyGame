using UnityEngine;
using UnityEngine.EventSystems;
using Shared.HexGrid;



public class DragController : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    public RectTransform currentTransform;
    private GameObject mainContent;
    private Vector3 currentPossition;
    private int startIndex;

    public bool playerInventory;
    public HexCoordinates coordinates;

    private int totalChild;

    public void OnPointerDown(PointerEventData eventData)
    {
        currentPossition = currentTransform.position;
        mainContent = currentTransform.parent.gameObject;
        totalChild = mainContent.transform.childCount;
        startIndex = currentTransform.GetSiblingIndex();
    }

    public void OnDrag(PointerEventData eventData)
    {
        currentTransform.position =
            new Vector3(currentTransform.position.x, eventData.position.y, currentTransform.position.z);

        for (int i = 1; i < totalChild; i++)
        {
            if (i != currentTransform.GetSiblingIndex())
            {
                Transform otherTransform = mainContent.transform.GetChild(i);
                int distance = (int)Vector3.Distance(currentTransform.position,
                    otherTransform.position);
                if (distance <= 10)
                {
                    Vector3 otherTransformOldPosition = otherTransform.position;
                    otherTransform.position = new Vector3(otherTransform.position.x, currentPossition.y,
                        otherTransform.position.z);
                    currentTransform.position = new Vector3(currentTransform.position.x, otherTransformOldPosition.y,
                        currentTransform.position.z);
                    currentTransform.SetSiblingIndex(otherTransform.GetSiblingIndex());
                    currentPossition = currentTransform.position;
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentTransform.GetSiblingIndex() != startIndex)
        {
            Debug.Log("This needs to go to the server");
            ClientSend.RequestChangeOrderOfStrategy(coordinates, startIndex - 1, currentTransform.GetSiblingIndex() - 1, playerInventory);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        currentTransform.position = currentPossition;
    }
}
