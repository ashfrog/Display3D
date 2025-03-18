using UnityEngine;
using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using ExcelDataReader;
using System.Text;
using Unity.VisualScripting;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// 将Excel文件读取为DataSet
/// </summary>
public class ExcelReader
{
    public const string dataFolder = "data"; // Excel文件夹名
    public static string excelFileName; // Excel文件名

    public static DataSet ReadExcel(string xlsFileName = "data.xlsx")
    {
        excelFileName = xlsFileName;
        DataSet dataSet = null;
        try
        {
            // 设置Excel文件路径
            string excelPath = Path.Combine(Application.streamingAssetsPath, dataFolder, excelFileName);

            using (var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read))
            {
                // 创建Excel读取器
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // 将Excel数据读入DataSet
                    dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true // 使用第一行作为列名
                        }
                    });

                    return dataSet;
                }
            }
        }
        catch (Exception e)
        {
            string errorMessage = $"读取Excel文件时发生错误: {e.Message}";
            Debug.LogError(errorMessage);
        }
        return dataSet;
    }

    public static DateTime? TryParseExcelDate(object cellValue)
    {
        try
        {
            switch (cellValue)
            {
                case double d:
                    return DateTime.FromOADate(d);
                case DateTime dt:
                    return dt;
                case string s when DateTime.TryParse(s, out var parsedDate):
                    return parsedDate;
                default:
                    return null;
            }
        }
        catch
        {
            return null;
        }
    }

}
