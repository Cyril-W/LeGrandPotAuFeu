using UnityEngine;

namespace LeGrandPotAuFeu.StateMachine {
	public abstract class Decision : ScriptableObject {
		public abstract bool Decide(StateController controller);
	}
}