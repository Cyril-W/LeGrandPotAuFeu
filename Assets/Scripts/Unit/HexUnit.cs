using LeGrandPotAuFeu.Grid;
using UnityEngine;
using LeGrandPotAuFeu.Utility;
using System.Collections.Generic;
using System.Collections;

namespace LeGrandPotAuFeu.Unit {
	public abstract class HexUnit : MonoBehaviour {
		public UnitStats unitStats;
		public HexGrid Grid { get; set; }
		public abstract HexCell Location { get; set; }
		public float Orientation {
			get {
				return orientation;
			}
			set {
				orientation = value;
				transform.localRotation = Quaternion.Euler(0f, value, 0f);
			}
		}

		protected const float travelSpeed = 4f;
		protected const float rotationSpeed = 180f;

		protected float orientation;
		protected HexCell location, currentTravelLocation;
		protected List<HexCell> pathToTravel;

		public abstract void Die();

		public void ValidateLocation() {
			transform.localPosition = location.Position;
		}

		public bool IsValidDestination(HexCell cell) {
			return cell.IsVisible && cell.IsExplored && !cell.IsUnderwater && !cell.Unit;
		}

		public IEnumerator Travel(List<HexCell> path) {
			location.Unit = null;
			location = path[path.Count - 1];
			location.Unit = this;
			pathToTravel = path;
			StopAllCoroutines();
			yield return TravelPath();
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
				moveCost +=	toCell.UrbanLevel + toCell.FarmLevel + toCell.PlantLevel;
			}
			return moveCost;
		}

		protected abstract IEnumerator TravelPath();

		protected IEnumerator LookAt(Vector3 point) {
			point.y = transform.localPosition.y;
			Quaternion fromRotation = transform.localRotation;
			Quaternion toRotation = Quaternion.LookRotation(point - transform.localPosition);
			float angle = Quaternion.Angle(fromRotation, toRotation);

			if (angle > 0f) {
				float speed = rotationSpeed / angle;
				for (float t = Time.deltaTime * speed; t < 1f; t += Time.deltaTime * speed) {
					transform.localRotation =
						Quaternion.Slerp(fromRotation, toRotation, t);
					yield return null;
				}
			}

			transform.LookAt(point);
			orientation = transform.localRotation.eulerAngles.y;
		}

		public static Vector3 BezierGetDerivative(Vector3 a, Vector3 b, Vector3 c, float t) {
			return 2f * ((1f - t) * (b - a) + t * (c - b));
		}

		public static Vector3 BezierGetPoint(Vector3 a, Vector3 b, Vector3 c, float t) {
			float r = 1f - t;
			return r * r * a + 2f * r * t * b + t * t * c;
		}
	}
}