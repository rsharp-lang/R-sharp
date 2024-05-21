#Region "Microsoft.VisualBasic::fbafd12ef953545ac0ddf1a7f4a05084, R#\Runtime\Interop\RInteropAttributes\RPolyglotSymbolAttribute.vb"

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

    '   Total Lines: 27
    '    Code Lines: 19 (70.37%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (29.63%)
    '     File Size: 800 B


    '     Class RPolyglotSymbolAttribute
    ' 
    '         Properties: [Alias]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetAlternativeNames, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection

Namespace Runtime.Interop

    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=True, Inherited:=True)>
    Public Class RPolyglotSymbolAttribute : Inherits RInteropAttribute

        Public ReadOnly Property [Alias] As String

        Sub New(name As String)
            [Alias] = name
        End Sub

        Public Overrides Function ToString() As String
            Return [Alias]
        End Function

        Public Shared Iterator Function GetAlternativeNames(pkg As Type) As IEnumerable(Of String)
            Dim attrs = pkg.GetCustomAttributes(Of RPolyglotSymbolAttribute)

            For Each a As RPolyglotSymbolAttribute In attrs
                Yield a.Alias
            Next
        End Function

    End Class
End Namespace
