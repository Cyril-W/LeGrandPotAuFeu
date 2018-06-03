﻿using LeGrandPotAuFeu.Utility;
using UnityEngine;

namespace LeGrandPotAuFeu.Grid {
	public class HexGridChunk : MonoBehaviour {
		public HexMesh terrain, roads, water, waterShore;
		public HexFeatureManager features;

		static Color weights1 = new Color(1f, 0f, 0f);
		static Color weights2 = new Color(0f, 1f, 0f);
		static Color weights3 = new Color(0f, 0f, 1f);

		HexCell[] cells;
		Canvas gridCanvas;

		void Awake() {
			gridCanvas = GetComponentInChildren<Canvas>();

			cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
		}

		void LateUpdate() {
			Triangulate(cells);
			enabled = false;
		}

		public void ShowUI(bool visible) {
			gridCanvas.gameObject.SetActive(visible);
		}

		public void Refresh() {
			enabled = true;
		}

		public void AddCell(int index, HexCell cell) {
			cells[index] = cell;
			cell.chunk = this;
			cell.transform.SetParent(transform, false);
			cell.UIRect.SetParent(gridCanvas.transform, false);
		}

		public void Triangulate(HexCell[] cells) {
			terrain.Clear();
			roads.Clear();
			water.Clear();
			waterShore.Clear();
			features.Clear();
			for (int i = 0; i < cells.Length; i++) {
				Triangulate(cells[i]);
			}
			terrain.Apply();
			roads.Apply();
			water.Apply();
			waterShore.Apply();
			features.Apply();
		}

		void Triangulate(HexCell cell) {
			for (var d = HexDirection.NE; d <= HexDirection.NW; d++) {
				Triangulate(d, cell);
			}
			if (!cell.IsUnderwater) {
				if (!cell.HasRoads) {
					features.AddFeature(cell, cell.Position);
				}
				if (cell.IsSpecial) {
					features.AddSpecialFeature(cell, cell.Position);
				}
			}
		}

		// Contains "TriangulateWithoutRiver" from the tutorial
		void Triangulate(HexDirection direction, HexCell cell) {
			Vector3 center = cell.Position;
			EdgeVertices e = new EdgeVertices(
					center + HexMetrics.GetSolidCorner(direction, false),
					center + HexMetrics.GetSolidCorner(direction, true)
			);

			// Section named "TriangulateWithoutRiver" in the tutorial
			TriangulateEdgeFan(center, e, cell.Index);

			if (cell.HasRoads) {
				Vector2 interpolators = GetRoadInterpolators(direction, cell);
				TriangulateRoad(
					center,
					Vector3.Lerp(center, e.v1, interpolators.x),
					Vector3.Lerp(center, e.v5, interpolators.y),
					e, cell.HasRoadThroughEdge(direction), cell.Index
				);
			}
			// End section
			
			if (direction <= HexDirection.SE) {
				TriangulateConnection(direction, cell, e);
			}

			if (cell.IsUnderwater) {
				TriangulateWater(direction, cell, center);
			} else if (!cell.HasRoadThroughEdge(direction)) {
				features.AddFeature(cell, (center + e.v1 + e.v5) * (1f / 3f));
			}
		}


		Vector2 GetRoadInterpolators(HexDirection direction, HexCell cell) {
			Vector2 interpolators;
			if (cell.HasRoadThroughEdge(direction)) {
				interpolators.x = interpolators.y = 0.5f;
			} else {
				interpolators.x =	cell.HasRoadThroughEdge(direction.Previous()) ? 0.5f : 0.25f;
				interpolators.y =	cell.HasRoadThroughEdge(direction.Next()) ? 0.5f : 0.25f;
			}
			return interpolators;
		}

		void TriangulateRoadEdge(Vector3 center, Vector3 mL, Vector3 mR, float index) {
			roads.AddTriangle(center, mL, mR);
			roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
			Vector3 indices;
			indices.x = indices.y = indices.z = index;
			roads.AddTriangleCellData(indices, weights1);
		}

		void TriangulateRoadSegment(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6, Color w1, Color w2, Vector3 indices) {
			roads.AddQuad(v1, v2, v4, v5);
			roads.AddQuad(v2, v3, v5, v6);
			roads.AddQuadUV(0f, 1f, 0f, 0f);
			roads.AddQuadUV(1f, 0f, 0f, 0f);
			roads.AddQuadCellData(indices, w1, w2);
			roads.AddQuadCellData(indices, w1, w2);
		}

		void TriangulateRoad(Vector3 center, Vector3 mL, Vector3 mR, EdgeVertices e, bool hasRoadThroughCellEdge, float index) {
			if (hasRoadThroughCellEdge) {
				Vector3 indices;
				indices.x = indices.y = indices.z = index;
				Vector3 mC = Vector3.Lerp(mL, mR, 0.5f);
				TriangulateRoadSegment(mL, mC, mR, e.v2, e.v3, e.v4, weights1, weights1, indices);
				roads.AddTriangle(center, mL, mC);
				roads.AddTriangle(center, mC, mR);
				roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f));
				roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f));
				roads.AddTriangleCellData(indices, weights1);
				roads.AddTriangleCellData(indices, weights1);
			} else {
				TriangulateRoadEdge(center, mL, mR, index);
			}
		}


		void TriangulateWater(HexDirection direction, HexCell cell, Vector3 center) {
			center.y = cell.WaterSurfaceY;

			HexCell neighbor = cell.GetNeighbor(direction);
			if (neighbor != null && !neighbor.IsUnderwater) {
				TriangulateWaterShore(direction, cell, neighbor, center);
			} else {
				TriangulateOpenWater(direction, cell, neighbor, center);
			}
		}

		void TriangulateOpenWater(HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center) {
			Vector3 c1 = center + HexMetrics.GetWaterCorner(direction, false);
			Vector3 c2 = center + HexMetrics.GetWaterCorner(direction, true);

			water.AddTriangle(center, c1, c2);
			Vector3 indices;
			indices.x = indices.y = indices.z = cell.Index;
			water.AddTriangleCellData(indices, weights1);

			if (direction <= HexDirection.SE && neighbor != null) {
				Vector3 bridge = HexMetrics.GetWaterBridge(direction);
				Vector3 e1 = c1 + bridge;
				Vector3 e2 = c2 + bridge;

				water.AddQuad(c1, c2, e1, e2);
				indices.y = neighbor.Index;
				water.AddQuadCellData(indices, weights1, weights2);

				if (direction <= HexDirection.E) {
					HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
					if (nextNeighbor == null || !nextNeighbor.IsUnderwater) {
						return;
					}
					water.AddTriangle(c2, e2, c2 + HexMetrics.GetWaterBridge(direction.Next()));
					indices.z = nextNeighbor.Index;
					water.AddTriangleCellData(indices, weights1, weights2, weights3);
				}
			}
		}

		void TriangulateWaterShore(HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center) {
			EdgeVertices e1 = new EdgeVertices(
				center + HexMetrics.GetWaterCorner(direction, false),
				center + HexMetrics.GetWaterCorner(direction, true)
			);
			water.AddTriangle(center, e1.v1, e1.v2);
			water.AddTriangle(center, e1.v2, e1.v3);
			water.AddTriangle(center, e1.v3, e1.v4);
			water.AddTriangle(center, e1.v4, e1.v5);
			Vector3 indices;
			indices.x = indices.z = cell.Index;
			indices.y = neighbor.Index;
			water.AddTriangleCellData(indices, weights1);
			water.AddTriangleCellData(indices, weights1);
			water.AddTriangleCellData(indices, weights1);
			water.AddTriangleCellData(indices, weights1);

			Vector3 center2 = neighbor.Position;
			center2.y = center.y;
			EdgeVertices e2 = new EdgeVertices(
				center2 + HexMetrics.GetSolidCorner(direction.Opposite(), true),
				center2 + HexMetrics.GetSolidCorner(direction.Opposite(), false)
			);
			waterShore.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
			waterShore.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
			waterShore.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
			waterShore.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
			waterShore.AddQuadUV(0f, 0f, 0f, 1f);
			waterShore.AddQuadUV(0f, 0f, 0f, 1f);
			waterShore.AddQuadUV(0f, 0f, 0f, 1f);
			waterShore.AddQuadUV(0f, 0f, 0f, 1f);
			waterShore.AddQuadCellData(indices, weights1, weights2);
			waterShore.AddQuadCellData(indices, weights1, weights2);
			waterShore.AddQuadCellData(indices, weights1, weights2);
			waterShore.AddQuadCellData(indices, weights1, weights2);

			HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
			if (nextNeighbor != null) {
				Vector3 v3 = nextNeighbor.Position + (nextNeighbor.IsUnderwater ?
					HexMetrics.GetWaterCorner(direction.Previous(), false) :
					HexMetrics.GetSolidCorner(direction.Previous(), false));
				v3.y = center.y;
				waterShore.AddTriangle(e1.v5, e2.v5, v3);
				waterShore.AddTriangleUV(
					new Vector2(0f, 0f),
					new Vector2(0f, 1f),
					new Vector2(0f, nextNeighbor.IsUnderwater ? 0f : 1f)
				);
				indices.z = nextNeighbor.Index;
				waterShore.AddTriangleCellData(indices, weights1, weights2, weights3);
			}
		}


		void TriangulateCorner(Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
			HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
			HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

			if (leftEdgeType == HexEdgeType.Slope) {
				if (rightEdgeType == HexEdgeType.Slope) {
					TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
				} else if (rightEdgeType == HexEdgeType.Flat) {
					TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
				} else {
					TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
				}
			} else if (rightEdgeType == HexEdgeType.Slope) {
				if (leftEdgeType == HexEdgeType.Flat) {
					TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
				} else {
					TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
				}
			} else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
				if (leftCell.Elevation < rightCell.Elevation) {
					TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
				} else {
					TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
				}
			} else {
				terrain.AddTriangle(bottom, left, right);
				Vector3 indices;
				indices.x = bottomCell.Index;
				indices.y = leftCell.Index;
				indices.z = rightCell.Index;
				terrain.AddTriangleCellData(indices, weights1, weights2, weights3);
			}

			features.AddWall(bottom, bottomCell, left, leftCell, right, rightCell);
		}

		void TriangulateCornerTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
			Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
			Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
			Color w3 = HexMetrics.TerraceLerp(weights1, weights2, 1);
			Color w4 = HexMetrics.TerraceLerp(weights1, weights3, 1);
			Vector3 indices;
			indices.x = beginCell.Index;
			indices.y = leftCell.Index;
			indices.z = rightCell.Index;

			terrain.AddTriangle(begin, v3, v4);
			terrain.AddTriangleCellData(indices, weights1, w3, w4);

			for (int i = 2; i < HexMetrics.terraceSteps; i++) {
				Vector3 v1 = v3;
				Vector3 v2 = v4;
				Color w1 = w3;
				Color w2 = w4;
				v3 = HexMetrics.TerraceLerp(begin, left, i);
				v4 = HexMetrics.TerraceLerp(begin, right, i);
				w3 = HexMetrics.TerraceLerp(weights1, weights2, i);
				w4 = HexMetrics.TerraceLerp(weights1, weights3, i);
				terrain.AddQuad(v1, v2, v3, v4);
				terrain.AddQuadCellData(indices, w1, w2, w3, w4);
			}

			terrain.AddQuad(v3, v4, left, right);
			terrain.AddQuadCellData(indices, w3, w4, weights2, weights3);
		}

		void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
			float b = 1f / (rightCell.Elevation - beginCell.Elevation);
			b = (b < 0) ? -b : b;
			Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(right), b);
			Color boundaryWeights = Color.Lerp(weights1, weights3, b);
			Vector3 indices;
			indices.x = beginCell.Index;
			indices.y = leftCell.Index;
			indices.z = rightCell.Index;

			TriangulateBoundaryTriangle(begin, weights1, left, weights2, boundary, boundaryWeights, indices);

			if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
				TriangulateBoundaryTriangle(left, weights2, right, weights3, boundary, boundaryWeights, indices);
			} else {
				terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
				terrain.AddTriangleCellData(indices, weights2, weights3, boundaryWeights);
			}
		}

		void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
			float b = 1f / (leftCell.Elevation - beginCell.Elevation);
			b = (b < 0) ? -b : b;
			Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(left), b);
			Color boundaryWeights = Color.Lerp(weights1, weights2, b);
			Vector3 indices;
			indices.x = beginCell.Index;
			indices.y = leftCell.Index;
			indices.z = rightCell.Index;

			TriangulateBoundaryTriangle(right, weights3, begin, weights1, boundary, boundaryWeights, indices);

			if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
				TriangulateBoundaryTriangle(left, weights2, right, weights3, boundary, boundaryWeights, indices);
			} else {
				terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
				terrain.AddTriangleCellData(indices, weights2, weights3, boundaryWeights);
			}
		}


		void TriangulateBoundaryTriangle(Vector3 begin, Color beginWeights, Vector3 left, Color leftWeights, Vector3 boundary, Color boundaryWeights, Vector3 indices) {
			Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
			Color w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, 1);

			terrain.AddTriangleUnperturbed(HexMetrics.Perturb(begin), v2, boundary);
			terrain.AddTriangleCellData(indices, beginWeights, w2, boundaryWeights);

			for (int i = 2; i < HexMetrics.terraceSteps; i++) {
				Vector3 v1 = v2;
				Color w1 = w2;
				v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, i));
				w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, i);
				terrain.AddTriangleUnperturbed(v1, v2, boundary);
				terrain.AddTriangleCellData(indices, w1, w2, boundaryWeights);
			}

			terrain.AddTriangleUnperturbed(v2, HexMetrics.Perturb(left), boundary);
			terrain.AddTriangleCellData(indices, w2, leftWeights, boundaryWeights);
		}

		void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e1) {
			HexCell neighbor = cell.GetNeighbor(direction);
			if (neighbor == null) {
				return;
			}

			Vector3 bridge = HexMetrics.GetBridge(direction);
			bridge.y = neighbor.Position.y - cell.Position.y;
			EdgeVertices e2 = new EdgeVertices(
					e1.v1 + bridge,
					e1.v5 + bridge
			);

			bool hasRoad = cell.HasRoadThroughEdge(direction);

			if (cell.GetEdgeType(direction) == HexEdgeType.Slope) {
				TriangulateEdgeTerraces(e1, cell, e2, neighbor, hasRoad);
			} else {
				TriangulateEdgeStrip(e1, weights1, cell.Index, e2, weights2, neighbor.Index, hasRoad);
			}

			features.AddWall(e1, cell, e2, neighbor, hasRoad);

			HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
			if (direction <= HexDirection.E && nextNeighbor != null) {
				Vector3 v5 = e1.v5 + HexMetrics.GetBridge(direction.Next());
				v5.y = nextNeighbor.Position.y;

				if (cell.Elevation <= neighbor.Elevation) {
					if (cell.Elevation <= nextNeighbor.Elevation) {
						TriangulateCorner(e1.v5, cell, e2.v5, neighbor, v5, nextNeighbor);
					} else {
						TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
					}
				} else if (neighbor.Elevation <= nextNeighbor.Elevation) {
					TriangulateCorner(e2.v5, neighbor, v5, nextNeighbor, e1.v5, cell);
				} else {
					TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
				}
			}
		}

		void TriangulateEdgeTerraces(EdgeVertices begin, HexCell beginCell, EdgeVertices end, HexCell endCell, bool hasRoad) {
			EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
			Color w2 = HexMetrics.TerraceLerp(weights1, weights2, 1);
			float i1 = beginCell.Index;
			float i2 = endCell.Index;

			TriangulateEdgeStrip(begin, weights1, i1, e2, w2, i2, hasRoad);

			for (int i = 2; i < HexMetrics.terraceSteps; i++) {
				EdgeVertices e1 = e2;
				Color w1 = w2;
				e2 = EdgeVertices.TerraceLerp(begin, end, i);
				w2 = HexMetrics.TerraceLerp(weights1, weights2, i);
				TriangulateEdgeStrip(e1, w1, i1, e2, w2, i2, hasRoad);
			}

			TriangulateEdgeStrip(e2, w2, i1, end, weights2, i2, hasRoad);
		}

		void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, float index) {
			terrain.AddTriangle(center, edge.v1, edge.v2);
			terrain.AddTriangle(center, edge.v2, edge.v3);
			terrain.AddTriangle(center, edge.v3, edge.v4);
			terrain.AddTriangle(center, edge.v4, edge.v5);

			Vector3 indices;
			indices.x = indices.y = indices.z = index;
			terrain.AddTriangleCellData(indices, weights1);
			terrain.AddTriangleCellData(indices, weights1);
			terrain.AddTriangleCellData(indices, weights1);
			terrain.AddTriangleCellData(indices, weights1);
		}

		void TriangulateEdgeStrip(EdgeVertices e1, Color w1, float index1, EdgeVertices e2, Color w2, float index2, bool hasRoad = false) {
			terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
			terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
			terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
			terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);

			Vector3 indices;
			indices.x = indices.z = index1;
			indices.y = index2;
			terrain.AddQuadCellData(indices, w1, w2);
			terrain.AddQuadCellData(indices, w1, w2);
			terrain.AddQuadCellData(indices, w1, w2);
			terrain.AddQuadCellData(indices, w1, w2);

			if (hasRoad) {
				TriangulateRoadSegment(e1.v2, e1.v3, e1.v4, e2.v2, e2.v3, e2.v4, w1, w2, indices);
			}
		}
	}
}