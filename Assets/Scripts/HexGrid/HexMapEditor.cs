using UnityEngine;
using UnityEngine.EventSystems;

namespace LeGrandPotAuFeu.HexGrid {
	public class HexMapEditor : MonoBehaviour {
		[Header("Color to pick from the UI")]
		public Color[] colors;

		[Header("Drag'n'Drop")]
		public HexGrid hexGrid;

		Color activeColor;
		int activeElevation;
		int activeWaterLevel;
		bool applyColor;
		bool applyElevation = true;
		bool applyWaterLevel = true;
		int brushSize;

		void Awake() {
			SelectColor(-1);
			SetElevation(0);
		}

		void Update() {
			if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
				HandleInput();
			}
		}

		void HandleInput() {
			Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(inputRay, out hit)) {
				EditCells(hexGrid.GetCell(hit.point));
			}
		}

		void EditCells(HexCell center) {
			int centerX = center.coordinates.X;
			int centerZ = center.coordinates.Z;

			for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
				for (int x = centerX - r; x <= centerX + brushSize; x++) {
					EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
				}
			}
			for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
				for (int x = centerX - brushSize; x <= centerX + r; x++) {
					EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
				}
			}
		}

		void EditCell(HexCell cell) {
			if (cell) {
				if (applyColor) {
					cell.Color = activeColor;
				}
				if (applyElevation) {
					cell.Elevation = activeElevation;
				}
				if (applyWaterLevel) {
					cell.WaterLevel = activeWaterLevel;
				}
			}
		}

		public void SelectColor(int index) {
			applyColor = index >= 0;
			if (applyColor) {
				activeColor = colors[index];
			}
		}

		public void SetElevation(float elevation) {
			activeElevation = (int)elevation;
		}

		public void SetApplyElevation(bool toggle) {
			applyElevation = toggle;
		}

		public void SetApplyWaterLevel(bool toggle) {
			applyWaterLevel = toggle;
		}

		public void SetWaterLevel(float level) {
			activeWaterLevel = (int)level;
		}

		public void SetBrushSize(float size) {
			brushSize = (int)size;
		}

		public void ShowUI(bool visible) {
			hexGrid.ShowUI(visible);
		}
	}
}