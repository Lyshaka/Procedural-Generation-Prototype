using UnityEngine;

public static class Utilities
{
	public static int CHUNK_SIZE;

	public static Vector3Int ChunkToWorld(Vector2Int chunkPos)
	{
		return new(chunkPos.x * CHUNK_SIZE, chunkPos.y * CHUNK_SIZE);
	}

	public static Vector2Int FloorPosition(Vector3 position)
	{
		return new(
			Mathf.FloorToInt(position.x),
			Mathf.FloorToInt(position.y));
	}

	public static Vector2Int PosToChunk(Vector2 position)
	{
		return new(
			Mathf.FloorToInt(position.x / CHUNK_SIZE),
			Mathf.FloorToInt(position.y / CHUNK_SIZE));
	}

	public static Vector3 PosToWorld(Vector2Int position)
	{
		return new(position.x + 0.5f, position.y + 0.5f);
	}
}
