using LeGrandPotAuFeu.Grid;
using UnityEngine;
using LeGrandPotAuFeu.Utility;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;

namespace LeGrandPotAuFeu.Unit {
	enum HexUnitType {
		Player, Carrot, Leek, Potato, Turnip
	}
	public class HexUnit : MonoBehaviour {
		public delegate void EndMovement(HexUnit unit);
		public static event EndMovement OnMovementEnd;

		public HexGrid Grid { get; set; }
		public HexDirection FacingDirection {
			get {
				var hexDirections = System.Enum.GetValues(typeof(HexDirection)).Cast<HexDirection>();
				var facingDirection = hexDirections.OrderBy(x => Mathf.Abs(orientation - x.Angle())).First();
				return facingDirection;
			}
		}
		public float Orientation {
			get {
				return orientation;
			}
			set {
				orientation = value;
				transform.localRotation = Quaternion.Euler(0f, value, 0f);
				if (location) {
					UpdateDangerousCells();
				}
			}
		}
		public HexCell Location {
			get {
				return location;
			}
			set {
				if (location) {
					Grid.DecreaseVisibility(this);
					location.Unit = null;
				}
				location = value;
				value.Unit = this;
				Grid.IncreaseVisibility(this);
				transform.localPosition = value.Position;
			}
		}
		public int EnduranceLeft { get; private set; }
		public int Type { get; set; }
		public bool IsSelected {
			get { return selected; }
			set {
				if (Type > 0) {
					selected = value;
					UpdateDangerousCells();
				}
			}
		}

		public int visionRange = 3;
		[Range(0, 6)]	public int numberDirection = 6;
		[SerializeField] int endurance = 12;
		[SerializeField] float travelSpeed = 4f;
		[SerializeField] float rotationSpeed = 180f;
		float orientation;
		bool selected;
		HexCell location, currentTravelLocation;
		List<HexCell> pathToTravel, visibleCells;

		void OnEnable() {
			// If there is a recompile
			if (location) {
				transform.localPosition = location.Position;
				if (currentTravelLocation) {
					Grid.IncreaseVisibility(this);
					Grid.DecreaseVisibility(this, currentTravelLocation);
					currentTravelLocation = null;
				}
			}
			EnduranceLeft = endurance;
			UpdateDangerousCells();
		}

		public void ResetEnduranceLeft() {
			EnduranceLeft = endurance;
		}

		public void ValidateLocation() {
			transform.localPosition = location.Position;
		}

		public void Die() {
			if (location) {
				Grid.DecreaseVisibility(this);
			}
			location.Unit = null;
			Destroy(gameObject);
		}

		public void Save(BinaryWriter writer) {
			location.coordinates.Save(writer);
			writer.Write(orientation);
			writer.Write(Type);
		}

		public static void Load(BinaryReader reader, HexGrid grid) {
			HexCoordinates coordinates = HexCoordinates.Load(reader);
			float orientation = reader.ReadSingle();
			int type = reader.ReadInt32();
			grid.AddUnit(grid.GetCell(coordinates), orientation, type);
		}

		public static bool IsValidDestination(HexCell cell) {
			return !cell.IsUnderwater && !cell.Unit && cell.IsExplored;
		}

		public int GetMoveCost(HexCell fromCell, HexCell toCell, HexDirection direction) {
			HexEdgeType edgeType = fromCell.GetEdgeType(toCell);
			if (edgeType == HexEdgeType.Cliff || fromCell.Walled != toCell.Walled) {
				return -1;
			}
			int moveCost;
			if (fromCell.HasRoadThroughEdge(direction)) {
				moveCost = 1;
			} else {
				moveCost = edgeType == HexEdgeType.Flat ? 2 : 3;
				moveCost += toCell.UrbanLevel + toCell.FarmLevel + toCell.PlantLevel;
			}
			return moveCost;
		}

		public void Travel(List<HexCell> path) {
			location.Unit = null;
			location = path[path.Count - 1];
			location.Unit = this;
			pathToTravel = path;
			StopAllCoroutines();
			StartCoroutine(TravelPath());
		}

		IEnumerator TravelPath() {
			if (Type == 0) {
				EnduranceLeft -= pathToTravel[pathToTravel.Count - 1].Distance;
			} else {
				EnduranceLeft = 0;
			}

			Vector3 a, b, c = pathToTravel[0].Position;
			yield return LookAt(pathToTravel[1].Position);
			Grid.DecreaseVisibility(this, currentTravelLocation ? currentTravelLocation : pathToTravel[0]);

			float t = Time.deltaTime * travelSpeed;
			for (int i = 1; i < pathToTravel.Count; i++) {
				currentTravelLocation = pathToTravel[i];
				a = c;
				b = pathToTravel[i - 1].Position;
				if (i < pathToTravel.Count - 1) {
					c = (b + pathToTravel[i].Position) * 0.5f;
				} else {
					c = pathToTravel[i].Position;
				}
				Grid.IncreaseVisibility(this, pathToTravel[i]);
				UpdateDangerousCells();
				TryToSaveHero(currentTravelLocation);
				for (; t < 1f; t += Time.deltaTime * travelSpeed) {
					transform.localPosition = BezierGetPoint(a, b, c, t);
					Vector3 d = BezierGetDerivative(a, b, c, t);
					d.y = 0f; // so they will not lean forward
					transform.localRotation = Quaternion.LookRotation(d);
					yield return null;
				}
				Grid.DecreaseVisibility(this, pathToTravel[i]);
				t -= 1f;
			}
			currentTravelLocation = null;

			transform.localPosition = location.Position;
			orientation = transform.localRotation.eulerAngles.y;
			Grid.IncreaseVisibility(this);
			ListPool<HexCell>.Add(pathToTravel);
			pathToTravel = null;

			OnMovementEnd(this);
		}

		protected virtual void TryToSaveHero(HexCell cell) {
			// do nothing, it is only for HexPlayer
		}

		IEnumerator LookAt(Vector3 point) {
			point.y = transform.localPosition.y;
			Quaternion fromRotation = transform.localRotation;
			Quaternion toRotation = Quaternion.LookRotation(point - transform.localPosition);

			float angle = Quaternion.Angle(fromRotation, toRotation);
			if (angle > 0f) {
				float speed = rotationSpeed / angle;

				for (float t = Time.deltaTime * speed; t < 1f; t += Time.deltaTime * speed) {
					transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, t);
					yield return null;
				}
			}

			transform.LookAt(point);
			orientation = transform.localRotation.eulerAngles.y;
		}

		Vector3 BezierGetPoint(Vector3 a, Vector3 b, Vector3 c, float t) {
			float r = 1f - t;
			return r * r * a + 2f * r * t * b + t * t * c;
		}

		Vector3 BezierGetDerivative(Vector3 a, Vector3 b, Vector3 c, float t) {
			return 2f * ((1f - t) * (b - a) + t * (c - b));
		}

		public void UpdateDangerousCells() {			
			if (Type > 0) {
				// get old list
				if (visibleCells != null) {
					SetDangerousCells(false);
				}
				// construct new list
				visibleCells = Grid.GetVisibleCells(location, visionRange, false, FacingDirection, numberDirection);
				// divide list between old and new

				// set aspect of new list
				if (IsSelected) {
					SetDangerousCells(IsSelected);
				}
			}
		}

		public HexCell GetRandomVisibleCell() {
			// Not for player
			if (Type > 0) {
				HexCell randomCell = null;
				int i = 0;
				do {
					randomCell = visibleCells[Random.Range(0, visibleCells.Count)];
					i++;
				} while (i < 100 && !IsValidDestination(randomCell));
				if (i == 100) {
					return null;
				} else {
					return randomCell;
				}
			} else
				return null;
		}

		void SetDangerousCells(bool isActive) {
			for (int i = 0; i < visibleCells.Count; i++) {
				var cell = visibleCells[i];
				if (isActive && cell.IsExplored) {
					var color = Grid.dangerousGradient.Evaluate((float)i / visibleCells.Count);
					cell.SetFullColor(color);
				} else {
					cell.SetFullColor();
				}
			}
		}
	}
}