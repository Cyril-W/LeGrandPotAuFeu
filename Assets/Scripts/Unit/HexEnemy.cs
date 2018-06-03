using LeGrandPotAuFeu.Grid;
using UnityEngine;
using System.IO;
using LeGrandPotAuFeu.Utility;
using System.Collections;
using LeGrandPotAuFeu.StateMachine;

namespace LeGrandPotAuFeu.Unit {
	enum EnemyType {
		Leek, Potato, Turnip, Carrot
	}
	public class HexEnemy : HexUnit {
		public int lineVisionRange;
		public int type;
		public bool IsVisible {
			get {
				return visible;
			}
			private set {
				visible = value;
			}
		}
		public override HexCell Location {
			get {
				return location;
			}
			set {
				if (location) {
					location.Unit = null;
				}
				location = value;
				value.Unit = this;
				transform.localPosition = value.Position;
			}
		}
		public StateController stateController { get; private set; }

		bool visible;
		[SerializeField] MeshRenderer[] meshes;

		void OnEnable() {
			stateController = GetComponent<StateController>();
			if (location) {
				transform.localPosition = location.Position;
				if (currentTravelLocation) {
					currentTravelLocation = null;
				}
			}
		}

		public override void Die() {
			location.Unit = null;
			Destroy(gameObject);
		}

		public void UpdateVisibility(HexCell dynamicLocation = null) {
			IsVisible = dynamicLocation ? dynamicLocation.IsVisible : location.IsVisible;
			meshes[type].enabled = IsVisible;

			var cells = Grid.GetAreaVisibleCells(location, areaVisionRange, false);
			cells.AddRange(Grid.GetLineVisibleCells(location, lineVisionRange, facingDirection));
			foreach (var cell in cells) {
				if (IsVisible && cell.IsExplored) {
					cell.EnableHighlight(Grid.enemyColor, false);
				} else {
					cell.DisableUI(false);
				}
			}
		}

		protected override IEnumerator TravelPath() {
			Vector3 a, b, c = pathToTravel[0].Position;
			transform.localPosition = c;
			yield return LookAt(pathToTravel[1].Position);

			float t = Time.deltaTime * travelSpeed;
			for (int i = 1; i < pathToTravel.Count; i++) {
				currentTravelLocation = pathToTravel[i];
				a = c;
				b = pathToTravel[i - 1].Position;
				c = (b + currentTravelLocation.Position) * 0.5f;
				for (; t < 1f; t += Time.deltaTime * travelSpeed) {
					transform.localPosition = BezierGetPoint(a, b, c, t);
					Vector3 d = BezierGetDerivative(a, b, c, t);
					d.y = 0f;
					transform.localRotation = Quaternion.LookRotation(d);
					yield return null;
				}
				UpdateVisibility(pathToTravel[i]);
				t -= 1f;
			}
			currentTravelLocation = null;

			a = c;
			b = location.Position;
			c = b;
			for (; t < 1f; t += Time.deltaTime * travelSpeed) {
				transform.localPosition = BezierGetPoint(a, b, c, t);
				Vector3 d = BezierGetDerivative(a, b, c, t);
				d.y = 0f;
				transform.localRotation = Quaternion.LookRotation(d);
				yield return null;
			}

			transform.localPosition = location.Position;
			orientation = transform.localRotation.eulerAngles.y;
			ListPool<HexCell>.Add(pathToTravel);
			pathToTravel = null;
		}

		public void Save(BinaryWriter writer) {
			location.coordinates.Save(writer);
			writer.Write(orientation);
			writer.Write((byte)type);
		}

		public static void Load(BinaryReader reader, HexGrid grid) {
			HexCoordinates coordinates = HexCoordinates.Load(reader);
			float orientation = reader.ReadSingle();
			int type = reader.ReadByte();
			grid.AddUnit(grid.GetCell(coordinates), orientation, type);
		}
	}
}