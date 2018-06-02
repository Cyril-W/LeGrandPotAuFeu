using LeGrandPotAuFeu.Grid;
using LeGrandPotAuFeu.UI;
using LeGrandPotAuFeu.Unit;
using UnityEngine;

namespace LeGrandPotAuFeu {
	public class GameController : MonoBehaviour {
		public HexGrid grid;
		public HexGameUI gameUI;

		public void OnTurnPlayerTurnFinish() {
			Debug.Log("activate enemies");

			gameUI.OnTurnPlayerTurnBegin();
		}
	}
}
