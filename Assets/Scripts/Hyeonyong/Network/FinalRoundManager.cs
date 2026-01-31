using NUnit.Framework;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FinalRoundManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject backToRoomBtn;
    [SerializeField] AudioClip loseOrWinAudio;
    [SerializeField] GameObject speakerPrefab;
    GameObject speaker;
    List<PhotonView> myPhotonView = new List<PhotonView>();
    PhotonView[] allPhotonView;

    private void Start()
    {
        if (speaker == null)
            speaker = PhotonNetwork.Instantiate(speakerPrefab.name, Vector3.zero, Quaternion.identity, 0);

        allPhotonView = FindObjectsByType<PhotonView>(FindObjectsSortMode.None);

        SoundManager.Instance.PlayBGM(loseOrWinAudio);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (!PhotonNetwork.IsMasterClient)
        {
            backToRoomBtn.gameObject.SetActive(false);
        }
    }
    public void BackToRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(CoLoadRoom()); 
        }
    }

    IEnumerator CoLoadRoom()
    {
        //GetMyPhotonViews();
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.DestroyAll();

        //while (myPhotonView.Count > 0)
        //{
        //    myPhotonView.RemoveAll(view => view == null || view.gameObject == null);
        //    yield return null;
        //}

        //

        int inGamePing = PhotonNetwork.GetPing();
        //혹시 진짜 혹시라도 몰라서 해당 연산 도중 ping이 오를 경우를 대비해 200을 추가함
        float waitTime = (inGamePing + 200) / 1000f;
        yield return CoroutineManager.waitForSeconds(waitTime);

        PhotonNetwork.IsMessageQueueRunning = false;
        yield return CoroutineManager.waitForSeconds(1.0f);
        PhotonNetwork.LoadLevel("Room_new");
    }

 
    void GetMyPhotonViews()
    {
        foreach (PhotonView view in allPhotonView)
        {
            if (view.IsMine)
            {
                myPhotonView.Add(view);
            }
        }
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            backToRoomBtn.gameObject.SetActive(true);
            //PhotonNetwork.DestroyAll();
        }
        else
        {
            backToRoomBtn.gameObject.SetActive(false);
        }
    }
}
