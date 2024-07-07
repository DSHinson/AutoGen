using System;
using System.Collections.Generic;
using System.Text;

namespace Generator.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class Autogen : Attribute
    {
    }
}
