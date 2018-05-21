using LeGrandPotAuFeu.Grid;
using UnityEngine;
using System.IO;
using LeGrandPotAuFeu.Utility;

namespace LeGrandPotAuFeu.Unit {
	public class HexUnit : MonoBehaviour {
		public HexCell Location {
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
		HexCell location;

		public float Orientation {
			get {
				return orientation;
			}
			set {
				orientation = value;
				transform.localRotation = Quaternion.Euler(0f, value, 0f);
			}
		}
		float orientation;

		public static HexUnit unitPrefab;

		public void ValidateLocation() {
			transform.localPosition = location.Position;
		}

		public void Die() {
			location.Unit = null;
			Destroy(gameObject);
		}

		public bool IsValidDestination(HexCell cell) {
			return !cell.IsUnderwater && !cell.Unit;
		}

		public void Save(BinaryWriter writer) {
			location.coordinates.Save(writer);
			writer.Write(orientation);
		}

		public static void Load(BinaryReader reader, HexGrid grid) {
			HexCoordinates coordinates = HexCoordinates.Load(reader);
			float orientation = reader.ReadSingle();
			grid.AddUnit(Instantiate(unitPrefab), grid.GetCell(coordinates), orientation);
		}
	}
}