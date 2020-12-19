#Region "Microsoft.VisualBasic::c887fa840d8fcf3c634c21f97be655aa, R#\System\Package\PackageFile\PackageLoader.vb"

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

    '     Module PackageLoader2
    ' 
    '         Function: GetPackageDirectory
    ' 
    '         Sub: LoadPackage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.System.Configuration

Namespace System.Package.File

    Public Module PackageLoader2

        <Extension>
        Public Function GetPackageDirectory(opt As Options, packageName$) As String
            Dim libDir As String

            libDir = opt.lib & $"/Library/{packageName}"
            libDir = libDir.GetDirectoryFullPath

            Return libDir
        End Function

        ''' <summary>
        ''' attach installed package
        ''' </summary>
        ''' <param name="dir"></param>
        ''' <param name="env"></param>
        Public Sub LoadPackage(dir As String, env As GlobalEnvironment)
            Dim meta As DESCRIPTION = $"{dir}/index.json".LoadJsonFile(Of DESCRIPTION)
            Dim symbols As Dictionary(Of String, String) = $"{dir}/manifest/symbols.json".LoadJsonFile(Of Dictionary(Of String, String))

            For Each symbol In symbols
                Using bin As New BinaryReader($"{dir}/src/{symbol.Value}".Open)
                    Call BlockReader.Read(bin).Parse(desc:=meta).Evaluate(env)
                End Using
            Next

            Dim onLoad As String = $"{dir}/.onload"

            If onLoad.FileExists Then
                Using bin As New BinaryReader(onLoad.Open)
                    Call DirectCast(BlockReader.Read(bin).Parse(desc:=meta).Evaluate(env), DeclareNewFunction).Invoke(env, params:={})
                End Using
            End If
        End Sub
    End Module
End Namespace
