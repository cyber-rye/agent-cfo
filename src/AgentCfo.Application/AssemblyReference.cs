using System.Reflection;

namespace AgentCfo.Application;

public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
