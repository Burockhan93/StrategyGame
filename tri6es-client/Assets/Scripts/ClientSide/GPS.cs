using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using Shared.Game;
using Shared.HexGrid;

public class GPS : MonoBehaviour
{
    public void StartLocation()
    {
        StartCoroutine(GetLocation());
    }

    public IEnumerator GetLocation()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            Permission.RequestUserPermission(Permission.CoarseLocation);
        }
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
            yield return new WaitForSeconds(10);

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 10;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            yield break;
        }
        else
        {
            float lat = Input.location.lastData.latitude;
            float lon = Input.location.lastData.longitude;
            HexCell cell = GameLogic.grid.GetCell(lon, lat);
            if (cell != null)
            {
                ClientSend.UpdatePosition(cell.coordinates);
                HexMapCamera hexMapCamera = FindObjectOfType<HexMapCamera>();
                if(cell != null)
                    hexMapCamera.focusedCell = cell;
            }
            /*
            gpsOut.text = "Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + 100f + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp;
            // Access granted and location value could be retrieved
            print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
            */
        }

        // Stop service if there is no need to query location updates continuously
        Input.location.Stop();
    }
}
