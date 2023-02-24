using System;
using System.Reflection;

namespace SimpleFFmpegGUI.WPF.Converters
{
    public static class AttributeHelper
    {
        public static TResult GetAttributeValue<T, TResult>(object obj, Func<T, TResult> valueConverter) where T : Attribute
        {
            MemberInfo[] member = obj.GetType().GetMember(obj.ToString());
            if (member != null && member.Length != 0)
            {
                object[] customAttributes = member[0].GetCustomAttributes(typeof(T), inherit: false);
                if (customAttributes != null && customAttributes.Length != 0)
                {
                    return valueConverter(customAttributes[0] as T);
                }
            }

            throw new Exception();
        }

    }
}