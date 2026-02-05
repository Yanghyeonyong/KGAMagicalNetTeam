using Photon.Pun;
using UnityEngine;


public class BossEntryTrigger : MonoBehaviourPun
{
    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player") && PhotonNetwork.IsMasterClient)
        {
            isTriggered = true;
            BossRaidManager.Instance.StartBossRaid();
        }
    }
}