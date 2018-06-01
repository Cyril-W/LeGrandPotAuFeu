using LeGrandPotAuFeu.Grid;
using UnityEngine;
using System.IO;
using LeGrandPotAuFeu.Utility;
using System.Collections.Generic;
using System.Collections;

namespace LeGrandPotAuFeu.Unit {
	public class HexPlayer : HexUnit {
		public override HexCell Location {
			get {
				return location;
			}
			set {
				if (location) {
					Grid.DecreaseVisibility(location, unitStats.visionRange);
					location.Unit = null;
				}
				location = value;
				value.Unit = this;
				Grid.IncreaseVisibility(value, unitStats.visionRange);
				transform.localPosition = value.Position;
			}
		}

		void OnEnable() {
			if (location) {
				transform.localPosition = location.Position;
				if (currentTravelLocation) {
					Grid.IncreaseVisibility(location, unitStats.visionRange);
					Grid.DecreaseVisibility(currentTravelLocation, unitStats.visionRange);
					currentTravelLocation = null;
				}
			}
		}

		public override void Die() {
			if (location) {
				Grid.DecreaseVisibility(location, unitStats.visionRange);
				Grid.ResetExplored();
			}
			location.Unit = null;
			Destroy(gameObject);
		}

		protected override IEnumerator TravelPath() {
			Vector3 a, b, c = pathToTravel[0].Position;
			transform.localPosition = c;
			yield return LookAt(pathToTravel[1].Position);
			Grid.DecreaseVisibility(currentTravelLocation ? currentTravelLocation : pathToTravel[0], unitStats.visionRange);

			float t = Time.deltaTime * travelSpeed;
			for (int i = 1; i < pathToTravel.Count; i++) {
				currentTravelLocation = pathToTravel[i];
				a = c;
				b = pathToTravel[i - 1].Position;
				c = (b + currentTravelLocation.Position) * 0.5f;
				Grid.IncreaseVisibility(pathToTravel[i], unitStats.visionRange);
				for (; t < 1f; t += Time.deltaTime * travelSpeed) {
					transform.localPosition = BezierGetPoint(a, b, c, t);
					Vector3 d = BezierGetDerivative(a, b, c, t);
					d.y = 0f;
					transform.localRotation = Quaternion.LookRotation(d);					
					yield return null;
				}
				Grid.DecreaseVisibility(pathToTravel[i], unitStats.visionRange);
				t -= 1f;
			}
			currentTravelLocation = null;

			a = c;
			b = location.Position;
			c = b;
			Grid.IncreaseVisibility(location, unitStats.visionRange);
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
		}

		public static void Load(BinaryReader reader, HexGrid grid) {
			HexCoordinates coordinates = HexCoordinates.Load(reader);
			float orientation = reader.ReadSingle();
			grid.AddUnit(grid.GetCell(coordinates), orientation, -1);
		}
	}
}