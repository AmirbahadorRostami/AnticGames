using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TacticalGame.Grid;

namespace TacticalGame.Units.Types
{
    /// <summary>
    /// Enemy unit that uses the grid to efficiently find and intercept units.
    /// </summary>
    public class EnemyUnit : BaseUnit
    {
        [Header("Enemy-Specific Settings")]
        [SerializeField] private float searchRadius = 8f;
        [SerializeField] private float attackRange = 1.0f;
        [SerializeField] private float attackDamage = 50f;
        [SerializeField] private float attackCooldown = 0.5f;
        
        private BaseUnit currentTarget;
        private float attackTimer;
        private bool isAttacking = false;

        protected override void Start()
        {
            base.Start();
            isMoving = false;
            InvokeRepeating("FindNewTarget", 0.5f, 1.0f);
        }

        protected override void Update()
        {
            base.Update();
            
            if (currentTarget != null && !isAttacking)
            {
                float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
                
                if (distance <= attackRange)
                {
                    StopMoving();
                    LookAtTarget();
                    
                    attackTimer -= Time.deltaTime;
                    if (attackTimer <= 0)
                    {
                        Attack();
                        attackTimer = attackCooldown;
                    }
                }
                else
                {
                    StartMoving();
                }
            }
        }

        private void LookAtTarget()
        {
            if (currentTarget == null) return;
            
            Vector3 targetDirection = currentTarget.transform.position - transform.position;
            targetDirection.y = 0;
            if (targetDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    Quaternion.LookRotation(targetDirection), 
                    Time.deltaTime * 10f
                );
            }
        }
        
        private void FindNewTarget()
        {
            if (isAttacking) return;
            
            if (GridManager.Instance == null || GridManager.Instance.Grid == null)
                return;
                
            List<IGridEntity> nearbyEntities = GridManager.Instance.Grid.GetEntitiesInRadius(transform.position, searchRadius);
            
            BaseUnit bestTarget = null;
            float closestDistanceToFlag = float.MaxValue;
            
            foreach (IGridEntity entity in nearbyEntities)
            {
                if (entity.EntityType == EntityType.Enemy || entity.EntityType == EntityType.Flag)
                    continue;
                
                if (!(entity is BaseUnit unitEntity))
                    continue;
                
                if (flagTransform != null)
                {
                    float distanceToFlag = Vector3.Distance(unitEntity.transform.position, flagTransform.position);
                    if (distanceToFlag < closestDistanceToFlag)
                    {
                        closestDistanceToFlag = distanceToFlag;
                        bestTarget = unitEntity;
                    }
                }
            }
            
            if (bestTarget != null)
            {
                SetUnitTarget(bestTarget);
            }
            else if (flagTransform != null)
            {
                currentTarget = null;
                SetTarget(flagTransform);
                StartMoving();
            }
        }
        
        private void SetUnitTarget(BaseUnit target)
        {
            currentTarget = target;
            SetTarget(target.transform);
            isAttacking = true;
            StartMoving();
            
            if (eventManager != null)
            {
                eventManager.EnemyTargetingUnit(gameObject, target.gameObject);
            }
        }
        
        private void Attack()
        {
            if (currentTarget == null)
                return;
                
            currentTarget.TakeDamage(attackDamage);
            StartCoroutine(AttackAnimation());
        }
        
        private IEnumerator AttackAnimation()
        {
            Vector3 startPos = transform.position;
            Vector3 lungePos = startPos + transform.forward * 0.5f;
            
            float duration = 0.15f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                transform.position = Vector3.Lerp(startPos, lungePos, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            elapsed = 0f;
            
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                transform.position = Vector3.Lerp(lungePos, startPos, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            {
                isAttacking = false;
                if (eventManager != null)
                {
                    eventManager.EnemyIdle(gameObject);
                }
            }
        }
    }
}