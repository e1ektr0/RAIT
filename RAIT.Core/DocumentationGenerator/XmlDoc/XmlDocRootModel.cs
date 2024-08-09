using System.Xml.Serialization;

namespace RAIT.Core.XmlDoc;

[XmlRoot(ElementName="doc")]
public class XmlDocRootModel { 

    [XmlElement(ElementName="assembly")] 
    public Assembly? Assembly { get; set; } 

    [XmlElement(ElementName="members")] 
    public Members? Members { get; set; } 
}