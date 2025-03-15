using UnityEngine;

public class Node : IHeapItem<Node>
{
	private bool _walkable;
	private Vector2Int _worldPos;
	private Vector2Int _gridPos;
	private int heapIndex;

	public int gCost;
	public int hCost;
	public int fCost => gCost + hCost;
	public Node parent;

	public bool Walkable => _walkable;
	public Vector2Int WorldPos => _worldPos;
	public Vector2Int GridPos => _gridPos;



	public Node(bool walkable, Vector2Int worldPos, Vector2Int gridPos)
	{
		_walkable = walkable;
		_worldPos = worldPos;
		_gridPos = gridPos;
	}

	public int HeapIndex
	{
		get { return heapIndex; }
		set { heapIndex = value; }
	}

	public int CompareTo(Node nodeToCompare)
	{
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0)
			compare = hCost.CompareTo(nodeToCompare.hCost);
		return -compare;
	}
}