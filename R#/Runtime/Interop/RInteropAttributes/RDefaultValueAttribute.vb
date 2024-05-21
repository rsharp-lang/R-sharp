#Region "Microsoft.VisualBasic::3fd5164868c1c23aed8781ddfd4f0f91, R#\Runtime\Interop\RInteropAttributes\RDefaultValueAttribute.vb"

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

    '   Total Lines: 43
    '    Code Lines: 26 (60.47%)
    ' Comment Lines: 9 (20.93%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (18.60%)
    '     File Size: 1.64 KB


    '     Class RDefaultValueAttribute
    ' 
    '         Properties: defaultValue
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ParseDefaultValue, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Runtime.Interop

    ''' <summary>
    ''' 这个需要在目标类型与字符串之间存在有一个隐式转换的操作符定义
    ''' param as type = "xxxxxx"
    ''' </summary>
    <AttributeUsage(AttributeTargets.Parameter)>
    Public Class RDefaultValueAttribute : Inherits RInteropAttribute

        Public ReadOnly Property defaultValue As String

        Sub New([default] As String)
            defaultValue = [default]
        End Sub

        Public Overrides Function ToString() As String
            Return defaultValue
        End Function

        ''' <summary>
        ''' This method require a implit ctype operator that parse string to target <paramref name="parameterType"/>
        ''' </summary>
        ''' <param name="parameterType"></param>
        ''' <returns></returns>
        Public Function ParseDefaultValue(parameterType As Type) As Object
            Static implictCTypeCache As New Dictionary(Of Type, IImplictCTypeOperator(Of String, Object))

            Dim implict As IImplictCTypeOperator(Of String, Object) = implictCTypeCache _
                .ComputeIfAbsent(
                    key:=parameterType,
                    lazyValue:=Function()
                                   Return ImplictCType.GetWideningOperator(Of String)(parameterType)
                               End Function
                 )
            Dim value As Object = implict(defaultValue)

            Return value
        End Function
    End Class
End Namespace
