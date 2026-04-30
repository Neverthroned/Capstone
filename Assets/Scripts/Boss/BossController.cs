using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossState
{
    Idle,
    PlasmaAttack,
    BlackHole,
    Invincible,
    FlamethrowerAttack,
    Dead
}

public class BossController : MonoBehaviour
{
    [Header("References")]
    public Plasma plasmaSpawner;
    public BlackHole blackHoleSpawner;
    public Flamethrower flamethrowerController;
    public BossStats bossStats;

    [Header("Attack Timing")]
    public float idleDuration = 2f;
    public float plasmaAttackDuration = 4f;
    public float flamethrowerDuration = 3f;

    [Header("Phase Thresholds")]
    [Range(0f, 1f)] public float phase2Threshold = 0.5f;      // 50% - first black hole
    [Range(0f, 1f)] public float finalBlackHoleThreshold = 0.02f; // 2% - second black hole

    private BossState _currentState = BossState.Idle;
    private bool _inPhase2 = false;
    private bool _firstBlackHoleUsed = false;
    private bool _secondBlackHoleUsed = false;

    private void Start()
    {
        AudioManager.Instance.PlayPhase1Music();
        EnterState(BossState.Idle);
    }

    private void Update()
    {
        // Health threshold checks run every frame regardless of state
        CheckHealthThresholds();
    }

    private void CheckHealthThresholds()
    {
        // Don't interrupt black hole or death states
        if (_currentState == BossState.BlackHole ||
            _currentState == BossState.Invincible ||
            _currentState == BossState.Dead) return;

        float healthPercent = bossStats.GetHealthPercent();

        if (!_firstBlackHoleUsed && healthPercent <= phase2Threshold)
        {
            _firstBlackHoleUsed = true;
            _inPhase2 = true;
            ChangeState(BossState.BlackHole);
            return;
        }

        if (!_secondBlackHoleUsed && _inPhase2 && healthPercent <= finalBlackHoleThreshold)
        {
            _secondBlackHoleUsed = true;
            ChangeState(BossState.BlackHole);
            return;
        }
    }

    public void ChangeState(BossState newState)
    {
        StopAllCoroutines();
        ExitState(_currentState);
        _currentState = newState;
        EnterState(newState);
    }

    private void EnterState(BossState state)
    {
        switch (state)
        {
            case BossState.Idle:
                StartCoroutine(IdleRoutine());
                break;
            case BossState.PlasmaAttack:
                StartCoroutine(PlasmaRoutine());
                break;
            case BossState.BlackHole:
                StartCoroutine(BlackHoleRoutine());
                break;
            case BossState.FlamethrowerAttack:
                StartCoroutine(FlamethrowerRoutine());
                break;
            case BossState.Dead:
                StartCoroutine(DeadRoutine());
                break;
        }
    }

    private void ExitState(BossState state)
    {
        switch (state)
        {
            case BossState.FlamethrowerAttack:
                flamethrowerController.ToggleFlamethrower(false);
                break;
        }
    }

    // Picks next attack based on phase
    private BossState PickNextAttack()
    {
        if (_inPhase2)
        {
            // Alternate between flamethrower and plasma in phase 2
            return Random.Range(0, 2) == 0
                ? BossState.FlamethrowerAttack
                : BossState.PlasmaAttack;
        }
        return BossState.PlasmaAttack;
    }

    private IEnumerator IdleRoutine()
    {
        yield return new WaitForSeconds(idleDuration);
        ChangeState(PickNextAttack());
    }

    private IEnumerator PlasmaRoutine()
    {
        int count = _inPhase2 ? 10 : 5;
        plasmaSpawner.SpawnPlasma(count);
        yield return new WaitForSeconds(plasmaAttackDuration);
        ChangeState(BossState.Idle);
    }

    private IEnumerator BlackHoleRoutine()
    {
        bossStats.SetInvincible(true);
        blackHoleSpawner.SpawnBlackHole();
        AudioManager.Instance.StartBlackHole();

        yield return new WaitUntil(() => BlackHoleProjectile.ActiveBlackHoles.Count == 0);

        bossStats.SetInvincible(false);
        AudioManager.Instance.StopBlackHole();

        if (_inPhase2)
            AudioManager.Instance.PlayPhase2Music();

        if (_secondBlackHoleUsed)
        {
            ChangeState(BossState.Dead);
            yield break; // Changed from return
        }

        ChangeState(BossState.Idle);
    }

    private IEnumerator FlamethrowerRoutine()
    {
        flamethrowerController.ToggleFlamethrower(true);
        yield return new WaitForSeconds(flamethrowerDuration);
        flamethrowerController.ToggleFlamethrower(false);
        ChangeState(BossState.Idle);
    }

    private IEnumerator DeadRoutine()
    {
        // Trigger death animation or sequence here
        Debug.Log("Boss is dead");
        yield return null;
    }
}