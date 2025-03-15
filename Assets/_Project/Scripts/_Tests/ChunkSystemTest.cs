using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChunkSystemTest : MonoBehaviour
{
	public static ChunkSystemTest Instance {  get; private set; }

	[Header("Properties")]
	[SerializeField, Range(0, 20)] int gridRadius = 2;
	//[SerializeField] int chunkSize = 16;
	[SerializeField] int chunkLifetime = 50;
	
	[Header("Technical")]
	[SerializeField] GameObject chunkPrefab;

	[Header("Debug")]
	[SerializeField] ChunkTest[,] chunkGrid;

	public Vector2Int playerChunkPosition;

	// Private
	int _gridSize;
	Dictionary<Vector2Int, ChunkTest> _loadedChunks;
	List<Vector2Int> _chunksToUnload;

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	private void Start()
	{
		GenerateGrid();
	}

	private void FixedUpdate()
	{
		for (int x = -gridRadius + playerChunkPosition.x; x <= gridRadius + playerChunkPosition.x; x++)
		{
			for (int y = -gridRadius + playerChunkPosition.y; y <= gridRadius + playerChunkPosition.y; y++)
			{
				Vector2Int key = new(x, y);
				if (_loadedChunks.ContainsKey(key))
				{
					_loadedChunks[key].lifetime = chunkLifetime;
				}
				else
				{
					LoadChunk(key.x, key.y);
				}
			}
		}


		foreach (var chunk in _loadedChunks)
		{
			chunk.Value.lifetime--;
			if (chunk.Value.lifetime <= 0)
			{
				_chunksToUnload.Add(chunk.Key);
			}
		}

		foreach (var chunkCoord in _chunksToUnload)
		{
			_loadedChunks.Remove(chunkCoord, out ChunkTest chunk);
			chunk.Unload();
		}

		_chunksToUnload.Clear();
	}

	public void GenerateGrid()
	{
		_gridSize = gridRadius * 2 + 1;
		_loadedChunks = new Dictionary<Vector2Int, ChunkTest>();
		_chunksToUnload = new List<Vector2Int>();

		for (int i = -gridRadius; i <= gridRadius; i++)
		{
			for (int j = -gridRadius; j <= gridRadius; j++)
			{
				LoadChunk(i, j);
			}
		}
	}

	public void LoadChunk(int x, int y)
	{
		GameObject obj = Instantiate(chunkPrefab, new(x, y), Quaternion.identity, transform);

		ChunkTest chunk = obj.GetComponent<ChunkTest>();
		chunk.SetCoord(x, y);
		chunk.lifetime = chunkLifetime;

		_loadedChunks.Add(new(x, y), chunk);
	}
}
