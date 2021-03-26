using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkMpn.CustomActionToApiConverter
{
    class CustomAction
    {
        [Category("General")]
        [ReadOnly(true)]
        public string Name { get; set; }

        private bool ShouldSerializeName() => false;

        [Category("General")]
        [ReadOnly(true)]
        [DisplayName("Message Name")]
        public string MessageName { get; set; }

        private bool ShouldSerializeMessageName() => false;

        [Category("General")]
        [ReadOnly(true)]
        public string Description { get; set; }

        private bool ShouldSerializeDescription() => false;

        [Category("General")]
        [ReadOnly(true)]
        [DisplayName("Bound To")]
        public string PrimaryEntity { get; set; }

        private bool ShouldSerializePrimaryEntity() => false;

        [Category("Parameters")]
        [ReadOnly(true)]
        [DisplayName("Request Parameters")]
        public ParameterCollection<RequestParameter> RequestParameters { get; set; }

        private bool ShouldSerializeRequestParameters() => false;

        [Category("Parameters")]
        [ReadOnly(true)]
        [DisplayName("Response Parameters")]
        public ParameterCollection<ResponseParameter> ResponseParameters { get; set; }

        private bool ShouldSerializeResponseParameters() => false;
    }

    [TypeConverter(typeof(ExpandableCollectionConverter))]
    class ParameterCollection<T> : List<T> where T : Parameter
    {
        public ParameterCollection(IEnumerable<T> list) : base(list)
        {
        }
    }

    class ExpandableCollectionConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var collection = (System.Collections.ICollection)value;
            return $"({collection.Count} parameter{(collection.Count == 1 ? "" : "s")})";
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context) => true;

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var parameters = (IEnumerable<Parameter>)value;
            var properties = parameters.Select(param => new ExpandableCollectionItem(value, param));

            return new PropertyDescriptorCollection(properties.ToArray());
        }
    }

    class ExpandableCollectionItem : PropertyDescriptor
    {
        private readonly object _collection;
        private readonly Parameter _item;

        public ExpandableCollectionItem(object collection, Parameter item): base(item.Name, Attribute.GetCustomAttributes(item.GetType()))
        {
            _collection = collection;
            _item = item;
        }

        public override Type ComponentType => _collection.GetType();

        public override bool IsReadOnly => true;

        public override Type PropertyType => _item.GetType();

        public override bool CanResetValue(object component) => false;

        public override object GetValue(object component) => _item;

        public override void ResetValue(object component) => throw new NotImplementedException();

        public override void SetValue(object component, object value) => throw new NotImplementedException();

        public override bool ShouldSerializeValue(object component) => false;
    }

    [TypeConverter(typeof(ParameterConverter))]
    class Parameter
    {
        [Browsable(false)]
        public string Name { get; set; }

        [Browsable(false)]
        public Type Type { get; set; }

        [ReadOnly(true)]
        public string BindingInformation { get; set; }

        private bool ShouldSerializeBindingInformation() => false;
    }

    class RequestParameter : Parameter
    {
        [ReadOnly(true)]
        public bool Required { get; set; }

        private bool ShouldSerializeRequired() => false;
    }

    class ResponseParameter : Parameter
    {
    }

    class ParameterConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var param = (Parameter)value;

            // TODO: Use XRM-specific language to match the type names shown in the Custom Action editor UI
            return param.Type.Name;
        }
    }
}
