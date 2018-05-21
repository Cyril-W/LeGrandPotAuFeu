using UnityEngine;
using UnityEngine.UI;

namespace LeGrandPotAuFeu.UI {
	public class SaveLoadItem : MonoBehaviour {
		[HideInInspector]
		public SaveLoadMenu menu;
		public Text label;

		public string MapName {
			get {
				return mapName;
			}
			set {
				mapName = value;
				label.text = value;
			}
		}

		string mapName;

		public void Select() {
			menu.SelectItem(mapName);
		}
	}
}