using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ForgottenSkillsTweaks
{

    internal sealed class TazerBounceChain
    {
        public int bouncesRemaining;
        public float range = 50f;
        public bool filterByLoS = true;

        public readonly List<HealthComponent> bouncedObjects = new List<HealthComponent>();
        public readonly HashSet<HealthComponent> damaged = new HashSet<HealthComponent>();

        private BullseyeSearch search;


        public HurtBox PickNextTarget(Vector3 position, TeamIndex ownerTeam)
        {
            if (search == null)
                search = new BullseyeSearch();

            search.searchOrigin = position;
            search.searchDirection = Vector3.zero;
            search.teamMaskFilter = TeamMask.allButNeutral;
            search.teamMaskFilter.RemoveTeam(ownerTeam);
            search.filterByLoS = filterByLoS;
            search.sortMode = BullseyeSearch.SortMode.Distance;
            search.maxDistanceFilter = range;
            search.RefreshCandidates();

            foreach (HurtBox hurtBox in search.GetResults())
            {
                HealthComponent hc = hurtBox ? hurtBox.healthComponent : null;
                if (hc && !bouncedObjects.Contains(hc) && !damaged.Contains(hc))
                {
                    bouncedObjects.Add(hc);
                    return hurtBox;
                }
            }
            return null;
        }
    }

    public sealed class TazerRicochetController : NetworkBehaviour
    {
        public int bounces = 2;
        public float bounceRange = 50f;

        private static GameObject tazerPrefab;
        private static TazerBounceChain _pendingChain;

        internal TazerBounceChain chain;
        private ProjectileController _projectileController;

        [SystemInitializer]
        public static void init()
        {
            tazerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainTazer.prefab").WaitForCompletion();

            if (tazerPrefab && !tazerPrefab.GetComponent<TazerRicochetController>())
                tazerPrefab.AddComponent<TazerRicochetController>();

            On.RoR2.Projectile.ProjectileImpactExplosion.OnProjectileImpact += (orig, self, impactInfo) =>
            {
                if (!NetworkServer.active
                    || !self.TryGetComponent(out TazerRicochetController ricochet)
                    || ricochet.chain == null
                    || !impactInfo.collider
                    || !impactInfo.collider.TryGetComponent(out HurtBox hurtBox)
                    || !hurtBox.healthComponent)
                {
                    orig(self, impactInfo);
                    return;
                }

                HealthComponent hitHealth = hurtBox.healthComponent;


                if (ricochet.chain.damaged.Contains(hitHealth))
                    return;


                Vector3 origin = hurtBox.transform.position;
                ProjectileDamage pd = self.GetComponent<ProjectileDamage>();
                float damage = pd.damage;
                float force = pd.force;
                bool crit = pd.crit;
                ProcChainMask procChainMask = ricochet._projectileController.procChainMask;
                GameObject owner = ricochet._projectileController.owner;
                TazerBounceChain chain = ricochet.chain;

                orig(self, impactInfo);


                if (!chain.bouncedObjects.Contains(hitHealth))
                    chain.bouncedObjects.Add(hitHealth);
                TryBounce(chain, origin, damage, force, crit, procChainMask, owner);
            };


            On.RoR2.HealthComponent.TakeDamage += (orig, self, damageInfo) =>
            {
                if (NetworkServer.active
                    && damageInfo != null
                    && damageInfo.inflictor
                    && damageInfo.inflictor.TryGetComponent(out TazerRicochetController ricochet)
                    && ricochet.chain != null
                    && !ricochet.chain.damaged.Add(self))
                {
                    return;
                }

                orig(self, damageInfo);
            };
        }

        private void Awake()
        {
            _projectileController = GetComponent<ProjectileController>();

            if (_pendingChain != null)
            {
                chain = _pendingChain;
                _pendingChain = null;
            }
            else
            {
                chain = new TazerBounceChain { bouncesRemaining = bounces, range = bounceRange };
            }
        }


        private static void TryBounce(TazerBounceChain chain, Vector3 origin, float damage, float force,
                                      bool crit, ProcChainMask procChainMask, GameObject owner)
        {
            if (chain.bouncesRemaining <= 0)
                return;

            HurtBox next = chain.PickNextTarget(origin, TeamComponent.GetObjectTeam(owner));
            if (!next)
                return;

            Vector3 targetPosition = next.healthComponent.body
                ? next.healthComponent.body.corePosition
                : next.transform.position;
            Vector3 direction = (targetPosition - origin).normalized;

            chain.bouncesRemaining--;
            _pendingChain = chain;

            ProjectileManager.instance.FireProjectile(new FireProjectileInfo
            {
                projectilePrefab = tazerPrefab,
                position = origin + direction * 1f,
                rotation = Util.QuaternionSafeLookRotation(direction),
                owner = owner,
                damage = damage,
                force = force,
                crit = crit,
                procChainMask = procChainMask,
                damageTypeOverride = new DamageTypeCombo(DamageType.Shock5s, DamageTypeExtended.Electrical, DamageSource.Secondary),
            });

            _pendingChain = null;
        }
    }
}