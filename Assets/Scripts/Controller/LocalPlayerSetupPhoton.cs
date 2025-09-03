using UnityEngine;
#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
#endif

/// Enable cameras and input-producing behaviours only for the local player.
public class LocalPlayerSetupPhoton : MonoBehaviour
{
    [Header("Assign")]
    public GameObject camerasRoot;
    public Behaviour[] localOnlyBehaviours; // PlayerInput, RibbitInputs, RibbitThirdPersonMotor, CameraToggleController, CharacterAnimationDriver
    public Collider[] localOnlyColliders;

#if PHOTON_UNITY_NETWORKING
    private PhotonView _view;
#endif

    void Awake()
    {
#if PHOTON_UNITY_NETWORKING
        _view = GetComponent<PhotonView>();
        bool isMine = _view != null && _view.IsMine;
#else
        bool isMine = true;
#endif
        if (camerasRoot) camerasRoot.SetActive(isMine);
        if (localOnlyBehaviours != null) foreach (var b in localOnlyBehaviours) if (b) b.enabled = isMine;
        if (localOnlyColliders != null) foreach (var c in localOnlyColliders) if (c) c.enabled = isMine;
        if (!isMine)
        {
            int remoteLayer = LayerMask.NameToLayer("RemotePlayer");
            if (remoteLayer >= 0) gameObject.layer = remoteLayer;
        }
    }
}