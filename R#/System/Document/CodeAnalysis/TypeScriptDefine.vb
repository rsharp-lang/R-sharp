Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports pkg = SMRUCC.Rsharp.Development.Package.Package

Namespace Development.CodeAnalysis

    ''' <summary>
    ''' the generator module for the type script definition
    ''' </summary>
    Public Module TypeScriptDefine

        ''' <summary>
        ''' extract the typescript module definition 
        ''' </summary>
        ''' <param name="pkg"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ExtractModule(pkg As pkg) As String
            Dim ts As New StringBuilder

            Call ts.AppendLine($"declare namespace {pkg.namespace} {{")

            For Each api As NamedValue(Of MethodInfo) In ImportsPackage.GetAllApi(pkg)
                Dim rfunc As New RMethodInfo(api)
                Dim returns = rfunc.returns.MapTypeScriptType
                Dim params As String() = rfunc.parameters _
                    .Select(AddressOf MapTypeScriptParameter) _
                    .ToArray

                Call ts.AppendLine($"   function {api.Name}({params.JoinBy(", ")}): any;")
            Next

            Call ts.AppendLine("}")

            Return ts.ToString
        End Function

        Private Function MapTypeScriptParameter(p As RMethodArgument) As String
            Dim name As String = p.name
            Dim type As String = p.type.MapTypeScriptType
            Dim optVal As String

            If p.isOptional Then
                Dim def = p.default

                If def Is Nothing Then
                    optVal = "null"
                Else
                    Select Case RType.TypeOf(def).mode
                        Case TypeCodes.boolean
                            Dim b = CLRVector.asLogical(def)
                            optVal = If(b.Length = 1, b(0).ToString.ToLower, b.GetJson)
                        Case TypeCodes.integer
                            Dim i = CLRVector.asInteger(def)
                            optVal = If(i.Length = 1, i(0).ToString, i.GetJson)
                        Case TypeCodes.double
                            Dim d = CLRVector.asNumeric(def)
                            optVal = If(d.Length = 1, d(0).ToString, d.GetJson)
                        Case TypeCodes.string
                            Dim s = CLRVector.asCharacter(def)
                            optVal = If(s.Length = 1, $"'{s(0)}'", s.GetJson)
                        Case Else
                            optVal = "null"
                    End Select
                End If

                Return $"{name}?:{type} = {optVal}"
            Else
                Return $"{name}:{type}"
            End If
        End Function

        <Extension>
        Private Function MapTypeScriptType(type As RType) As String
            If type Is RType.any Then
                Return "any"
            ElseIf type Is RType.list Then
                Return "object"
            ElseIf type.mode = TypeCodes.boolean Then
                Return "boolean"
            ElseIf type.mode = TypeCodes.closure Then
                Return "any"
            ElseIf type.mode = TypeCodes.double Then
                Return "number"
            ElseIf type.mode = TypeCodes.string Then
                Return "string"
            Else
                Return "object"
            End If
        End Function
    End Module
End Namespace