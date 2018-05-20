using UnityEngine;

namespace LeGrandPotAuFeu.HexGrid {
	public class HexGridChunk : MonoBehaviour {
		public HexMesh terrain, roads, water, waterShore;
		public HexFeatureManager features;

		static Color color1 = new Color(1f, 0f, 0f);
		static Color color2 = new Color(0f, 1f, 0f);
		static Color color3 = new Color(0f, 0f, 1f);

		HexCell[] cells;
		Canvas gridCanvas;

		void Awake() {
			gridCanvas = GetComponentInChildren<Canvas>();

			cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
			ShowUI(false);
		}

		void LateUpdate() {
			Triangulate(cells);
			enabled = false;
		}

		public void AddCell(int index, HexCell cell) {
			cells[index] = cell;
			cell.chunk = this;
			cell.transform.SetParent(transform, false);
			cell.uiRect.SetParent(gridCanvas.transform, false);
		}

		public void Refresh() {
			enabled = true;
		}

		public void ShowUI(bool visible) {
			gridCanvas.gameObject.SetActive(visible);
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
			TriangulateEdgeFan(center, e, cell.TerrainTypeIndex);

			if (cell.HasRoads) {
				Vector2 interpolators = GetRoadInterpolators(direction, cell);
				TriangulateRoad(
					center,
					Vector3.Lerp(center, e.v1, interpolators.x),
					Vector3.Lerp(center, e.v5, interpolators.y),
					e, cell.HasRoadThroughEdge(direction)
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

		void TriangulateRoadSegment(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6) {
			roads.AddQuad(v1, v2, v4, v5);
			roads.AddQuad(v2, v3, v5, v6);
			roads.AddQuadUV(0f, 1f, 0f, 0f);
			roads.AddQuadUV(1f, 0f, 0f, 0f);
		}

		void TriangulateRoad(Vector3 center, Vector3 mL, Vector3 mR, EdgeVertices e, bool hasRoadThroughCellEdge) {
			if (hasRoadThroughCellEdge) {
				Vector3 mC = Vector3.Lerp(mL, mR, 0.5f);
				TriangulateRoadSegment(mL, mC, mR, e.v2, e.v3, e.v4);
				roads.AddTriangle(center, mL, mC);
				roads.AddTriangle(center, mC, mR);
				roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f));
				roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f));
			} else {
				TriangulateRoadEdge(center, mL, mR);
			}
		}

		void TriangulateRoadEdge(Vector3 center, Vector3 mL, Vector3 mR) {
			roads.AddTriangle(center, mL, mR);
			roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
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

			if (direction <= HexDirection.SE && neighbor != null) {
				Vector3 bridge = HexMetrics.GetWaterBridge(direction);
				Vector3 e1 = c1 + bridge;
				Vector3 e2 = c2 + bridge;

				water.AddQuad(c1, c2, e1, e2);

				if (direction <= HexDirection.E) {
					HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
					if (nextNeighbor == null || !nextNeighbor.IsUnderwater) {
						return;
					}
					water.AddTriangle(
						c2, e2, c2 + HexMetrics.GetWaterBridge(direction.Next())
					);
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
			}
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
				TriangulateEdgeStrip(e1, color1, cell.TerrainTypeIndex, e2, color2, neighbor.TerrainTypeIndex, hasRoad);
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
			Color c2 = HexMetrics.TerraceLerp(color1, color2, 1);
			float t1 = beginCell.TerrainTypeIndex;
			float t2 = endCell.TerrainTypeIndex;

			TriangulateEdgeStrip(begin, color1, t1, e2, c2, t2, hasRoad);

			for (int i = 2; i < HexMetrics.terraceSteps; i++) {
				EdgeVertices e1 = e2;
				Color c1 = c2;
				e2 = EdgeVertices.TerraceLerp(begin, end, i);
				c2 = HexMetrics.TerraceLerp(color1, color2, i);
				TriangulateEdgeStrip(e1, c1, t1, e2, c2, t2, hasRoad);
			}

			TriangulateEdgeStrip(e2, c2, t1, end, color2, t2, hasRoad);
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
				terrain.AddTriangleColor(color1, color2, color3);
				Vector3 types;
				types.x = bottomCell.TerrainTypeIndex;
				types.y = leftCell.TerrainTypeIndex;
				types.z = rightCell.TerrainTypeIndex;
				terrain.AddTriangleTerrainTypes(types);
			}

			features.AddWall(bottom, bottomCell, left, leftCell, right, rightCell);
		}

		void TriangulateCornerTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
			Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
			Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
			Color c3 = HexMetrics.TerraceLerp(color1, color2, 1);
			Color c4 = HexMetrics.TerraceLerp(color1, color3, 1);
			Vector3 types;
			types.x = beginCell.TerrainTypeIndex;
			types.y = leftCell.TerrainTypeIndex;
			types.z = rightCell.TerrainTypeIndex;

			terrain.AddTriangle(begin, v3, v4);
			terrain.AddTriangleColor(color1, c3, c4);
			terrain.AddTriangleTerrainTypes(types);

			for (int i = 2; i < HexMetrics.terraceSteps; i++) {
				Vector3 v1 = v3;
				Vector3 v2 = v4;
				Color c1 = c3;
				Color c2 = c4;
				v3 = HexMetrics.TerraceLerp(begin, left, i);
				v4 = HexMetrics.TerraceLerp(begin, right, i);
				c3 = HexMetrics.TerraceLerp(color1, color2, i);
				c4 = HexMetrics.TerraceLerp(color1, color3, i);
				terrain.AddQuad(v1, v2, v3, v4);
				terrain.AddQuadColor(c1, c2, c3, c4);
				terrain.AddQuadTerrainTypes(types);
			}

			terrain.AddQuad(v3, v4, left, right);
			terrain.AddQuadColor(c3, c4, color2, color3);
			terrain.AddQuadTerrainTypes(types);
		}

		void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
			float b = 1f / (rightCell.Elevation - beginCell.Elevation);
			b = (b < 0) ? -b : b;
			Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(right), b);
			Color boundaryColor = Color.Lerp(color1, color3, b);
			Vector3 types;
			types.x = beginCell.TerrainTypeIndex;
			types.y = leftCell.TerrainTypeIndex;
			types.z = rightCell.TerrainTypeIndex;

			TriangulateBoundaryTriangle(begin, color1, left, color2, boundary, boundaryColor, types);

			if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
				TriangulateBoundaryTriangle(left, color2, right, color3, boundary, boundaryColor, types);
			} else {
				terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
				terrain.AddTriangleColor(color2, color3, boundaryColor);
				terrain.AddTriangleTerrainTypes(types);
			}
		}

		void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
			float b = 1f / (leftCell.Elevation - beginCell.Elevation);
			b = (b < 0) ? -b : b;
			Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(left), b);
			Color boundaryColor = Color.Lerp(color1, color2, b);
			Vector3 types;
			types.x = beginCell.TerrainTypeIndex;
			types.y = leftCell.TerrainTypeIndex;
			types.z = rightCell.TerrainTypeIndex;

			TriangulateBoundaryTriangle(right, color3, begin, color1, boundary, boundaryColor, types);

			if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
				TriangulateBoundaryTriangle(left, color2, right, color3, boundary, boundaryColor, types);
			} else {
				terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
				terrain.AddTriangleColor(color2, color3, boundaryColor);
				terrain.AddTriangleTerrainTypes(types);
			}
		}

		void TriangulateBoundaryTriangle(Vector3 begin, Color beginColor, Vector3 left, Color leftColor, Vector3 boundary, Color boundaryColor, Vector3 types) {
			Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
			Color c2 = HexMetrics.TerraceLerp(beginColor, leftColor, 1);

			terrain.AddTriangleUnperturbed(HexMetrics.Perturb(begin), v2, boundary);
			terrain.AddTriangleColor(beginColor, c2, boundaryColor);
			terrain.AddTriangleTerrainTypes(types);

			for (int i = 2; i < HexMetrics.terraceSteps; i++) {
				Vector3 v1 = v2;
				Color c1 = c2;
				v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, i));
				c2 = HexMetrics.TerraceLerp(beginColor, leftColor, i);
				terrain.AddTriangleUnperturbed(v1, v2, boundary);
				terrain.AddTriangleColor(c1, c2, boundaryColor);
				terrain.AddTriangleTerrainTypes(types);
			}

			terrain.AddTriangleUnperturbed(v2, HexMetrics.Perturb(left), boundary);
			terrain.AddTriangleColor(c2, leftColor, boundaryColor);
			terrain.AddTriangleTerrainTypes(types);
		}

		void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, float type) {
			terrain.AddTriangle(center, edge.v1, edge.v2);
			terrain.AddTriangle(center, edge.v2, edge.v3);
			terrain.AddTriangle(center, edge.v3, edge.v4);
			terrain.AddTriangle(center, edge.v4, edge.v5);

			terrain.AddTriangleColor(color1);
			terrain.AddTriangleColor(color1);
			terrain.AddTriangleColor(color1);
			terrain.AddTriangleColor(color1);

			Vector3 types;
			types.x = types.y = types.z = type;
			terrain.AddTriangleTerrainTypes(types);
			terrain.AddTriangleTerrainTypes(types);
			terrain.AddTriangleTerrainTypes(types);
			terrain.AddTriangleTerrainTypes(types);
		}

		void TriangulateEdgeStrip(EdgeVertices e1, Color c1, float type1, EdgeVertices e2, Color c2, float type2, bool hasRoad = false) {
			terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
			terrain.AddQuadColor(c1, c2);
			terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
			terrain.AddQuadColor(c1, c2);
			terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
			terrain.AddQuadColor(c1, c2);
			terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
			terrain.AddQuadColor(c1, c2);

			Vector3 types;
			types.x = types.z = type1;
			types.y = type2;
			terrain.AddQuadTerrainTypes(types);
			terrain.AddQuadTerrainTypes(types);
			terrain.AddQuadTerrainTypes(types);
			terrain.AddQuadTerrainTypes(types);

			if (hasRoad) {
				TriangulateRoadSegment(e1.v2, e1.v3, e1.v4, e2.v2, e2.v3, e2.v4);
			}
		}
	}
}