using System.Xml.Serialization;

namespace Musiccast.Service
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:schemas-upnp-org:device-1-0")]
    [XmlRoot(Namespace = "urn:schemas-upnp-org:device-1-0", IsNullable = false, ElementName ="root")]
    public partial class DLNADescription
    {
        /// <remarks/>
        public rootSpecVersion specVersion { get; set; }

        /// <remarks/>
        public rootDevice device { get; set; }

        /// <remarks/>
        [XmlElement(Namespace = "urn:schemas-yamaha-com:device-1-0")]
        public X_device X_device { get; set; }

        [XmlIgnore]
        public string Location { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:schemas-upnp-org:device-1-0")]
    public partial class rootSpecVersion
    {

        /// <remarks/>
        public byte major { get; set; }

        /// <remarks/>
        public byte minor { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:schemas-upnp-org:device-1-0")]
    public partial class rootDevice
    {
        /// <remarks/>
        [XmlElement(Namespace = "urn:schemas-dlna-org:device-1-0")]
        public string X_DLNADOC { get; set; }

        /// <remarks/>
        public string deviceType { get; set; }

        /// <remarks/>
        public string friendlyName { get; set; }

        /// <remarks/>
        public string manufacturer { get; set; }

        /// <remarks/>
        public string manufacturerURL { get; set; }

        /// <remarks/>
        public string modelDescription { get; set; }

        /// <remarks/>
        public string modelName { get; set; }

        /// <remarks/>
        public string modelNumber { get; set; }

        /// <remarks/>
        public string modelURL { get; set; }

        /// <remarks/>
        public string serialNumber { get; set; }

        /// <remarks/>
        public string UDN { get; set; }

        /// <remarks/>
        [XmlArrayItem("icon", IsNullable = false)]
        public rootDeviceIcon[] iconList { get; set; }

        /// <remarks/>
        [XmlArrayItem("service", IsNullable = false)]
        public rootDeviceService[] serviceList { get; set; }

        /// <remarks/>
        public string presentationURL { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:schemas-upnp-org:device-1-0")]
    public partial class rootDeviceIcon
    {
        /// <remarks/>
        public string mimetype { get; set; }

        /// <remarks/>
        public byte width { get; set; }

        /// <remarks/>
        public byte height { get; set; }

        /// <remarks/>
        public byte depth { get; set; }

        /// <remarks/>
        public string url { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:schemas-upnp-org:device-1-0")]
    public partial class rootDeviceService
    {
        /// <remarks/>
        public string serviceType { get; set; }

        /// <remarks/>
        public string serviceId { get; set; }

        /// <remarks/>
        public string SCPDURL { get; set; }

        /// <remarks/>
        public string controlURL { get; set; }

        /// <remarks/>
        public string eventSubURL { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:schemas-yamaha-com:device-1-0")]
    [XmlRoot(Namespace = "urn:schemas-yamaha-com:device-1-0", IsNullable = false)]
    public partial class X_device
    {

        /// <remarks/>
        public string X_URLBase { get; set; }

        /// <remarks/>
        [XmlArrayItem("X_service", IsNullable = false)]
        public X_deviceX_service[] X_serviceList { get; set; }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true, Namespace = "urn:schemas-yamaha-com:device-1-0")]
    public partial class X_deviceX_service
    {

        /// <remarks/>
        public string X_specType { get; set; }

        /// <remarks/>
        public string X_yxcControlURL { get; set; }

        /// <remarks/>
        public string X_yxcVersion { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool X_yxcVersionSpecified { get; set; }

        /// <remarks/>
        public string X_controlURL { get; set; }

        /// <remarks/>
        public string X_unitDescURL { get; set; }
    }






}
