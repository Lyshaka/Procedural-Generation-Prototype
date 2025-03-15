using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public static class Pathfinding
{
	const int STRAIGHT_COST = 100;
	const int DIAGONAL_COST = 145;

	public static async Task<List<Vector2Int>> FindPathAsync(Vector2Int startPos, Vector2Int targetPos)
	{
		return await Task.Run(() => FindPath(startPos, targetPos));
	}

	public static List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int targetPos)
	{
#if UNITY_EDITOR
		Stopwatch swLoad = Stopwatch.StartNew();
#endif
		Vector2Int startChunk = ChunkManager.PosToChunk(startPos);
		Vector2Int targetChunk = ChunkManager.PosToChunk(targetPos);

		// Extend chunk range to 1 in each direction to cover a bigger area
		int minX = Mathf.Min(startChunk.x, targetChunk.x) - 1;
		int maxX = Mathf.Max(startChunk.x, targetChunk.x) + 1;
		int minY = Mathf.Min(startChunk.y, targetChunk.y) - 1;
		int maxY = Mathf.Max(startChunk.y, targetChunk.y) + 1;

		// Number of chunks that will be covered by the algorithm in each direction
		Vector2Int range = new((maxX - minX) + 1, (maxY - minY) + 1);

		// Botton left chunk of the bounding area
		Vector2Int offsetStartChunk = new(minX, minY);

		int gridWidth = range.x * ChunkManager.CHUNK_SIZE;
		int gridHeight = range.y * ChunkManager.CHUNK_SIZE;

		Node[,] walkableGrid = new Node[range.x * ChunkManager.CHUNK_SIZE, range.y * ChunkManager.CHUNK_SIZE];

		// Browsing each chunk (cx, cy)
		for (int cx = 0; cx < range.x; cx++)
		{
			for (int cy = 0; cy < range.y; cy++)
			{
				ChunkObject chunk = ChunkManager.Instance.GetChunk(offsetStartChunk + new Vector2Int(cx, cy));
				if (chunk != null)
				{
					// Browsing each tile (tx, ty) in the chunk (cx, cy)
					for (int tx = 0; tx < ChunkManager.CHUNK_SIZE; tx++)
					{
						for (int ty = 0; ty < ChunkManager.CHUNK_SIZE; ty++)
						{
							int destX = cx * ChunkManager.CHUNK_SIZE + tx;
							int destY = cy * ChunkManager.CHUNK_SIZE + ty;

							int srcIndex = ty * ChunkManager.CHUNK_SIZE + tx;
							walkableGrid[destX, destY] =
								new(chunk.tilesWalkable[srcIndex],
								new Vector2Int(destX, destY) + (Vector2Int)ChunkManager.ChunkToWorld(offsetStartChunk),
								new(destX, destY));
						}
					}
				}
				else
				{
#if UNITY_EDITOR
					swLoad.Stop();
					UnityEngine.Debug.LogError($"Chunk couldn't be loaded, elapsed time : {swLoad.Elapsed.TotalMilliseconds}ms");
#endif
					return null; // Chunk isn't loaded, abort pathfinding
				}
			}
		}

#if UNITY_EDITOR
		swLoad.Stop();
		Stopwatch swPathfind = Stopwatch.StartNew();
#endif

		//for (int x = 0; x < gridWidth; x++)
		//{
		//	for (int y = 0; y < gridHeight; y++)
		//	{
		//		UnityEngine.Debug.DrawLine(
		//			ChunkManager.ChunkToWorld(offsetStartChunk) + new Vector3(x, y, -5f),
		//			ChunkManager.ChunkToWorld(offsetStartChunk) + new Vector3(x, y, -5f) + Vector3.one,
		//			walkableGrid[x, y].Walkable ? Color.green : Color.red, 10f, false);
		//	}
		//}


		// Convert world position to local grid position
		Vector2Int startLocalPos = startPos - (Vector2Int)ChunkManager.ChunkToWorld(offsetStartChunk);
		Vector2Int targetLocalPos = targetPos - (Vector2Int)ChunkManager.ChunkToWorld(offsetStartChunk);

		// Start A* algorithm
		Node startNode = walkableGrid[startLocalPos.x, startLocalPos.y];
		Node targetNode = walkableGrid[targetLocalPos.x, targetLocalPos.y];

		Heap<Node> openSet = new(gridWidth * gridHeight);
		HashSet<Node> closedSet = new();

		openSet.Add(startNode);

		while (openSet.Count > 0)
		{
			Node currentNode = openSet.RemoveFirst();
;
			closedSet.Add(currentNode);

			if (currentNode == targetNode) // Found Path !
			{
				List<Vector2Int> path = new();

				while (currentNode != startNode)
				{
					currentNode = currentNode.parent;
					path.Add(currentNode.WorldPos);
				}
;
				path.Reverse();
#if UNITY_EDITOR
				swPathfind.Stop();
				UnityEngine.Debug.Log($"Path found in {swPathfind.Elapsed.TotalMilliseconds}ms, loaded chunks in {swLoad.Elapsed.TotalMilliseconds}ms");
#endif
				return path; 
			}

			foreach (Node neighbor in GetNeighbors(currentNode))
			{
				if (!neighbor.Walkable || closedSet.Contains(neighbor))
					continue;

				int costToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
				if (costToNeighbor < currentNode.gCost || !openSet.Contains(neighbor))
				{
					neighbor.gCost = costToNeighbor;
					neighbor.hCost = GetDistance(neighbor, targetNode);
					neighbor.parent = currentNode;

					if (!openSet.Contains(neighbor))
						openSet.Add(neighbor);
				}
			}
		}

#if UNITY_EDITOR
		swPathfind.Stop();
		UnityEngine.Debug.LogWarning($"Something went wrong in path calculation, elapsed time : {swPathfind.Elapsed.TotalMilliseconds}ms");
#endif
		return null; // Something went wrong :s

		// Utilities
		List<Node> GetNeighbors(Node node)
		{
			List<Node> neighbors = new();

			int x = node.GridPos.x;
			int y = node.GridPos.y;

			bool left = x > 0 && walkableGrid[x - 1, y].Walkable;
			bool right = x < gridWidth - 1 && walkableGrid[x + 1, y].Walkable;
			bool up = y < gridHeight - 1 && walkableGrid[x, y + 1].Walkable;
			bool down = y > 0 && walkableGrid[x, y - 1].Walkable;

			if (left) neighbors.Add(walkableGrid[x - 1, y]);
			if (right) neighbors.Add(walkableGrid[x + 1, y]);
			if (up) neighbors.Add(walkableGrid[x, y + 1]);
			if (down) neighbors.Add(walkableGrid[x, y - 1]);

			// Check diagonals only if both adjacent cardinal directions are walkable
			if (left && up) neighbors.Add(walkableGrid[x - 1, y + 1]);
			if (right && up) neighbors.Add(walkableGrid[x + 1, y + 1]);
			if (left && down) neighbors.Add(walkableGrid[x - 1, y - 1]);
			if (right && down) neighbors.Add(walkableGrid[x + 1, y - 1]);

			return neighbors;
		}


		int GetDistance(Node a, Node b)
		{
			int dstX = Mathf.Abs(a.GridPos.x - b.GridPos.x); 
			int dstY = Mathf.Abs(a.GridPos.y - b.GridPos.y);

			if (dstX > dstY)
				return (DIAGONAL_COST * dstY + STRAIGHT_COST * (dstX - dstY));
			return (DIAGONAL_COST * dstX + STRAIGHT_COST * (dstY - dstX));
		}
	}
}
