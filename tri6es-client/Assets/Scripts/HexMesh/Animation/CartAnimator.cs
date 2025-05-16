using UnityEngine;
using Shared.HexGrid;
using Shared.Communication;

public class CartAnimator : MonoBehaviour
{
    private Vector3 start;

    private Vector3 end;

    private Vector3 firstCorner;
    private Vector3 secondCorner;

    private float len1, len2, len3, totalLen;

    public float deltaTime = Constants.SEC_PER_GAMETICK;

    private float time;

    public MeshRenderer TeamColor;

    private void Update()
    {
        this.transform.position = this.getPosition(time);
        time += Time.deltaTime;
    }

    public void SetValues(Vector3 start, Vector3 end, HexDirection direction, byte tribe, Color ressourceColor)
    {
        this.start = start;
        this.end = end;
        this.time = 0;
        firstCorner = start + 0.5f * (MeshMetrics.GetFirstSolidRoadCorner(direction) + MeshMetrics.GetFirstSolidRoadCorner(direction.Next()));
        secondCorner = end + 0.5f * (MeshMetrics.GetFirstSolidRoadCorner(direction.Opposite()) + MeshMetrics.GetFirstSolidRoadCorner(direction.Opposite().Next()));

        len1 = Vector3.Distance(start, firstCorner);
        len2 = Vector3.Distance(firstCorner, secondCorner);
        len3 = Vector3.Distance(secondCorner, end);

        totalLen = len1 + len2 + len3;

        this.transform.rotation = Quaternion.LookRotation(end - start, Vector3.up);

        TeamColor.material.SetColor("_BaseColor", MeshMetrics.TribeToColor(tribe));

        transform.GetChild(0).GetComponent<MeshRenderer>().materials[2].SetColor("_BaseColor", ressourceColor);
    }

    private Vector3 getPosition(float time)
    {
        float progress = time / deltaTime;

        if (progress < len1 / totalLen)
        {
            return Vector3.Lerp(start, firstCorner, (progress) / (len1 / totalLen));
        }
        else if (progress < (len1 + len2) / totalLen)
        {
            return Vector3.Lerp(firstCorner, secondCorner, (progress - (len1 / totalLen)) / ((len2) / totalLen));
        }        
        else if (progress < 1)
        {
            return Vector3.Lerp(secondCorner, end, (progress - (len1 + len2) / totalLen) / ((len3 ) / totalLen));
        }
        else 
        {
            return end;
        }
    }            
}
