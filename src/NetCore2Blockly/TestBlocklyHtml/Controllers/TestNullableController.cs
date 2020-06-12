﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using TestBlocklyHtml.resolveAtRuntime;

namespace TestBlocklyHtml.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class VariousTestsController : ControllerBase
    {
        
        public VariousTestsController()
        {
        }

        [HttpGet("{id?}")]
        public string ActionWithNullParameter(int? id)
        {
            return (id == null) ? "from GET no parameter" : $"from GET parameter {id}";
        }

        [HttpPatch("{id?}")]
        public string ActionWithNullParameterPATCH(int? id)
        {
            return (id == null) ? "from PATCH no parameter" : $"from PATCH parameter {id}";
        }
        [HttpGet("{id}")]
        public string ActionWith2ParametersAndARoute(int id, int x, int y)
        {
            return $"received route {id} and parameters {x} {y}";
        }

        [HttpPost()]
        public string ActionWithDictionary([FromBody]Dictionary<string, string> id)
        {
            var str =
                string.Join(",",
                id.Select(it => it.Key + "= " + it.Value)
                );
            return $"received {str}";
        }

        [HttpGet]
        public bool AddRuntimeController([FromServices] ApplicationPartManager partManager, [FromServices]MyActionDescriptorChangeProvider provider)
        {
            var ass = CreateController("andrei");
            
            if (ass != null)
            {
                partManager.ApplicationParts.Add(new AssemblyPart(ass));
                // Notify change
                provider.HasChanged = true;
                provider.TokenSource.Cancel();
                return true;
            }
            return false;
        }

        private Assembly CreateController(string name)
        {
            
            string code = new StringBuilder()
                .AppendLine("using System;")
                .AppendLine("using Microsoft.AspNetCore.Mvc;")
                .AppendLine("namespace TestBlocklyHtml.Controllers")
                .AppendLine("{")
                .AppendLine("[Route(\"api/[controller]\")]")
                .AppendLine("[ApiController]")
                .AppendLine(string.Format("public class {0} : ControllerBase", name))
            
                .AppendLine(" {")
                .AppendLine("  public string Get()")
                .AppendLine("  {")
                .AppendLine(string.Format("return \"test - {0}\";", name))
                .AppendLine("  }")
                .AppendLine(" }")
                .AppendLine("}")
                .ToString();

            var codeString = SourceText.From(code);
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7_3);

            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(RouteAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ApiControllerAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ControllerBase).Assembly.Location),
            };

            var codeRun = CSharpCompilation.Create("Hello.dll",
                new[] { parsedSyntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
            using (var peStream = new MemoryStream())
            {
                if (!codeRun.Emit(peStream).Success)
                {
                    return null;
                }
                return Assembly.Load(peStream.ToArray());
            }



        }
        //var compilation = CSharpCompilation.Create("DynamicAssembly",
        //    new[] { CSharpSyntaxTree.ParseText(code) },
        //    new[] {
        //        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        //        MetadataReference.CreateFromFile(typeof(RemoteControllerFeatureProvider).Assembly.Location)
        //    },
        //    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        //var results = codeProvider.CompileAssemblyFromSource(parameters, code);
        //if (results.Errors.Count > 0)
        //{
        //    Console.WriteLine("Build Failed");
        //    foreach (CompilerError CompErr in results.Errors)
        //    {
        //        Console.WriteLine(
        //        "Line number " + CompErr.Line +
        //        ", Error Number: " + CompErr.ErrorNumber +
        //        ", '" + CompErr.ErrorText + ";" +
        //        Environment.NewLine + Environment.NewLine);
        //    }
        //}
        //else
        //{                
        //    return results.CompiledAssembly;

        //}

        //return null;
    }
}