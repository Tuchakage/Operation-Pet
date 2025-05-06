using UnityEngine;
using Photon.Pun;

public class BeamCollisionHandler : MonoBehaviour
{
    private LayerMask groundPlayerLayer;

    public void Initialize(LayerMask layer)
    {
        groundPlayerLayer = layer;
        Collider col = gameObject.GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & groundPlayerLayer) != 0)
        {
            PhotonView pv = GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
                Debug.Log("Beam destroyed due to ground player collision.");
            }
        }
    }
}
