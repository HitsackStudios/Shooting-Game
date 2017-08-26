using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.AI;

[RequireComponent (typeof (NavMeshAgent))]
public class Enemy : LivingEntity {

	public enum State {Idle,Chasing,Attacking};
	State currentState;

	public ParticleSystem deathEffect;
	NavMeshAgent pathfinder;
	Transform target;
	Material skinMaterial;
	Color originalcolour;
	LivingEntity targetEntity;

	float attackDistanceThreshold = .5f;
	float timeBetweenAttacks = 1;
	float nextAttackTime;

	float myCollisionRadius;
	float targetCollisionRadius;
	float damage=1;

	bool hasTarget;

	void Awake() {
		pathfinder = GetComponent<NavMeshAgent> ();	

		if (GameObject.FindGameObjectWithTag ("Player") != null) {
			hasTarget = true;

			target = GameObject.FindGameObjectWithTag ("Player").transform;
			targetEntity = target.GetComponent<LivingEntity> (); 

			myCollisionRadius = GetComponent<CapsuleCollider> ().radius;
			targetCollisionRadius = target.GetComponent<CapsuleCollider> ().radius;

		}
	}

	protected override void Start () {
		base.Start ();

		if (hasTarget) {
			currentState = State.Chasing;
			targetEntity.OnDeath += OnTargetDeath;
			StartCoroutine (UpdatePath ());
		}
	}

	public void SetCharacteristics(float moveSpeed,int hitsToKillPlayer,float enemyHealth,Color skinColour) {
		pathfinder.speed = moveSpeed;

		if (hasTarget) {
			damage = Mathf.Ceil( targetEntity.startingHealth / hitsToKillPlayer);
		}

		startingHealth = enemyHealth;

		skinMaterial = GetComponent<Renderer> ().material;
		skinMaterial.color = skinColour;
		originalcolour = skinMaterial.color;
	}

	public override void TakeHit (float damage, Vector3 hitPoint, Vector3 hitDirection)
	{
		if (damage >= health) {
			Destroy(Instantiate (deathEffect.gameObject, hitPoint, Quaternion.FromToRotation (Vector3.forward, hitDirection))as GameObject,deathEffect.startLifetime);
		}
		base.TakeHit (damage, hitPoint, hitDirection);
	}

	void OnTargetDeath() {
		hasTarget = false;
		currentState = State.Idle;
	}

	void Update () {
		if (hasTarget) {
			if (Time.time > nextAttackTime) {
				float sqrDistToTarget = (target.position - transform.position).sqrMagnitude;
				if (sqrDistToTarget < Mathf.Pow (attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2)) {
					nextAttackTime = Time.time + timeBetweenAttacks;
					StartCoroutine (Attack ());
				}
			}
		}
	}

	IEnumerator Attack() {

		currentState = State.Attacking;
		pathfinder.enabled = false;

		Vector3 originalPosition = transform.position;
		Vector3 dirToTarget = (target.position - transform.position).normalized;
		Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);

		float attackSpeed = 3;
		float percent = 0;

		skinMaterial.color = Color.red;
		bool hasAppliedDamage = false;

		while (percent <= 1) {

			if (percent >= 0.5f && !hasAppliedDamage) {
				hasAppliedDamage = true;
				targetEntity.TakeDamage(damage);
			}

			percent += Time.deltaTime * attackSpeed;
			float interpolation = 4 * (-Mathf.Pow(percent,2)+ percent);
			transform.position = Vector3.Lerp (originalPosition, attackPosition, interpolation);

			yield return null;
		}
		skinMaterial.color = originalcolour;
		currentState = State.Chasing;
		pathfinder.enabled = true;
	}

	IEnumerator UpdatePath() {
		float refreshRate = 1;

		while (hasTarget) {
			if (currentState == State.Chasing) {
				Vector3 dirToTarget = (target.position - transform.position).normalized;
				Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2);
				if (!dead) {
					pathfinder.SetDestination (targetPosition);
				}
			}
			yield return new WaitForSeconds (refreshRate);
		}
	}
}
	