using System.Xml.Serialization;

namespace RAIT.Core.XmlDoc;

[XmlRoot(ElementName="assembly")]
public class Assembly { 

    [XmlElement(ElementName="name")] 
    public string? Name { get; set; } 
}