using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private RectTransform rect;

    public float openPos;
    public float closePos;

    public float threshold;

    public float speed;

    public float velocityThreshold;
    public float velocity;

    private bool dragging;

    public float maxPos;
    public float closeThreshold;

    private int openedPanel = -1;

    private void Start()
    {

        openPos = -10;
        closePos = -rect.rect.size.y;
        velocity = 0;
    }

    private void Update()
    {
        if (!dragging)
        {
            rect.anchoredPosition = new Vector2(0, Mathf.Clamp(rect.anchoredPosition.y - velocity * Time.deltaTime,  closePos, openPos));
        }
        if (rect.anchoredPosition.y == closePos)
            OnClosed();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
        openPos = -10;
        return;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition = new Vector2(0, Mathf.Clamp(rect.anchoredPosition.y + (eventData.delta.y / canvas.scaleFactor), closePos, openPos));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        velocity = -eventData.delta.y / (canvas.scaleFactor * Time.deltaTime);

        if(velocity > -velocityThreshold && velocity < velocityThreshold)
        {
            if(rect.anchoredPosition.y < -rect.rect.size.y * threshold)
            {
                Close();
            }
            else
            {
                Open();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        return;
    }

    public void Open()
    {
        openedPanel = -1;
        openPos = -10;
        velocity = -speed;
        Debug.Log(velocity);
    }

    public void Open(int id)
    {
        if(id == openedPanel)
        {
            Close();
        }
        else
        {
            openedPanel = id;
            openPos = -10;
            velocity = -speed;
            Debug.Log(velocity);
        }
    }

    public void OpenHalfWay(float stoppingPoint)
    {
        float newopenPos = closePos + stoppingPoint;
        openPos = Mathf.Min(-10f, newopenPos);
        velocity = -speed;
    }

    public void Close()
    {
        velocity = speed;
    }

    public void OnClosed()
    {
        openedPanel = -1;
    }
}
