using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] private GameObject backgroundPrefab;
    [SerializeField] private Camera cam;
    [SerializeField] private float xBuffer = 3f;

    private Transform lastBackground;
    private Renderer lastRenderer;
    private float backgroundWidth;
    private float nextSpawnAtCamRightX;

    private List<GameObject> backgrounds = new List<GameObject>();

    private int objectPoolSize = 3;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (!cam) cam = Camera.main;
        if (!backgroundPrefab || !cam) return;

        if (backgrounds.Count == 0)
        {
            for (int i = 0; i < objectPoolSize; i++)
            {
                GameObject go = Instantiate(backgroundPrefab, transform);
                ReturnToPool(go);
                backgrounds.Add(go);
            }
        }

        foreach (GameObject go in backgrounds)
        {
            if (!go) continue;

            ReturnToPool(go);
            go.transform.position = transform.position;
        }

        GameObject nextBackground = GetNextObject();
        if (nextBackground == null) return;

        lastBackground = nextBackground.transform;
        lastBackground.position = transform.position;
        lastRenderer = lastBackground.GetComponent<Renderer>();
        backgroundWidth = lastRenderer.bounds.size.x;
        lastRenderer.sortingOrder = 0;

        UpdateNextSpawnTrigger();
    }

    private void Update()
    {
        if (!cam || !lastRenderer) return;

        float halfCamWidth = cam.orthographicSize * cam.aspect;
        float camRightEdge = cam.transform.position.x + halfCamWidth;

        if (camRightEdge >= nextSpawnAtCamRightX)
        {
            SpawnNextToRight();
            UpdateNextSpawnTrigger();
        }
    }

    private void ReturnToPool(GameObject background)
    {
        background.SetActive(false);
    }

    private GameObject GetNextObject()
    {
        foreach (GameObject go in backgrounds)
        {
            if (!go.activeSelf)
            {
                go.SetActive(true);
                return go;
            }
        }
        return null;
    }

    private void SpawnNextToRight()
    {
        Vector3 spawnPos = lastBackground.position;
        spawnPos.x += backgroundWidth;

        GameObject nextBackground = GetNextObject();

        if (nextBackground == null)
        {
            float previousDistance = 0;
            float distance;
            GameObject objectToReturn = null;
            foreach (GameObject go in backgrounds)
            {
                distance = Vector3.Distance(go.transform.position, cam.transform.position); 

                if (distance > previousDistance)
                {
                    previousDistance = distance;
                    objectToReturn = go;
                }
            }
            ReturnToPool(objectToReturn);
            nextBackground = GetNextObject();
        }

        if (nextBackground == null) return;

        lastBackground = nextBackground.transform;
        lastBackground.position = spawnPos;
        lastRenderer = lastBackground.GetComponent<Renderer>();
        lastRenderer.sortingOrder = 0;
    }

    private void UpdateNextSpawnTrigger()
    {
        nextSpawnAtCamRightX = lastRenderer.bounds.max.x - xBuffer;
    }
}
