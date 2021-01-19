#Region "Microsoft.VisualBasic::85ab6c55af992f50195aad8c3d17d614, studio\Rsharp_kit\devkit\package.vb"

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

' Module package
' 
'     Function: loadExpr
' 
'     Sub: loadPackage
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("package_utils")>
Module package

    <ExportAPI("read")>
    <RApiReturn(GetType(Expression))>
    Public Function loadExpr(<RRawVectorArgument> raw As Object, Optional env As Environment = Nothing) As Object
        raw = env.castToRawRoutine(raw, Encodings.UTF8WithoutBOM, True)

        If TypeOf raw Is Message Then
            Return raw
        End If

        Using io As New BinaryReader(DirectCast(raw, Stream))
            io.BaseStream.Seek(Scan0, SeekOrigin.Begin)
            Return BlockReader.Read(io).Parse(New DESCRIPTION)
        End Using
    End Function

    ''' <summary>
    ''' for debug used only
    ''' </summary>
    ''' <param name="dir"></param>
    ''' <param name="env"></param>
    <ExportAPI("loadPackage")>
    Public Sub loadPackage(dir As String, Optional env As Environment = Nothing)
        Call PackageLoader2.LoadPackage(dir, env.globalEnvironment)
    End Sub

    <ExportAPI("serialize")>
    Public Function serialize(method As DeclareNewFunction) As Object
        Using tmp As New Writer(New MemoryStream)
            Return New MemoryStream(tmp.GetBuffer(method))
        End Using
    End Function
End Module
