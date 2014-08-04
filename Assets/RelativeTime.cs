﻿using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

public class RelativeTime : MonoBehaviour {
	public const string CS_PROP_CLOCKS_PREV = "clocksPrev";
	public const string CS_PROP_CLOCKS_CURR = "clocksCurr";
	public const string CS_PROP_DELTA_TIME = "dt";
	public const int CS_NUM_THREADS = 256;

	public const string LIFE_PROPE_CLOCKS_CURR = "clocksCurr";
	public const string LIFE_PROP_ID = "id";

	public int numClocks = CS_NUM_THREADS;
	public float tEnd = 60f;
	public ComputeShader relativeTimeCompute;
	public GameObject flowerfab;

	private int numGroups;
	private Clock[] _clocks;
	private ComputeBuffer _clocksBuffer0, _clocksBuffer1;
	private GameObject[] _flowers;

	void Start() {
		numGroups = Mathf.CeilToInt((float)numClocks / CS_NUM_THREADS);
		numClocks = CS_NUM_THREADS * numGroups;

		_clocks = new Clock[numClocks];
		for (var i = 0; i < _clocks.Length; i++)
			_clocks[i] = new Clock(Vector2.zero, -1f);

		_clocksBuffer0 = new ComputeBuffer(numClocks, Marshal.SizeOf(typeof(Clock)));
		_clocksBuffer1 = new ComputeBuffer(numClocks, Marshal.SizeOf(typeof(Clock)));
		_clocksBuffer0.SetData(_clocks);

		_flowers = new GameObject[numClocks];

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
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (collider.Raycast(ray, out hit, float.MaxValue)) {
				var id = -1;
				_clocksBuffer0.GetData(_clocks);
				for (var i = 0; i < _clocks.Length; i++) {
					var c = _clocks[i];
					if (c.t < 0) {
						id = i;
						c.t = 0f;
						c.uv = hit.textureCoord;
						_clocks[i] = c;
						_clocksBuffer0.SetData(_clocks);
						break;
					}
				}
				if (id >= 0) {
					if (_flowers[id] != null)
						Destroy(_flowers[id]);

					Debug.Log("Add flower");
					var go = (_flowers[id] = (GameObject)Instantiate(flowerfab)).transform;
					go.parent = transform;
					go.position = hit.point;
					var mat = go.renderer.material;
					mat.SetInt(LIFE_PROP_ID, id);
				}
			}
		}

		relativeTimeCompute.SetBuffer(0, CS_PROP_CLOCKS_PREV, _clocksBuffer0);
		relativeTimeCompute.SetBuffer(0, CS_PROP_CLOCKS_CURR, _clocksBuffer1);
		relativeTimeCompute.SetFloat(CS_PROP_DELTA_TIME, Time.deltaTime);
		relativeTimeCompute.Dispatch(0, numGroups, 1, 1);
		var tmpBuffer = _clocksBuffer0; _clocksBuffer0 = _clocksBuffer1; _clocksBuffer1 = tmpBuffer;

		Shader.SetGlobalBuffer(LIFE_PROPE_CLOCKS_CURR, _clocksBuffer0);
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
