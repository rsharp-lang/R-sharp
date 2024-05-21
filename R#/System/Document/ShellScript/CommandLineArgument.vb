#Region "Microsoft.VisualBasic::bae37953c936c3a34aae79dec16992dc, R#\System\Document\ShellScript\CommandLineArgument.vb"

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

    '   Total Lines: 49
    '    Code Lines: 39 (79.59%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: NaN%
    ' 
    '   Blank Lines: 10 (20.41%)
    '     File Size: 1.59 KB


    '     Class CommandLineArgument
    ' 
    '         Properties: defaultValue, description, isLiteral, isNumeric, name
    '                     type
    ' 
    '         Function: GetLiteralValue, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Development.CommandLine

    Public Class CommandLineArgument

        Public Property name As String
        Public Property defaultValue As String
        Public Property type As String
        Public Property description As String
        Public Property isLiteral As Boolean

        Public ReadOnly Property isNumeric As Boolean
            Get
                Return type = "integer" OrElse
                    type = "numeric" OrElse
                    type = "double"
            End Get
        End Property

        Public Function GetLiteralValue() As Object
            Dim defaultValue As String = Me.defaultValue

            If defaultValue.StringEmpty Then
                Return ""
            Else
                defaultValue = defaultValue.Trim(""""c)
            End If

            If defaultValue.TextEquals("null") Then
                Return Nothing
            End If

            Select Case type
                Case "integer" : Return CLRVector.asInteger(defaultValue)
                Case "numeric", "double" : Return CLRVector.asNumeric(defaultValue)
                Case "boolean" : Return CLRVector.asLogical(defaultValue)
                Case "string" : Return CLRVector.asCharacter(defaultValue)
                Case Else
                    Return defaultValue
            End Select
        End Function

        Public Overrides Function ToString() As String
            Return $"{name}: {description}"
        End Function

    End Class
End Namespace
