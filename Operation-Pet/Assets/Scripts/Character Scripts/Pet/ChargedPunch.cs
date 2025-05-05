using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class ChargedPunch : MonoBehaviourPunCallbacks
{
    public float chargeTime = 2.0f; // Time required to charge the punch
    public float pushBackForce = 5.0f; // Force to knock the target back
    public float shrinkFactor = 0.5f; // Factor by which the target shrinks
    public float punchRange = 3.0f; // Defines the range for detecting targets
    public LayerMask targetLayer; // Layer for objects that can be punched

    private bool isCharging = false; // Tracks if the punch is being charged
    private bool canPunch = true; // Ensures cooldown after each punch
    private Dictionary<Transform, int> hitCounts = new Dictionary<Transform, int>(); // Tracks hit counts for multiple targets

    void Update()
    {
        if (!photonView.IsMine) return;

        // Start charging punch when right mouse button is pressed
        if (Input.GetMouseButtonDown(1) && canPunch)
        {
            StartCoroutine(ChargePunch());
        }
    }

    [PunRPC]
    private void PerformPunchRPC()
    {
        StartCoroutine(ChargePunch());
    }

    private IEnumerator ChargePunch()
    {
        isCharging = true;
        float chargeProgress = 0;

        while (Input.GetMouseButton(1) && chargeProgress < chargeTime)
        {
            chargeProgress += Time.deltaTime; // Increment charge progress
            yield return null; // Wait for next frame
        }

        // If fully charged and button is released, execute the punch
        if (chargeProgress >= chargeTime)
        {
            photonView.RPC("ExecutePunchRPC", RpcTarget.AllBuffered);
        }

        isCharging = false;
    }

    [PunRPC]
    private void ExecutePunchRPC()
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

        // Start cooldown after the punch
        StartCoroutine(PunchCooldown());
    }

    private IEnumerator PunchCooldown()
    {
        canPunch = false;
        yield return new WaitForSeconds(1.0f); // Cooldown duration
        canPunch = true;
    }

    private void PushBackTarget(Transform target)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 pushDirection = (target.position - transform.position).normalized;
            rb.AddForce(pushDirection * pushBackForce, ForceMode.Impulse);
        }
    }

    private void ShrinkTarget(Transform target)
    {
        target.localScale *= shrinkFactor;
    }
}