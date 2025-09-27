using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OnlineOrdering
{
    // ---------- Address ----------
    public class Address
    {
        private string _street;
        private string _city;
        private string _stateOrProvince;
        private string _country;

        public Address(string street, string city, string stateOrProvince, string country)
        {
            _street = street;
            _city = city;
            _stateOrProvince = stateOrProvince;
            _country = country;
        }

        // Propiedades (TitleCase) que encapsulan los campos privados
        public string Street { get => _street; set => _street = value; }
        public string City { get => _city; set => _city = value; }
        public string StateOrProvince { get => _stateOrProvince; set => _stateOrProvince = value; }
        public string Country { get => _country; set => _country = value; }

        public bool IsInUSA()
        {
            if (string.IsNullOrWhiteSpace(_country)) return false;
            var c = _country.Trim().ToLowerInvariant();
            return c == "usa" || c == "united states" || c == "united states of america" || c == "us";
        }

        public string ToMultiLine()
        {
            var sb = new StringBuilder();
            sb.AppendLine(_street);
            sb.AppendLine($"{_city}, {_stateOrProvince}");
            sb.Append(_country);
            return sb.ToString();
        }
    }

    // ---------- Customer ----------
    public class Customer
    {
        private string _name;
        private Address _address;

        public Customer(string name, Address address)
        {
            _name = name;
            _address = address;
        }

        public string Name { get => _name; set => _name = value; }
        public Address Address { get => _address; set => _address = value; }

        public bool LivesInUSA() => _address != null && _address.IsInUSA();

        public string GetShippingLabel()
        {
            var sb = new StringBuilder();
            sb.AppendLine(_name);
            sb.Append(_address.ToMultiLine());
            return sb.ToString();
        }
    }

    // ---------- Product ----------
    public class Product
    {
        private string _name;
        private string _productId;
        private decimal _pricePerUnit;
        private int _quantity;

        public Product(string name, string productId, decimal pricePerUnit, int quantity)
        {
            _name = name;
            _productId = productId;
            _pricePerUnit = pricePerUnit;
            _quantity = quantity;
        }

        public string Name { get => _name; set => _name = value; }
        public string ProductId { get => _productId; set => _productId = value; }
        public decimal PricePerUnit { get => _pricePerUnit; set => _pricePerUnit = value; }
        public int Quantity { get => _quantity; set => _quantity = value; }

        public decimal TotalCost() => _pricePerUnit * _quantity;
    }

    // ---------- Order ----------
    public class Order
    {
        private readonly List<Product> _products = new List<Product>();
        private Customer _customer;

        public Order(Customer customer) => _customer = customer;

        public Customer Customer { get => _customer; set => _customer = value; }

        public void AddProduct(Product product)
        {
            if (product != null) _products.Add(product);
        }

        public string GetPackingLabel()
        {
            var sb = new StringBuilder();
            sb.AppendLine("PACKING LABEL");
            foreach (var p in _products)
            {
                sb.AppendLine($"{p.Name} (ID: {p.ProductId})");
            }
            return sb.ToString();
        }

        public string GetShippingLabel()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SHIPPING LABEL");
            sb.Append(_customer.GetShippingLabel());
            return sb.ToString();
        }

        public decimal ComputeTotal()
        {
            decimal subtotal = 0m;
            foreach (var p in _products)
            {
                subtotal += p.TotalCost();
            }

            decimal shipping = _customer.LivesInUSA() ? 5m : 35m;
            return subtotal + shipping;
        }
    }

    // ---------- Program ----------
    class Program
    {
        static void Main()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            // Order 1: USA customer
            var address1 = new Address("123 Apple St.", "Provo", "UT", "USA");
            var customer1 = new Customer("Taylor Jenkins", address1);

            var order1 = new Order(customer1);
            order1.AddProduct(new Product("Widget", "WID-1001", 12.99m, 2));
            order1.AddProduct(new Product("Cable", "CAB-2002", 4.49m, 3));

            // Order 2: International customer
            var address2 = new Address("456 Maple Ave.", "Toronto", "ON", "Canada");
            var customer2 = new Customer("Avery Park", address2);

            var order2 = new Order(customer2);
            order2.AddProduct(new Product("Gaming Mouse", "GM-3003", 39.95m, 1));
            order2.AddProduct(new Product("Mouse Pad", "MP-4004", 9.99m, 2));
            order2.AddProduct(new Product("USB Hub", "USB-5005", 14.50m, 1));

            PrintOrder(order1);
            Console.WriteLine(new string('-', 40));
            PrintOrder(order2);
        }

        private static void PrintOrder(Order order)
        {
            Console.WriteLine(order.GetPackingLabel());
            Console.WriteLine(order.GetShippingLabel());
            Console.WriteLine($"Total: {order.ComputeTotal():C}");
        }
    }
}
