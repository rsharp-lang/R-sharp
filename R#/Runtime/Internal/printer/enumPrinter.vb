#Region "Microsoft.VisualBasic::839e379b89faa057ed2b02b9537cb640, R#\Runtime\Internal\printer\enumPrinter.vb"

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

    '     Module enumPrinter
    ' 
    '         Function: defaultValueToString, printClass
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.ConsolePrinter

    Public Module enumPrinter

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="obj">
        ''' This object should be an enum value.
        ''' </param>
        ''' <returns></returns>
        Public Function printClass(obj As Object) As String
            Dim type As REnum = REnum.GetEnumList(obj.GetType)
            Dim base As Type = type.baseType
            Dim describ$ = DirectCast(obj, [Enum]).Description
            Dim print$ = $"{{{base.Name.ToLower} {type.IntValue(obj)}}} {obj.ToString}"

            If describ = obj.ToString Then
                Return print
            Else
                Return $"{print} #{describ}"
            End If
        End Function

        Public Function defaultValueToString([default] As Object, type As Type) As String
            Dim s As String = [default].ToString

            If s.IsPattern("\d+") Then
                ' is flag combinations
                s = GetAllEnumFlags([default], type) _
                    .Select(Function(flag) flag.ToString) _
                    .JoinBy("|")
            End If

            Return $"[{s}]"
        End Function
    End Module
End Namespace
