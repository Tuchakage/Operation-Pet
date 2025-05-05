using UnityEngine;
using Photon.Pun;
using System.Collections;

public class LightBeam : MonoBehaviourPunCallbacks
{
    public GameObject beamPrefab; // Prefab of the beam
    public float beamDuration = 5.0f; // Duration before the beam disappears
    public float beamWidth = 1.0f; // Width of the beam
    public Color beamColor = Color.white; // Color of the beam
    public Camera playerCamera; // Player's camera for targeting
    public LayerMask targetLayer; // Layer for spawning the beam
    public LayerMask groundPlayerLayer; // Layer for players that should destroy the beam
    private GameObject beamInstance; // Instantiated beam reference

    void Update()
    {
        if (!photonView.IsMine) return;

        // Check for player input to create the beam
        if (Input.GetKeyDown(KeyCode.Q))
        {
            photonView.RPC("CreateBeamRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    private void CreateBeamRPC()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Perform a raycast to find the target position
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
        {
            Vector3 beamPosition = hit.point;

            // Instantiate the beam using PhotonNetwork
            beamInstance = PhotonNetwork.Instantiate(beamPrefab.name, beamPosition, Quaternion.identity);

            // Adjust the beam's scale and color
            beamInstance.transform.localScale = new Vector3(beamWidth, 5000f, beamWidth);
            beamInstance.GetComponent<Renderer>().material.color = beamColor;

            // Attach collision detection
            beamInstance.AddComponent<BeamCollisionHandler>().InitializeBeam(this);

            // Schedule destruction after beamDuration
            photonView.RPC("DestroyBeamRPC", RpcTarget.AllBuffered, beamInstance.GetComponent<PhotonView>().ViewID);
        }
    }

    [PunRPC]
    private void DestroyBeamRPC(int beamViewID)
    {
        StartCoroutine(DestroyBeamAfterTimer(beamViewID));
    }

    private IEnumerator DestroyBeamAfterTimer(int beamViewID)
    {
        yield return new WaitForSeconds(beamDuration);

        PhotonView beamPhotonView = PhotonView.Find(beamViewID);
        if (beamPhotonView != null)
        {
            PhotonNetwork.Destroy(beamPhotonView.gameObject);
            Debug.Log($"Light Beam destroyed after {beamDuration} seconds.");
        }
        else
        {
            Debug.LogError("Failed to find light beam object for destruction.");
        }
    }
}

public class BeamCollisionHandler : MonoBehaviour
{
    private LightBeam parentScript;

    public void InitializeBeam(LightBeam script)
    {
        parentScript = script;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & parentScript.groundPlayerLayer) != 0)
        {
            PhotonView beamPV = GetComponent<PhotonView>();
            if (beamPV != null && beamPV.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
                Debug.Log($"Beam destroyed as ground player entered.");
            }
        }
    }
}