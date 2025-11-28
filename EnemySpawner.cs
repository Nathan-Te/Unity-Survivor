using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float spawnRadius = 20f;

    private float _timer;
    private Transform _playerTransform;

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            _playerTransform = PlayerController.Instance.transform;
        }
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        _timer += Time.deltaTime;
        if (_timer >= spawnInterval)
        {
            SpawnEnemy();
            _timer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        // Position aléatoire sur un cercle autour du joueur
        Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPos = _playerTransform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        // TODO : Commenté car désactivé
        // On récupère depuis le Pool au lieu d'Instantiate
        //GameObject enemyObj = EnemyPool.Instance.GetEnemy(spawnPos, Quaternion.identity);

        //// IMPORTANT : On réinitialise les stats de l'ennemi recyclé
        //if (enemyObj.TryGetComponent<EnemyController>(out var controller))
        //{
        //    controller.ResetEnemy();
        //}
    }
}