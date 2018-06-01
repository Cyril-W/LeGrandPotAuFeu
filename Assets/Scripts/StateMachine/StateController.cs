using LeGrandPotAuFeu.Unit;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LeGrandPotAuFeu.StateMachine {
	public class StateController : MonoBehaviour {
		public bool aiActive;
		public UnitStats unitStats;
		public State currentState;
		public State remainState;

		[HideInInspector]
		public NavMeshAgent navMeshAgent;
		[HideInInspector]
		public List<Transform> wayPointList;
		[HideInInspector]
		public int nextWayPoint;
		[HideInInspector]
		public Transform chaseTarget;
		[HideInInspector]
		public float stateTimeElapsed;

		Vector3 eyes;

		void Awake() {
			navMeshAgent = GetComponent<NavMeshAgent>();
			eyes = transform.position + Vector3.up;
		}

		public void SetupAI(bool aiActivationFromTankManager, List<Transform> wayPointsFromTankManager) {
			wayPointList = wayPointsFromTankManager;
			aiActive = aiActivationFromTankManager;
			if (aiActive) {
				navMeshAgent.enabled = true;
			} else {
				navMeshAgent.enabled = false;
			}
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