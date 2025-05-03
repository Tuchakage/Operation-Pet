using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class CharacterPunch : MonoBehaviourPunCallbacks
{
    public float punchCooldown = 1.0f;
    public float pushBackForce = 2.0f;
    public float punchRange = 2.0f; // Defines the range for detecting targets
    public LayerMask targetLayer; // Layer for objects that can be punched
    private bool canPunch = true;

    private Dictionary<Transform, int> hitCounts = new Dictionary<Transform, int>(); // Tracks hit counts for multiple targets

    void Update()
    {
        if (photonView.IsMine)
        {
            // Check if the left mouse button is clicked and punching is allowed
            if (Input.GetMouseButtonDown(0) && canPunch)
            {
                photonView.RPC("PerformPunchRPC", RpcTarget.AllBuffered);
            }
        }
    }

    [PunRPC]
    private void PerformPunchRPC()
    {
        StartCoroutine(Punch());
    }

    private IEnumerator Punch()
    {
        // Find all objects within punch range
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, punchRange, targetLayer);

        foreach (Collider hit in hitTargets)
        {
            Transform target = hit.transform;

            if (!hitCounts.ContainsKey(target))
            {
                hitCounts[target] = 0;
            }

            PushBackTarget(target);
            hitCounts[target]++;

            // Shrink the target after 3 hits
            if (hitCounts[target] >= 3)
            {
                ShrinkTarget(target);
                hitCounts[target] = 0; // Reset hit count
            }
        }

        // Prevent further punches until cooldown ends
        canPunch = false;
        yield return new WaitForSeconds(punchCooldown);
        canPunch = true;
    }

    private void PushBackTarget(Transform target)
    {
        // Apply a force to move the target back
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 pushDirection = (target.position - transform.position).normalized;
            rb.AddForce(pushDirection * pushBackForce, ForceMode.Impulse);
        }
    }

    private void ShrinkTarget(Transform target)
    {
        // Reduce the target's size
        target.localScale *= 0.5f;
    }
}