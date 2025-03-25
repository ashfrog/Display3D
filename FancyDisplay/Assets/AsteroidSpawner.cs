using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Asteroid Settings")]
    public GameObject asteroidPrefab;   // 陨石预制体
    public int asteroidCount = 50;      // 一次生成的陨石数量
    public float spawnZ = 700f;         // 陨石初始 z 坐标
    public Vector2 spawnArea = new Vector2(200f, 200f); // x,y生成范围

    [Header("Movement Settings")]
    public float minSpeed = 50f;  // 最小移动速度 (z轴方向)
    public float maxSpeed = 120f; // 最大移动速度 (z轴方向)

    [Header("Spawn Frequency")]
    public float spawnInterval = 2f; // 生成周期（秒）

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnAsteroids();
            timer = 0f;
        }
    }

    void SpawnAsteroids()
    {
        for (int i = 0; i < asteroidCount; i++)
        {
            // 随机生成 x,y 坐标
            float randomX = Random.Range(-spawnArea.x / 2f, spawnArea.x / 2f);
            float randomY = Random.Range(-spawnArea.y / 2f, spawnArea.y / 2f);
            Vector3 spawnPos = new Vector3(randomX, randomY, spawnZ);

            // 创建陨石
            GameObject asteroid = Instantiate(asteroidPrefab, spawnPos, Quaternion.identity);

            // 随机给陨石设置一个速度
            float speed = Random.Range(minSpeed, maxSpeed);
            asteroid.AddComponent<AsteroidMover>().SetSpeed(speed);
        }
    }
}

// 用于控制陨石移动的脚本
public class AsteroidMover : MonoBehaviour
{
    private float moveSpeed;

    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }

    void Update()
    {
        // 沿 z 轴向相机方向移动
        transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);

        // 当陨石超出一定范围后销毁，避免堆积
        if (transform.position.z < -100f)
        {
            Destroy(gameObject);
        }
    }
}