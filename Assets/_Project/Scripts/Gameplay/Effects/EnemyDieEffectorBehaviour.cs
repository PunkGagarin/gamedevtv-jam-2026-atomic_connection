using _Project.Scripts.Gameplay.Enemies;
using _Project.Scripts.Gameplay.Enemies.Components;
using UnityEngine;

public class EnemyDieEffectorBehaviour : MonoBehaviour
{
    public EnemyVitality enemyVitality;

    // TODO 1: вынести в сервис управления эффектами
    private const string DIE_EFFECT_PREFAB_PATH = "Test/EnemyDie";

    private void OnKilled(EnemyUnit unit)
    {
        GameObject prefab = Resources.Load<GameObject>(DIE_EFFECT_PREFAB_PATH);

        Instantiate(prefab, transform.position, Quaternion.identity);
    }

    private void Start()
    {
        enemyVitality.Killed += OnKilled;
    }
    
    private void OnDestroy() {
        enemyVitality.Killed -= OnKilled;
    }
}
