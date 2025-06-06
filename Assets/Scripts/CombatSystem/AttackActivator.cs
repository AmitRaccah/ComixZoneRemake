using System.Collections.Generic;
using UnityEngine;

public class AttackActivator : MonoBehaviour
{
    [SerializeField] private Transform handSocket;   //Hand/Leg transform

    [SerializeField] private AttackData[] attacks;   //array for attack types

    [SerializeField] private GameObject hitboxPrefab;   //prefab of sphereCollider + HitboxController

    private Dictionary<string, AttackData> map; //Hit name by string

    private void Awake()
    {
        map = new Dictionary<string, AttackData>();

        foreach(AttackData data in attacks) 
        {
            if (!map.ContainsKey(data.attackName))
                map.Add(data.attackName, data);
        }
    }


    public void ActivateAttack(string name)
    {
        if (!map.TryGetValue(name, out AttackData data))
        {
            Debug.LogWarning($"AttackActivator: {name} - does not exsist");
            return;
        }

        GameObject go = Instantiate(hitboxPrefab);

        HitboxController ctrl = go.GetComponent<HitboxController>();
        ctrl.Init(data, handSocket);

        CombatBus.Publish(new AttackPerformedEvent(name, gameObject.GetInstanceID()));



    }






    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
