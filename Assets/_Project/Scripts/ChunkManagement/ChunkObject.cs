using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkObject : MonoBehaviour
{
	[Header("Technical")]
	[SerializeField] Tilemap chunkTilemap;
	[SerializeField] TilemapCollider2D chunkTilemapCollider;
	[SerializeField] GameObject sprite;

	// Private
	Vector2Int _position;
	Vector3Int _worldPosition;

	// Public
	public int lifetime;

	public Tilemap ChunkTilemap => chunkTilemap;
	public Vector2Int Position => _position;
	public Vector3Int WorldPosition => _worldPosition;

	readonly int[] tilesValues = new int[ChunkManager.CHUNK_SIZE * ChunkManager.CHUNK_SIZE];
	readonly GroundTile[] tiles = new GroundTile[ChunkManager.CHUNK_SIZE * ChunkManager.CHUNK_SIZE];
	readonly Vector3Int[] tilesPositions = new Vector3Int[ChunkManager.CHUNK_SIZE * ChunkManager.CHUNK_SIZE];
	public readonly bool[] tilesWalkable = new bool[ChunkManager.CHUNK_SIZE * ChunkManager.CHUNK_SIZE];

	private void FixedUpdate()
	{
		ManageCollider();
	}

	void ManageCollider()
	{
		chunkTilemapCollider.enabled =
			Mathf.Abs(Position.x - ChunkManager.Instance.PlayerChunkPosition.x) <= 2 &&
			Mathf.Abs(Position.y - ChunkManager.Instance.PlayerChunkPosition.y) <= 1;
	}

	public void SetDebugMode(bool debugMode)
	{
		if (debugMode)
		{
			sprite.transform.position = _worldPosition + new Vector3Int(ChunkManager.CHUNK_SIZE / 2, ChunkManager.CHUNK_SIZE / 2);
			sprite.transform.localScale = new(ChunkManager.CHUNK_SIZE, ChunkManager.CHUNK_SIZE, ChunkManager.CHUNK_SIZE);
		}

		sprite.SetActive(debugMode);
	}

	public void LoadChunk(Vector2Int position)
	{
		int chunkSize = ChunkManager.CHUNK_SIZE;
		int size = ChunkManager.Instance.Size;
		float scale = ChunkManager.Instance.Scale;
		int seed = ChunkManager.Instance.Seed;

		GroundTile groundTile = ChunkManager.Instance.groundTile;
		GroundTile voidTile = ChunkManager.Instance.voidTile;

		GroundTile[] pathTiles = new GroundTile[4];
		pathTiles[0] = ChunkManager.Instance.pathTile0;
		pathTiles[1] = ChunkManager.Instance.pathTile1;
		pathTiles[2] = ChunkManager.Instance.pathTile2;
		pathTiles[3] = ChunkManager.Instance.pathTile3;


		_position = position;
		_worldPosition.x = position.x * chunkSize;
		_worldPosition.y = position.y * chunkSize;

		SetDebugMode(ChunkManager.Instance.DebugMode);

		for (int x = 0; x < chunkSize; x++)
		{
			for (int y = 0; y < chunkSize; y++)
			{
				float t = Mathf.PerlinNoise(
					(float)(_worldPosition.x + x) / size * scale + seed,
					(float)(_worldPosition.y + y) / size * scale + seed);

				// Seed the random generator based on position
				Random.InitState(((_worldPosition.x + x) << 16) ^ (_worldPosition.y + y));

				// Default to void tile
				tilesValues[y * chunkSize + x] = 0;
				tiles[y * chunkSize + x] = voidTile;
					
				if (t > 0.4f && t <= 0.65f)
				{
					tilesValues[y * chunkSize + x] = 2;
					tiles[y * chunkSize + x] = Random.value < 0.8f ? groundTile : pathTiles[Random.Range(0, 4)];
				}
				if (t > 0.48f && t <= 0.56f)
				{
					tilesValues[y * chunkSize + x] = 1;
					tiles[y * chunkSize + x] = groundTile;
				}

				tilesPositions[y * chunkSize + x] = new Vector3Int(_worldPosition.x + x, _worldPosition.y + y);
				tilesWalkable[y * chunkSize + x] = tiles[y * chunkSize + x].walkable;
			}
		}

		chunkTilemap.SetTiles(tilesPositions, tiles);
	}

	public void UnloadChunk()
	{
		Destroy(gameObject);
	}
}
