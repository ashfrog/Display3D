using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 校企合作展示块动态生成
/// </summary>
public class AsteroidSpawner : MonoBehaviour
{
    [Header("Asteroid Settings")]
    public DisplayBox displayBox;   // 陨石预制体
    public int asteroidCount = 50;      // 一次生成的陨石数量
    public float spawnZ = 700f;         // 陨石初始 z 坐标
    public Vector2 spawnArea = new Vector2(200f, 200f); // x,y生成范围

    [Header("Movement Settings")]
    public float minSpeed = 50f;  // 最小移动速度 (z轴方向)
    public float maxSpeed = 120f; // 最大移动速度 (z轴方向)

    [Header("Spawn Frequency")]
    public float spawnInterval = 2f; // 生成周期（秒）

    private float timer;


    DataSet dataSet;
    DataTable dataTable;
    [SerializeField]
    List<Texture2D> texture2Ds = new List<Texture2D>();

    int generateIndex;
    private void Start()
    {
        dataSet = ExcelReader.ReadExcel("校企合作.xlsx");
        dataTable = dataSet.Tables[0];
        foreach (DataRow row in dataTable.Rows)
        {
            try
            {
                string file = System.IO.Path.Combine(Application.streamingAssetsPath, ExcelReader.dataFolder, row[0].ToString());
                if (FileUtils.IsImgFile(file))
                {
                    Texture2D texture2D = LoadTexture(file);
                    texture2Ds.Add(texture2D);
                }
                else
                {
                    Debug.Log(file + "不存在");
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
    private Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

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
            DisplayBox asteroid = Instantiate(displayBox, spawnPos, Quaternion.identity);
            if (generateIndex >= texture2Ds.Count)
            {
                generateIndex = 0;
            }
            //展示图片
            asteroid.SetImg(texture2Ds[generateIndex]);
            generateIndex++;
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