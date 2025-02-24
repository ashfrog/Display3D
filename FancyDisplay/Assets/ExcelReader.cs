using UnityEngine;
using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using ExcelDataReader;
using System.Text;

public class ExcelReader : MonoBehaviour
{
    [SerializeField]
    private string excelFileName = "data.xlsx"; // Excel文件名

    private string excelPath;
    private DataSet dataSet;

    private void Start()
    {
        // 设置Excel文件路径
        excelPath = Path.Combine(Application.streamingAssetsPath, excelFileName);

        // 读取Excel文件
        ReadExcel();
    }

    private void ReadExcel()
    {
        try
        {
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

                    // 处理Excel数据
                    ProcessExcelData();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"读取Excel文件时发生错误: {e.Message}");
        }
    }

    private void ProcessExcelData()
    {
        if (dataSet == null || dataSet.Tables.Count == 0)
        {
            Debug.LogError("没有找到Excel数据");
            return;
        }

        // 遍历所有工作表
        foreach (DataTable table in dataSet.Tables)
        {
            Debug.Log($"处理工作表: {table.TableName}");

            // 遍历所有行
            foreach (DataRow row in table.Rows)
            {
                // 示例：打印每一行的数据
                string rowData = "";
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    rowData += $"{table.Columns[i].ColumnName}: {row[i]}, ";
                }
                Debug.Log(rowData);

                // 在这里处理你的数据
                // 例如: 创建游戏对象、更新UI等
            }
        }
    }

    // 示例：获取指定单元格的值
    public string GetCellValue(int sheetIndex, int row, int column)
    {
        try
        {
            if (dataSet != null && dataSet.Tables.Count > sheetIndex)
            {
                DataTable table = dataSet.Tables[sheetIndex];
                if (table.Rows.Count > row && table.Columns.Count > column)
                {
                    return table.Rows[row][column].ToString();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"获取单元格值时发生错误: {e.Message}");
        }
        return string.Empty;
    }

    // 示例：根据列名获取某一列的所有值
    public List<string> GetColumnData(int sheetIndex, string columnName)
    {
        List<string> columnData = new List<string>();
        try
        {
            if (dataSet != null && dataSet.Tables.Count > sheetIndex)
            {
                DataTable table = dataSet.Tables[sheetIndex];
                if (table.Columns.Contains(columnName))
                {
                    foreach (DataRow row in table.Rows)
                    {
                        columnData.Add(row[columnName].ToString());
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"获取列数据时发生错误: {e.Message}");
        }
        return columnData;
    }
}