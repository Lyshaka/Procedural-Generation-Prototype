using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapTests : MonoBehaviour
{
	[SerializeField] Tilemap tilemap;
	[SerializeField] TileBase groundTile;
	[SerializeField] TileBase voidTile;

	private void Start()
	{
		//groundTile

		for (int x = 0; x < 10; x++)
		{
			for (int y = 0; y < 10; y++)
			{
				tilemap.SetTile(new Vector3Int(x, y, 0), Random.value < 0.5f ? groundTile : voidTile);
			}
		}
	}
}
