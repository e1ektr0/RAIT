using System.Reflection;

namespace RAIT.Core;

internal class RequestDetails
{
    public List<InputParameter> InputParameters { get; }
    public string Route { get; }
    public IEnumerable<CustomAttributeData> CustomAttributes { get; }

    public RequestDetails(List<InputParameter> inputParameters, string route,
        IEnumerable<CustomAttributeData> customAttributes)
    {
        InputParameters = inputParameters;
        Route = route;
        CustomAttributes = customAttributes;
    }
}