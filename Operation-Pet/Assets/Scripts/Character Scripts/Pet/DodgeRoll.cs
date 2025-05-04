using Photon.Pun;
using System.Collections;
using UnityEngine;

public class DodgeRoll : MonoBehaviourPunCallbacks
{
    public float rollDistance = 5.0f; // Distance covered in the dodge roll
    public float rollSpeed = 10.0f; // Speed of the dodge roll
    public float rollCooldown = 1.5f; // Time before the player can roll again
    private bool isRolling = false; // Tracks if the player is currently rolling
    private bool canRoll = true; // Tracks if the player can roll after cooldown
    private Rigidbody rb; // Reference to Rigidbody

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get Rigidbody component
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smoother movement
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // Check for player input to dodge roll (e.g., pressing Right Shift)
        if (Input.GetKeyDown(KeyCode.RightShift) && canRoll)
        {
            photonView.RPC("PerformRollRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    private void PerformRollRPC()
    {
        StartCoroutine(PerformRoll());
    }

    private IEnumerator PerformRoll()
    {
        canRoll = false; // Disable rolling during cooldown
        isRolling = true;

        Vector3 rollDirection = transform.forward; // Roll in the direction the player is facing
        float rollTime = rollDistance / rollSpeed; // Duration of the roll

        float elapsedTime = 0;
        while (elapsedTime < rollTime)
        {
            rb.MovePosition(rb.position + rollDirection * rollSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isRolling = false;

        // Start cooldown
        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }
}