﻿using BlazorBoilerplate.Infrastructure.Storage.DataInterfaces;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using Finbuckle.MultiTenant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BlazorBoilerplate.EntityGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembly assembly = typeof(ApplicationUser).GetTypeInfo().Assembly;

            foreach (Type type in assembly.GetTypes().Where(t => t.Namespace == typeof(ApplicationUser).Namespace && t.IsPublic))
            {
                var iProperties = new StringBuilder();
                var properties = new StringBuilder();

                foreach (var prop in type.GetProperties().Where(p => !p.GetCustomAttributes().Any(a => a.GetType().Name == "JsonIgnoreAttribute")))
                {
                    var propertyType = prop.PropertyType;
                    var setter = @"
            set { SetValue(value); }";

                    string propertyTypeName = propertyType.Name;

                    if (propertyType.IsGenericType)
                    {
                        propertyTypeName = propertyType.GetGenericArguments()[0].Name;

                        if (propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            propertyTypeName = $"{propertyTypeName}?";
                        else if (propertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                        {
                            iProperties.Append(@$"
        ICollection<I{propertyTypeName}> {prop.Name} {{ get; }}
");
                            properties.Append(@$"
        ICollection<I{propertyTypeName}> I{type.Name}.{prop.Name} {{ get => {prop.Name}.Select(i => (I{propertyTypeName})i).ToList(); }}
");
                            propertyTypeName = $"NavigationSet<{propertyTypeName}>";
                            setter = string.Empty;
                        }
                    }

                    if (!propertyType.IsGenericType || propertyType.GetGenericTypeDefinition() != typeof(ICollection<>))
                    {
                        var iPropertyTypeName = propertyTypeName;

                        if (propertyType.Namespace == typeof(ApplicationUser).Namespace)
                        {
                            iPropertyTypeName = $"I{propertyTypeName}";
                            properties.Append(@$"
        {iPropertyTypeName} I{type.Name}.{prop.Name} {{ get => {prop.Name}; set => {prop.Name} = ({propertyTypeName})value; }}
");
                        }

                        iProperties.Append(@$"
        {iPropertyTypeName} {prop.Name} {{ get; set; }}
");
                    }
                    properties.Append(@$"
        public {propertyTypeName} {prop.Name}
        {{
            get {{ return GetValue<{propertyTypeName}>(); }}{setter}
        }}
");
                }

                var iEntity = @$"//Autogenerated by BlazorBoilerplate.EntityGenerator
using BlazorBoilerplate.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BlazorBoilerplate.Shared.DataInterfaces
{{
    public interface I{type.Name}
    {{{iProperties}
    }}
}}
";
                var breezeEntity = @$"//Autogenerated by BlazorBoilerplate.EntityGenerator
using BlazorBoilerplate.Constants;
using BlazorBoilerplate.Shared.DataInterfaces;
using Breeze.Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BlazorBoilerplate.Shared.Dto.Db
{{
    public partial class {type.Name} : BaseEntity, I{type.Name}
    {{{properties}
    }}
}}
";
                //args[0]=$(SolutionDir) in Visual Studio Project Properties -> Debug -> Application Arguments
                File.WriteAllText(@$"{args[0]}\Shared\BlazorBoilerplate.Shared.DataInterfaces\I{type.Name}.g.cs", iEntity);
                File.WriteAllText(@$"{args[0]}\Shared\BlazorBoilerplate.Shared\Dto\Db\{type.Name}.g.cs", breezeEntity);
            }
        }
    }
}
