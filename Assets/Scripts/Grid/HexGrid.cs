using LeGrandPotAuFeu.Unit;
using LeGrandPotAuFeu.Utility;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LeGrandPotAuFeu.Grid {
	public class HexGrid : MonoBehaviour {
		[Header("Prefabs")]
		public HexUnit playerPrefab;
		public HexUnit[] enemyPrefabs;
		public HexCell cellPrefab;
		public RectTransform cellUI;
		public HexGridChunk chunkPrefab;
		[Header("Game UI Colors")]
		public Color startColor = Color.blue;
		public Color pathColor = Color.gray;
		public Color endColor = Color.green;
		public Color enemyVisionColor = Color.yellow;
		[Header("Noise Texture")]
		public Texture2D noiseSource;
		[Header("Size of the map")]
		public int cellCountX = 20;
		public int cellCountZ = 15;		
		[Header("Seed for the hash")]
		public int seed = 1234;
		[Header("Debug tool")]
		public bool displayCoordinates = false;

		public HexUnit Player { get; private set; }
		public List<HexUnit> Enemies { get; private set; }

		public bool HasPath {
			get {
				return currentPathExists;
			}
		}

		HexCell[] cells;
		HexGridChunk[] chunks;
		int chunkCountX, chunkCountZ;
		HexCellPriorityQueue searchFrontier;
		int searchFrontierPhase;
		HexCell currentPathFrom, currentPathTo;
		bool currentPathExists;
		HexCellShaderData cellShaderData;

		void Awake() {
			Enemies = new List<HexUnit>();
			HexMetrics.noiseSource = noiseSource;
			HexMetrics.InitializeHashGrid(seed);
			cellShaderData = gameObject.AddComponent<HexCellShaderData>();
			cellShaderData.Grid = this;
			CreateMap(cellCountX, cellCountZ);
		}

		void OnEnable() {
			if (!HexMetrics.noiseSource) {
				HexMetrics.noiseSource = noiseSource;
				HexMetrics.InitializeHashGrid(seed);
				ResetVisibility();
			}
		}

		public bool CreateMap(int x, int z) {
			if (x <= 0 || x % HexMetrics.chunkSizeX != 0 ||	z <= 0 || z % HexMetrics.chunkSizeZ != 0) {
				Debug.LogError("Unsupported map size.");
				return false;
			}

			ClearPath();
			ClearUnits();
			if (chunks != null) {
				for (int i = 0; i < chunks.Length; i++) {
					Destroy(chunks[i].gameObject);
				}
			}
			cellCountX = x;
			cellCountZ = z;
			chunkCountX = cellCountX / HexMetrics.chunkSizeX;
			chunkCountZ = cellCountZ / HexMetrics.chunkSizeZ;
			cellShaderData.Initialize(cellCountX, cellCountZ);
			CreateChunks();
			CreateCells();
			return true;
		}

		void CreateChunks() {
			chunks = new HexGridChunk[chunkCountX * chunkCountZ];

			for (int z = 0, i = 0; z < chunkCountZ; z++) {
				for (int x = 0; x < chunkCountX; x++) {
					HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
					chunk.transform.SetParent(transform);
				}
			}
		}

		void CreateCells() {
			cells = new HexCell[cellCountZ * cellCountX];

			for (int z = 0, i = 0; z < cellCountZ; z++) {
				for (int x = 0; x < cellCountX; x++) {
					CreateCell(x, z, i++);
				}
			}
		}

		void CreateCell(int x, int z, int i) {
			HexCell cell = cells[i] = Instantiate(cellPrefab);
			Vector3 position;
			position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
			position.y = 0f;
			position.z = z * (HexMetrics.outerRadius * 1.5f);
			cell.transform.localPosition = position;
			cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
			cell.Index = i;
			cell.ShaderData = cellShaderData;

			cell.Explorable =	x > 0 && z > 0 && x < cellCountX - 1 && z < cellCountZ - 1;

			if (x > 0) {
				cell.SetNeighbor(HexDirection.W, cells[i - 1]);
			}
			if (z > 0) {
				if ((z & 1) == 0) {
					cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
					if (x > 0) {
						cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
					}
				} else {
					cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
					if (x < cellCountX - 1) {
						cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
					}
				}
			}

			RectTransform newCellUI = Instantiate(cellUI);
			newCellUI.anchoredPosition = new Vector2(position.x, position.z);
			cell.gameObject.name = "Hex Cell " + cell.coordinates.ToString();
			cell.UIRect = newCellUI;
			cell.UIRect.name = "Hex Cell UI " + cell.coordinates.ToString();

			if (displayCoordinates) {
				cell.SetLabel(cell.coordinates.ToStringOnSeparateLines(), 4);
			}

			cell.Elevation = 0;

			AddCellToChunk(x, z, cell);
		}

		void AddCellToChunk(int x, int z, HexCell cell) {
			int chunkX = x / HexMetrics.chunkSizeX;
			int chunkZ = z / HexMetrics.chunkSizeZ;
			HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

			int localX = x - chunkX * HexMetrics.chunkSizeX;
			int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
			chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
		}

		public HexCell GetCell(int xOffset, int zOffset) {
			return cells[xOffset + zOffset * cellCountX];
		}

		public HexCell GetCell(int cellIndex) {
			return cells[cellIndex];
		}

		public HexCell GetCell(Ray ray) {
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				return GetCell(hit.point);
			}
			return null;
		}

		public HexCell GetCell(Vector3 position) {
			position = transform.InverseTransformPoint(position);
			HexCoordinates coordinates = HexCoordinates.FromPosition(position);
			int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
			return cells[index];
		}

		public HexCell GetCell(HexCoordinates coordinates) {
			int z = coordinates.Z;
			if (z < 0 || z >= cellCountZ) {
				return null;
			}
			int x = coordinates.X + z / 2;
			if (x < 0 || x >= cellCountX) {
				return null;
			}
			return cells[x + z * cellCountX];
		}

		public void ShowUI(bool visible) {
			for (int i = 0; i < chunks.Length; i++) {
				chunks[i].ShowUI(visible);
			}
		}

		public void Save(BinaryWriter writer) {
			writer.Write(cellCountX);
			writer.Write(cellCountZ);

			for (int i = 0; i < cells.Length; i++) {
				cells[i].Save(writer);
			}

			Player.Save(writer);

			writer.Write(Enemies.Count);
			foreach (var enemy in Enemies) {
				enemy.Save(writer);
			}
		}

		public void Load(BinaryReader reader, int header) {
			ClearPath();
			ClearUnits();
			int x = 20, z = 15;
			if (header >= 1) {
				x = reader.ReadInt32();
				z = reader.ReadInt32();
			}
			if (x != cellCountX || z != cellCountZ) {
				if (!CreateMap(x, z)) {
					return;
				}
			}

			for (int i = 0; i < cells.Length; i++) {
				cells[i].Load(reader, header);
			}
			for (int i = 0; i < chunks.Length; i++) {
				chunks[i].Refresh();
			}

			if (header >= 2) {
				HexUnit.Load(reader, this); // load the player

				int ennemyCount = reader.ReadInt32();
				for (int i = 0; i < ennemyCount; i++) {
					HexUnit.Load(reader, this); // load the enemies
				}
			}
		}

		public void FindPath(HexUnit unit, HexCell toCell) {
			HexCell fromCell = unit.Location;
			bool isPlayer = unit.Type == 0;
			if (isPlayer) {
				ClearPath();
			}
			currentPathFrom = fromCell;
			currentPathTo = toCell;
			currentPathExists = Search(fromCell, toCell, unit);
			if (currentPathExists && isPlayer) {
				ShowPath();
			}
		}

		void ShowPath() {
			if (currentPathExists) {
				HexCell current = currentPathTo;
				while (current != currentPathFrom) {					
					current.SetOutlineColor(pathColor);
					current = current.PathFrom;
				}
			}
			currentPathFrom.SetOutlineColor(startColor);
			currentPathTo.SetOutlineColor(endColor);
			currentPathTo.SetLabel(currentPathTo.Distance.ToString());
		}

		bool Search(HexCell fromCell, HexCell toCell, HexUnit unit) {
			searchFrontierPhase += 2;
			if (searchFrontier == null) {
				searchFrontier = new HexCellPriorityQueue();
			} else {
				searchFrontier.Clear();
			}

			fromCell.SearchPhase = searchFrontierPhase;
			fromCell.Distance = 0;
			searchFrontier.Enqueue(fromCell);
			while (searchFrontier.Count > 0) {
				HexCell current = searchFrontier.Dequeue();
				current.SearchPhase += 1;

				if (current == toCell) {
					return true;
				}

				for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
					HexCell neighbor = current.GetNeighbor(d);
					if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase) {
						continue;
					}
					if (!HexUnit.IsValidDestination(neighbor)) {
						continue;
					}
					int moveCost = unit.GetMoveCost(current, neighbor, d);
					if (moveCost < 0) {
						continue;
					}

					int distance = current.Distance + moveCost;
					int endurance = unit.Type == 0 ? unit.EnduranceLeft : int.MaxValue;
					if (endurance <= 0 || (distance - 1) / endurance > 0) {
						continue;
					}

					if (neighbor.SearchPhase < searchFrontierPhase) { // no distance found
						neighbor.SearchPhase = searchFrontierPhase;
						neighbor.Distance = distance;
						neighbor.PathFrom = current;
						neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(toCell.coordinates);
						searchFrontier.Enqueue(neighbor);
					} else if (distance < neighbor.Distance) { // shortest distance found
						int oldPriority = neighbor.SearchPriority;
						neighbor.Distance = distance;
						neighbor.PathFrom = current;
						searchFrontier.Change(neighbor, oldPriority);
					}
				}
			}
			return false;
		}

		public void ClearPath() {
			if (currentPathExists) {
				HexCell current = currentPathTo;
				while (current != currentPathFrom) {
					current.SetLabel(null);
					current.SetOutlineColor();
					current = current.PathFrom;
				}
				current.SetOutlineColor();
				currentPathExists = false;
			}
			currentPathFrom = currentPathTo = null;
		}

		public List<HexCell> GetPath() {
			if (!currentPathExists) {
				return null;
			}
			List<HexCell> path = ListPool<HexCell>.Get();
			for (HexCell c = currentPathTo; c != currentPathFrom; c = c.PathFrom) {
				path.Add(c);
			}
			path.Add(currentPathFrom);
			path.Reverse();
			return path;
		}

		public void AddUnit(HexCell location, float orientation, int type) {
			bool isEnemy = type > 0;
			HexUnit unit;
			if (isEnemy) {
				unit = Instantiate(enemyPrefabs[type - 1], transform, false);
				Enemies.Add(unit);								
			} else {
				if (Player) {
					Player.Die();
				}
				unit = Instantiate(playerPrefab, transform, false);
				Player = unit;
			}
			unit.Grid = this;
			unit.Type = type;
			unit.Location = location;
			unit.Orientation = orientation;
			var unitName = ((HexUnitType)type).ToString();
			if (isEnemy) {
				unitName += Enemies.FindAll(x => x.Type == type).Count;
			}
			unit.name = unitName;
		}

		public void RemoveUnit(HexUnit unit) {
			unit.Die();
			if (unit.Type == 0) {
				Player = null;
			} else {
				Enemies.Remove(unit);
			}
		}

		void ClearUnits() {
			if (Player) {
				Player.Die();
				Player = null;
			}
			foreach (var enemy in Enemies) {
				enemy.Die();
			}
			Enemies.Clear();			
		}

		public List<HexCell> GetVisibleCells(HexCell fromCell, int range, bool applyElevation, HexDirection facingDirection, int nbDirections) {
			List<HexCell> visibleCells = ListPool<HexCell>.Get();

			searchFrontierPhase += 2;
			if (searchFrontier == null) {
				searchFrontier = new HexCellPriorityQueue();
			} else {
				searchFrontier.Clear();
			}

			if (applyElevation) {
				range += fromCell.ViewElevation;
			}
			fromCell.SearchPhase = searchFrontierPhase;
			fromCell.Distance = 0;
			searchFrontier.Enqueue(fromCell);
			HexCoordinates fromCoordinates = fromCell.coordinates;
			while (searchFrontier.Count > 0) {
				HexCell current = searchFrontier.Dequeue();
				current.SearchPhase += 1;
				visibleCells.Add(current);

				for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
					HexCell neighbor = current.GetNeighbor(d);
					if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase) {
						continue;
					}

					int distance = current.Distance + 1;
					if (distance + neighbor.ViewElevation > range || distance > fromCoordinates.DistanceTo(neighbor.coordinates)) {
						continue;
					}

					if (neighbor.SearchPhase < searchFrontierPhase) {
						neighbor.SearchPhase = searchFrontierPhase;
						neighbor.Distance = distance;
						neighbor.SearchHeuristic = 0;
						searchFrontier.Enqueue(neighbor);
					} else if (distance < neighbor.Distance) {
						int oldPriority = neighbor.SearchPriority;
						neighbor.Distance = distance;
						searchFrontier.Change(neighbor, oldPriority);
					}
				}
			}
			return visibleCells;
		}

		public void IncreaseVisibility(HexUnit unit, HexCell dynamicLocation = null) {
			HexCell fromCell = dynamicLocation ? dynamicLocation : unit.Location;
			bool isPlayer = unit.Type == 0;
			List<HexCell> cells = GetVisibleCells(fromCell, (isPlayer ? unit.visionRange : 1), isPlayer, unit.FacingDirection, 6);
			for (int i = 0; i < cells.Count; i++) {
				if (isPlayer) {
					cells[i].IncreaseVisibility();
				} else {
					cells[i].IsDeadly = true;
				}

				var enemy = i > 0 ? cells[i].Unit : null;
				if (enemy) {
					enemy.UpdateVisibleCells();
				}
			}
			ListPool<HexCell>.Add(cells);			
		}

		public void DecreaseVisibility(HexUnit unit, HexCell dynamicLocation = null) {
			HexCell fromCell = dynamicLocation ? dynamicLocation : unit.Location;
			bool isPlayer = unit.Type == 0;
			List<HexCell> cells = GetVisibleCells(fromCell, (isPlayer ? unit.visionRange : 1), isPlayer, unit.FacingDirection, 6);
			for (int i = 0; i < cells.Count; i++) {
				if (isPlayer) {
					cells[i].DecreaseVisibility();
				} else {
					cells[i].IsDeadly = false;
				}

				var enemy = i > 0 ? cells[i].Unit : null;
				if (enemy) {
					enemy.UpdateVisibleCells();
				}
			}
			ListPool<HexCell>.Add(cells);
		}

		public void ResetVisibility() {
			for (int i = 0; i < cells.Length; i++) {
				cells[i].ResetVisibility();
			}
			if (Player) {
				IncreaseVisibility(Player);
			}
		}

		public void ResetExplored() {
			for (int i = 0; i < cells.Length; i++) {
				cells[i].ResetExplored();
			}
		}
	}
}