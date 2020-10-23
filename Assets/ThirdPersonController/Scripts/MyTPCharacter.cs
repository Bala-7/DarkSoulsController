using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTPCharacter : MonoBehaviour
{
    public enum ANIM_TYPE { FULL_BODY, BODY_LEG_SEPARATED}
    private Animator fullBodyAnimator;
    public Animator legsAnimator;
    public Animator bodyAnimator;

    private void Awake()
    {
        fullBodyAnimator = GetComponent<Animator>();
    }

    public Animator GetFullBodyAnimator() { return fullBodyAnimator; }

    public Animator GetLegsAnimator() { return legsAnimator; }

    public Animator GetBodyAnimator() { return bodyAnimator; }



}
