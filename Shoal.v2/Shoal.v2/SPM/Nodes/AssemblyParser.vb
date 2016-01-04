Imports Microsoft.VisualBasic.Scripting.ShoalShell.HTML

Namespace SPM.Nodes

    Public Module AssemblyParser

        ''' <summary>
        ''' 解析出错或返回空集合并在终端上面打印出错误信息
        ''' </summary>
        ''' <param name="Path">不需要特殊处理，函数会自动转换为全路径</param>
        ''' <returns></returns>
        Public Function LoadAssembly(Path As String) As PartialModule()
            Dim Assembly As System.Reflection.Assembly

            Try
                Assembly = System.Reflection.Assembly.LoadFile(VisualBasic.FileIO.FileSystem.GetFileInfo(Path).FullName)
            Catch ex As Exception
                Return __exHandler(ex, Path)
            End Try

            Try
                Dim assemblyValue = SPM.Nodes.Assembly.CreateObject(Of Assembly)(Assembly)
                Dim Namespaces = (From Type In Assembly.DefinedTypes '.AsParallel
                                  Let [Namespace] = __getNamespaceEntry(Type, assemblyValue)  '得到原始的部分的模块定义
                                  Where Not [Namespace] Is Nothing
                                  Select [Namespace]).ToArray
                Return Namespaces
            Catch ex As Exception
                Return __exHandler(ex, Path)
            End Try
        End Function

        Private Function __exHandler(ex As Exception, path As String) As PartialModule()
            Call App.LogException($"Assembly Parsing Error:  {path.ToFileURL}".__DEBUG_ECHO, $"{NameOf(AssemblyParser)}::{NameOf(LoadAssembly)}")
            Call App.LogException(ex, $"{NameOf(AssemblyParser)}::{NameOf(LoadAssembly)}")
            Return New PartialModule() {}
        End Function

        Private Function __getNamespaceEntry(Type As Type, Assembly As Assembly) As SPM.Nodes.PartialModule
            If Not Type.IsClass Then
                Return Nothing
            End If

            Dim attrs As Object() =
                Type.GetCustomAttributes(
                    attributeType:=Microsoft.VisualBasic.Scripting.MetaData.PackageNamespace.TypeInfo,
                    inherit:=False)

            If attrs.IsNullOrEmpty Then
                attrs = (From ns As Object
                         In Type.GetCustomAttributes(
                             attributeType:=Microsoft.VisualBasic.CommandLine.Reflection.Namespace.TypeInfo,
                             inherit:=False)
                         Let nsEntry = DirectCast(ns, Microsoft.VisualBasic.CommandLine.Reflection.Namespace)
                         Select New Scripting.MetaData.PackageNamespace(nsEntry)).ToArray
                If attrs.IsNullOrEmpty Then
                    Return Nothing
                End If
            End If

            Dim nsAttr As MetaData.PackageNamespace =
                DirectCast(attrs(Scan0), Scripting.MetaData.PackageNamespace)
            Return __nsParser(Type, nsAttr, Assembly)
        End Function

        Private Function __nsParser(Type As Type,
                                    nsEntry As Scripting.MetaData.PackageNamespace,
                                    Assembly As Assembly) As SPM.Nodes.PartialModule
            Dim Functions = Microsoft.VisualBasic.CommandLine.Interpreter.GetAllCommands(Type, False)
            Dim EntryPoints = (From Func In Functions Select __entryPointParser(Func)).ToArray
            Dim assm As Assembly = Serialization.ShadowsCopy.ShadowsCopy(Assembly)
            Return New PartialModule(nsEntry) With {
                .Assembly = assm.InvokeSet(Of String)(NameOf(Assembly.TypeId), Type.FullName),
                .EntryPoints = EntryPoints
            }
        End Function

        ''' <summary>
        ''' 直接导入静态方法
        ''' </summary>
        ''' <param name="[module]"></param>
        ''' <returns></returns>
        Public Function [Imports]([module] As Type) As Interpreter.Linker.APIHandler.APIEntryPoint()
            Dim Functions = Microsoft.VisualBasic.CommandLine.Interpreter.GetAllCommands([module], False)
            Return APIParser(Functions.ToArray)
        End Function

        Public Function APIParser(EntryPoints As Generic.IEnumerable(Of CommandLine.Reflection.EntryPoints.APIEntryPoint)) As Interpreter.Linker.APIHandler.APIEntryPoint()
            Dim OverloadsGroup = (From api As CommandLine.Reflection.EntryPoints.APIEntryPoint
                                  In EntryPoints
                                  Select api
                                  Group api By api.Name.ToLower Into Group).ToArray
            Dim __LoadedEntryPoints = (From apiGroup
                                       In OverloadsGroup
                                       Select New Interpreter.Linker.APIHandler.APIEntryPoint(
                                           apiGroup.Group.First.Name,
                                           apiGroup.Group.ToArray)).ToArray
            Return __LoadedEntryPoints
        End Function

        Private Function __entryPointParser(Command As CommandLine.Reflection.EntryPoints.APIEntryPoint) As SPM.Nodes.EntryPointMeta
            Return New SPM.Nodes.EntryPointMeta() With {
                .Description = Command.Info,
                .Name = Command.Name,
                .ReturnedType = Command.EntryPoint.ReturnType.FullName,
                .Parameters = __getParameters(Command.EntryPoint)
            }
        End Function

        Private Function __getParameters(Method As System.Reflection.MethodInfo) As ComponentModel.TripleKeyValuesPair()
            Dim parameters = Method.GetParameters
            Dim LQuery = (From p As System.Reflection.ParameterInfo
                              In parameters
                          Let attrs = p.GetCustomAttributes(Microsoft.VisualBasic.Scripting.MetaData.Parameter.TypeInfo, True)
                          Let attr = If(attrs.IsNullOrEmpty,
                              New Scripting.MetaData.Parameter(p.Name),
                              DirectCast(attrs(Scan0), Scripting.MetaData.Parameter))
                          Select New ComponentModel.TripleKeyValuesPair With {
                              .Key = attr.Alias,
                              .Value1 = attr.Description,
                              .Value2 = p.ParameterType.FullName}).ToArray
            Return LQuery
        End Function
    End Module
End Namespace