using UnityEngine;
using System.Collections.Generic;

public class SegmentSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject[] segmentPrefabs;

    [Header("Spawning")]
    [SerializeField] private float spawnAheadDistance = 15f;   
    public float minGap = 0.5f;
    public float maxGap = 1.5f;
    [SerializeField] private float minHeightOffset = -1.5f;
    [SerializeField] private float maxHeightOffset = 1.5f;
    [SerializeField] private float minSegmentY = -1f;
    [SerializeField] private float maxSegmentY = 1f;

    [Header("Cleanup")]
    [SerializeField] private float despawnBehindDistance = 25f; 

    private readonly List<GameObject> segments = new();

    private int lastIndex;

    private GameObject lastSegment;
    private Renderer lastRenderer;

    public void Initialize()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i]) Destroy(segments[i]);
        }
        segments.Clear();

        float firstSegmentY = Mathf.Clamp(player.position.y - 1f, minSegmentY, maxSegmentY);

        lastSegment = SpawnSegment(segmentPrefabs[0], new Vector3(player.position.x, firstSegmentY, 0f));
        lastIndex = 0;

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

        List<int> possibleIndices = new();

        if (lastIndex == 1 || lastIndex == 3)
        {
            possibleIndices.Add(0);
        }
        else
        {
            for (int i = 0; i < segmentPrefabs.Length;  i++)
            {
                if (lastIndex == i) continue;

                possibleIndices.Add(i);
            }
        }

        int ind = Random.Range(0, possibleIndices.Count);
        int index = possibleIndices[ind];

        GameObject prefab = segmentPrefabs[index];

        GameObject newSeg = Instantiate(prefab, transform);
        Renderer newRend = newSeg.GetComponent<Renderer>();

        float xSpawn = lastRenderer.bounds.max.x + (newRend.bounds.size.x / 2f) + gap;
        float ySpawn = Mathf.Clamp(lastSegment.transform.position.y + heightOffset, minSegmentY, maxSegmentY);

        newSeg.transform.position = new Vector3(xSpawn, ySpawn, 0f);

        segments.Add(newSeg);

        lastIndex = index;
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
