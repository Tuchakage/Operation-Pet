using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class CharacterPunch : MonoBehaviourPunCallbacks
{
    public float punchCooldown = 1.0f;
    public float pushBackForce = 2.0f;
    public float punchRange = 2.0f;
    public LayerMask targetLayer;

    private bool canPunch = true;
    private Dictionary<Transform, int> hitCounts = new Dictionary<Transform, int>();

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetMouseButtonDown(0) && canPunch)
        {
            photonView.RPC("PerformPunchRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void PerformPunchRPC()
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
}