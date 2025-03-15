using TMPro;
using UnityEngine;

public class PlayerTest : MonoBehaviour
{
	[SerializeField] float moveSpeed = 10f;

	Vector2Int _currentChunkPosition;
	Vector2Int _previousChunkPosition;

	Vector3 _move;

	private void Start()
	{
		_currentChunkPosition = new(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
		_previousChunkPosition = _currentChunkPosition;
	}

	void Update()
	{
		_move = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		transform.position += moveSpeed * Time.deltaTime * _move.normalized;

		_currentChunkPosition.x = Mathf.RoundToInt(transform.position.x);
		_currentChunkPosition.y = Mathf.RoundToInt(transform.position.y);

		if (_currentChunkPosition != _previousChunkPosition)
		{
			Debug.Log("Entered chunk : " + _currentChunkPosition);
			_previousChunkPosition = _currentChunkPosition;
		}

		ChunkSystemTest.Instance.playerChunkPosition = new(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
	}
}
