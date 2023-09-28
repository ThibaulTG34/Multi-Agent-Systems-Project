using UnityEngine;
public class RandomPlane : MonoBehaviour
{
    public GameObject prefab;
    public int nbObjects = 10;
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;
    public float y = 1.2f;
    void Start()
    {
        for (int i = 0; i < nbObjects; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(minX, maxX), y, Random.Range(minZ, maxZ));
            Instantiate(prefab, randomPos, Quaternion.identity);
        }
    }
    void Update()
    {

    }
}