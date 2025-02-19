using UnityEngine;
using Photon.Pun;

public class ball : MonoBehaviourPunCallbacks
{
    public float StartSpeed = 5f;

    public float MaxSpeed = 20;

    public float SpeedIncrease = 0.25f;

    private float currentSpeed;

    private Vector2 currentDir;
    public Vector2 score;

    private float scoreGoalL;
    private float scoreGoalR;   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Initialise starting speed
        currentSpeed = StartSpeed;

        //Initialise Direction
        currentDir = Random.insideUnitCircle.normalized;

    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.CurrentRoom.Players.Count == 0)
            return;

        //Calculate where the ball is going to go
        Vector2 moveDir = currentDir * currentSpeed * Time.deltaTime;
        transform.Translate(new Vector3(moveDir.x, moveDir.y, 0f));
        ChangeScore(score);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log(other.name);
        if (other.tag == "Boundary")
        {
            //Reverse the direction if it hits the top of the wall

            currentDir.y *= -1;
        }
        else if (other.tag == "Player")
        {
            //Reverse the direction if it hits the player pad
            currentDir.x *= -1;
        }
        else if (other.tag == "GoalL")
        {
            score.x++;
            ChangeColorTo(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
            ChangePositionTo(new Vector3(0f, 1.5f, -2f));
            ChangeDirTo(Random.insideUnitCircle.normalized);
            ChangeScore(score);
        }
        else if (other.tag == "GoalR") 
        {
            score.y++;
            ChangeColorTo(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
            ChangePositionTo(new Vector3(0f, 1.5f, -2f));
            ChangeDirTo(Random.insideUnitCircle.normalized);
            ChangeScore(score);
        }
    }



    [PunRPC]
    void ChangeScore(Vector2 score) 
    {
        SetScoreboard(score);
        //If the one calling this function is me
        if (photonView.IsMine) 
        {
            //Change the score for everyone
            photonView.RPC("ChangeScore", RpcTarget.OthersBuffered, score);
        }
    }

    [PunRPC]
    void SetScoreboard(Vector2 score)
    {
        scoreGoalL = score.x;
        scoreGoalR = score.y;
    }

    [PunRPC]
    void ChangeColorTo(Vector3 color) 
    {
        GetComponent<Renderer>().material.color = new Color(color.x, color.y, color.z, 1f);

        if (photonView.IsMine)
        {
            photonView.RPC("ChangeColorTo", RpcTarget.OthersBuffered, color);
        }

    }

    [PunRPC]
    void ChangePositionTo(Vector3 mycurrentPos) 
    {
        GetComponent<Transform>().position = mycurrentPos;
        if (photonView.IsMine) 
        {
            photonView.RPC("ChangePositionTo", RpcTarget.OthersBuffered, mycurrentPos);
        }
    }

    [PunRPC]
    void ChangeDirTo(Vector2 mycurrentDir) 
    {
        currentDir = mycurrentDir;

        if (photonView.IsMine) 
        {
            //Calling the function from this computers to all the other computers remotely
            photonView.RPC("ChangeDirTo", RpcTarget.OthersBuffered, mycurrentDir);
        }
    }
}
