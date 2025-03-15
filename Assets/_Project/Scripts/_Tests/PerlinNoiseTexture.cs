using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public class PerlinNoiseTexture : MonoBehaviour
{
	public int textureWidth = 256;
	public int textureHeight = 256;
	public float scale = 10f;
	[Range(0.0f, 1.0f)] public float threshold = 0.5f;

	public bool randomSeed = false;
	public int size = 50000;

	[Header("Red")]
	[Range(0.0f, 1.0f)] public float redThresholdMin = 0.5f;
	[Range(0.0f, 1.0f)] public float redThresholdMax = 0.5f;

	[Header("Green")]
	[Range(0.0f, 1.0f)] public float greenThresholdMin = 0.5f;
	[Range(0.0f, 1.0f)] public float greenThresholdMax = 0.5f;

	[Header("Blue")]
	[Range(0.0f, 1.0f)] public float blueThresholdMin = 0.5f;
	[Range(0.0f, 1.0f)] public float blueThresholdMax = 0.5f;

	[Header("Seeds : ")]
	public float seedX = 1_000_000f;
	public float seedY = 1_000_000f;

	SpriteRenderer _spriteRenderer;
	Texture2D _texture;

	Stopwatch sw = new();

	private void Start()
	{
		_spriteRenderer = GetComponentInChildren<SpriteRenderer>();

		
		GenerateTexture();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			GenerateTexture();
		}
		if (Input.GetKeyDown(KeyCode.K))
		{
			RemoveIsolatedPixels(ref _texture);
		}
	}

	void GenerateTexture()
	{
		sw.Restart();

		_texture = new Texture2D(textureWidth, textureHeight);
		Color32[] pixels = new Color32[textureWidth * textureHeight];
		_texture.filterMode = FilterMode.Point;
		//Random.InitState((int)Time.time * 1000);
		//float seed = Random.Range(0f, 1000f);

		if (randomSeed)
		{
			seedX = Random.Range(-1_000_000f, 1_000_000f);
			seedY = Random.Range(-1_000_000f, 1_000_000f);
		}

		Parallel.For(0, textureHeight, y =>
		{
			for (int x = 0; x < textureWidth; x++)
			{
				float noiseValue = (Mathf.PerlinNoise((float)x / textureWidth * scale + seedX, (float)y / textureHeight * scale + seedY));

				Color color = new(0f, 0f, 0f);

				if (noiseValue > redThresholdMin && noiseValue < redThresholdMax)
					color = Color.red;
				if (noiseValue > greenThresholdMin && noiseValue < greenThresholdMax)
					color = Color.green;
				if (noiseValue > blueThresholdMin && noiseValue < blueThresholdMax)
					color = Color.blue;


				//pixels[y * textureWidth + x] = new Color(noiseValue, noiseValue, noiseValue);
				pixels[y * textureWidth + x] = color;
			}
		});

		_texture.SetPixels32(pixels);
		_texture.Apply();

		// Assign texture to a material or sprite
		_spriteRenderer.sprite = Sprite.Create(_texture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f));

		sw.Stop();
		UnityEngine.Debug.Log("Time : " + (sw.Elapsed.TotalMilliseconds) + "ms");
	}

	void RemoveIsolatedPixels(ref Texture2D texture)
	{
		int width = texture.width, height = texture.height;
		Color32[] pixels = texture.GetPixels32();
		bool[,] visited = new bool[width, height];
		List<Vector2Int> largeRedPixels = new List<Vector2Int>();

		// Find and keep only large red blobs
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (!visited[x, y] && pixels[y * width + x].r > 0.5f) // Check for red pixels
				{
					List<Vector2Int> blob = FloodFill(pixels, visited, width, height, x, y);
					if (blob.Count >= size) // Keep only large blobs
					{
						largeRedPixels.AddRange(blob);
					}
				}
			}
		}

		// Modify existing texture instead of creating a new one
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				int index = y * width + x;
				if (!largeRedPixels.Contains(new Vector2Int(x, y)))
				{
					pixels[index] = Color.black; // Remove unwanted pixels (black background)
				}
			}
		}

		texture.SetPixels32(pixels);
		texture.Apply();
		_spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f));
	}



	List<Vector2Int> FloodFill(Color32[] pixels, bool[,] visited, int width, int height, int startX, int startY)
	{
		List<Vector2Int> blob = new List<Vector2Int>();
		Queue<Vector2Int> queue = new Queue<Vector2Int>();
		queue.Enqueue(new Vector2Int(startX, startY));
		visited[startX, startY] = true; // Mark as visited immediately

		while (queue.Count > 0)
		{
			Vector2Int pos = queue.Dequeue();
			blob.Add(pos);

			// Check all 4 neighbors (up, down, left, right)
			Vector2Int[] neighbors = {
			new(pos.x + 1, pos.y),
			new(pos.x - 1, pos.y),
			new(pos.x, pos.y + 1),
			new(pos.x, pos.y - 1)
		};

			foreach (Vector2Int neighbor in neighbors)
			{
				if (neighbor.x >= 0 && neighbor.x < width && neighbor.y >= 0 && neighbor.y < height)
				{
					int index = neighbor.y * width + neighbor.x;
					if (!visited[neighbor.x, neighbor.y] && pixels[index].r > 0.5f) // Check red channel
					{
						queue.Enqueue(neighbor);
						visited[neighbor.x, neighbor.y] = true; // Mark as visited **before** adding to queue
					}
				}
			}
		}

		return blob;
	}


	float Threshold(float a)
	{
		if (a > threshold)
			return Mathf.Ceil(a);
		return Mathf.Floor(a);
	}
}
