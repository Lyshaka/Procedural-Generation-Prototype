using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public static Player Instance { get; private set; }

	[SerializeField] float moveSpeed = 10f;
	[SerializeField] Transform predator;

	Vector2 _velocity;

	Rigidbody2D rb;

	public Vector2 Velocity => _velocity;

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();


		//for (int i = -20; i < 20; i++)
		//{
		//	for (int j = -20; j < 20; j++)
		//	{
		//		Debug.Log($"Pos : ({i},{j}) => Chunk : ({ChunkManager.PosToChunk(new Vector2Int(i, j))})");
		//	}
		//}

	}

	private void Update()
	{
		_velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * moveSpeed;

		rb.linearVelocity = _velocity;

		RequestPath();
	}

	async void RequestPath()
	{
		Vector2Int startPos = ChunkManager.FloorPosition(predator.position);
		Vector2Int targetPos = ChunkManager.FloorPosition(transform.position);
		//Debug.Log($"Start: {startPos}, Target: {targetPos}");

		List<Vector2Int> path = await Pathfinding.FindPathAsync(startPos, targetPos);

		if (path != null)
		{
			for (int i = 0; i < path.Count - 1; i++)
			{
				Debug.DrawLine(path[i] + new Vector2(0.5f, 0.5f), path[i + 1] + new Vector2(0.5f, 0.5f), Color.red, 0f, false);
			}
		}
		else
		{
			Debug.Log("Path not found !");
		}
	}
}
