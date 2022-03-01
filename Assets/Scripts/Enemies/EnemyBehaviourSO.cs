using System.Collections.Generic;
using UnityEngine;

namespace Enemies {

    // DEPRECATED
    public class EnemyBehaviourSO
    // ,ISerializationCallbackReceiver
    {
        enum Mode {
            Static,
            SpawnLaunch,
            FollowPath,
            Kamikaze,
            AttackPlayerFromDistance,
            Retreat,
        }

        [SerializeField] Mode mode;
        [SerializeField] Transform path;

        [SerializeField] Transform spawnLocation;
        [SerializeField] Vector3 spawnHeading = Vector3.down;

        [SerializeField][Range(0f, 10f)] float attackDistance = 0f;

        Transform GetPath() {
            switch (mode)
            {
                case Mode.FollowPath:
                case Mode.Retreat:
                    return path;
                default:
                    throw new UnityException("Invalid action for mode: " + mode);
            }
        }

        Transform GetSpawnLocation() {
            switch (mode)
            {
                case Mode.AttackPlayerFromDistance:
                case Mode.SpawnLaunch:
                case Mode.Kamikaze:
                    return spawnLocation;
                default:
                    throw new UnityException("Invalid action for mode: " + mode);
            }
        }

        Vector3 GetSpawnHeading() {
            switch (mode)
            {
                case Mode.AttackPlayerFromDistance:
                case Mode.SpawnLaunch:
                case Mode.Kamikaze:
                    return spawnHeading;
                default:
                    throw new UnityException("Invalid action for mode: " + mode);
            }
        }

        float GetAttackDistance() {
            switch (mode)
            {
                case Mode.AttackPlayerFromDistance:
                    return attackDistance;
                case Mode.SpawnLaunch:
                case Mode.Kamikaze:
                case Mode.FollowPath:
                case Mode.Retreat:
                case Mode.Static:
                default:
                    throw new UnityException("Invalid action for mode: " + mode);
            }
        }
        // void MaybeNullifyPath() {
        //     switch (mode)
        //     {
        //         case Mode.Pathfollower:
        //         case Mode.Retreat:
        //             break;
        //         case Mode.SpawnLaunch:
        //         case Mode.Kamikaze:
        //         case Mode.AttackPlayerFromDistance:
        //         case Mode.Static:
        //         default:
        //             path = null;
        //             break;
        //     }
        // }

        // void MaybeNullifySpawn() {
        //     switch (mode)
        //     {
        //         case Mode.AttackPlayerFromDistance:
        //         case Mode.SpawnLaunch:
        //         case Mode.Kamikaze:
        //             break;
        //         case Mode.Pathfollower:
        //         case Mode.Retreat:
        //         case Mode.Static:
        //         default:
        //             spawnLocation = null;
        //             spawnHeading = Vector3.down;
        //             break;
        //     }
        // }

        // void MaybeNullifyAttackDistance() {
        //     switch (mode)
        //     {
        //         case Mode.AttackPlayerFromDistance:
        //             break;
        //         case Mode.SpawnLaunch:
        //         case Mode.Kamikaze:
        //         case Mode.Pathfollower:
        //         case Mode.Retreat:
        //         case Mode.Static:
        //         default:
        //             attackDistance = 0f;
        //             break;
        //     }
        // }

        // public void OnAfterDeserialize() {
        //     MaybeNullifyPath();
        //     MaybeNullifySpawn();
        // }
        // public void OnBeforeSerialize() {}
    }
}

