using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace MAF.CustomerSupport.Tools;

/// <summary>
/// Customer Support Tools for the agent
/// </summary>
public static class CustomerSupportTools
{
    private static readonly Dictionary<string, OrderInfo> Orders = new()
    {
        ["ORD-001"] = new("ORD-001", "Shipped", "John Doe", "2025-01-15"),
        ["ORD-002"] = new("ORD-002", "Processing", "Jane Smith", "2025-01-18"),
        ["ORD-003"] = new("ORD-003", "Delivered", "Bob Wilson", "2025-01-10"),
    };

    private static readonly Dictionary<string, CustomerInfo> Customers = new()
    {
        ["john@example.com"] = new("CUST-001", "John Doe", "john@example.com", "Gold"),
        ["jane@example.com"] = new("CUST-002", "Jane Smith", "jane@example.com", "Silver"),
    };

    [Description("Look up order status by order number")]
    public static string LookupOrder(
        [Description("Order number (e.g., ORD-001)")] string orderNumber)
    {
        if (Orders.TryGetValue(orderNumber.ToUpper(), out var order))
        {
            return $"""
                Order Found:
                - Order Number: {order.OrderNumber}
                - Status: {order.Status}
                - Customer: {order.CustomerName}
                - Expected Delivery: {order.DeliveryDate}
                """;
        }
        return $"Order {orderNumber} not found in our system.";
    }

    [Description("Look up customer by email address")]
    public static string LookupCustomer(
        [Description("Customer email address")] string email)
    {
        if (Customers.TryGetValue(email.ToLower(), out var customer))
        {
            return $"""
                Customer Found:
                - ID: {customer.CustomerId}
                - Name: {customer.Name}
                - Email: {customer.Email}
                - Tier: {customer.Tier}
                """;
        }
        return $"Customer with email {email} not found.";
    }

    [Description("Create a support ticket for issues requiring human attention")]
    public static string CreateSupportTicket(
        [Description("Issue description")] string description,
        [Description("Customer email")] string customerEmail,
        [Description("Priority: Low, Medium, High")] string priority = "Medium")
    {
        var ticketId = $"TKT-{DateTime.Now:yyyyMMddHHmmss}";
        return $"""
            Support Ticket Created:
            - Ticket ID: {ticketId}
            - Customer: {customerEmail}
            - Priority: {priority}
            - Description: {description}
            - Status: Open
            
            A support representative will contact you within 24 hours.
            """;
    }

    [Description("Get FAQ answer for common topics")]
    public static string GetFAQ(
        [Description("Topic: shipping, returns, payments, account")] string topic)
    {
        return topic.ToLower() switch
        {
            "shipping" => "Standard shipping takes 5-7 business days. Express shipping (2-3 days) is available for $9.99.",
            "returns" => "We accept returns within 30 days of purchase. Items must be unused and in original packaging.",
            "payments" => "We accept Visa, Mastercard, American Express, PayPal, and Apple Pay.",
            "account" => "You can manage your account at account.techcorp.com. Reset password via the 'Forgot Password' link.",
            _ => $"I don't have FAQ information for '{topic}'. Please ask about: shipping, returns, payments, or account."
        };
    }
}

public record OrderInfo(string OrderNumber, string Status, string CustomerName, string DeliveryDate);
public record CustomerInfo(string CustomerId, string Name, string Email, string Tier);
