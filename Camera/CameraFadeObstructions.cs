using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFadeObstructions : MonoBehaviour
{
    private ObjectFader _fader;

    [SerializeField] private GameObject player;


    private void Update()
    {
        if(player != null)
        {
            Vector3 dir = player.transform.position - transform.position;

            Ray ray = new Ray(transform.position, dir);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit))
            {
                if (hit.collider == null)
                    return;

                if(hit.collider.gameObject == player)
                {
                    if (_fader != null) _fader._doFade = false;
                }
                else
                {
                    _fader = hit.collider.gameObject.GetComponent<ObjectFader>();
                    
                    if(_fader != null)
                        _fader._doFade = true;
                }
                    

            }
        }
    }
}
