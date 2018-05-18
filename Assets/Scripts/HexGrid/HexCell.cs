using UnityEngine;

namespace LeGrandPotAuFeu.HexGrid {
	public class HexCell : MonoBehaviour {
		public HexCoordinates coordinates;
		public RectTransform uiRect;
		public HexGridChunk chunk;
		public int UrbanLevel {
			get {
				return urbanLevel;
			}
			set {
				if (urbanLevel != value) {
					urbanLevel = value;
					RefreshSelfOnly();
				}
			}
		}
		public int FarmLevel {
			get {
				return farmLevel;
			}
			set {
				if (farmLevel != value) {
					farmLevel = value;
					RefreshSelfOnly();
				}
			}
		}
		public int PlantLevel {
			get {
				return plantLevel;
			}
			set {
				if (plantLevel != value) {
					plantLevel = value;
					RefreshSelfOnly();
				}
			}
		}
		int urbanLevel, farmLevel, plantLevel;
		public bool Walled {
			get {
				return walled;
			}
			set {
				if (walled != value) {
					walled = value;
					Refresh();
				}
			}
		}
		bool walled;
		public int WaterLevel {
			get {
				return waterLevel;
			}
			set {
				if (waterLevel == value) {
					return;
				}
				waterLevel = value;
				Refresh();
			}
		}
		int waterLevel;
		public bool IsUnderwater {
			get {
				return waterLevel > elevation;
			}
		}
		public float WaterSurfaceY {
			get {
				return
					(waterLevel + HexMetrics.waterElevationOffset) *
					HexMetrics.elevationStep;
			}
		}
		public Color Color {
			get {
				return color;
			}
			set {
				if (color == value) {
					return;
				}
				color = value;
				Refresh();
			}
		}
		Color color;
		public Vector3 Position {
			get {
				return transform.localPosition;
			}
		}
		public int Elevation {
			get {
				return elevation;
			}
			set {
				if (elevation == value) {
					return;
				}

				elevation = value;
				Vector3 position = transform.localPosition;
				position.y = value * HexMetrics.elevationStep;
				position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
				transform.localPosition = position;

				Vector3 uiPosition = uiRect.localPosition;
				uiPosition.z = -position.y;
				uiRect.localPosition = uiPosition;

				Refresh();
			}
		}
		int elevation = int.MinValue;

		[SerializeField]
		HexCell[] neighbors;

		public HexCell GetNeighbor(HexDirection direction) {
			return neighbors[(int)direction];
		}

		public void SetNeighbor(HexDirection direction, HexCell cell) {
			neighbors[(int)direction] = cell;
			cell.neighbors[(int)direction.Opposite()] = this;
		}

		public HexEdgeType GetEdgeType(HexDirection direction) {
			return HexMetrics.GetEdgeType(
					elevation, neighbors[(int)direction].elevation
			);
		}

		public HexEdgeType GetEdgeType(HexCell otherCell) {
			return HexMetrics.GetEdgeType(
					elevation, otherCell.elevation
			);
		}

		void RefreshSelfOnly() {
			chunk.Refresh();
		}
		void Refresh() {
			if (chunk) {
				chunk.Refresh();
				for (int i = 0; i < neighbors.Length; i++) {
					HexCell neighbor = neighbors[i];
					if (neighbor != null && neighbor.chunk != chunk) {
						neighbor.chunk.Refresh();
					}
				}
			}
		}
	}
}
