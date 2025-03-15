using System;
using UnityEngine;
using System.Collections.Generic;

public class PathRequestManager : MonoBehaviour
{
	static PathRequestManager instance;

	readonly Queue<PathRequest> _pathRequestQueue = new();
	PathRequest _currentPathRequest;
	Pathfinding _pathfinding;
	bool _isProcessingPath;

	private void Awake()
	{
		instance = this;
		_pathfinding = GetComponent<Pathfinding>();
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
		PathRequest newRequest = new(pathStart, pathTarget, callback);
		instance._pathRequestQueue.Enqueue(newRequest);
		instance.TryProcessNext();
	}

	void TryProcessNext()
	{
		if (!_isProcessingPath && _pathRequestQueue.Count > 0)
		{
			_currentPathRequest = _pathRequestQueue.Dequeue();
			_isProcessingPath = true;
			_pathfinding.StartFindPath(_currentPathRequest.pathStart, _currentPathRequest.pathTarget);
		}
	}

	public void FinishedProcessingPath(Vector2Int[] path, bool success)
	{
		_currentPathRequest.callback(path, success);
		_isProcessingPath = false;
		TryProcessNext();
	}

	struct PathRequest
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
	}
}
