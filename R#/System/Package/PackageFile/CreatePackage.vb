#Region "Microsoft.VisualBasic::613f58f298b19db32735ad8ef158aa82, R#\System\Package\PackageFile\CreatePackage.vb"

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

'     Module CreatePackage
' 
'         Function: Build
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.System.Package.File.Expression

Namespace System.Package.File

    Public Module CreatePackage

        ''' <summary>
        ''' build a R# package file
        ''' </summary>
        ''' <param name="target">
        ''' the target directory that contains the necessary 
        ''' files for create a R# package file.</param>
        ''' <param name="outfile">the output zip file stream</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function Build(desc As DESCRIPTION, target As String, outfile As Stream) As Message
            Dim srcR As String = $"{target}/R".GetDirectoryFullPath
            Dim file As New PackageModel With {
                .info = desc,
                .symbols = New Dictionary(Of String, RExpression)
            }

            For Each script As String In srcR.ListFiles("*.R")
                Dim loader As New RInterpreter()
                Dim [imports] As Environment = Nothing

                Call loader.Source(script, globalEnv:=[imports])

                For Each symbol As Symbol In [imports].symbols.Values
                    file.symbols(symbol.name) = RExpression.CreateFromSymbol(symbol)
                Next
            Next

            Call file.Flush(outfile)

            Return Nothing
        End Function
    End Module
End Namespace
