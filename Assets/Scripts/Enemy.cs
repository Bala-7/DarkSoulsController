using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Animator _animator;
    public Transform chest;

    private void Awake()
    {
        _animator = transform.Find("Body").GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Hit() {
        _animator.Play("Hit");
    }

    public void Kill() {
        _animator.Play("Death");

    }

    public Transform GetChestTransform() { return chest; }
}
