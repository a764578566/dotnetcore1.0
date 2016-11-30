using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TangChao.JiZhi.Pos.DAL
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class IgnorePropertyAttribute : Attribute
    {
        public IgnorePropertyAttribute(bool ignore)
        {
            Value = ignore;
        }
        public bool Value { get; set; }
    }
}
