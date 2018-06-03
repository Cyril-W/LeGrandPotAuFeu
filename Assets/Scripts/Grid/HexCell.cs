﻿using LeGrandPotAuFeu.Unit;
using LeGrandPotAuFeu.Utility;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace LeGrandPotAuFeu.Grid {
	public class HexCell : MonoBehaviour {
		public HexCoordinates coordinates;
		public RectTransform UIRect {
			get { return uiRect; }
			set {
				uiRect = value;

				var imgs = value.GetComponentsInChildren<Image>();
				fullImg = imgs[0];
				outlineImg = imgs[1];				
				label = value.GetComponentInChildren<Text>();

				if (!fullImg || !outlineImg || !label) {
					Debug.LogError("HexCell UI Rect does not have all components");
				}
			}
		}
		public HexGridChunk chunk;

		public Vector3 Position {
			get {
				return transform.localPosition;
			}
		}
		public int Distance {
			get {
				return distance;
			}
			set {
				distance = value;
			}
		}
		[SerializeField] HexCell[] neighbors;

		public int Index { get; set; }
		public int TerrainTypeIndex {
			get {
				return terrainTypeIndex;
			}
			set {
				if (terrainTypeIndex != value) {
					terrainTypeIndex = value;
					ShaderData.RefreshTerrain(this);
				}
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
				int originalViewElevation = ViewElevation;
				elevation = value;
				if (ViewElevation != originalViewElevation) {
					ShaderData.ViewElevationChanged();
				}
				RefreshPosition();
				for (int i = 0; i < roads.Length; i++) {
					if (roads[i] && GetElevationDifference((HexDirection)i) > 1) {
						SetRoad(i, false);
					}
				}
				Refresh();
			}
		}
		public int WaterLevel {
			get {
				return waterLevel;
			}
			set {
				if (waterLevel == value) {
					return;
				}
				int originalViewElevation = ViewElevation;
				waterLevel = value;
				if (ViewElevation != originalViewElevation) {
					ShaderData.ViewElevationChanged();
				}
				Refresh();
			}
		}
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
		public int ViewElevation {
			get {
				return elevation >= waterLevel ? elevation : waterLevel;
			}
		}
		public float WaterSurfaceY {
			get {
				return
					(waterLevel + HexMetrics.waterElevationOffset) *
					HexMetrics.elevationStep;
			}
		}
		public bool IsUnderwater {
			get {
				return waterLevel > elevation;
			}
		}
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
		public bool IsSpecial {
			get {
				return specialIndex > 0;
			}
		}
		public bool IsVisible {
			get {
				return visibility > 0 && Explorable;
			}
		}
		public bool IsExplored {
			get {
				return explored && Explorable;
			}
			private set {
				explored = value;
			}
		}
		public bool Explorable { get; set; }

		public HexCell PathFrom { get; set; }
		public HexCell NextWithSamePriority { get; set; }
		public int SearchHeuristic { get; set; }
		public int SearchPriority {
			get {
				return distance + SearchHeuristic;
			}
		}
		public int SearchPhase { get; set; }

		public HexUnit Unit { get; set; }
		public HexCellShaderData ShaderData { get; set; }

		RectTransform uiRect;
		Text label;
		Image outlineImg;
		Image fullImg;
		int distance;
		int terrainTypeIndex;
		int elevation = int.MinValue;
		int waterLevel;
		int urbanLevel;
		int farmLevel;
		int plantLevel;
		int specialIndex;
		int visibility;
		bool walled;
		bool explored;
		[SerializeField] bool[] roads;

		public void SetLabel(string text) {
			label.text = text;
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

		void SetRoad(int index, bool state) {
			roads[index] = state;
			neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
			neighbors[index].RefreshSelfOnly();
			RefreshSelfOnly();
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

		void Refresh() {
			if (chunk) {
				chunk.Refresh();
				for (int i = 0; i < neighbors.Length; i++) {
					HexCell neighbor = neighbors[i];
					if (neighbor != null && neighbor.chunk != chunk) {
						neighbor.chunk.Refresh();
					}
				}
				if (Unit) {
					Unit.ValidateLocation();
				}
			}
		}

		void RefreshSelfOnly() {
			chunk.Refresh();
			if (Unit) {
				Unit.ValidateLocation();
			}
		}

		void RefreshPosition() {
			Vector3 position = transform.localPosition;
			position.y = elevation * HexMetrics.elevationStep;
			position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
			transform.localPosition = position;

			Vector3 uiPosition = UIRect.localPosition;
			uiPosition.z = -position.y;
			UIRect.localPosition = uiPosition;
		}

		public void IncreaseVisibility() {
			visibility += 1;
			if (visibility == 1) {
				IsExplored = true;
				ShaderData.RefreshVisibility(this);
			}
		}

		public void DecreaseVisibility() {
			visibility -= 1;
			if (visibility == 0) {
				ShaderData.RefreshVisibility(this);
			}
		}

		public void EnableHighlight(Color color, bool isOutline = true) {
			if (isOutline) {
				outlineImg.color = color;
				outlineImg.enabled = true;
			} else {
				fullImg.color = color;
				fullImg.enabled = true;
			}
		}

		public void DisableUI(bool isOutline = true) {
			if (isOutline) {
				outlineImg.enabled = false;
			} else {
				fullImg.enabled = false;
			}
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
			writer.Write(IsExplored);
		}

		public void Load(BinaryReader reader, int header) {
			terrainTypeIndex = reader.ReadByte();
			ShaderData.RefreshTerrain(this);
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
			IsExplored = header >= 3 ? reader.ReadBoolean() : false;
			ShaderData.RefreshVisibility(this);
		}

		public void ResetVisibility() {
			if (visibility > 0) {
				visibility = 0;
				ShaderData.RefreshVisibility(this);
			}
			DisableUI(false);
		}

		public void ResetExplored() {
			explored = false;
			ShaderData.RefreshExplored(this);
		}
	}
}
