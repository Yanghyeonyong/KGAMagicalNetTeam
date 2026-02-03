using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using UnityEngine;

public class BossRaidManager : MonoBehaviourPunCallbacks
{
    public static BossRaidManager Instance;

    [Header("세팅")]
    public Transform[] playerTeleportPoints; 
    public DragonAI bossAI;

    [Header("종료 세팅")]
    public Transform[] rewardPoints;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //호출 진입점
    public void StartBossRaid()
    {
        photonView.RPC(nameof(RpcStartRaidSequence), RpcTarget.All);
    }

    [PunRPC]
    public void RpcStartRaidSequence()
    {
        StartCoroutine(CoStartRaid());
    }

    IEnumerator CoStartRaid()
    {
        //연출
        yield return new WaitForSeconds(1.0f);

        //소집
        TeleportLocalPlayer();

        yield return new WaitForSeconds(1.0f);

        if (PhotonNetwork.IsMasterClient && bossAI != null)
        {
            bossAI.WakeUp();
        }
    }

    private void TeleportLocalPlayer()
    {
        GameObject myPlayer = GetLocalPlayerObject();
        if (myPlayer != null && playerTeleportPoints != null && playerTeleportPoints.Length > 0)
        {

            Rigidbody rb = myPlayer.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;

            int pointIndex = GetPlayerIndex(playerTeleportPoints.Length);

            myPlayer.transform.position = playerTeleportPoints[pointIndex].position;
            myPlayer.transform.rotation = playerTeleportPoints[pointIndex].rotation;
        }
    }

    private int GetPlayerIndex(int maxPoints)
    {
        Player[] players = PhotonNetwork.PlayerList;

        Array.Sort(players, (p1, p2) => p1.ActorNumber.CompareTo(p2.ActorNumber));

        int myIndex = 0;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].IsLocal)
            {
                myIndex = i;
                break;
            }
        }
        //안전장치로 나누기연산
        return myIndex % maxPoints;
    }


    private GameObject GetLocalPlayerObject()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
        {
            PhotonView pv = p.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                return p;
            }
        }
        return null;
    }


    //사망RPC
    public void OnBossDefeated()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RpcTeleportToReward), RpcTarget.All);
        }
    }

    [PunRPC]
    public void RpcTeleportToReward()
    {
        GameObject myPlayer = GetLocalPlayerObject();

        if (myPlayer != null && rewardPoints != null)
        {
            Rigidbody rb = myPlayer.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            int pointIndex = GetPlayerIndex(rewardPoints.Length);
            myPlayer.transform.position = rewardPoints[pointIndex].position;
            myPlayer.transform.rotation = rewardPoints[pointIndex].rotation;
        }
    }

}
