using System.Reflection;

namespace Cookbook.Infrastructure;

public class InfrastructureAssemblyReference
{
    public static Assembly Assembly
    {
        get => typeof(InfrastructureAssemblyReference).Assembly;
    }

    public static string AssemblyName
    {
        get => Assembly.GetName().Name ?? string.Empty;
    }
}