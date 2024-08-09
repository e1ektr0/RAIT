using System.Xml.Serialization;

namespace RAIT.Core.XmlDoc;

[XmlRoot(ElementName="member")]
public class Member { 

    [XmlElement(ElementName="summary")] 
    public string? Summary { get; set; } 

    [XmlAttribute(AttributeName="name")] 
    public string? Name { get; set; } 

    [XmlText] 
    public string? Text { get; set; } 

    [XmlElement(ElementName="example")] 
    public string? Example { get; set; } 
}