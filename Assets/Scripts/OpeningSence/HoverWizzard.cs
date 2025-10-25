using UnityEngine;

public class SimpleHover : MonoBehaviour
{
    public float amplitude = 0.1f;
    public float speed = 2f;

    Vector3 startPos;
    float t;

    void Awake()
    {
        startPos = transform.position;
    }

    void Update()
    {
        t += Time.deltaTime * speed;
        float y = Mathf.Sin(t) * amplitude;
        transform.position = startPos + new Vector3(0f, y, 0f);
    }
}
