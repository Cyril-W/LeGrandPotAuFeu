using UnityEngine;

namespace LeGrandPotAuFeu.Utility {
	public enum HexDirection {
		NE, E, SE, SW, W, NW
	}
	public static class HexDirectionExtensions {
		public static HexDirection Opposite(this HexDirection direction) {
			return (int)direction < 3 ? (direction + 3) : (direction - 3);
		}

		public static HexDirection Previous(this HexDirection direction) {
			return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
		}

		public static HexDirection Next(this HexDirection direction) {
			return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
		}

		public static float Angle(this HexDirection direction) {
			switch (direction) {
				case HexDirection.NE:
					return 30;
				case HexDirection.E:
					return 90;
				case HexDirection.SE:
					return 150;
				case HexDirection.SW:
					return 210;
				case HexDirection.W:
					return 270;
				case HexDirection.NW:
					return 330;
				default:
					return 0;
			}
		}

		public static float GetRandomDirection() {
			var hexDirections = System.Enum.GetValues(typeof(HexDirection));
			var randomDirection = (HexDirection)hexDirections.GetValue(Random.Range(0, hexDirections.Length));
			return randomDirection.Angle();
		}
	}
}