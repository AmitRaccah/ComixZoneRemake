using UnityEngine;

[RequireComponent(typeof(HazardPoolMember))]
[RequireComponent(typeof(Collider))]
public class HazardSweepMover : MonoBehaviour
{
    private int dirSign;          
    private float speed;          
    private float remaining;      
    private HazardPoolMember member;

    private void Awake()
    {
        member = GetComponent<HazardPoolMember>();
    }

    public void Setup(int directionSign, float moveSpeed, float distance)
    {
        dirSign = directionSign >= 0 ? +1 : -1;
        speed = Mathf.Max(0f, moveSpeed);
        remaining = Mathf.Max(0f, distance);
        enabled = true;
    }

    private void OnDisable() => enabled = false;

    private void Update()
    {
        if (remaining <= 0f)
        {
            if (HazardPoolManager.Instance && member)
                HazardPoolManager.Instance.Return(member);
            return;
        }

        float step = speed * Time.deltaTime;
        transform.position += new Vector3(dirSign * step, 0f, 0f);
        remaining -= step;

        if (remaining <= 0f && HazardPoolManager.Instance && member)
            HazardPoolManager.Instance.Return(member);
    }
}
