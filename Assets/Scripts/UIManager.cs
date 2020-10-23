using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager s; // Singleton

    private Camera camera;
    private Image lockIcon;
    private Enemy lockedEnemy;

    private void Awake()
    {
        if (!s) s = this;
        else Destroy(gameObject);

        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        lockIcon = transform.Find("LockIcon").GetComponent<Image>();
        lockIcon.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (lockedEnemy) {
            lockIcon.rectTransform.position = camera.WorldToScreenPoint(lockedEnemy.GetChestTransform().position);
        }
    }

    public void LockEnemy(Enemy e) {
        lockedEnemy = e;
        lockIcon.enabled = true;
    }

    public void UnlockEnemy() {
        lockedEnemy = null;
        lockIcon.enabled = false;
    }

}
