using System.IO;
using UnityEngine;
using UnityEngine.UI;

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
		int urbanLevel;
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
		int farmLevel;
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
		int plantLevel;
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
		public int TerrainTypeIndex {
			get {
				return terrainTypeIndex;
			}
			set {
				if (terrainTypeIndex != value) {
					terrainTypeIndex = value;
					Refresh();
				}
			}
		}
		int terrainTypeIndex;
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
				RefreshPosition();
				for (int i = 0; i < roads.Length; i++) {
					if (roads[i] && GetElevationDifference((HexDirection)i) > 1) {
						SetRoad(i, false);
					}
				}
				Refresh();
			}
		}
		int elevation = int.MinValue;
		public bool HasRoads {
			get {
				for (int i = 0; i < roads.Length; i++) {
					if (roads[i]) {
						return true;
					}
				}
				return false;
			}
		}
		public int SpecialIndex {
			get {
				return specialIndex;
			}
			set {
				if (specialIndex != value) {
					specialIndex = value;
					RemoveRoads();
					RefreshSelfOnly();
				}
			}
		}
		int specialIndex;
		public bool IsSpecial {
			get {
				return specialIndex > 0;
			}
		}
		public int Distance {
			get {
				return distance;
			}
			set {
				distance = value;
				UpdateDistanceLabel();
			}
		}
		int distance;
		public HexCell PathFrom { get; set; }
		public HexCell NextWithSamePriority { get; set; }
		public int SearchHeuristic { get; set; }
		public int SearchPriority {
			get {
				return distance + SearchHeuristic;
			}
		}

		[SerializeField] HexCell[] neighbors;
		[SerializeField] bool[] roads;

		void UpdateDistanceLabel() {
			Text label = uiRect.GetComponent<Text>();
			label.text = distance == int.MaxValue ? "" : distance.ToString();
		}

		public bool HasRoadThroughEdge(HexDirection direction) {
			return roads[(int)direction];
		}

		public void AddRoad(HexDirection direction) {
			if (!roads[(int)direction] && !IsSpecial && !GetNeighbor(direction).IsSpecial && GetElevationDifference(direction) <= 1) {
				SetRoad((int)direction, true);
			}
		}

		public void RemoveRoads() {
			for (int i = 0; i < neighbors.Length; i++) {
				if (roads[i]) {
					SetRoad(i, false);
				}
			}
		}

		void SetRoad(int index, bool state) {
			roads[index] = state;
			neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
			neighbors[index].RefreshSelfOnly();
			RefreshSelfOnly();
		}

		public int GetElevationDifference(HexDirection direction) {
			int difference = elevation - GetNeighbor(direction).elevation;
			return difference >= 0 ? difference : -difference;
		}

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
			return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
		}

		void RefreshPosition() {
			Vector3 position = transform.localPosition;
			position.y = elevation * HexMetrics.elevationStep;
			position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
			transform.localPosition = position;

			Vector3 uiPosition = uiRect.localPosition;
			uiPosition.z = -position.y;
			uiRect.localPosition = uiPosition;
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

		public void DisableHighlight() {
			Image highlight = uiRect.GetComponentInChildren<Image>();
			highlight.enabled = false;
		}

		public void EnableHighlight(Color color) {
			Image highlight = uiRect.GetComponentInChildren<Image>();
			highlight.color = color;
			highlight.enabled = true;
		}

		public void Save(BinaryWriter writer) {
			writer.Write((byte)terrainTypeIndex);
			writer.Write((byte)elevation);
			writer.Write((byte)waterLevel);
			writer.Write((byte)urbanLevel);
			writer.Write((byte)farmLevel);
			writer.Write((byte)plantLevel);
			writer.Write((byte)specialIndex);
			writer.Write(walled);
			// 0 if no road, 1 if road ... 6 bits, from right to left = from 0 to 5
			int roadFlags = 0;
			for (int i = 0; i < roads.Length; i++) {
				if (roads[i]) {
					roadFlags |= 1 << i;
				}
			}
			writer.Write((byte)roadFlags);
		}

		public void Load(BinaryReader reader) {
			terrainTypeIndex = reader.ReadByte();
			elevation = reader.ReadByte();
			RefreshPosition();
			waterLevel = reader.ReadByte();
			urbanLevel = reader.ReadByte();
			farmLevel = reader.ReadByte();
			plantLevel = reader.ReadByte();
			specialIndex = reader.ReadByte();
			walled = reader.ReadBoolean();
			// see comment in above function Save()
			int roadFlags = reader.ReadByte();
			for (int i = 0; i < roads.Length; i++) {
				roads[i] = (roadFlags & (1 << i)) != 0;
			}
		}
	}
}
