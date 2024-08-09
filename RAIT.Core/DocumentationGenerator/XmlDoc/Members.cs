using System.Xml.Serialization;

namespace RAIT.Core.XmlDoc;

[XmlRoot(ElementName="members")]
public class Members { 

    [XmlElement(ElementName="member")] 
    public List<Member>? Member { get; set; } 
}