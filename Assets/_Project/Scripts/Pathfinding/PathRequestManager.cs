using System;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PathRequestManager : MonoBehaviour
{
	static PathRequestManager instance;

	Queue<PathResult> results = new Queue<PathResult>();

	Pathfinding _pathfinding;
	bool _isProcessingPath;

	private void Awake()
	{
		instance = this;
		_pathfinding = GetComponent<Pathfinding>();
	}

	private void Update()
	{
		if (results.Count > 0)
		{
			int itemsInQueue = results.Count;
			lock (results)
			{
				for (int i = 0; i < itemsInQueue; i++)
				{
					PathResult result = results.Dequeue();
					result.callback(result.path, result.success);
				}
			}
		}
	}

	public static void RequestPath(Vector3 pathStart, Vector3 pathTarget, Action<Vector2Int[], bool> callback)
	{
		RequestPath(
			Utilities.FloorPosition(pathStart),
			Utilities.FloorPosition(pathTarget),
			callback);
	}

	public static void RequestPath(Vector2Int pathStart, Vector2Int pathTarget, Action<Vector2Int[], bool> callback)
	{
		RequestPath(new(pathStart, pathTarget, callback));
	}

	public static void RequestPath(PathRequest request)
	{
		ThreadStart threadStart = delegate
		{
			instance._pathfinding.FindPath(request, instance.FinishedProcessingPath);
		};

		Thread thread = new(threadStart);
		thread.Start();
		//threadStart.Invoke();
	}

	public void FinishedProcessingPath(PathResult result)
	{
		//new PathResult(path, success, originalRequest.callback);
		lock (results)
		{
			results.Enqueue(result);
		}
	}
}

public struct PathRequest
{
	public Vector2Int pathStart;
	public Vector2Int pathTarget;
	public Action<Vector2Int[], bool> callback;

	public PathRequest(Vector2Int pathStart, Vector2Int pathTarget, Action<Vector2Int[], bool> callback)
	{
		this.pathStart = pathStart;
		this.pathTarget = pathTarget;
		this.callback = callback;
	}

	public PathRequest(Vector3 pathStart, Vector3 pathTarget, Action<Vector2Int[], bool> callback)
	{
		this.pathStart = Utilities.FloorPosition(pathStart);
		this.pathTarget = Utilities.FloorPosition(pathTarget);
		this.callback = callback;
	}
}

public struct PathResult
{
	public Vector2Int[] path;
	public bool success;
	public Action<Vector2Int[], bool> callback;

	public PathResult(Vector2Int[] path, bool success, Action<Vector2Int[], bool> callback)
	{
		this.path = path;
		this.success = success;
		this.callback = callback;
	}
}