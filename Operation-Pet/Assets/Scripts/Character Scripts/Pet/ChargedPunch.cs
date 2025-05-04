using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class ChargedPunch : MonoBehaviourPunCallbacks
{
    public float punchCooldown = 1.0f;
    public float pushBackForce = 5.0f;
    public float punchRange = 3.0f;
    public float disableMovementDuration = 2.0f; // Time before punched target can move again
    public LayerMask targetLayer;

    private bool canPunch = true;
    private Dictionary<Transform, int> hitCounts = new Dictionary<Transform, int>();

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetMouseButtonDown(1) && canPunch)
        {
            photonView.RPC("PerformChargedPunchRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void PerformChargedPunchRPC()
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(Punch());
    }

    private IEnumerator Punch()
    {
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, punchRange, targetLayer);

        foreach (Collider hit in hitTargets)
        {
            Transform target = hit.transform;

            if (!hitCounts.ContainsKey(target))
                hitCounts[target] = 0;

            PushBackTarget(target);
            hitCounts[target]++;

            // Disable movement for a short duration
            StartCoroutine(DisableMovement(target));
        }

        canPunch = false;
        yield return new WaitForSeconds(punchCooldown);
        canPunch = true;
    }

    private void PushBackTarget(Transform target)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 pushDir = (target.position - transform.position).normalized;
            rb.AddForce(pushDir * pushBackForce, ForceMode.Impulse);
        }
    }

    private IEnumerator DisableMovement(Transform target)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Disable movement
            yield return new WaitForSeconds(disableMovementDuration);
            rb.isKinematic = false; // Enable movement again
        }
    }
}