Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime.Interop

    ''' <summary>
    ''' Used for VB.NET object instance method
    ''' </summary>
    Public Class RMethodOverloads : Implements RFunction, RPrint

        ReadOnly methods As MethodInfo()

        Public ReadOnly Property name As String Implements RFunction.name

        Sub New([overloads] As IEnumerable(Of MethodInfo))
            methods = [overloads].ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetMethod(params As NamedValue(Of Object)()) As MethodInfo
            Return methods _
                .OrderByDescending(Function(m) CalcMethodScore(m, params)) _
                .First
        End Function

        Public Shared Function CalcMethodScore(method As MethodInfo, params As NamedValue(Of Object)()) As Double

        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="arguments">Object list parameter is not allowed!</param>
        ''' <returns></returns>
        Public Function Invoke(envir As Environment, arguments() As InvokeParameter) As Object Implements RFunction.Invoke
            Dim params As NamedValue(Of Object)() = InvokeParameter.CreateArguments(envir, arguments).NamedValues
            Dim method As MethodInfo = GetMethod(params)
            ' Dim result As Object = method.Invoke(Nothing,)

            Throw New NotImplementedException
        End Function

        Public Function GetPrintContent() As String Implements RPrint.GetPrintContent
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace