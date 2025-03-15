using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkManager : MonoBehaviour
{
	public static ChunkManager Instance { get; private set; }

	public const int CHUNK_SIZE = 16;

	[Header("Generation")]
	[SerializeField] int size = 512;
	[SerializeField] float scale = 15f;
	[SerializeField] int seed = 0;

	[Header("Properties")]
	[SerializeField, Range(0, 20)] int gridRadius = 4;
	[SerializeField] int chunkLifetime = 50;
	[SerializeField, Tooltip("Maximum number of chunk loading in the same frame")] int maxChunkLoading = 2;
	[SerializeField, Tooltip("Maximum number of chunk unloading in the same frame")] int maxChunkUnloading = 2;


	[Header("Technical")]
	[SerializeField] Grid chunkGrid;
	[SerializeField] GameObject chunkPrefab;

	private readonly Dictionary<Vector2Int, ChunkObject> _loadedChunks = new();
	private readonly Queue<Vector2Int> _chunksToLoad = new();
	private readonly Queue<Vector2Int> _chunksToUnload = new();

	bool _debugMode;

	public Vector2Int PlayerChunkPosition => Utilities.PosToChunk(Player.Instance.transform.position);


	public int Size => size;
	public float Scale => scale;
	public int Seed => seed;
	public bool DebugMode => _debugMode;

	public GroundTile groundTile;
	public GroundTile pathTile0;
	public GroundTile pathTile1;
	public GroundTile pathTile2;
	public GroundTile pathTile3;
	public GroundTile pathTile4;
	public GroundTile voidTile;


	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
		Utilities.CHUNK_SIZE = CHUNK_SIZE;
	}

	private void Start()
	{
		groundTile = Resources.Load<GroundTile>("Tiles/Ground_00");
		pathTile0 = Resources.Load<GroundTile>("Tiles/Path_00");
		pathTile1 = Resources.Load<GroundTile>("Tiles/Path_01");
		pathTile2 = Resources.Load<GroundTile>("Tiles/Path_02");
		pathTile3 = Resources.Load<GroundTile>("Tiles/Path_03");
		pathTile4 = Resources.Load<GroundTile>("Tiles/Path_04");
		voidTile = Resources.Load<GroundTile>("Tiles/Void");
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F3))
		{
			_debugMode = !_debugMode;
			foreach (var chunk in _loadedChunks)
			{
				chunk.Value.SetDebugMode(_debugMode);
			}
		}
	}

	private void FixedUpdate()
	{
		// Reset existing chunks lifetime and enqueue new ones to load if needed
		for (int x = -gridRadius + PlayerChunkPosition.x; x <= gridRadius + PlayerChunkPosition.x; x++)
		{
			for (int y = -gridRadius + PlayerChunkPosition.y; y <= gridRadius + PlayerChunkPosition.y; y++)
			{
				Vector2Int chunkPos = new(x, y);
				if (_loadedChunks.ContainsKey(chunkPos))
				{
					// Reset lifetime
					_loadedChunks[chunkPos].lifetime = chunkLifetime;
				}
				else if (!_chunksToLoad.Contains(chunkPos))
				{
					// Add new chunk to load queue
					_chunksToLoad.Enqueue(chunkPos);
				}
			}
		}

		// Decrease lifetime and mark chunks for unloading
		foreach (var chunk in _loadedChunks)
		{
			// If chunk is within active range, reset its lifetime
			if (IsChunkInRange(chunk.Key, PlayerChunkPosition))
			{
				chunk.Value.lifetime = chunkLifetime;
			}
			else
			{
				chunk.Value.lifetime--;
				if (chunk.Value.lifetime <= 0 && !_chunksToUnload.Contains(chunk.Key))
				{
					_chunksToUnload.Enqueue(chunk.Key);
				}
			}
		}

		// Load "maxChunkLoading" chunks on that frame if some are in the load queue
		for (int i = 0; i < maxChunkLoading && _chunksToLoad.Count > 0; i++)
			LoadChunk(_chunksToLoad.Dequeue());

		// Unload "maxChunkUnloading" chunks on that frame if some are in the unload queue
		for (int i = 0; i < maxChunkUnloading && _chunksToUnload.Count > 0; i++)
		{
			Vector2Int chunkCoord = _chunksToUnload.Dequeue();
			if (_loadedChunks.TryGetValue(chunkCoord, out ChunkObject chunk))
			{
				chunk.UnloadChunk();
				_loadedChunks.Remove(chunk.Position);
			}
		}
	}

	private bool IsChunkInRange(Vector2Int chunkPos, Vector2Int playerPos)
	{
		return Mathf.Abs(chunkPos.x - playerPos.x) <= gridRadius && Mathf.Abs(chunkPos.y - playerPos.y) <= gridRadius;
	}

	public ChunkObject GetChunk(Vector2Int chunkPos)
	{
		if (_loadedChunks.ContainsKey(chunkPos))
			return _loadedChunks[chunkPos];
		return null;
	}

	private void LoadChunk(Vector2Int chunkPos)
	{
		if (!_loadedChunks.ContainsKey(chunkPos))
		{
			GameObject obj = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, chunkGrid.transform);
			ChunkObject chunk = obj.GetComponent<ChunkObject>();
			chunk.LoadChunk(chunkPos);
			chunk.lifetime = chunkLifetime;
			_loadedChunks.Add(chunkPos, chunk);
		}
	}
}
