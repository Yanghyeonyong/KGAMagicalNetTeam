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
        if (myPlayer != null)
        {
            Player[] players = PhotonNetwork.PlayerList;

            //ActorNumber순 정렬
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

            //포인트 개수보다 사람이 많으면? 대비
            int pointIndex = myIndex % playerTeleportPoints.Length;

            myPlayer.transform.position = playerTeleportPoints[pointIndex].position;
            myPlayer.transform.rotation = playerTeleportPoints[pointIndex].rotation;
        }
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
}
