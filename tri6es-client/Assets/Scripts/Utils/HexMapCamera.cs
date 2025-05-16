using UnityEngine;
using Shared.HexGrid;

public class HexMapCamera : MonoBehaviour
{
	public float smoothness;

	public float zoom = 1f;

	public Vector3 offset;

	public HexCell focusedCell;

	public HexGrid hexGrid;

	public bool panning;

	private void Awake()
	{
		this.enabled = false;
	}

	private void Update()
	{
		if(!panning)
			UpdateCamera();
	}

	public void SetGrid(HexGrid hexGrid)
	{
		this.hexGrid = hexGrid;
		this.enabled = true;

		focusedCell = hexGrid.GetCell(0, 0);
	}

	private void UpdateCamera()
	{
		Vector3 position = focusedCell.Position + new Vector3(0, focusedCell.Elevation * MeshMetrics.elevationStep);
		position += zoom * offset;
		Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, position, Time.deltaTime/(float)smoothness);
	}

	public void MoveCamera(HexCell cell)
	{
		focusedCell = cell;
	}

	public void HandleMove()
	{
		if (hexGrid == null)
		{
			return;
		}
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit))
		{
			HexCell cell = hexGrid.GetCell(hit.point);
			if(cell != null)
			{
				focusedCell = cell;
				ClientSend.UpdatePosition(cell.coordinates);
			}
		}
	}
}
