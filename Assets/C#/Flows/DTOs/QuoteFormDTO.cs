using System;
using System.Collections.Generic;

/// <summary>
/// 销售报价单数据模型：替代脆弱的 string[] 数组
/// 类型安全、自动映射、兼容旧 UI
/// </summary>
[Serializable]
public class QuoteFormDTO
{
    public string BillNo;
    public string Date;
    public string CustomerName;
    public string Currency;
    public string PriceList;
    public string DiscountList;
    public string SalesGroup;
    public string Salesperson;
    public string Status;
    public string ValidUntil;
    public string Approver;
    public string Remark;

    public List<ProductLineDTO> Products = new List<ProductLineDTO>();

    public string[] ToInputBoxArray()
    {
        return new string[]
        {
            BillNo, Date, CustomerName, Currency, PriceList, DiscountList,
            SalesGroup, Salesperson, Status, ValidUntil, Status, Salesperson, Remark
        };
    }

    public static QuoteFormDTO FromInputBoxArray(string[] arr)
    {
        if (arr == null || arr.Length < 13) return new QuoteFormDTO();
        return new QuoteFormDTO
        {
            BillNo = arr[0],
            Date = arr[1],
            CustomerName = arr[2],
            Currency = arr[3],
            PriceList = arr[4],
            DiscountList = arr[5],
            SalesGroup = arr[6],
            Salesperson = arr[7],
            Status = arr[8],
            ValidUntil = arr[9],
            Approver = arr[10],
            Remark = arr[12]
        };
    }
}

[Serializable]
public class ProductLineDTO
{
    public string ProductCode, ProductName, Model, Unit;
    public float Quantity, UnitPrice, TotalAmount;
    public float TaxRate, DiscountRate;
    public string DeliveryDate, Remark;
}