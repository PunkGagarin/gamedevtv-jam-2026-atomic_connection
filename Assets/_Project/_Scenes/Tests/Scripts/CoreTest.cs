using System;
using UnityEngine;

public class CoreTest : MonoBehaviour
{
    public ObjectWrapper[] objects;

    private void Update()
    {
        float time = Time.time;
        
        for (int i = 0; i < objects.Length; i++)
        {
            ObjectWrapper wrapper = objects[i];
            if (wrapper.GameObject == null) continue;

            float t = wrapper.OrbitTime > 0f ? wrapper.OrbitTime : 1f;
            float angle = (time / t) * 360f;
            float radians = angle * Mathf.Deg2Rad;

            Vector3 up = wrapper.OrbitUp.sqrMagnitude > 0.001f
                ? wrapper.OrbitUp.normalized : Vector3.forward;

            Vector3 right = Vector3.Cross(up, Vector3.forward).normalized;
            if (right.sqrMagnitude < 0.001f)
                right = Vector3.Cross(up, Vector3.right).normalized;
            Vector3 forward = Vector3.Cross(right, up).normalized;

            Vector3 offset = (right * Mathf.Cos(radians) + forward * Mathf.Sin(radians))
                             * wrapper.Radius;

            Quaternion tilt = Quaternion.Euler(wrapper.OrbitTilt.x, 0f, wrapper.OrbitTilt.y);
            offset = tilt * offset;

            wrapper.GameObject.transform.position = transform.position + offset;
        }
    }
}

[Serializable]
public struct ObjectWrapper
{
    public GameObject GameObject;
    public float Radius;
    public float OrbitTime;
    public Vector2 OrbitTilt;
    public Vector3 OrbitUp;
}
