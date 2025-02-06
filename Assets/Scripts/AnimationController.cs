using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public static void PlayCurseAnim(Animator anim)
    {
        //If Curse corresponds to integer 1
        anim?.SetInteger("CardEffect", 1);
    }
}
