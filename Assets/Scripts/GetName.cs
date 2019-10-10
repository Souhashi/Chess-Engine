using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GetName : MonoBehaviour , IPunInstantiateMagicCallback
{
    // Start is called before the first frame update
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        PhotonView p = GetComponent<PhotonView>();

        gameObject.name = p.InstantiationData[0].ToString();

    }
}
