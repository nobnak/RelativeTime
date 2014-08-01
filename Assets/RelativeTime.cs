using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

public class RelativeTime : MonoBehaviour {
	public const string CS_PROP_CLOCKS = "clocks";
	public const string CS_PROP_DELTA_TIME = "dt";
	public const int CS_NUM_THREADS = 256;

	public int numClocks = CS_NUM_THREADS;
	public float tEnd = 60f;
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

		StartCoroutine(Logger());
	}

	IEnumerator Logger() {
		var log = new StringBuilder();
		while (enabled) {
			yield return new WaitForSeconds(1f);
			log.Length = 0;
			float dt;
			_clocksBuffer.GetData(_clocks);
			for (var i = 0; i < _clocks.Length; i++) {
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
		if (_clocksBuffer != null)
			_clocksBuffer.Release();
	}

	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (collider.Raycast(ray, out hit, float.MaxValue)) {
				for (var i = 0; i < _clocks.Length; i++) {
					var c = _clocks[i];
					if (c.t < 0f || c.t >= tEnd) {
						c.t = 0f;
						_clocks[i] = c;
						break;
					}
				}
				_clocksBuffer.SetData(_clocks);
			}
		}

		relativeTimeCompute.SetBuffer(0, CS_PROP_CLOCKS, _clocksBuffer);
		relativeTimeCompute.SetFloat(CS_PROP_DELTA_TIME, Time.deltaTime);
		relativeTimeCompute.Dispatch(0, numGroups, 1, 1);
	}

	[StructLayout(LayoutKind.Sequential)]
	struct Clock {
		public float t;

		public static readonly Clock Null = new Clock() { uv = Vector2.zero, t = -1f };
	}
}
