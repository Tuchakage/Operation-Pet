using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CharacterPunch : MonoBehaviourPunCallbacks
{
    InputSystem_Actions playerActionAsset;

    public float punchCooldown = 1.0f; // Cooldown between punches
    public float pushBackForce = 2.0f; // Knockback force
    public float punchRange = 2.0f; // Punch reach distance
    public LayerMask targetLayer; // Layer that includes punchable targets
    public int maxHitsBeforeStun = 4; // Number of punches needed to disable movement
    public float stunDuration = 3.0f; // Duration of movement disable

    private bool canPunch = true;
    private Dictionary<Transform, int> hitCounts = new Dictionary<Transform, int>();

    void Awake()
    {
        playerActionAsset = new InputSystem_Actions();
    }

    void Update()
    {


    }

    void OnEnable()
    {
        if (!photonView.IsMine) return;

        //Enable the Player Action Map 
        playerActionAsset.Pet.Enable();

        //Subscribe the Jump Event to Jump function
        playerActionAsset.Pet.LightAttack.started += RPCPerformPunch;


    }


    void OnDisable()
    {
        if (!photonView.IsMine) return;
        //Disable the Player Action Map 
        playerActionAsset.Pet.Disable();

        //UnSubscribe the Jump Event from the Jump function
        playerActionAsset.Pet.LightAttack.started -= RPCPerformPunch;
    }

    void RPCPerformPunch(InputAction.CallbackContext context)
    {
        if (canPunch)
        {
            photonView.RPC("PerformPunch", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void PerformPunch()
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

            if (hitCounts[target] >= maxHitsBeforeStun)
            {
                photonView.RPC("DisableTargetMovementRPC", RpcTarget.AllBuffered, target.GetComponent<PhotonView>().ViewID);
                hitCounts[target] = 0; // Reset punch count after stun is applied
            }
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

    [PunRPC]
    private void DisableTargetMovementRPC(int targetViewID)
    {
        PhotonView targetPhotonView = PhotonView.Find(targetViewID);
        if (targetPhotonView == null)
        {
            Debug.LogError("Failed to find target for movement disable.");
            return;
        }

        GameObject target = targetPhotonView.gameObject;
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Disable movement
            StartCoroutine(EnableMovementAfterDelay(rb));
        }
    }

    private IEnumerator EnableMovementAfterDelay(Rigidbody rb)
    {
        yield return new WaitForSeconds(stunDuration);
        rb.isKinematic = false; // Restore movement
    }
}