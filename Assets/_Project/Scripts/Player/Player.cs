using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public static Player Instance { get; private set; }

	[SerializeField] float moveSpeed = 10f;
	[SerializeField] Transform[] predators;

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

		//RequestPath();
	}

	//async void RequestPath()
	//{
	//	List<Vector2Int>[] paths = new List<Vector2Int>[predators.Length];

	//	for (int i = 0; i < predators.Length; i++)
	//	{
	//		paths[i] = await Pathfinding.FindPathAsync(predators[i].position, transform.position);
	//		if (paths[i] != null)
	//		{
	//			for (int j = 0; j < paths[i].Count - 1; j++)
	//			{
	//				Debug.DrawLine(paths[i][j] + new Vector2(0.5f, 0.5f), paths[i][j + 1] + new Vector2(0.5f, 0.5f), Color.red, 0.1f, false);
	//			}
	//		}
	//		else
	//		{
	//			Debug.Log("Path not found !");
	//		}
	//	}


	//}
}
