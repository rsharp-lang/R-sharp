#Region "Microsoft.VisualBasic::89a6476d476a60ccebeb35f2ac13110a, R#\Runtime\Interop\RInteropAttributes\RDefaultFunctionAttribute.vb"

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


    ' Code Statistics:

    '   Total Lines: 33
    '    Code Lines: 23 (69.70%)
    ' Comment Lines: 3 (9.09%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (21.21%)
    '     File Size: 1.14 KB


    '     Class RDefaultFunctionAttribute
    ' 
    '         Function: GetDefaultFunction, ToString
    ' 
    '     Class RDefaultFunction
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Runtime.Interop

    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class RDefaultFunctionAttribute : Inherits Attribute

        Public Overrides Function ToString() As String
            Return $"f()"
        End Function

        Public Shared Function GetDefaultFunction(symbol As String, obj As Object) As RMethodInfo
            Dim funcs = obj.GetType.GetMethods(DataFramework.PublicProperty)
            Dim getFlag = funcs _
                .Where(Function(f) f.GetCustomAttribute(Of RDefaultFunctionAttribute) IsNot Nothing) _
                .FirstOrDefault

            If getFlag Is Nothing Then
                Return Nothing
            Else
                Return New RMethodInfo(symbol, getFlag, obj)
            End If
        End Function

    End Class

    ''' <summary>
    ''' A template for check of the <see cref="RDefaultExpressionAttribute"/>
    ''' </summary>
    Public MustInherit Class RDefaultFunction
    End Class
End Namespace
