using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropChunk : Health
{
    public AudioClip audioClip;

    public override void Die()
    {
        AudioManager.instance.Play3DSoundOnPosition(transform.position, audioClip);

        Destroy(gameObject);
    }
}
