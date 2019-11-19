Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Package

    ''' <summary>
    ''' Helper methods for add .NET function into <see cref="Environment"/> target
    ''' </summary>
    Public Module ImportsPackage

        <Extension>
        Public Sub ImportsStatic(envir As Environment, package As Type)
            Dim [global] As GlobalEnvironment = envir.GlobalEnvironment
            Dim docs As ProjectType = [global].packages.packageDocs.GetAnnotations(package)
            Dim methods = package.GetMethods(BindingFlags.Public Or BindingFlags.Static)
            Dim Rmethods = methods _
                .Select(Function(m)
                            Dim flag = m.GetCustomAttribute(Of ExportAPIAttribute)
                            Dim name = If(flag Is Nothing, m.Name, flag.Name)

                            Return New RMethodInfo(name, m, Nothing)
                        End Function) _
                .ToArray
            Dim symbol As Variable

            For Each api As RMethodInfo In Rmethods
                symbol = [global].FindSymbol(api.name)

                If symbol Is Nothing Then
                    [global].Push(api.name, api, TypeCodes.closure)
                Else
                    symbol.value = api
                End If
            Next
        End Sub

        <Extension>
        Public Sub ImportsInstance(envir As Environment, target As Object)
            Dim methods = target.GetType.GetMethods(BindingFlags.Public Or BindingFlags.Instance)
            Dim Rmethods = methods _
                .Select(Function(m)
                            Dim flag = m.GetCustomAttribute(Of ExportAPIAttribute)
                            Dim name = If(flag Is Nothing, m.Name, flag.Name)

                            Return New RMethodInfo(name, m, target)
                        End Function) _
                .ToArray
            Dim [global] As GlobalEnvironment = envir.GlobalEnvironment

            For Each api As RMethodInfo In Rmethods
                Call [global].Push(api.name, api, TypeCodes.closure)
            Next
        End Sub
    End Module
End Namespace
