﻿using UnityEngine;
using System.Collections;
using Lockstep;

namespace LockStep.Mono
{
	public class GridMono : MonoBehaviour
	{
		[SerializeField]
		private Vector2d _mapCenter;

		public Vector2d Offset {
			get {
				return _mapCenter - new Vector2d(_mapWidth / 2, _mapHeight / 2);
			}
		}


		[SerializeField]
		private int _mapWidth = 100;

		public int MapWidth { get { return _mapWidth; } }

		[SerializeField]
		private int _mapHeight = 100;

		public int MapHeight { get { return _mapHeight; } }

		[SerializeField]
		private bool _useDiagonalConnections = true;

		public bool UseDiagonalConnetions { get { return _useDiagonalConnections; } }

		void Start()
		{
			OnSave();
			OnApply();
		}

		protected void OnSave()
		{
			this._mapCenter = new Vector2d(transform.position);
		}

		protected void OnApply()
		{
			GridManager.Settings = new GridSettings(this.MapWidth, this.MapHeight, this.Offset.x, this.Offset.y, this.UseDiagonalConnetions);
			GridManager.Initialize();
			GridManager.GetNode(4, 4).AddObstacle();
		}

		#if UNITY_EDITOR
		public bool Show;

		void OnDrawGizmos()
		{
			if (!Show)
				return;

			Gizmos.color = Color.green;
			Vector3 offset = Offset.ToVector3(this.transform.position.y);
			Vector3 scale = Vector3.one * .9f;
			scale.y = 0.01f;
			for (int x = 0; x < MapWidth; x++) {
				for (int y = 0; y < MapHeight; y++) {
					Vector3 drawPos = new Vector3(x, 0f, y);
					drawPos += offset;
					if (Application.isPlaying) {
						var Grid = GridManager.GetNode(x, y);
						if (Grid.Unpassable()) {
							Gizmos.color = Color.red;
						} else {
							Gizmos.color = Color.green;
						}
					}
					Gizmos.DrawCube(drawPos, scale);
				}
			}
		}
		#endif
	}
}



