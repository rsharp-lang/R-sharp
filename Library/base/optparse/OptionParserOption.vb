#Region "Microsoft.VisualBasic::460db68c2759154fe4370fb5cd9ce7d0, Library\base\optparse\OptionParserOption.vb"

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

    '   Total Lines: 62
    '    Code Lines: 40 (64.52%)
    ' Comment Lines: 11 (17.74%)
    '    - Xml Docs: 81.82%
    ' 
    '   Blank Lines: 11 (17.74%)
    '     File Size: 2.36 KB


    ' Class OptionParserOption
    ' 
    '     Properties: [default], action, help, opt_str, type
    ' 
    '     Function: CreateArgumentInternal, isLogical, opt_names
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine
Imports SMRUCC.Rsharp.Development.CommandLine

Public Class OptionParserOption

    Public Property opt_str As String()
    Public Property [default] As Object
    Public Property type As String
    Public Property help As String

    ''' <summary>
    ''' A character string that describes the action optparse should take when it encounters an option,
    ''' either “store”, “store_true”, “store_false”, or “callback”. 
    ''' 
    ''' An action of “store” signifies that optparse should store the specified following value if the option is found on the command string. 
    ''' “store_true” stores TRUE if the option is found and “store_false” stores FALSE if the option is found. 
    ''' 
    ''' “callback” stores the return value produced by the function specified in the callback argument. 
    ''' If callback is not NULL then the default is “callback” else “store”.
    ''' </summary>
    ''' <returns></returns>
    Public Property action As String

    Public Iterator Function opt_names(Optional convert_hyphens_to_underscores As Boolean = False) As IEnumerable(Of String)
        For Each name As String In opt_str
            name = CommandLine.TrimNamePrefix(name)

            If convert_hyphens_to_underscores Then
                name = name.Replace("-", "_")
            End If

            Yield name
        Next
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function isLogical() As Boolean
        Return action = "store_true" OrElse action = "store_false"
    End Function

    Friend Function CreateArgumentInternal() As CommandLineArgument
        Dim defaultVal As String = [default]

        If defaultVal Is Nothing Then
            If action = "store_true" Then
                defaultVal = "TRUE"
            ElseIf action = "store_false" Then
                defaultVal = "FALSE"
            End If
        End If

        Return New CommandLineArgument With {
            .defaultValue = If([default] Is Nothing, "NULL", CStr([default])),
            .description = help,
            .isLiteral = True,
            .name = opt_str.JoinBy(", "),
            .type = type
        }
    End Function

End Class
