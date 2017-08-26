using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

	public bool devMode;

	public Wave[] waves;
	public Enemy enemy;

	LivingEntity playerEntity;
	Transform playerT;

	Wave currentWave;
	int currentWaveNumber;

	int enemiesRemainingToSpawn;
	int enemiesRemainingAlive;
	float nextSpawnTime;

	float timeBetweenCampingChecks=2;
	float campThresholdDistance=1.5f;
	float nextCampingCheckTime;
	Vector3 campPostitionOld;
	bool isCamping;
	bool isDisabled;

	MapGenerator map;

	public event System.Action<int> OnNewWave;

	void Start() {
		playerEntity = FindObjectOfType<Player> ();
		playerT = playerEntity.transform;

		nextCampingCheckTime = timeBetweenCampingChecks + Time.time;
		campPostitionOld = playerT.position;
		playerEntity.OnDeath += OnPlayerDeath;

		map = FindObjectOfType<MapGenerator> ();
		NextWave ();
	}

	void Update() {
		if (!isDisabled) {
			if (Time.time > nextCampingCheckTime) {
				nextCampingCheckTime = Time.time + timeBetweenCampingChecks;

				isCamping = Vector3.Distance (playerT.position, campPostitionOld) < campThresholdDistance;
				campPostitionOld = playerT.position;
			}

			if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime) {
				enemiesRemainingToSpawn--;
				nextSpawnTime = currentWave.timeBetweenSpawns;
			
				StartCoroutine ("SpawnEnemy");
			}
		}

		if (devMode) {
			if (Input.GetKeyDown (KeyCode.Backspace)) {
				StopCoroutine ("SpawnEnemy");
				foreach (Enemy enemy in FindObjectsOfType<Enemy>()) {
					GameObject.Destroy (enemy.gameObject);
				}
				NextWave ();
			}
		}
	}

	IEnumerator SpawnEnemy () {
		float spawnDelay = 1;
		float tileFlashSpeed = 4;

		Transform spawnTile = map.GetRandomOpenTile ();
		if (isCamping) {
			spawnTile = map.GetTileFromPosition (playerT.position);
		}
		Material tileMat = spawnTile.GetComponent<Renderer> ().material;
		Color initialColor = Color.white;
		Color flashColor = Color.red;
		float spawnTimer = 0;

		while (spawnTimer < spawnDelay) {

			tileMat.color = Color.Lerp (initialColor, flashColor, Mathf.PingPong (spawnTimer * tileFlashSpeed, 1));

			spawnTimer += Time.deltaTime;
			yield return null;
		}

		Enemy spawnedEnemy = Instantiate (enemy, spawnTile.position + Vector3.up, Quaternion.identity)as Enemy;
		spawnedEnemy.OnDeath += OnEnemyDeath;
		spawnedEnemy.SetCharacteristics (currentWave.moveSpeed, currentWave.hasToKillPlayer, currentWave.enemyHealth, currentWave.skinColour);
	}

	void OnPlayerDeath() {
		isDisabled = true;
	}

	void OnEnemyDeath() {
		enemiesRemainingAlive--;

		if (enemiesRemainingAlive == 0) {
			NextWave ();		
		}
	}

	void ResetPlayerPosition () {
		playerT.position = map.GetTileFromPosition (Vector3.zero).position + Vector3.up * 3;
	}

	void NextWave() {
		currentWaveNumber++;

		if (currentWaveNumber - 1 < waves.Length) {
			currentWave = waves [currentWaveNumber - 1];
		
			enemiesRemainingToSpawn = currentWave.enemyCount;
			enemiesRemainingAlive = enemiesRemainingToSpawn;

			if (OnNewWave != null) {
				OnNewWave (currentWaveNumber);
			}
			ResetPlayerPosition ();
		}
	}

	[System.Serializable]
	public class Wave {
		public bool infinte;
		public int enemyCount;
		public float timeBetweenSpawns;

		public float moveSpeed;
		public int hasToKillPlayer;
		public float enemyHealth;
		public Color skinColour;
	}
}
