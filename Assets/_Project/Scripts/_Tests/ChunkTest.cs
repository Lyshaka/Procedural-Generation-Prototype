using UnityEngine;
using TMPro;

public class ChunkTest : MonoBehaviour
{
	[Header("Current Coordinates")]
	[SerializeField] Vector2Int coord;

	[Header("Technical")]
	[SerializeField] TextMeshPro coordTMP;
	[SerializeField] TextMeshPro lifetimeTMP;

	public int lifetime;

	public Vector2Int Coord => coord;

	private void Update()
	{
		lifetimeTMP.text = $"{lifetime}";
	}

	public void SetCoord(int x, int y)
	{
		coord = new(x, y);
		coordTMP.text = $"({coord.x},{coord.y})";
	}

	public void Unload()
	{
		Destroy(gameObject, 0.1f);
	}
}
