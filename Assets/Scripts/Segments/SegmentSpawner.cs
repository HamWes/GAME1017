using UnityEngine;
using System.Collections.Generic;

public class SegmentSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject[] segmentPrefabs;

    [Header("Spawning")]
    [SerializeField] private float spawnAheadDistance = 15f;   
    [SerializeField] private float minGap = 0.5f;
    [SerializeField] private float maxGap = 1.5f;
    [SerializeField] private float minHeightOffset = -1.5f;
    [SerializeField] private float maxHeightOffset = 1.5f;

    [Header("Cleanup")]
    [SerializeField] private float despawnBehindDistance = 25f; 

    private readonly List<GameObject> segments = new();

    private GameObject lastSegment;
    private Renderer lastRenderer;

    public void Initialize()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i]) Destroy(segments[i]);
        }
        segments.Clear();

        lastSegment = SpawnSegment(segmentPrefabs[0],
            new Vector3(player.position.x, player.position.y - 1f, 0f));

        lastRenderer = lastSegment.GetComponent<Renderer>();

        while (lastRenderer.bounds.max.x < player.position.x + spawnAheadDistance)
        {
            SpawnNextSegment();
        }
    }

    private void Update()
    {
        if (!player || !lastRenderer) return;

        while (lastRenderer.bounds.max.x < player.position.x + spawnAheadDistance)
        {
            SpawnNextSegment();
        }

        CleanupOldSegments();
    }

    private void SpawnNextSegment()
    {
        float gap = Random.Range(minGap, maxGap);
        float heightOffset = Random.Range(minHeightOffset, maxHeightOffset);

        GameObject prefab = segmentPrefabs[Random.Range(0, segmentPrefabs.Length)];

        GameObject newSeg = Instantiate(prefab, transform);
        Renderer newRend = newSeg.GetComponent<Renderer>();

        float xSpawn = lastRenderer.bounds.max.x + (newRend.bounds.size.x / 2f) + gap;
        float ySpawn = lastSegment.transform.position.y + heightOffset;

        newSeg.transform.position = new Vector3(xSpawn, ySpawn, 0f);

        segments.Add(newSeg);

        lastSegment = newSeg;
        lastRenderer = newRend;
    }

    private GameObject SpawnSegment(GameObject prefab, Vector3 position)
    {
        GameObject seg = Instantiate(prefab, position, Quaternion.identity, transform);
        segments.Add(seg);
        return seg;
    }

    private void CleanupOldSegments()
    {
        float despawnX = player.position.x - despawnBehindDistance;

        while (segments.Count > 0)
        {
            GameObject first = segments[0];
            if (!first)
            {
                segments.RemoveAt(0);
                continue;
            }

            Renderer r = first.GetComponent<Renderer>();
            if (r && r.bounds.max.x < despawnX)
            {
                Destroy(first);
                segments.RemoveAt(0);
            }
            else
            {
                break; 
            }
        }
    }
}
