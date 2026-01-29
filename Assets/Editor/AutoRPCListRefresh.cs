using UnityEngine;
using UnityEditor;
using Photon.Pun;

[InitializeOnLoad]
public class AutoRPCListRefresh
{
    static AutoRPCListRefresh()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            UpdateRpc();
        }
    }

    [MenuItem("Photon/Force Update RPC List")]
    public static void UpdateRpc()
    {
        PhotonNetwork.PhotonServerSettings.RpcList.Clear();
        PhotonEditor.UpdateRpcList();

        EditorUtility.SetDirty(PhotonNetwork.PhotonServerSettings);
        AssetDatabase.SaveAssets();
    }
}
