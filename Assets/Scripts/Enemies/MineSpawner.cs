using System.Collections;
using System.Collections.Generic;
using Audio;
using Core;
using UnityEngine;

namespace Enemies {

    public class MineSpawner : MonoBehaviour {
        [Header("Minespawner")]
        [Space]
        [SerializeField][Range(0f, 10f)] float initialDelay = 3f;
        [SerializeField][Range(0f, 10f)] float spawnInterval = 3f;
        [SerializeField][Range(0f, 10f)] float spawnVariance = 0.4f;

        [Header("Audio")]
        [Space]
        [SerializeField] Sound openSound;
        [SerializeField] Sound spawnSound;

        // cached
        EnemyShip enemy;
        Animator anim;

        public void PlayOpenSound() {
            openSound.Play();
        }

        public void PlaySpawnSound() {
            spawnSound.Play();
        }

        void Awake() {
            enemy = GetComponent<EnemyShip>();
            anim = GetComponentInChildren<Animator>();
        }

        void Start() {
            openSound.Init(this);
            spawnSound.Init(this);
            StartCoroutine(ISpawn());
        }

        void TriggerSpawn() {
            anim.SetTrigger("SpawnMine");
        }

        IEnumerator ISpawn() {
            yield return new WaitForSeconds(initialDelay);
            while (enemy.isAlive) {
                TriggerSpawn();
                yield return new WaitForSeconds(Utils.RandomVariance(spawnInterval, spawnVariance, spawnInterval / 2f));
            }
        }
    }
}

