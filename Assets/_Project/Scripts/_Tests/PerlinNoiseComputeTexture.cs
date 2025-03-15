using System.Diagnostics;
using UnityEngine;

public class PerlinNoiseComputeTexture : MonoBehaviour
{
	public ComputeShader computeShader;
	public int textureSize = 1024;
	public float scale = 10f;
	[Range(0f, 1f)] public float threshold = 0.5f;
	public Renderer _renderer;

	private RenderTexture renderTexture;

	private Stopwatch sw = new();

	void Start()
	{
		renderTexture = new RenderTexture(textureSize, textureSize, 0);
		renderTexture.enableRandomWrite = true;
		renderTexture.filterMode = FilterMode.Point;
		renderTexture.Create();

		_renderer.sharedMaterial.mainTexture = renderTexture;

		GenerateNoise();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			GenerateNoise();
		}
	}

	void GenerateNoise()
	{
		sw.Start();

		int kernel = computeShader.FindKernel("CSMain");
		computeShader.SetTexture(kernel, "Result", renderTexture);
		computeShader.SetInt("Width", textureSize);
		computeShader.SetInt("Height", textureSize);
		computeShader.SetFloat("Scale", scale);
		computeShader.SetFloat("Threshold", threshold);
		computeShader.SetFloat("Seed", Time.time);

		int threadGroups = Mathf.CeilToInt(textureSize / 8.0f);
		computeShader.Dispatch(kernel, threadGroups, threadGroups, 1);
		
		sw.Stop();
		UnityEngine.Debug.Log("Time : " + (sw.Elapsed.TotalMilliseconds) + "ms");
	}
}
