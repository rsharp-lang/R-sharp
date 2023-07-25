#Region "Microsoft.VisualBasic::b8135a6ed2e8107b504bdaef51fe7d65, D:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/Linq/Expression/OutputProjection.vb"

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

    '   Total Lines: 41
    '    Code Lines: 25
    ' Comment Lines: 7
    '   Blank Lines: 9
    '     File Size: 1.33 KB


    '     Class OutputProjection
    ' 
    '         Properties: fields, keyword
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Exec, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' data projection: ``SELECT &lt;fields>``
    ''' </summary>
    Public Class OutputProjection : Inherits LinqKeywordExpression

        ''' <summary>
        ''' produce a javascript object or dataframe row
        ''' </summary>
        ''' <returns></returns>
        Public Property fields As NamedValue(Of Expression)()

        Public Overrides ReadOnly Property keyword As String
            Get
                Return "SELECT"
            End Get
        End Property

        Sub New(fields As IEnumerable(Of NamedValue(Of Expression)))
            Me.fields = fields.ToArray
        End Sub

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Dim obj As New JavaScriptObject

            For Each field In fields
                obj(field.Name) = field.Value.Exec(context)
            Next

            Return obj
        End Function

        Public Overrides Function ToString() As String
            Return $"SELECT new {{{vbCrLf}{fields.Select(Function(a) $"    {a.Name} = {a.Value}").JoinBy(", " & vbCrLf)}{vbCrLf}}}"
        End Function
    End Class
End Namespace
