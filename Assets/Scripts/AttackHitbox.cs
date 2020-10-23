using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private BoxCollider _collider;
    private Enemy attackedEnemy;
    private DarkSoulsController _playerController;
    [SerializeField]
    private List<Enemy> inRangeEnemies;
    private void Awake()
    {
        inRangeEnemies = new List<Enemy>();
        _playerController = transform.parent.parent.GetComponent<DarkSoulsController>();
        _collider = GetComponent<BoxCollider>();
        //_collider.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        Enemy attackedEnemy = other.GetComponent<Enemy>();
        if (attackedEnemy && !inRangeEnemies.Contains(attackedEnemy)) {
            inRangeEnemies.Add(attackedEnemy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy attackedEnemy = other.GetComponent<Enemy>();
        if (attackedEnemy && inRangeEnemies.Contains(attackedEnemy))
        {
            inRangeEnemies.Remove(attackedEnemy);
        }
    }

    

    public Enemy BackstabbedEnemy() {

        Vector3 playerDirection = transform.parent.forward;
        foreach (Enemy e in inRangeEnemies) {
            Vector3 enemyDirection = e.transform.forward;

            float angle = Vector3.Angle(playerDirection, enemyDirection);

            if (angle < 20)
            {
                attackedEnemy = e;
                return e;
            }
        }

        

        return null;
    }

    private void ExecuteHitOnEnemy() {

        Debug.Log("Attacked enemy: " + attackedEnemy.gameObject.name);
        attackedEnemy.Hit();
    }

    public void KillEnemy(Enemy e, float delay) {
        if (inRangeEnemies.Contains(e)) {
            Invoke("DelayedBackstab", delay);
        }
    }

    private void DelayedBackstab() {
s        attackedEnemy.Kill();
    }

    public void HitAllEnemies(float delay) {
        Invoke("DelayedHitAllEnemies", delay);
    }

    private void DelayedHitAllEnemies() {
        foreach (Enemy e in inRangeEnemies)
        {
            e.Hit();
        }

    }

    void OnDrawGizmos()
    {

#if UNITY_EDITOR
        ShowGizmo();
#endif

    }

    private void ShowGizmo() {
        Color c = Color.yellow;
        c.a = 0.5f;

        Gizmos.color = c;
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
