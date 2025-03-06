using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public class FHClientController : MonoBehaviour
{
    public FHTcpClient fhTcpClient;

    private bool exit;

    [SerializeField]
    private GameObject offLineStatue;

    public Action<DTOInfo> receiveData;

    public string ipHost = "127.0.0.1:4849";

    private void Update()
    {
    }

    // Start is called before the first frame update
    private void Start()
    {
        fhTcpClient = new FHTcpClient();

        fhTcpClient.InitConfig(ipHost);

        exit = false;
        fhTcpClient.FHTcpClientReceive = ReceiveData;
        fhTcpClient.Connected += ((client) =>
        {

        });

        if (offLineStatue != null)
        {
            StartCoroutine(OfflineStatueView());
        }
    }

    private static SemaphoreSlim semaphore = new SemaphoreSlim(1); // 限制同时进行的连接数

    private IEnumerator LoopReconnect()
    {
        while (!exit)
        {
            //等待一帧
            yield return new WaitForEndOfFrame();

            if (fhTcpClient != null && !fhTcpClient.IsOnline())
            {
                ThreadPool.QueueUserWorkItem(async state =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        fhTcpClient.StartConnect();
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
            }
            // 添加1秒的延迟
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator OfflineStatueView()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            bool isonline = fhTcpClient != null && fhTcpClient.IsOnline();
            offLineStatue.SetActive(!isonline);
        }
    }

    private void ReceiveData(DTOInfo info)
    {
        receiveData?.Invoke(info);
    }

    /// <summary>
    /// 转发对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dataTypeEnum"></param>
    /// <param name="orderTypeEnum"></param>
    /// <param name="data"></param>
    public void Send<T>(DataTypeEnum dataTypeEnum, OrderTypeEnum orderTypeEnum, T data)
    {
        fhTcpClient.Send(dataTypeEnum, orderTypeEnum, data);
    }

    public void Send<T>(OrderTypeEnum orderTypeEnum, T data)
    {
        fhTcpClient.Send(DataTypeEnum.S_MainHostOld, orderTypeEnum, data);
    }

    public void Send(OrderTypeEnum orderTypeEnum)
    {
        fhTcpClient.Send(DataTypeEnum.S_MainHostOld, orderTypeEnum, "");
    }

    public void SendHex(DataTypeEnum dataTypeEnum, OrderTypeEnum orderTypeEnum, string obj)
    {
        fhTcpClient.SendHexStr(dataTypeEnum, orderTypeEnum, obj);
    }
    private void OnEnable()
    {

        exit = false;
        StartCoroutine(LoopReconnect());
    }

    private void OnDisable()
    {
        exit = true;
        if (fhTcpClient != null)
        {
            fhTcpClient.Close();
        }
    }
}
