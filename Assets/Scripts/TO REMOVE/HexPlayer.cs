//using LeGrandPotAuFeu.Grid;
//using System;
//using System.Collections.Generic;
//using UnityEngine;

//namespace LeGrandPotAuFeu.Unit {
//	enum HexHeroType {
//		Ranger, Mage, Elf, Barbarian, Thief, Paladin, Minstrel, Dwarf, Ogre
//	}
//	public class HexPlayer : HexUnit {
//		public delegate void SaveHero(HexCell cell, string heroType);
//		public static event SaveHero OnHeroSaved;

//		Dictionary<string, HexCell> heroesLocation = new Dictionary<string, HexCell>(); // could be replaced with int index rather than hexcell
//		[SerializeField] GameObject[] heroMeshes;

//		private void Awake() {
//			heroesLocation.Clear();
//			foreach (var heroType in Enum.GetNames(typeof(HexHeroType))) {
//				heroesLocation.Add(heroType, null);
//			}
//		}

//		public void UpdateCellWithHero(string heroType, HexCell cell) {
//			heroesLocation[heroType] = cell;
//		}

//		public HexCell GetCellWithHero(string heroType) {
//			HexCell cell;
//			if (heroesLocation.TryGetValue(heroType, out cell) && cell) {
//				return cell;
//			} else {
//				return null;
//			}
//		}

//		public void UpdateHeroDisplay(int index, bool isActive) {
//			heroMeshes[index].SetActive(isActive);
//		}

//		protected override void TryToSaveHero(HexCell cell) {
//			if (cell.SpecialIndex > 0) {
//				var heroType = ((HexHeroType)cell.SpecialIndex).ToString();
//				OnHeroSaved(cell, heroType);
//			}
//		}
//	}
//}