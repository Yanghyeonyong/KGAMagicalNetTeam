using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tornado : MonoBehaviourPun
{
    [Header("Data")]
    [SerializeField] private TornadoSO data;

    private HashSet<Rigidbody> activeTargets = new HashSet<Rigidbody>();
    private int shooterID;
    private Vector3 moveDirection;

    [PunRPC]
    public void RPC_Setup(int shooterID)
    {
        this.shooterID = shooterID;
        if (photonView.IsMine)
        {
            moveDirection = transform.forward;
            moveDirection.y = 0;
            moveDirection.Normalize();
            if (moveDirection == Vector3.zero) moveDirection = Vector3.forward;
            StartCoroutine(LifetimeRoutine());
        }
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * data.rotationSpeed * Time.deltaTime, Space.World);
        if (photonView.IsMine) MoveTornado();
    }

    private void LateUpdate()
    {
        if (!photonView.IsMine) return;
        Vector3 currentEuler = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0, currentEuler.y, 0);
    }

    private void FixedUpdate()
    {
        ControlSatellites();
    }

    private void MoveTornado()
    {
        Vector3 nextPosition = transform.position + (moveDirection * data.moveSpeed * Time.deltaTime);
        RaycastHit hit;
        int layerMask = 1 << LayerMask.NameToLayer("Ground");
        if (layerMask == 0) layerMask = ~0;

        if (Physics.Raycast(nextPosition + Vector3.up * 5.0f, Vector3.down, out hit, 20.0f, layerMask))
            nextPosition.y = hit.point.y;
        transform.position = nextPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        // [수정 1] IsValidTarget 내부도 부모를 찾도록 되어 있는지 확인 필요하지만, 
        // 일단 여기서 걸러지지 않도록 주의
        if (!IsValidTarget(other)) return;

        // 1. 파편(ChunkNode) 처리
        ChunkNode chunk = other.GetComponentInParent<ChunkNode>(); // 혹시 몰라 Parent 포함
        if (chunk != null)
        {
            if (chunk.CheckInteractable(gameObject, data, shooterID))
            {
                chunk.OnMagicInteract(gameObject, data, shooterID);
                Rigidbody chunkRb = other.GetComponent<Rigidbody>();
                if (chunkRb != null && !activeTargets.Contains(chunkRb))
                {
                    activeTargets.Add(chunkRb);
                }
            }
            return;
        }

        // [수정 2] 부모에서 인터페이스 찾기 (시체의 뼈다귀를 때렸을 때 본체를 찾기 위함)
        IMagicInteractable obj = other.GetComponentInParent<IMagicInteractable>();
        if (obj == null) return;

        // 리지드바디는 부딪힌 그 녀석(뼈대일 수 있음)
        Rigidbody targetRb = other.GetComponent<Rigidbody>();

        // 만약 부딪힌 녀석이 리지드바디가 없다면(루트일 경우 등), 부모나 본체에서 찾아봄
        if (targetRb == null) targetRb = other.GetComponentInParent<Rigidbody>();
        if (targetRb == null) return;

        if (obj.CheckInteractable(gameObject, data, shooterID))
        {
            HumanoidRagdollController ragdollCtrl = other.GetComponentInParent<HumanoidRagdollController>();
            if (ragdollCtrl != null)
            {
                // [핵심] 토네이도 상태 진입 알림
                ragdollCtrl.SetTornadoState(true);

                // 물리 주체를 무조건 Hips로 교체 (이미 래그돌이어도 Hips를 가져옴)
                Rigidbody hips = ragdollCtrl.GetRagdollHips();
                if (hips != null) targetRb = hips;
            }

            if (!activeTargets.Contains(targetRb))
            {
                activeTargets.Add(targetRb);
                obj.OnMagicInteract(gameObject, data, shooterID);
            }
        }
    }

    private bool IsValidTarget(Collider other)
    {
        PhotonView targetPV = other.GetComponentInParent<PhotonView>();
        if (targetPV == null) return true;

        if (targetPV.CompareTag("Player") || other.CompareTag("Player"))
        {
            if (targetPV.OwnerActorNr == shooterID) return false;
            bool isFriendlyFireOn = PhotonNetwork.CurrentRoom.GetProps<bool>(NetworkProperties.FRIENDLYFIRE);
            return isFriendlyFireOn;
        }
        return true;
    }

    private void OnTriggerExit(Collider other)
    {
        // [수정 3] Root 콜라이더가 꺼져서 발생하는 가짜 Exit 이벤트를 무시해야 함
        HumanoidRagdollController ragdollCtrl = other.GetComponentInParent<HumanoidRagdollController>();

        if (ragdollCtrl != null)
        {
            // 컨트롤러가 "나 아직 토네이도 안이야(IsInTornado)"라고 하면 내보내지 않음
            if (ragdollCtrl.IsInTornado) return;

            Rigidbody hips = ragdollCtrl.GetRagdollHips();
            if (hips != null && activeTargets.Contains(hips)) ReleaseTarget(hips, false);
        }
        else
        {
            // 일반 물체
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null && activeTargets.Contains(rb)) ReleaseTarget(rb, false);
        }
    }

    private void ControlSatellites()
    {
        List<Rigidbody> toRelease = new List<Rigidbody>();
        List<Rigidbody> toSwapAdd = new List<Rigidbody>();
        List<Rigidbody> toSwapRemove = new List<Rigidbody>();

        foreach (var rb in activeTargets)
        {
            if (rb == null) continue;

            // 거리 체크 (OnTriggerExit을 막았으니 여기서 거리가 멀어지면 놔줘야 함)
            Vector3 offset = rb.position - transform.position;
            float distance = offset.magnitude;

            // [수정 4] 최대 거리보다 멀어지면 강제 방생 (안전 장치)
            if (distance > data.maxDistance * 1.5f || offset.y > data.releaseHeight)
            {
                toRelease.Add(rb);
                continue;
            }

            // 자가 치유: Root가 잘못 들어왔으면 Hips로 교체
            if (rb.isKinematic)
            {
                var ctrl = rb.GetComponent<HumanoidRagdollController>(); // Root에는 컴포넌트 있음
                // 뼈대에는 컴포넌트가 없으므로 부모 체크
                if (ctrl == null) ctrl = rb.GetComponentInParent<HumanoidRagdollController>();

                if (ctrl != null)
                {
                    Rigidbody hips = ctrl.GetRagdollHips();
                    if (hips != null && hips != rb && !hips.isKinematic)
                    {
                        toSwapRemove.Add(rb);
                        toSwapAdd.Add(hips);
                        continue;
                    }
                }

                var chunk = rb.GetComponent<ChunkNode>();
                if (chunk != null && chunk.IsFrozen && !chunk.IsIndestructible)
                {
                    chunk.Unfreeze();
                }
            }

            // 회전 및 인력 계산
            Vector3 dirToCenter = -offset.normalized;
            Vector3 horizontalDir = new Vector3(dirToCenter.x, 0, dirToCenter.z).normalized;
            Vector3 tangentDir = Vector3.Cross(horizontalDir, Vector3.up).normalized;

            float distFactor = Mathf.Clamp01(distance / data.maxDistance);
            float currentSuction = Mathf.Lerp(data.suctionSpeed, data.suctionSpeed * 2.5f, distFactor);
            float heightFactor = (offset.y < 2.0f) ? 2.0f : 1.0f;
            float currentLift = data.liftSpeed * heightFactor;

            Vector3 targetVelocity = (tangentDir * data.orbitSpeed) + (horizontalDir * currentSuction);
            Vector3 newVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * data.captureStrength);
            newVelocity.y = currentLift;

            rb.linearVelocity = newVelocity;
        }

        foreach (var r in toSwapRemove) activeTargets.Remove(r);
        foreach (var a in toSwapAdd) activeTargets.Add(a);
        foreach (var rb in toRelease) ReleaseTarget(rb, true);
    }

    private void ReleaseTarget(Rigidbody rb, bool applyForce)
    {
        if (activeTargets.Contains(rb))
        {
            activeTargets.Remove(rb);
            if (rb != null)
            {
                var ragdollCtrl = rb.GetComponentInParent<HumanoidRagdollController>();
                if (ragdollCtrl != null) ragdollCtrl.SetTornadoState(false);

                rb.useGravity = true;
                rb.GetComponent<IPhysicsObject>()?.OnStatusChange(false);

                if (applyForce)
                {
                    Vector3 explodeDir = (rb.position - transform.position).normalized + Vector3.up;
                    rb.AddForce(explodeDir * data.ejectForce, ForceMode.Impulse);
                }
            }
        }
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(data.duration);
        List<Rigidbody> finalTargets = new List<Rigidbody>(activeTargets);
        foreach (var rb in finalTargets) ReleaseTarget(rb, true);
        activeTargets.Clear();
        if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
    }

    private bool ChunkNodeCheck(Collider other) { return true; }
}