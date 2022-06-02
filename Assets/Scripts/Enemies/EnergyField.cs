using System.Collections.Generic;
using UnityEngine;
using DigitalRuby.LightningBolt;

using Core;
using Audio;
using Damage;
using Player;

namespace Enemies {

    public class EnergyField : MonoBehaviour {

        [SerializeField] DamageClass disruptor;
        [SerializeField][Range(0f, 10f)] float damageMultiplier = 1f;
        [SerializeField][Range(0f, 15f)] float effectiveDistance = 5f;
        [SerializeField][Range(0f, 5f)] float activationDelay = 1f;

        [Space]

        [SerializeField] EnemyShip poleA;
        [SerializeField] EnemyShip poleB;
        [SerializeField] LightningBoltScript boltA;
        [SerializeField] LightningBoltScript boltB;
        [SerializeField] BoxCollider2D box;
        [SerializeField] Transform beamCenter;

        [Space]

        [SerializeField] Gradient activeBoltA;
        [SerializeField] Gradient activeBoltB;

        [Space]

        [SerializeField] List<Collider2D> ignoreColliders = new List<Collider2D>();

        [Space]

        [SerializeField] LoopableSound fieldNoise;

        LineRenderer lineA;
        LineRenderer lineB;
        Gradient initialGradientA;
        Gradient initialGradientB;

        int numResults = 0;
        int numHits = 0;
        RaycastHit2D[] hits = new RaycastHit2D[20];
        DamageableBehaviour target;
        DamageReceiver actor;
        PlayerInputHandler inputHandler;

        float t = 0;

        void Start() {
            fieldNoise.Init(this);
            lineA = boltA.GetComponent<LineRenderer>();
            lineB = boltB.GetComponent<LineRenderer>();
            initialGradientA = lineA.colorGradient;
            initialGradientB = lineB.colorGradient;
            IgnoreColliders();
        }

        void Update() {
            if (CheckIsLiving()) {
                HandleUpdateBox();
                HandleBeamIntersection();
                TurnOnBeam();
            } else {
                TurnOffBeam();
                KillPoles();
            }
            t += Time.deltaTime;
        }

        void HandleUpdateBox() {
            if (poleA == null || poleB == null) return;
            beamCenter.position = (poleA.transform.position + poleB.transform.position) * 0.5f;
            box.transform.position = beamCenter.position;
            box.size = new Vector2(Vector2.Distance(poleA.transform.position, poleB.transform.position), box.size.y);
            box.transform.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, poleB.transform.position - poleA.transform.position));
        }

        void HandleBeamIntersection() {
            if (t < activationDelay) return;
            numHits = 0;
            numResults = box.Cast(Vector2.right, hits, 0f, true);
            for (int i = 0; i < numResults; i++) {
                if (CheckForHit(hits[i])) numHits++;
            }

            if (HasTarget()) {
                DamageActor(target);
            } else {
                SetTarget(null);
            }

            if (numHits > 0 || HasTarget()) {
                fieldNoise.Play();
                lineA.colorGradient = activeBoltA;
                lineB.colorGradient = activeBoltB;
            } else {
                fieldNoise.Stop();
                lineA.colorGradient = initialGradientA;
                lineB.colorGradient = initialGradientB;
            }
        }

        bool CheckForHit(RaycastHit2D hit) {
            if (hit.collider == null) return false;
            if (hit.collider.tag == UTag.EnergyField) return false;
            foreach (var ignoreCollider in ignoreColliders) if (ignoreCollider == hit.collider) return false;
            actor = hit.collider.GetComponent<DamageReceiver>();
            if (actor == null) return false;
            if (actor.root == null) return false;
            if (actor.root == target) return false;

            if (!HasTarget() && actor.root.tag == UTag.Player) {
                SetTarget(actor.root);
                // TakeOverPlayerControl();
                return true;
            }

            DamageActor(actor.root);
            return true;
        }

        void SetTarget(DamageableBehaviour newTarget = null) {
            target = newTarget;
            if (target != null) {
                boltA.EndObject = target.gameObject;
                boltB.StartObject = target.gameObject;
            } else {
                boltA.EndObject = poleB.gameObject;
                boltB.StartObject = poleA.gameObject;
            }
        }

        // void TakeOverPlayerControl() {
        //     if (target == null) return;
        //     inputHandler = target.GetComponent<PlayerInputHandler>();
        //     if (inputHandler == null) return;
        //     inputHandler.SetMode(PlayerInputControlMode.GameBrain);
        //     inputHandler.SetAutoMoveTarget(beamCenter);
        // }

        // void RelinquishPlayerControl() {
        //     if (inputHandler == null) return;
        //     inputHandler.SetMode(PlayerInputControlMode.Player);
        // }

        void DamageActor(DamageableBehaviour receiver) {
            if (receiver == null) return;
            receiver.TakeDamage(disruptor.baseDamage * damageMultiplier * Time.deltaTime, DamageType.Disruptor);
        }

        void TurnOnBeam() {
            if (t < activationDelay) return;
            boltA.gameObject.SetActive(true);
            boltB.gameObject.SetActive(true);
        }

        void TurnOffBeam() {
            // RelinquishPlayerControl();
            boltA.gameObject.SetActive(false);
            boltB.gameObject.SetActive(false);
            fieldNoise.Stop();
        }

        void KillPoles() {
            if (poleA != null && poleA.isAlive) poleA.TakeDamage(1000f, DamageType.Instakill);
            if (poleB != null && poleB.isAlive) poleB.TakeDamage(1000f, DamageType.Instakill);
        }

        bool HasTarget() {
            return target != null && target.isAlive && Vector2.Distance(target.transform.position, beamCenter.position) < effectiveDistance;
        }

        bool CheckIsLiving() {
            if (poleA == null || !poleA.isActiveAndEnabled || !poleA.isAlive) return false;
            if (poleB == null || !poleB.isActiveAndEnabled || !poleB.isAlive) return false;
            return true;
        }

        void IgnoreColliders() {
            foreach (var ignoreCollider in ignoreColliders) {
                Physics2D.IgnoreCollision(box, ignoreCollider);
            }
        }
    }
}
