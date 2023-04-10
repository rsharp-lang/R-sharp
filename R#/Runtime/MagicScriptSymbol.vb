#Region "Microsoft.VisualBasic::cdbc604d81a063d3776320521a0f96b9, E:/GCModeller/src/R-sharp/R#//Runtime/MagicScriptSymbol.vb"

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

    '   Total Lines: 67
    '    Code Lines: 48
    ' Comment Lines: 12
    '   Blank Lines: 7
    '     File Size: 2.52 KB


    '     Class MagicScriptSymbol
    ' 
    '         Properties: commandArguments, commandLine, debug, dir, file
    '                     fullName, log4vb_redirect, silent, startup_time
    ' 
    '         Function: toList, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Interop.CType

Namespace Runtime

    Public Class MagicScriptSymbol : Implements ICTypeList

        ''' <summary>
        ''' dir path that contains the running script
        ''' </summary>
        ''' <returns></returns>
        Public Property dir As String
        ''' <summary>
        ''' the file name of current running script
        ''' </summary>
        ''' <returns></returns>
        Public Property file As String
        ''' <summary>
        ''' the full path of the current running script
        ''' </summary>
        ''' <returns></returns>
        Public Property fullName As String

        <RNameAlias("startup.time")>
        Public Property startup_time As String
        Public Property debug As Boolean
        Public Property silent As Boolean
        Public Property log4vb_redirect As Boolean
        Public Property commandLine As Dictionary(Of String, String())
        Public Property commandArguments As String()

        Public Function toList() As list Implements ICTypeList.toList
            Dim commandLine As New list With {
                .slots = Me.commandLine _
                    .ToDictionary(Function(a) a.Key,
                                    Function(a)
                                        Return CObj(a.Value)
                                    End Function)
            }

            Return New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {NameOf(dir), dir},
                    {NameOf(file), file},
                    {NameOf(fullName), fullName},
                    {"startup.time", startup_time},
                    {NameOf(debug), debug},
                    {NameOf(silent), silent},
                    {"log4vb.redirect", log4vb_redirect},
                    {NameOf(commandLine), New list With {
                        .slots = New Dictionary(Of String, Object) From {
                                {"commandLine", commandLine},
                                {"arguments", commandArguments}
                            }
                        }
                    }
                }
            }
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
