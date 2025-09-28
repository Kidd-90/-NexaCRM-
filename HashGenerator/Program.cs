// See https://aka.ms/new-console-template for more information
using BCrypt.Net;

Console.WriteLine("Manager password hash:");
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("manager123"));

Console.WriteLine("\nSales password hash:");
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("sales123"));

Console.WriteLine("\nDevelop password hash:");
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("develop123"));
