using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace GK {
	public class StopMotion : MonoBehaviour {

		public Transform RootBone;
		public int StoppedFrameCount = 5;

		int recordedFrame = -1;

		List<Transform> transforms = null;
		List<STransform> actualPositions = null;
		List<STransform> renderedPositions = null;

		IEnumerator endOfFrameCoroutine;

		void OnEnable() {
			transforms = null;
			actualPositions = null;
			renderedPositions = null;

			endOfFrameCoroutine = EndOfFrameCoroutine();

			StartCoroutine(endOfFrameCoroutine);
		}

		void OnDisable() {
			StopCoroutine(endOfFrameCoroutine);
		}

		void LateUpdate() {
			if(transforms == null) {
				transforms = new List<Transform>(RootBone.GetComponentsInChildren<Transform>());
			} 

			RecordTransform(ref actualPositions);

			if(renderedPositions == null || Time.frameCount - recordedFrame >= StoppedFrameCount) {
				recordedFrame = Time.frameCount;

				RecordTransform(ref renderedPositions);
			} else {
				RestoreRecord(renderedPositions);
			}
		}

		IEnumerator EndOfFrameCoroutine() {
			var endOfFrame = new WaitForEndOfFrame();

			while(true) {
				yield return endOfFrame;

				RestoreRecord(actualPositions);
			}
		}

		void RecordTransform(ref List<STransform> record) {
			if(record == null) {
				record = new List<STransform>(transforms.Count);

				foreach(var t in transforms) {
					record.Add(STransform.FromTransform(t));
				}
			} else {
				for(int i = 0; i < transforms.Count; i++) {
					record[i] = STransform.FromTransform(transforms[i]);
				}
			}

			Debug.Assert(transforms.Count == record.Count);
		}

		void RestoreRecord(List<STransform> record) {
			Debug.Assert(record != null);
			Debug.Assert(record.Count == transforms.Count);

			for(int i = 0; i < transforms.Count; i++) {
				record[i].WriteTo(transforms[i]);
			}
		}


		void Reset() {
			var smr = GetComponentInChildren<SkinnedMeshRenderer>();

			if(smr != null) {
				RootBone = smr.rootBone;
			} else {
				RootBone = null;
			}

			StoppedFrameCount = 5;
		}

		struct STransform {
			public Vector3 LocalPosition;
			public Quaternion LocalRotation;
			public Vector3 LocalScale;

			public static STransform FromTransform(Transform t) {
				return new STransform {
					LocalPosition = t.localPosition,
					LocalRotation = t.localRotation,
					LocalScale    = t.localScale
				};
			}

			public void WriteTo(Transform t) {
				t.localPosition = LocalPosition;
				t.localRotation = LocalRotation;
				t.localScale    = LocalScale;
			}
		}
	}
}
