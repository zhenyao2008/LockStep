﻿using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;
using System.ComponentModel;
using System;

namespace Lockstep.Mono
{
	public enum BoundingType : byte
	{
		None,
		Circle,
		AABox,
		Polygon
	}

	[RequireComponent(typeof(Collider))]
	public sealed partial class BoundingBox : MonoBehaviour
	{
		//[SerializeField, FormerlySerializedAs("Shape")]
		private BoundingType Shape = BoundingType.AABox;

		//public BoundingType Shape { get { return _shape; } }
		[HideInInspector]
		public Bounds m_Bound;

		public FixedAABB2D m_AABB = new FixedAABB2D(Vector2d.zero, 0, 0);

		public void Initialize()
		{
			m_AABB.update(m_Bound.center.x, m_Bound.center.z, m_Bound.extents.x, m_Bound.extents.z);
		}

		long GetCeiledSnap(long f, long snap)
		{
			return (f + snap - 1) / snap * snap;
		}

		long GetFlooredSnap(long f, long snap)
		{
			return (f / snap) * snap;
		}

		public void AutoSet()
		{
			Collider col = GetComponent<Collider>();
			if (col) {
				m_Bound = col.bounds;

				Type type = col.GetType();

				if (type == typeof(SphereCollider) || type == typeof(CapsuleCollider)) {
					Shape = BoundingType.Circle;
				} else {
					Shape = BoundingType.AABox;
				}
			}
		}

		public void UpdateValues()
		{
			m_AABB.update(transform.position.x, transform.position.z);
		}

		public void GetCoveredSnappedPositions(long snapSpacing, FastList<Vector2d> output)
		{
			long xmin = GetFlooredSnap(m_AABB.XMin - FixedMath.Half, snapSpacing);
			long ymin = GetFlooredSnap(m_AABB.YMin - FixedMath.Half, snapSpacing);

			long xmax = GetCeiledSnap(m_AABB.XMax + FixedMath.Half - xmin, snapSpacing) + xmin;
			long ymax = GetCeiledSnap(m_AABB.YMax + FixedMath.Half - ymin, snapSpacing) + ymin;
			//Debug.LogFormat("XMin{0:F}, YMin{1:F}, XMax{2:F}, YMax{3:F}", m_AABB.XMin.ToFloat(), m_AABB.YMin.ToFloat(), m_AABB.XMax.ToFloat(), m_AABB.YMax.ToFloat());
			//Debug.LogFormat("xmin{0:F}, ymin{1:F}, xmax{2:F}, ymax{3:F}", xmin.ToFloat(), ymin.ToFloat(), xmax.ToFloat(), ymax.ToFloat());
			//Used for getting snapped positions this body covered
			for (long x = xmin; x < xmax; x += snapSpacing) {
				for (long y = ymin; y < ymax; y += snapSpacing) {
					Vector2d checkPos = new Vector2d(x, y);

					if (IsPositionCovered(checkPos)) {
						output.Add(checkPos);
					}
				}
			}
		}

		public bool IsPositionCovered(Vector2d position)
		{
			switch (this.Shape) {
				case BoundingType.Circle:
					var radis = Math.Max(m_AABB.m_HalfX, m_AABB.m_HalfY);
					long maxDistance = radis + FixedMath.Half;
					maxDistance *= maxDistance;
					if ((m_AABB.m_Center - position).FastMagnitude() > maxDistance)
						return false;
					goto case BoundingType.AABox;
				case BoundingType.AABox:
					return m_AABB.intersect(position, FixedMath.Half, FixedMath.Half);
				case BoundingType.Polygon:
					break;
			}

			return false;
		}
		#if UNITY_EDITOR
		void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			AutoSet();
			switch (this.Shape) {
				case BoundingType.Circle:
					Gizmos.DrawWireSphere(m_Bound.center, Math.Max(m_Bound.size.x, m_Bound.size.z) / 2);
					break;
				case BoundingType.AABox:
					Gizmos.DrawWireCube(m_Bound.center, m_Bound.size);
					break;
				case BoundingType.Polygon:
					//Gizmos.DrawWireSphere(this.transform.position, this.Radius);
					break;
			}
		}
		#endif
	}
}

