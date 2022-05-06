using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    Animator animator;
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
        {
            Destroy(gameObject);
        }
    }
}
