using Photon.Pun;
using System.Collections;
using UnityEngine;

public class ChargedPunch : MonoBehaviourPunCallbacks
{
    public float chargeTime = 2.0f; // Time required to charge the punch
    public float pushBackForce = 5.0f; // Force to knock the target back
    public float shrinkFactor = 0.5f; // Factor by which the target shrinks
    public Transform target; // Assign the target (e.g., the cube) in the Inspector

    private bool isCharging = false; // Tracks if the punch is being charged
    private bool canPunch = true; // Ensures cooldown after each punch

    void Update()
    {
        if (!photonView.IsMine) return;
        // Check if the left mouse button is pressed
        if (Input.GetMouseButtonDown(1) && canPunch) // 0 represents the left mouse button
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
            yield return null; // Wait for the next frame
        }

        // If fully charged and button is released, execute the punch
        if (chargeProgress >= chargeTime)
        {
            ExecutePunch();
        }

        isCharging = false;
    }

    private void ExecutePunch()
    {
        if (target != null && Vector3.Distance(transform.position, target.position) <= 3.0f) // Example range
        {
            // Knock back the target
            PushBackTarget();

            // Shrink the target
            ShrinkTarget();
        }

        // Start cooldown after the punch
        StartCoroutine(PunchCooldown());
    }

    private IEnumerator PunchCooldown()
    {
        canPunch = false;
        yield return new WaitForSeconds(1.0f); // Cooldown duration (adjust as needed)
        canPunch = true;
    }

    private void PushBackTarget()
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 pushDirection = (target.position - transform.position).normalized;
            rb.AddForce(pushDirection * pushBackForce, ForceMode.Impulse);
        }
    }

    private void ShrinkTarget()
    {
        // Shrink the target's scale
        target.localScale *= shrinkFactor;
    }
}
