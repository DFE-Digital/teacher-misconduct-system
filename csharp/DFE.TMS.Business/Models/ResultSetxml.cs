using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.TMS.Business.Models
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class resultset
    {

        private resultsetResult[] resultField;

        private byte morerecordsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("result")]
        public resultsetResult[] result
        {
            get
            {
                return this.resultField;
            }
            set
            {
                this.resultField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte morerecords
        {
            get
            {
                return this.morerecordsField;
            }
            set
            {
                this.morerecordsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class resultsetResult
    {

        private resultsetResultDuration durationField;

        private resultsetResultRank rankField;

        private resultsetResultTimezonecode timezonecodeField;

        private resultsetResultInnercalendarid innercalendaridField;

        private resultsetResultModifiedon modifiedonField;

        private resultsetResultCreatedon createdonField;

        private resultsetResultCreatedby createdbyField;

        private string calendarruleidField;

        private string patternField;

        private string businessunitidField;

        private resultsetResultExtentcode extentcodeField;

        private resultsetResultModifiedby modifiedbyField;

        private resultsetResultCalendarid calendaridField;

        private string organizationidField;

        private resultsetResultStarttime starttimeField;

        /// <remarks/>
        public resultsetResultDuration duration
        {
            get
            {
                return this.durationField;
            }
            set
            {
                this.durationField = value;
            }
        }

        /// <remarks/>
        public resultsetResultRank rank
        {
            get
            {
                return this.rankField;
            }
            set
            {
                this.rankField = value;
            }
        }

        /// <remarks/>
        public resultsetResultTimezonecode timezonecode
        {
            get
            {
                return this.timezonecodeField;
            }
            set
            {
                this.timezonecodeField = value;
            }
        }

        /// <remarks/>
        public resultsetResultInnercalendarid innercalendarid
        {
            get
            {
                return this.innercalendaridField;
            }
            set
            {
                this.innercalendaridField = value;
            }
        }

        /// <remarks/>
        public resultsetResultModifiedon modifiedon
        {
            get
            {
                return this.modifiedonField;
            }
            set
            {
                this.modifiedonField = value;
            }
        }

        /// <remarks/>
        public resultsetResultCreatedon createdon
        {
            get
            {
                return this.createdonField;
            }
            set
            {
                this.createdonField = value;
            }
        }

        /// <remarks/>
        public resultsetResultCreatedby createdby
        {
            get
            {
                return this.createdbyField;
            }
            set
            {
                this.createdbyField = value;
            }
        }

        /// <remarks/>
        public string calendarruleid
        {
            get
            {
                return this.calendarruleidField;
            }
            set
            {
                this.calendarruleidField = value;
            }
        }

        /// <remarks/>
        public string pattern
        {
            get
            {
                return this.patternField;
            }
            set
            {
                this.patternField = value;
            }
        }

        /// <remarks/>
        public string businessunitid
        {
            get
            {
                return this.businessunitidField;
            }
            set
            {
                this.businessunitidField = value;
            }
        }

        /// <remarks/>
        public resultsetResultExtentcode extentcode
        {
            get
            {
                return this.extentcodeField;
            }
            set
            {
                this.extentcodeField = value;
            }
        }

        /// <remarks/>
        public resultsetResultModifiedby modifiedby
        {
            get
            {
                return this.modifiedbyField;
            }
            set
            {
                this.modifiedbyField = value;
            }
        }

        /// <remarks/>
        public resultsetResultCalendarid calendarid
        {
            get
            {
                return this.calendaridField;
            }
            set
            {
                this.calendaridField = value;
            }
        }

        /// <remarks/>
        public string organizationid
        {
            get
            {
                return this.organizationidField;
            }
            set
            {
                this.organizationidField = value;
            }
        }

        /// <remarks/>
        public resultsetResultStarttime starttime
        {
            get
            {
                return this.starttimeField;
            }
            set
            {
                this.starttimeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class resultsetResultDuration
    {

        private string formattedvalueField;

        private ushort valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string formattedvalue
        {
            get
            {
                return this.formattedvalueField;
            }
            set
            {
                this.formattedvalueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public ushort Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class resultsetResultRank
    {

        private byte formattedvalueField;

        private byte valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte formattedvalue
        {
            get
            {
                return this.formattedvalueField;
            }
            set
            {
                this.formattedvalueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public byte Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class resultsetResultTimezonecode
    {

        private byte formattedvalueField;

        private byte valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte formattedvalue
        {
            get
            {
                return this.formattedvalueField;
            }
            set
            {
                this.formattedvalueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public byte Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class resultsetResultInnercalendarid
    {

        private ushort typeField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class resultsetResultModifiedon
    {

        private string dateField;

        private string timeField;

        private System.DateTime valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string date
        {
            get
            {
                return this.dateField;
            }
            set
            {
                this.dateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public System.DateTime Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class resultsetResultCreatedon
    {

        private string dateField;

        private string timeField;

        private System.DateTime valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string date
        {
            get
            {
                return this.dateField;
            }
            set
            {
                this.dateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public System.DateTime Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class resultsetResultCreatedby
    {

        private string dscField;

        private string nameField;

        private string yomiField;

        private byte typeField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string dsc
        {
            get
            {
                return this.dscField;
            }
            set
            {
                this.dscField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string yomi
        {
            get
            {
                return this.yomiField;
            }
            set
            {
                this.yomiField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class resultsetResultExtentcode
    {

        private byte formattedvalueField;

        private byte valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte formattedvalue
        {
            get
            {
                return this.formattedvalueField;
            }
            set
            {
                this.formattedvalueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public byte Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class resultsetResultModifiedby
    {

        private string dscField;

        private string nameField;

        private string yomiField;

        private byte typeField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string dsc
        {
            get
            {
                return this.dscField;
            }
            set
            {
                this.dscField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string yomi
        {
            get
            {
                return this.yomiField;
            }
            set
            {
                this.yomiField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class resultsetResultCalendarid
    {

        private ushort typeField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class resultsetResultStarttime
    {

        private string dateField;

        private string timeField;

        private System.DateTime valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string date
        {
            get
            {
                return this.dateField;
            }
            set
            {
                this.dateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public System.DateTime Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }



}
