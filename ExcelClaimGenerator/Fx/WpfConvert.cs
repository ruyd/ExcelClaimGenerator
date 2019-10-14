using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ExcelClaimGenerator
{
    public class GreaterThanToVisibilityConverter : MarkupExtension, IValueConverter
    {
        private static GreaterThanToVisibilityConverter _converter = null;

        public GreaterThanToVisibilityConverter()
        { }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = (int)(value) == 0 ? Visibility.Collapsed : Visibility.Visible;
            return result;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = (Visibility)(value) == Visibility.Collapsed ? 0 : 1;
            return result;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new GreaterThanToVisibilityConverter();
            return _converter;
        }
    }
    public class TextEqualsToBoolConverter : MarkupExtension, IValueConverter
    {
        private static TextEqualsToBoolConverter _converter = null;

        public TextEqualsToBoolConverter()
        { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value != null && parameter != null && value.ToString() == parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new TextEqualsToBoolConverter();
            return _converter;
        }
    }
    public class BoolToCollapseConverter : MarkupExtension, IValueConverter
    {
        private static BoolToCollapseConverter _converter;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new BoolToCollapseConverter();
            return _converter;
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? boolParameter = null;
            if (parameter != null)
            {
                bool tmp;
                if (bool.TryParse(parameter.ToString(), out tmp))
                    boolParameter = tmp;
            }

            if (targetType == typeof(Visibility) || targetType == typeof(Visibility?))
            {
                if (boolParameter.HasValue)
                    return ((value is bool && (bool)value) == boolParameter.Value) ? Visibility.Visible : Visibility.Collapsed;
                else
                    return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                if (boolParameter.HasValue)
                    return (((Visibility)value == Visibility.Visible) == boolParameter.Value) ? true : false;
                else
                    return (Visibility)value == Visibility.Visible ? true : false;
            }

            return value;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? boolParameter = null;
            if (parameter != null)
            {
                bool tmp;
                if (bool.TryParse(parameter.ToString(), out tmp))
                    boolParameter = tmp;
            }

            if (targetType == typeof(Visibility) || targetType == typeof(Visibility?))
            {
                if (boolParameter.HasValue)
                    return ((value != null && (bool)value == boolParameter.Value)) ? Visibility.Visible : Visibility.Collapsed;
                else
                    return (value != null && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                if (boolParameter.HasValue)
                    return (((Visibility)value == Visibility.Visible) == boolParameter.Value) ? true : false;
                else
                    return (Visibility)value == Visibility.Visible ? true : false;
            }

            return value;
        }
    }
    public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
    {
        private static BoolToVisibilityConverter _converter;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new BoolToVisibilityConverter();
            return _converter;
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool? boolParameter = null;
            if (parameter != null)
            {
                bool tmp;
                if (bool.TryParse(parameter.ToString(), out tmp))
                    boolParameter = tmp;
            }

            if (targetType == typeof(Visibility) || targetType == typeof(Visibility?))
            {
                if (boolParameter.HasValue)
                    return ((value is bool && (bool)value) == boolParameter.Value) ? Visibility.Visible : Visibility.Hidden;
                else
                    return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Hidden;
            }

            if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                if (boolParameter.HasValue)
                    return (((Visibility)value == Visibility.Visible) == boolParameter.Value) ? true : false;
                else
                    return (Visibility)value == Visibility.Visible ? true : false;
            }

            return value;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool? boolParameter = null;
            if (parameter != null)
            {
                bool tmp;
                if (bool.TryParse(parameter.ToString(), out tmp))
                    boolParameter = tmp;
            }

            if (targetType == typeof(Visibility) || targetType == typeof(Visibility?))
            {
                if (boolParameter.HasValue)
                    return ((value != null && (bool)value == boolParameter.Value)) ? Visibility.Visible : Visibility.Hidden;
                else
                    return (value != null && (bool)value) ? Visibility.Visible : Visibility.Hidden;
            }

            if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                if (boolParameter.HasValue)
                    return (((Visibility)value == Visibility.Visible) == boolParameter.Value) ? true : false;
                else
                    return (Visibility)value == Visibility.Visible ? true : false;
            }

            return value;
        }
    }
    public class PictureConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var picture = value as dynamic;
            return picture == null ? null : picture.Data;
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte[] data = value as byte[];
            return data == null ? null : new { Data = data };
        }
    }
    public class InverseBoolConverter : MarkupExtension, IValueConverter
    {
        public InverseBoolConverter()
        { }
        private static InverseBoolConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new InverseBoolConverter();
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result;
            if (value != null && bool.TryParse(value.ToString(), out result))
                return !result;
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result;
            if (bool.TryParse(value.ToString(), out result))
                return !result;
            else
                return null;
        }
    }
    public class IsNullConverter : MarkupExtension, IValueConverter
    {
        private static IsNullConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new IsNullConverter();
            return _converter;
        }

        public IsNullConverter() { }

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Guid) return ((Guid)value == Guid.Empty);
            return (value == null);
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class IsNotNullConverter : MarkupExtension, IValueConverter
    {
        private static IsNotNullConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new IsNotNullConverter();
            return _converter;
        }

        public IsNotNullConverter() { }

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Guid) return ((Guid)value != Guid.Empty);
            return (value != null);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class MultipleBooleanTesterConverter : MarkupExtension, IMultiValueConverter
    {
        private static MultipleBooleanTesterConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new MultipleBooleanTesterConverter();
            return _converter;
        }
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var temp = parameter.ToString().Split(',');
            var tests = new List<bool>();
            foreach (var s in temp)
                tests.Add(bool.Parse(s));

            int failcount = 0;
            for (int i = 0; i < values.Length; i++)
            {
                var val = false;
                var test = tests[i];

                bool.TryParse(values[i].ToString(), out val);

                if (val != test)
                    failcount++;
            }

            return (failcount == 0);
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class NumberEqualsConverter : MarkupExtension, IValueConverter
    {
        private static NumberEqualsConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new NumberEqualsConverter();
            return _converter;
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal first;
            decimal second;
            if (value != null && decimal.TryParse(value.ToString(), out first) && decimal.TryParse(parameter.ToString(), out second))
            {
                return first == second;
            }

            return false;
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class NumberGreaterConverter : MarkupExtension, IValueConverter
    {
        private static NumberGreaterConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new NumberGreaterConverter();
            return _converter;
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal first;
            decimal second;
            if (value != null && decimal.TryParse(value.ToString(), out first) && decimal.TryParse(parameter.ToString(), out second))
            {
                return first > second;
            }

            return false;
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

