using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

public class Mine : MonoBehaviourPunCallbacks
{
    public float explosionRadius = 5f; // Explosion range
    public float explosionForce = 10f; // Force applied to nearby objects
    public float upwardMod = 1f; // Controls vertical lift from explosion force
    public LayerMask affectedLayers; // Layers affected by the explosion

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Check if a player steps on the mine
        {
            Debug.Log($"{other.name} triggered the mine!");
            photonView.RPC("Explode", RpcTarget.All); // Trigger explosion
        }
    }

    [PunRPC]
    private void Explode()
    {
        // Detect all players in explosion radius
        Collider[] hitObjects = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider obj in hitObjects)
        {
            if (obj.CompareTag("Player")) // Ensure only players are affected
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardMod);
                    Debug.Log($"Knockback applied to {obj.name}");
                }
            }
        }

        PhotonNetwork.Destroy(gameObject); // Destroy mine after explosion
    }

}