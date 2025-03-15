using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;

public class Pathfinding : MonoBehaviour
{
	const int STRAIGHT_COST = 100;
	const int DIAGONAL_COST = 145;

	public void FindPath(PathRequest request, Action<PathResult> callback)
	{
		// Get the chunk the positions are in
		Vector2Int startChunk = Utilities.PosToChunk(request.pathStart);
		Vector2Int targetChunk = Utilities.PosToChunk(request.pathTarget);

		// Extend chunk range to 1 in each direction to cover a bigger area
		int minX = Mathf.Min(startChunk.x, targetChunk.x) - 1;
		int maxX = Mathf.Max(startChunk.x, targetChunk.x) + 1;
		int minY = Mathf.Min(startChunk.y, targetChunk.y) - 1;
		int maxY = Mathf.Max(startChunk.y, targetChunk.y) + 1;

		// Number of chunks that will be covered by the algorithm in each direction
		Vector2Int range = new((maxX - minX) + 1, (maxY - minY) + 1);

		// Botton left chunk of the bounding area
		Vector2Int offsetStartChunk = new(minX, minY);

		// Get chunk size and grid size based on chunk size
		int chunkSize = Utilities.CHUNK_SIZE;
		int gridWidth = range.x * chunkSize;
		int gridHeight = range.y * chunkSize;

		Node[,] walkableGrid = new Node[range.x * chunkSize, range.y * chunkSize];

		// Browsing each chunk (cx, cy)
		for (int cx = 0; cx < range.x; cx++)
		{
			for (int cy = 0; cy < range.y; cy++)
			{
				ChunkObject chunk = ChunkManager.Instance.GetChunk(offsetStartChunk + new Vector2Int(cx, cy));
				if (chunk == null)
				{
					callback(new PathResult(null, false, request.callback));
					return; // Chunk isn't loaded, abort pathfinding
				}

				// Browsing each tile (tx, ty) in the chunk (cx, cy)
				for (int tx = 0; tx < chunkSize; tx++)
				{
					for (int ty = 0; ty < chunkSize; ty++)
					{
						int destX = cx * chunkSize + tx;
						int destY = cy * chunkSize + ty;

						int srcIndex = ty * chunkSize + tx;
						walkableGrid[destX, destY] =
							new(chunk.tilesWalkable[srcIndex],
							new Vector2Int(destX, destY) + (Vector2Int)Utilities.ChunkToWorld(offsetStartChunk),
							new(destX, destY),
							chunk.tilesPenalty[srcIndex]);
					}
				}
			}
		}

		// Convert world position to local grid position
		Vector2Int startLocalPos = request.pathStart - (Vector2Int)Utilities.ChunkToWorld(offsetStartChunk);
		Vector2Int targetLocalPos = request.pathTarget - (Vector2Int)Utilities.ChunkToWorld(offsetStartChunk);

		Vector2Int[] waypoints = new Vector2Int[0];
		bool pathSuccess = false;

		// Start A* algorithm
		Node startNode = walkableGrid[startLocalPos.x, startLocalPos.y];
		Node targetNode = walkableGrid[targetLocalPos.x, targetLocalPos.y];

		if (startNode.Walkable && targetNode.Walkable)
		{
			Heap<Node> openSet = new(gridWidth * gridHeight);
			HashSet<Node> closedSet = new();


			openSet.Add(startNode);

			while (openSet.Count > 0)
			{
				Node currentNode = openSet.RemoveFirst();
				closedSet.Add(currentNode);

				if (currentNode == targetNode) // Found Path !
				{
					pathSuccess = true;
					break;
				}

				foreach (Node neighbor in GetNeighbors(currentNode))
				{
					if (!neighbor.Walkable || closedSet.Contains(neighbor))
						continue;

					int costToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor) + neighbor.Penalty;
					if (costToNeighbor < currentNode.gCost || !openSet.Contains(neighbor))
					{
						neighbor.gCost = costToNeighbor;
						neighbor.hCost = GetDistance(neighbor, targetNode);
						neighbor.parent = currentNode;

						if (!openSet.Contains(neighbor))
							openSet.Add(neighbor);
						else
							openSet.UpdateItem(neighbor);
					}
				}
			}
		}

		if (pathSuccess)
		{ 
			waypoints = RetracePath(startNode, targetNode);
			pathSuccess = waypoints.Length > 0;
		}
		
		callback(new PathResult(waypoints, pathSuccess, request.callback));
		return;

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

	Vector2Int[] RetracePath(Node startNode, Node endNode)
	{
		List<Node> path = new();
		Node currentNode = endNode;

		while (currentNode != startNode)
		{
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}

		Vector2Int[] waypoints = SimplifyPath(path);
		Array.Reverse(waypoints);

		return waypoints;
	}

	Vector2Int[] SimplifyPath(List<Node> path)
	{
		List<Vector2Int> waypoints = new();
		Vector2 directionOld = Vector2.zero;

		for (int i = 1; i < path.Count; i++)
		{
			Vector2 directionNew = new(path[i - 1].GridPos.x - path[i].GridPos.x, path[i - 1].GridPos.y - path[i].GridPos.y);
			if (directionNew != directionOld)
				waypoints.Add(path[i].WorldPos);
			directionOld = directionNew;
		}

		return waypoints.ToArray();
	}
}
