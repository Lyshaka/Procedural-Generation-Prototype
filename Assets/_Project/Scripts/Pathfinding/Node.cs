using UnityEngine;

public class Node : IHeapItem<Node>
{
	private bool _walkable;
	private Vector2Int _worldPos;
	private Vector2Int _gridPos;
	private int _heapIndex;
	private int _penalty;

	public int gCost;
	public int hCost;
	public int fCost => gCost + hCost;
	public Node parent;

	public bool Walkable => _walkable;
	public Vector2Int WorldPos => _worldPos;
	public Vector2Int GridPos => _gridPos;
	public int Penalty => _penalty;



	public Node(bool walkable, Vector2Int worldPos, Vector2Int gridPos, int penalty)
	{
		_walkable = walkable;
		_worldPos = worldPos;
		_gridPos = gridPos;
		_penalty = penalty;
	}

	public int HeapIndex
	{
		get { return _heapIndex; }
		set { _heapIndex = value; }
	}

	public int CompareTo(Node nodeToCompare)
	{
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0)
			compare = hCost.CompareTo(nodeToCompare.hCost);
		return -compare;
	}
}