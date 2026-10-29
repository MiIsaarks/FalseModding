using EntityStates;
using EntityStates.AffixVoid;
using HG;
using RoR2;
using RoR2.Projectile;
using RoR2BepInExPack.GameAssetPathsBetter;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ForgottenSkillsTweaks
{
    public sealed class TazerRicochetController : NetworkBehaviour
    {
        public int bouncesRemaining = 3;

        private static GameObject tazerPrefab;
        public float bounceRange = 50f;

        [SystemInitializer]
        public static void init()
        {
            tazerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainTazer.prefab").WaitForCompletion();

            if (tazerPrefab != null)
            {
                if (!tazerPrefab.GetComponent<TazerRicochetController>())
                {
                    tazerPrefab.AddComponent<TazerRicochetController>();
                }
            }
           

            On.RoR2.Projectile.ProjectileImpactExplosion.OnProjectileImpact += (orig, self, impactInfo) =>
            {
                if (NetworkServer.active
                && self.TryGetComponent(out TazerRicochetController ricochet)
                && impactInfo.collider
                && impactInfo.collider.TryGetComponent(out HurtBox hurtBox)
                && hurtBox.healthComponent)
                {
                    ricochet.TryRedirectTazer(hurtBox.healthComponent.gameObject, impactInfo.estimatedPointOfImpact);
                }

                orig(self, impactInfo);
            };

        }

        private ProjectileController _projectileController;
        private ProjectileDamage _projectileDamage;

        private float _startBounceTimer = -1f;

        private static int _pendingBounces = -1;
        private static List<GameObject> _pendingBounced;
        private GameObject _lastHitObject;

        private readonly List<GameObject> _bouncedObjects = new List<GameObject>();

        private readonly BullseyeSearch _bounceTargetSearch = new BullseyeSearch
        {
            queryTriggerInteraction = QueryTriggerInteraction.Ignore,
            filterByDistinctEntity = true,
            filterByLoS = true,
            sortMode = BullseyeSearch.SortMode.Distance,
            maxAngleFilter = 360f,
            teamMaskFilter = TeamMask.all
        };

        private void Awake()
        {
            _projectileController = GetComponent<ProjectileController>();
            _projectileDamage = GetComponent<ProjectileDamage>();

            _projectileController.onInitialized += onInitialized;

            if (_pendingBounces >= 0)
            {
                bouncesRemaining = _pendingBounces;
                _bouncedObjects.AddRange(_pendingBounced);
                _pendingBounces = -1;
                _pendingBounced = null;
            }
        }

        private void onInitialized(ProjectileController projectileController)
        {
            if (projectileController.owner)
            {
                _bounceTargetSearch.teamMaskFilter = TeamMask.GetEnemyTeams(TeamComponent.GetObjectTeam(projectileController.owner));
            }
        }

       

        [Server]
        private void TryRedirectTazer(GameObject hitObject, Vector3 origin)
        {
            if(bouncesRemaining <= 0 || !hitObject)
            {
                return;
            }

            _bouncedObjects.Add(hitObject);

            Vector3 searchOrigin = origin;
            if(hitObject.TryGetComponent(out CharacterBody body))
            {
                searchOrigin = body.corePosition;
            }
            _bounceTargetSearch.searchOrigin = searchOrigin;
            _bounceTargetSearch.searchDirection = transform.forward;
            _bounceTargetSearch.maxDistanceFilter = bounceRange;
            _bounceTargetSearch.RefreshCandidates();

            GameObject targetObject = null;
            Vector3 targetPosition = Vector3.zero;

            foreach (HurtBox hurtBox in _bounceTargetSearch.GetResults())
            {
                HealthComponent targetHealthComponent = hurtBox ? hurtBox.healthComponent : null;
                if (targetHealthComponent && !_bouncedObjects.Contains(targetHealthComponent.gameObject))
                {
                    targetObject = targetHealthComponent.gameObject;
                    targetPosition = targetHealthComponent.body ? targetHealthComponent.body.corePosition : hurtBox.transform.position;
                    _pendingBounces = bouncesRemaining - 1;
                    _pendingBounced = new List<GameObject>(_bouncedObjects);


                    

                    Vector3 direction = (targetPosition - origin).normalized;

                    FireProjectileInfo ricochetInfo = new FireProjectileInfo
                    {
                        projectilePrefab = tazerPrefab,
                        position = origin + direction * 1f,
                        rotation = Util.QuaternionSafeLookRotation(direction),
                        owner = _projectileController.owner,
                        damage = _projectileDamage.damage,
                        force = _projectileDamage.force,
                        crit = _projectileDamage.crit,
                        procChainMask = _projectileController.procChainMask,
                        damageTypeOverride = new DamageTypeCombo(DamageType.Shock5s, DamageTypeExtended.Electrical, DamageSource.Secondary),
                    };

                    ProjectileManager.instance.FireProjectile(ricochetInfo);
                    return;
                }
            }

           
        }
    }
}    