using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {

	public enum FireMode {Auto, Burst, Single};
	public FireMode fireMode;
	public int burstCount;
	int shotsRemainingInBurst;

	public Transform[] muzzle;
	public Projectile projectile;
	public float msBetweenShots = 100;
	public float muzzleVelocity = 35;

	public Transform shell;
	public Transform shellEjection;
	MuzzleFlash muzzleFlash;

	bool triggerReleasedSinceLastShot;

	float nextShotTime;

	void Start() {
		muzzleFlash.GetComponent<MuzzleFlash> ();
		shotsRemainingInBurst = burstCount;
	}

	void Shoot() {
		if (Time.time > nextShotTime) {

			if (fireMode == FireMode.Burst) {
				if (shotsRemainingInBurst == 0) {
					return;
				}
				shotsRemainingInBurst--;
			} else if (fireMode == FireMode.Burst) {
				if (!triggerReleasedSinceLastShot) {
					return;
				}
			}

			for (int i = 0; i < muzzle.Length; i++) {
				nextShotTime = Time.time + msBetweenShots / 1000;
				Projectile newProjectile = Instantiate (projectile, muzzle[i].position, muzzle[i].rotation)as Projectile;
				newProjectile.SetSpeed (muzzleVelocity);
			}
			Instantiate (shell, shellEjection.position, shellEjection.rotation);
			muzzleFlash.Activate ();
		}
	}

	/*public void Aim(Vector3 aimPoint) {
		transform.LookAt (aimPoint);
	}*/	

	public void OnTriggerHold() {
		Shoot ();
		triggerReleasedSinceLastShot = false;
	}

	public void OnTriggerRelease() {
		triggerReleasedSinceLastShot = true;
		shotsRemainingInBurst = burstCount;
	}
}
