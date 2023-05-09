Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Interop.[CType]
Imports any = Microsoft.VisualBasic.Scripting

Namespace Development.CodeAnalysis

    ''' <summary>
    ''' the general definition model of the symbol or function in typescript
    ''' </summary>
    Public Class SymbolTypeDefine : Implements ICTypeList

        Public Property name As String
        Public Property parameters As NamedValue(Of String)()

        ''' <summary>
        ''' the return/symbol value type of current function
        ''' </summary>
        ''' <returns></returns>
        Public Property value As String = "any"

        Public ReadOnly Property isSymbol As Boolean
            Get
                Return parameters Is Nothing
            End Get
        End Property

        Sub New(method As RMethodInfo)
            name = method.name
            value = method.GetUnionTypes _
                .Select(Function(ti) RType.GetRSharpType(ti).MapTypeScriptType) _
                .JoinBy("|")
            parameters = method.parameters _
                .Select(Function(a) a.MapTypeScriptParameter) _
                .ToArray
        End Sub

        Sub New(method As MethodInfo)
            name = method.Name
            value = RType.GetRSharpType(method.ReturnType).MapTypeScriptType
            parameters = method _
                .GetParameters _
                .Select(AddressOf ParseParameter) _
                .ToArray
        End Sub

        Sub New(symbol As SymbolExpression)
            name = symbol.GetSymbolName
            value = RType.GetType(symbol.type).MapTypeScriptType
            parameters = Nothing
        End Sub

        Private Shared Function ParseParameter(a As ParameterInfo) As NamedValue(Of String)
            Dim ts As String = RType.GetRSharpType(a.ParameterType).MapTypeScriptType

            If a.IsOptional Then
                Return New NamedValue(Of String)(a.Name, any.ToString(a.DefaultValue, null:="null"), ts)
            Else
                Return New NamedValue(Of String)(a.Name, Nothing, ts)
            End If
        End Function

        Public Overrides Function ToString() As String
            If isSymbol Then
                Return $"{name}: {value}"
            Else
                Return $"function {name}({getParameterList.JoinBy(", ")}): {value}"
            End If
        End Function

        Public Function GetTypeScriptDeclare() As String
            If isSymbol Then
                Return $"{name.Split("."c).Last}: {value}"
            Else
                Return $"function {name.Split("."c).Last}({getParameterList.JoinBy(", ")}): {value}"
            End If
        End Function

        Private Iterator Function getParameterList() As IEnumerable(Of String)
            For Each a As NamedValue(Of String) In parameters
                If a.Value Is Nothing Then
                    ' is required
                    Yield $"{a.Name}: {a.Description}"
                Else
                    Yield $"{a.Name}?: {a.Description}"
                End If
            Next
        End Function

        ''' <summary>
        ''' as.list(args(func))
        ''' </summary>
        ''' <returns></returns>
        Public Function toList() As list Implements ICTypeList.toList
            Dim list As New list With {.slots = New Dictionary(Of String, Object)}
            Dim parseVal As Object
            Dim meta As New list With {.slots = New Dictionary(Of String, Object)}

            Call meta.add("symbol", name)
            Call meta.add("value", value)
            Call list.add("", meta)

            For Each param As NamedValue(Of String) In parameters
                Select Case param.Description
                    Case "boolean" : parseVal = parseJSON(Of Boolean)(param.Value, AddressOf ParseBoolean)
                    Case "number" : parseVal = parseJSON(Of Double)(param.Value, AddressOf Conversion.Val)
                    Case "string" : parseVal = parseJSON(Of String)(param.Value, Function(s) s.Trim(""""c, "'"c))
                    Case Else
                        parseVal = param.Value
                End Select

                Call list.add(param.Name, parseVal)
            Next

            Return list
        End Function

        Private Shared Function parseJSON(Of T)(defVal As String, parse As Func(Of String, T)) As T()
            If defVal.StringEmpty OrElse defVal = "null" Then
                Return Nothing
            ElseIf defVal.StartsWith("["c) AndAlso defVal.EndsWith("]"c) Then
                Return defVal.LoadJSON(Of T())
            Else
                Return New T() {parse(defVal)}
            End If
        End Function
    End Class
End Namespace