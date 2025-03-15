using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
	public Transform target;
	public float speed = 20f;
	Vector2Int[] path;
	int targetIndex;

	int counter = 0;
	int counterMax = 10;

	private void Start()
	{
		//StartCoroutine(Wait());
	}

	private void FixedUpdate()
	{
		//Debug.Log("Counter : " + counter);
		if (counter <= 0)
		{
			RequestNewPath();
			counter = counterMax;
		}
		counter--;
	}

	IEnumerator Wait()
	{
		yield return new WaitForSeconds(0.5f);
		RequestNewPath();
	}

	public void RequestNewPath()
	{
		//Debug.Log("request");
		targetIndex = 0;
		path = null;
		PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
	}

	public void OnPathFound(Vector2Int[] newPath, bool success)
	{
		if (success)
		{
			StopAllCoroutines();
			path = newPath;
			StartCoroutine(FollowPath());
		}
		else
			Debug.Log("Nope");
	}

	IEnumerator FollowPath()
	{
		if (path == null || path.Length == 0)
		{
			RequestNewPath();
			yield break;
		}
			
		Vector2Int currentWaypoint = path[0];
		targetIndex = 0;

		while (true)
		{
			if (transform.position == Utilities.PosToWorld(currentWaypoint))
			{
				targetIndex++;
				if (path == null || targetIndex >= path.Length)
				{
					RequestNewPath();
					yield break;
				}
				currentWaypoint = path[targetIndex];
			}
			transform.position = Vector3.MoveTowards(transform.position, Utilities.PosToWorld(currentWaypoint), speed * Time.deltaTime);
			yield return null;
		}
	}

	private void OnDrawGizmos()
	{
		if (path != null)
		{
			for (int i = targetIndex; i < path.Length; i++)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawCube(Utilities.PosToWorld(path[i]), Vector3.one / 3f);

				if (i == targetIndex)
					Gizmos.DrawLine(transform.position, Utilities.PosToWorld(path[i]));
				else
					Gizmos.DrawLine(Utilities.PosToWorld(path[i - 1]), Utilities.PosToWorld(path[i]));

			}
		}
	}
}
