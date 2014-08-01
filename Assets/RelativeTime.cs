using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class RelativeTime : MonoBehaviour {
	public const string CS_PROP_CLOCKS = "clocks";
	public const string CS_PROP_DELTA_TIME = "dt";
	public const int CS_NUM_THREADS = 256;

	public int numClocks = CS_NUM_THREADS;
	public ComputeShader relativeTimeCompute;

	private int numGroups;
	private Clock[] _clocks;
	private ComputeBuffer _clocksBuffer;

	void Start() {
		numGroups = Mathf.CeilToInt((float)numClocks / CS_NUM_THREADS);
		numClocks = CS_NUM_THREADS * numGroups;

		_clocks = new Clock[numClocks];
		for (var i = 0; i < _clocks.Length; i++)
			_clocks[i] = Clock.Null;

		_clocksBuffer = new ComputeBuffer(numClocks, Marshal.SizeOf(typeof(Clock)));
		_clocksBuffer.SetData(_clocks);
	}

	void OnDestroy() {
		if (_clocksBuffer != null)
			_clocksBuffer.Release();
	}

	void Update () {
		relativeTimeCompute.SetFloat(CS_PROP_DELTA_TIME, Time.deltaTime);
		relativeTimeCompute.Dispatch(0, numGroups, 1, 1);
	}

	[StructLayout(LayoutKind.Sequential)]
	struct Clock {
		Vector2 uv;
		float t;

		public static readonly Clock Null = new Clock() { uv = Vector2.zero, t = -1f };
	}
}
