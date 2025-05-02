using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourceGenerator.SubModules;
public class UIGenerator
{
    public static void GenerateUI(SourceProductionContext context, ITypeSymbol type, AttributeData attr, out string log)
    {
        log = "Hello for GenerateUI";
    }
}
    

