using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;

    private Vector3 startPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.CurrentGameState != GameState.InGame) return;

        float distancePerFrame = speed * Time.deltaTime;

        transform.Translate(distancePerFrame, 0, 0);
    }

    public void ResetPlayer()
    {
        transform.position = startPosition;
    }
}
