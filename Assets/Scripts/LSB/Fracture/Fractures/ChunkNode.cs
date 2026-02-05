using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkNode : MonoBehaviour, IMagicInteractable
{
    [SerializeField] public ChunkNode[] neighbours;
    private HashSet<ChunkNode> _neighbours = new HashSet<ChunkNode>();
    public ChunkNode[] NeighboursArray { get; private set; } = new ChunkNode[0];

    private Rigidbody rb;

    [SerializeField] public bool IsIndestructible = false; // 앵커 여부
    [SerializeField] public float BreakForce = 50f;
    public bool HasBrokenLinks { get; private set; } = false;
    public bool IsFrozen => rb != null && rb.isKinematic;
    [SerializeField] public float DebrisLifetime = 5f;

    public static event Action<ChunkNode> OnAnyChunkBroken;
    public event Action OnDebrisDisable;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Setup()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (neighbours != null && neighbours.Length > 0 && _neighbours.Count == 0)
        {
            _neighbours = new HashSet<ChunkNode>(neighbours);
            _neighbours.RemoveWhere(n => n == null);
        }
        Freeze();
        RefreshNeighboursArray();
    }

    public void SyncNeighboursToArray()
    {
        _neighbours.RemoveWhere(n => n == null);
        neighbours = new ChunkNode[_neighbours.Count];
        _neighbours.CopyTo(neighbours);
    }

    public void CleanBrokenLinks()
    {
        int c = _neighbours.RemoveWhere(n => n == null || !n.IsFrozen);
        if (c > 0)
        {
            RefreshNeighboursArray();
            SyncNeighboursToArray();
        }
        HasBrokenLinks = false;
    }

    public void AddNeighbour(ChunkNode n)
    {
        if (!_neighbours.Contains(n)) { _neighbours.Add(n); RefreshNeighboursArray(); }
    }

    public void RemoveNeighbour(ChunkNode n)
    {
        if (_neighbours.Contains(n)) { _neighbours.Remove(n); RefreshNeighboursArray(); HasBrokenLinks = true; }
    }

    private void RefreshNeighboursArray()
    {
        NeighboursArray = new ChunkNode[_neighbours.Count];
        _neighbours.CopyTo(NeighboursArray);
    }

    public void ApplyExplosionForce(float explosionForce, Vector3 explosionPos, float explosionRadius, float upwardModifier)
    {
        if (IsIndestructible) return;
        if (IsFrozen) Unfreeze();
        if (rb != null) rb.AddExplosionForce(explosionForce, explosionPos, explosionRadius, upwardModifier, ForceMode.Impulse);
    }

    public void Unfreeze()
    {
        if (IsIndestructible) return;

        if (rb != null && rb.isKinematic)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            rb.gameObject.layer = LayerMask.NameToLayer("Default");

            rb.WakeUp();

            foreach (var neighbor in _neighbours)
            {
                if (neighbor != null) neighbor.RemoveNeighbour(this);
            }
            _neighbours.Clear();
            RefreshNeighboursArray();

            OnAnyChunkBroken?.Invoke(this);

            if (gameObject.activeInHierarchy) StartCoroutine(DisableDebrisRoutine());
        }
    }

    private IEnumerator DisableDebrisRoutine()
    {
        yield return CoroutineManager.waitForSeconds(DebrisLifetime);
        OnDebrisDisable?.Invoke();
        gameObject.SetActive(false);
    }

    private void Freeze()
    {
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
    }

    private void OnDrawGizmos()
    {
        if (IsIndestructible)
        {
            Gizmos.color = Color.red;
            Collider col = GetComponent<Collider>();
            Vector3 pos = (col != null) ? col.bounds.center : transform.position;
            Gizmos.DrawSphere(pos, 0.2f);
        }
    }

    public void OnMagicInteract(GameObject magic, MagicDataSO data, int attackerActorNr)
    {
        switch (data.magicType)
        {
            case MagicType.Fireball:
                FireballReaction(magic, data, attackerActorNr);
                break;
            case MagicType.Lightning:
                LightningReaction(magic, data, attackerActorNr);
                break;
            case MagicType.Tornado:
                TornadoReaction();
                break;
        }
    }

    public void FireballReaction(GameObject fireball, MagicDataSO data, int attackerActorNr)
    {
        ApplyExplosionForce(data.knockbackForce, fireball.transform.position, data.radius, data.forceUpward);
    }

    public void LightningReaction(GameObject lightning, MagicDataSO data, int attackerActorNr)
    {
        ApplyExplosionForce(data.knockbackForce, lightning.transform.position, data.radius, data.forceUpward);
    }

    public void TornadoReaction()
    {
        if (IsIndestructible) return;

        if (IsFrozen)
        {
            Unfreeze();
        }
    }

    public bool CheckInteractable(GameObject magic, MagicDataSO data, int attackerActorNr)
    {
        if (IsIndestructible) return false;

        return true;
    }
}