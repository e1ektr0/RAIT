using System.Xml.Serialization;

namespace RAIT.Core.DocumentationGenerator.XmlDoc;

[XmlRoot(ElementName="doc")]
public class XmlDoc { 

    [XmlElement(ElementName="assembly")] 
    public Assembly Assembly { get; set; } 

    [XmlElement(ElementName="members")] 
    public Members Members { get; set; } 
}