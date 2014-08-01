using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

public class RelativeTime : MonoBehaviour {
	public const string CS_PROP_CLOCKS_INPUT = "clocksIn";
	public const string CS_PROP_CLOCKS_OUTPUT = "clocksOut";
	public const string CS_PROP_DELTA_TIME = "dt";
	public const int CS_NUM_THREADS = 256;

	public int numClocks = CS_NUM_THREADS;
	public float tEnd = 60f;
	public ComputeShader relativeTimeCompute;

	private int numGroups;
	private Clock[] _clocks;
	private ComputeBuffer _clocksBuffer0, _clocksBuffer1;

	void Start() {
		numGroups = Mathf.CeilToInt((float)numClocks / CS_NUM_THREADS);
		numClocks = CS_NUM_THREADS * numGroups;

		_clocks = new Clock[numClocks];
		for (var i = 0; i < _clocks.Length; i++)
			_clocks[i] = new Clock(Vector2.zero, 0f);

		_clocksBuffer0 = new ComputeBuffer(numClocks, Marshal.SizeOf(typeof(Clock)));
		_clocksBuffer1 = new ComputeBuffer(numClocks, Marshal.SizeOf(typeof(Clock)));
		_clocksBuffer0.SetData(_clocks);

		StartCoroutine(Logger());
	}

	IEnumerator Logger() {
		var log = new StringBuilder();
		while (enabled) {
			yield return new WaitForSeconds(1f);
			log.Length = 0;
			_clocksBuffer0.GetData(_clocks);
			for (var i = 0; i < 100; i++) {
				var c = _clocks[i];
				if (c.t < 0 || c.t >= tEnd)
					continue;
				log.AppendFormat("[{0:f}] ", c.t);
			}
			if (log.Length > 0)
				Debug.Log(log.ToString());
		}
	}

	void OnDestroy() {
		if (_clocksBuffer0 != null)
			_clocksBuffer0.Release();
		if (_clocksBuffer1 != null)
			_clocksBuffer1.Release();
	}

	void Update () {
		relativeTimeCompute.SetBuffer(0, CS_PROP_CLOCKS_INPUT, _clocksBuffer0);
		relativeTimeCompute.SetBuffer(0, CS_PROP_CLOCKS_OUTPUT, _clocksBuffer1);
		relativeTimeCompute.SetFloat(CS_PROP_DELTA_TIME, Time.deltaTime);
		relativeTimeCompute.Dispatch(0, numGroups, 1, 1);
		var tmpBuffer = _clocksBuffer0; _clocksBuffer0 = _clocksBuffer1; _clocksBuffer1 = tmpBuffer;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct Clock {
		public Vector2 uv;
		public float t;

		public Clock(Vector2 uv, float t) {
			this.uv = uv;
			this.t = t;
		}

		public static readonly Clock Null = new Clock(Vector2.zero, -1f);
	}
}
