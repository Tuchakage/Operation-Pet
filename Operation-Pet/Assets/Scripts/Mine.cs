using UnityEngine;
using Photon.Pun;

public class Mine : MonoBehaviourPunCallbacks
{
    public float explosionRadius = 5f; // Explosion range
    public float explosionForce = 10f; // Force applied to nearby objects
    public LayerMask affectedLayers; // Layers affected by the explosion

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;

        // Check if the object that collided has the correct layer or tag
        if ((affectedLayers.value & (1 << collision.gameObject.layer)) > 0)
        {
            photonView.RPC("ExplodeRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void ExplodeRPC()
    {
        Collider[] hitObjects = Physics.OverlapSphere(transform.position, explosionRadius, affectedLayers);

        foreach (Collider obj in hitObjects)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        // Destroy the mine across the network
        PhotonNetwork.Destroy(gameObject);
    }
}