using UnityEngine;

namespace _Project.Scripts.Gameplay.Enemies.Components
{
    public class EnemyMovement : MonoBehaviour
    {
        private Transform _target;
        private float _speed;

        public void Configure(Transform target, float speed)
        {
            _target = target;
            _speed = Mathf.Max(0, speed);
        }

        public void Tick(float deltaTime)
        {
            if (_target == null || _speed <= 0 || deltaTime <= 0)
                return;

            transform.position = Vector3.MoveTowards(
                transform.position,
                _target.position,
                _speed * deltaTime);
        }

        public void Clear()
        {
            _target = null;
            _speed = 0;
        }
    }
}
