using Photon.Pun;
using System.Collections;
using UnityEngine;

public class CharacterPunch : MonoBehaviourPunCallbacks
{
    public float punchCooldown = 1.0f;
    public float pushBackForce = 2.0f;
    public Transform target; // Assign the target (e.g., the cube) in the Inspector
    private bool canPunch = true;

    private int hitCount = 0; // Keeps track of how many times the target has been hit

    void Update()
    {
        if (!photonView.IsMine) return;
        // Check if the left mouse button is clicked and punching is allowed
        if (Input.GetMouseButtonDown(0) && canPunch) // 0 represents the left mouse button
        {
            StartCoroutine(Punch());
        }
    }

    [PunRPC]
    private void PerformPunchRPC()
    {
        StartCoroutine(Punch());
    }

    private IEnumerator Punch()
    {
        // Simulate a punch by checking if the target is in range
        if (Vector3.Distance(transform.position, target.position) <= 2.0f) // Example range
        {
            PushBackTarget();
            hitCount++;

            // Shrink the target after 3 hits
            if (hitCount >= 3)
            {
                ShrinkTarget();
            }
        }

        // Prevent further punches until cooldown ends
        canPunch = false;
        yield return new WaitForSeconds(punchCooldown);
        canPunch = true;
    }

    private void PushBackTarget()
    {
        // Apply a force to move the target back
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 pushDirection = (target.position - transform.position).normalized;
            rb.AddForce(pushDirection * pushBackForce, ForceMode.Impulse);
        }
    }

    private void ShrinkTarget()
    {
        // Reduce the target's size
        target.localScale *= 0.5f; // Halves the size
        hitCount = 0; // Reset hit count
    }
}
