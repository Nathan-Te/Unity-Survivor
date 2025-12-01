using UnityEngine;

// Interface commune pour tous les mouvements
public interface IMotionStrategy
{
    void Update(ProjectileController pc, float deltaTime);
}

// 1. Mouvement Standard (Bolt / Nova)
public class LinearMotion : IMotionStrategy
{
    private Vector3 _startPos;
    private float _range;
    private float _speed;
    private bool _isHoming;
    private bool _isHostile;

    public LinearMotion(Vector3 startPos, float range, float speed, bool isHoming, bool isHostile)
    {
        _startPos = startPos;
        _range = range;
        _speed = speed;
        _isHoming = isHoming;
        _isHostile = isHostile;
    }

    public void Update(ProjectileController pc, float dt)
    {
        // Mouvement
        float moveDist = _speed * dt;
        pc.transform.Translate(Vector3.forward * moveDist);

        // Guidage (Homing)
        if (_isHoming && !_isHostile)
        {
            Transform target = EnemyManager.Instance.GetTarget(pc.transform.position, 10f, TargetingMode.Nearest, 0, false);
            if (target != null)
            {
                Vector3 dir = (target.position - pc.transform.position).normalized;
                pc.transform.rotation = Quaternion.Slerp(pc.transform.rotation, Quaternion.LookRotation(dir), 5f * dt);
            }
        }

        // Vérification Portée
        if (Vector3.Distance(_startPos, pc.transform.position) >= _range)
        {
            pc.Despawn();
        }
    }
}

// 2. Mouvement Orbital (Bouclier)
public class OrbitMotion : IMotionStrategy
{
    private float _currentAngle;
    private float _duration;
    private float _speed;
    private int _index;
    private int _totalCount;
    private float _orbitTimer;
    private float _radius;

    public OrbitMotion(float duration, float speed, int index, int count, float radius = 2.0f)
    {
        _duration = duration;
        _speed = speed;
        _index = index;
        _totalCount = count;
        _radius = radius;
        _orbitTimer = 0f;
    }

    public void Update(ProjectileController pc, float dt)
    {
        if (PlayerController.Instance == null)
        {
            pc.Despawn();
            return;
        }

        _orbitTimer += dt;

        // Calcul Position
        float angleSeparation = 360f / _totalCount;
        float baseAngle = angleSeparation * _index;
        float currentRotation = _orbitTimer * _speed * 40f;

        float finalAngleRad = (baseAngle + currentRotation) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(finalAngleRad), 0, Mathf.Sin(finalAngleRad)) * _radius;

        pc.transform.position = PlayerController.Instance.transform.position + Vector3.up + offset;

        // Gestion Durée
        _duration -= dt;
        if (_duration <= 0f)
        {
            pc.Despawn();
        }
    }
}

// 3. Comportement Smite (Météore / Retardement)
public class SmiteMotion : IMotionStrategy
{
    private float _delayTimer;
    private bool _hasExploded = false;

    public SmiteMotion(float delay)
    {
        _delayTimer = delay;
    }

    public void Update(ProjectileController pc, float dt)
    {
        if (_hasExploded) return;

        _delayTimer -= dt;
        if (_delayTimer <= 0f)
        {
            _hasExploded = true;
            // On déclenche l'explosion via le contrôleur
            pc.TriggerSmiteExplosion();
        }
    }
}