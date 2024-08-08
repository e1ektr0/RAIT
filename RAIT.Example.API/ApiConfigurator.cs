namespace RAIT.Example.API;

public static class ApiConfigurator
{
    public static Action<IMvcBuilder>? Configurate { get; set; }
    
}