using System;
using System.Collections.Generic;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class SilogRequest
    {
        public UserInformation UserInformation { get; set; }
        public StationInformation StationInformation { get; set; }
        public InvoiceInformation InvoiceInformation { get; set; }
    }

    public class UserInformation
    {
        public int SucursalId { get; set; }
        public string UserIdent { get; set; }
        public string UserPassword { get; set; }
    }

    public class StationInformation
    {
        public string Dispenser { get; set; }
        public string Island { get; set; }
        public string Hose { get; set; }
    }

    public class InvoiceInformation
    {
        public string PosPrefix { get; set; }
        public int PosConsecutive { get; set; }
        public string InvoiceDate { get; set; }
        public string Details { get; set; }
        public VehicleInformation VehicleInformation { get; set; }
        public InvoiceHolderInformation InvoiceHolderInformation { get; set; }
        public List<ProductInformation> ProductInformation { get; set; }
        public PaymentInformation PaymentInformation { get; set; }
    }

    public class VehicleInformation
    {
        public string Plate { get; set; }
        public string LastMaintenanceDate { get; set; }
        public string NextMaintenanceDate { get; set; }
        public string Mileage { get; set; }
    }

    public class InvoiceHolderInformation
    {
        public int TypeId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string FirstLastName { get; set; }
        public string SecondLastName { get; set; }
        public string Adress { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class ProductInformation
    {
        public string ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Discunt { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public bool SkipAuditWarehouseValues { get; set; }
    }

    public class PaymentInformation
    {
        public int PaymentFormId { get; set; }
        public int PaymentMethodId { get; set; }
        public int PaymentMeanId { get; set; }
        public int CardId { get; set; }
        public int TransaccionNumber { get; set; }
    }
}
