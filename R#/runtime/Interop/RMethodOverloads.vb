#Region "Microsoft.VisualBasic::6db7e492aee965fc0161cdd6c4ffe253, R#\Runtime\Interop\RMethodOverloads.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Class RMethodOverloads
    ' 
    '         Properties: name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CalcMethodScore, GetMethod, GetPrintContent, Invoke
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
