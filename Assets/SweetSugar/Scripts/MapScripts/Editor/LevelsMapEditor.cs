// // ©2015 - 2024 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SweetSugar.Scripts.MapScripts.Editor
{
	[CustomEditor (typeof(LevelsMap))]
	public class LevelsMapEditor : LevelsEditorBase {
		private LevelsMap _levelsMap;

		private float _width;
		private float _height;
		bool fixToggle;

		public void OnEnable () {
			_levelsMap = target as LevelsMap;
		}

		public override void OnInspectorGUI () {
			GUILayout.BeginVertical ();
			fixToggle = EditorGUILayout.Toggle ("fix map", fixToggle);
			if (!fixToggle) {
				if (_levelsMap.IsGenerated) {
					DrawLevelsSettings ();
					DrawStarsSettings ();
					DrawMapCameraSettings ();

					DrawLevelClickSettings ();

					if (GUILayout.Button ("Clear all", GUILayout.MinWidth (120)) &&
					    EditorUtility.DisplayDialog ("Delete all?",
						    "Are you sure that you want to delete all levels map settings?", "Delete", "Cancel")) {
						Clear ();
					}
				} else {
					DrawGenerateDraft ();
				}
			} else {
				DrawDefaultInspector ();
			}
			GUILayout.Space (16);
			GUILayout.EndVertical ();

			EditorUtility.SetDirty (_levelsMap);
		}

		private void DrawLevelsSettings () {
			GUILayout.BeginVertical ("Box");
			EditorGUILayout.LabelField ("General:");


			_levelsMap.TranslationType = (TranslationType)EditorGUILayout.EnumPopup ("Translation type", _levelsMap.TranslationType);

			GUILayout.EndVertical ();
		}

		private void Clear () {
			while (_levelsMap.transform.childCount > 0) {
				DestroyImmediate (_levelsMap.transform.GetChild (0).gameObject);
			}
			_levelsMap.IsGenerated = false;
			DisableScrolling ();
		}

		#region Generation

		private void DrawGenerateDraft () {
			GUILayout.BeginVertical ("Box");
			_levelsMap.Count = EditorGUILayout.IntField ("Count", _levelsMap.Count);
			_levelsMap.CharacterPrefab = EditorGUILayout.ObjectField ("Character prefab", _levelsMap.CharacterPrefab, typeof(Transform), false) as Transform;
			GUILayout.EndVertical ();

			if (GUILayout.Button ("Generate draft", GUILayout.MinWidth (120))) {
				Generate ();
				_levelsMap.IsGenerated = true;
				SetStarsEnabled (_levelsMap, false);
			}
		}

		private void Generate () {
			InitBounds ();
		}

		private void InitBounds () {
			_height = Camera.main.orthographicSize * 2 * 0.9f;
			_width = _height * 1.33333f * 0.9f;
		}
		private Vector3 GetPosition (float p, float width, float xOffset, float height, float yOffset) {
			return new Vector3 (
				xOffset + p * width - _width / 2f,
				yOffset + p * height - _height / 2f,
				0f);
		}

		/// <summary>
		/// Devide [0,1] line to array of points.
		/// If count = 1, ret {0}
		/// If count = 2, ret {0, 1}
		/// If count = 3, ret {0, 0.5, 1}
		/// If count = 4, ret {0, 0.25, 0.25, 1}
		/// </summary>
		private float[] DevideLineToPoints (int pointsCount) {
			if (pointsCount <= 0)
				return new float[0];

			float[] points = new float[pointsCount];
			for (int i = 0; i < pointsCount; i++)
				points [i] = i * 1f / (pointsCount - 1);

			return points;
		}

		#endregion

		#region Stars

		private void DrawStarsSettings () {
			if (_levelsMap.StarsEnabled) {
				if (GUILayout.Button ("Disable stars")) {
					SetStarsEnabled (_levelsMap, false);
				} else {
					DrawEnableState ();
				}
			} else {
				if (GUILayout.Button ("Enable stars")) {
					SetStarsEnabled (_levelsMap, true);
				}
			}
		}

		private void DrawEnableState () {
			GUILayout.BeginVertical ("Box");
			GUILayout.Label ("Stars enabled:");
			StarsType starsType = (StarsType)EditorGUILayout.EnumPopup ("Stars type", _levelsMap.StarsType);
			if (starsType != _levelsMap.StarsType)
				_levelsMap.SetStarsType (starsType);
			GUILayout.EndVertical ();
		}

		#endregion

		#region Map camera

		private void DrawMapCameraSettings () {
			if (_levelsMap.ScrollingEnabled) {
				if (GUILayout.Button ("Disable map scrolling"))
					DisableScrolling ();
				else
					DrawMapCameraBounds ();
			} else {
				if (GUILayout.Button ("Enable map scrolling"))
					EnableScrolling (Camera.main);
			}
		}

		private void EnableScrolling (Camera camera) {
			_levelsMap.ScrollingEnabled = true;
			_levelsMap.MapCamera = camera.gameObject.AddComponent<MapCamera> ();
			_levelsMap.MapCamera.Camera = camera;
			_levelsMap.MapCamera.Bounds.size = new Vector2 (camera.orthographicSize * 3f, camera.orthographicSize * 3f);
			EditorUtility.SetDirty (_levelsMap);
		}

		private void DisableScrolling () {
			_levelsMap.ScrollingEnabled = false;
			DestroyImmediate (_levelsMap.MapCamera);
			EditorUtility.SetDirty (_levelsMap);
		}

		private void DrawMapCameraBounds () {
			MapCamera mapCamera = _levelsMap.MapCamera;

			GUILayout.BeginVertical ("Box");

			EditorGUILayout.LabelField ("Map bounds:");

			mapCamera.Bounds.center = new Vector3 (
				EditorGUILayout.FloatField ("Center X", mapCamera.Bounds.center.x),
				mapCamera.Bounds.center.y,
				mapCamera.Bounds.center.z);
			mapCamera.Bounds.center = new Vector3 (
				mapCamera.Bounds.center.x,
				EditorGUILayout.FloatField ("Center Y", mapCamera.Bounds.center.y),
				mapCamera.Bounds.center.z);
			mapCamera.Bounds.size = new Vector3 (
				EditorGUILayout.FloatField ("Size X", mapCamera.Bounds.size.x),
				mapCamera.Bounds.size.y,
				mapCamera.Bounds.size.z);
			mapCamera.Bounds.size = new Vector3 (
				mapCamera.Bounds.size.x,
				EditorGUILayout.FloatField ("Size Y", mapCamera.Bounds.size.y),
				mapCamera.Bounds.size.z);

			GUILayout.EndVertical ();
			if(!Application.isPlaying)
			{
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				EditorUtility.SetDirty(mapCamera);
			}

			Camera camera = EditorGUILayout.ObjectField ("Map Camera", mapCamera.Camera, typeof(Camera), true) as Camera;
			if (camera != mapCamera.Camera) {
				if (camera == null) {
					DisableScrolling ();
				} else {
					Bounds bounds = mapCamera.Bounds;
					DisableScrolling ();
					EnableScrolling (camera);
					mapCamera = _levelsMap.MapCamera;
					mapCamera.Bounds = bounds;
					EditorUtility.SetDirty (mapCamera);
				}
			}
		}

		#endregion

		#region Level selection confirmation

		private void DrawLevelClickSettings () {
			if (_levelsMap.IsClickEnabled) {
				if (GUILayout.Button ("Disable levels click/tap")) {
					_levelsMap.IsClickEnabled = false;
					EditorUtility.SetDirty (_levelsMap);
				}
				DrawConfirmationSettings ();
			} else {
				if (GUILayout.Button ("Enable levels click/tap")) {
					_levelsMap.IsClickEnabled = true;
					EditorUtility.SetDirty (_levelsMap);
				}
			}
		}

		private void DrawConfirmationSettings () {
			GUILayout.BeginVertical ("Box");
			string helpString = "Level click/tap enabled.\n";

			if (_levelsMap.IsConfirmationEnabled) {
				helpString +=
					"Confirmation enabled: Click/tap level on map and catch 'LevelsMap.LevelSelected' event. After confirmation call 'LevelsMap.GoToLevel' method.";
				GUILayout.Box (helpString);
				if (GUILayout.Button ("Disable confirmation")) {
					_levelsMap.IsConfirmationEnabled = false;
					EditorUtility.SetDirty (_levelsMap);
				}
			} else {
				helpString += "Confirmation disabled: Click/tap level on map for character moving to level.";
				GUILayout.Box (helpString);
				if (GUILayout.Button ("Enable confirmation")) {
					_levelsMap.IsConfirmationEnabled = true;
					EditorUtility.SetDirty (_levelsMap);
				}
			}

			GUILayout.EndVertical ();
		}

		#endregion
	}
}
