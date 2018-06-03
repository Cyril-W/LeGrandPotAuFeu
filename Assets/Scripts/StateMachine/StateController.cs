using LeGrandPotAuFeu.Grid;
using System.Collections.Generic;
using UnityEngine;

namespace LeGrandPotAuFeu.StateMachine {
	public class StateController : MonoBehaviour {
		public bool aiActive = false;		
		public State currentState;
		public State remainState;

		[HideInInspector]
		public HexCell nextDestination;
		[HideInInspector]
		public float stateTimeElapsed;

		Vector3 eyes;

		void Awake() {
			eyes = transform.position + Vector3.up;
		}

		void Update() {
			if (!aiActive)
				return;
			currentState.UpdateState(this);
		}

		void OnDrawGizmos() {
			if (currentState != null) {
				Gizmos.color = currentState.sceneGizmoColor;
				Gizmos.DrawWireSphere(eyes, 1);
			}
		}

		public void TransitionToState(State nextState) {
			if (nextState != remainState) {
				currentState = nextState;
				OnExitState();
			}
		}

		public bool CheckIfCountDownElapsed(float duration) {
			stateTimeElapsed += Time.deltaTime;
			return (stateTimeElapsed >= duration);
		}

		private void OnExitState() {
			stateTimeElapsed = 0;
		}
	}
}