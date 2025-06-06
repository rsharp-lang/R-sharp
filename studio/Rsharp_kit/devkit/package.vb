﻿#Region "Microsoft.VisualBasic::48f38b9a5ccc5867f7c55e3e9e89abd5, studio\Rsharp_kit\devkit\package.vb"

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

    '   Total Lines: 168
    '    Code Lines: 118 (70.24%)
    ' Comment Lines: 30 (17.86%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 20 (11.90%)
    '     File Size: 6.47 KB


    ' Module package
    ' 
    '     Function: attach, createRDefinition, deserialize, loadExpr, Parse
    '               parseDll, ParseRDataRaw, serialize, UnpackObjects
    ' 
    '     Sub: loadPackage
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports roxygenNet
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.CommandLine
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.RDataSet.Convertor
Imports SMRUCC.Rsharp.RDataSet.Struct
Imports SMRUCC.Rsharp.RDataSet.Struct.LinkedList
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports LibDir = Microsoft.VisualBasic.FileIO.Directory
Imports R = SMRUCC.Rsharp.Runtime.Components.Rscript
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

<Package("package_utils")>
Module package

    ''' <summary>
    ''' parse raw bytes stream as R expression
    ''' </summary>
    ''' <param name="raw"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("read")>
    <RApiReturn(GetType(Expression))>
    Public Function loadExpr(<RRawVectorArgument> raw As Object, Optional env As Environment = Nothing) As Object
        raw = env.castToRawRoutine(raw, Encodings.UTF8WithoutBOM, True)

        If TypeOf raw Is Message Then
            Return raw
        End If

        Using io As New BinaryReader(DirectCast(raw, Stream))
            io.BaseStream.Seek(Scan0, SeekOrigin.Begin)

            If Not BlockReader.CheckMagic(io) Then
                Call io.BaseStream.Seek(Scan0, SeekOrigin.Begin)
                Call env.AddMessage("invalid magic header detected...", MSG_TYPES.WRN)
            End If

            Return BlockReader.ParseBlock(io).Parse(New DESCRIPTION)
        End Using
    End Function

    ''' <summary>
    ''' for debug used only
    ''' </summary>
    ''' <param name="dir"></param>
    ''' <param name="env"></param>
    <ExportAPI("loadPackage")>
    Public Sub loadPackage(dir As String, Optional quietly As Boolean = False, Optional env As Environment = Nothing)
        Call PackageLoader2.LoadPackage(LibDir.FromLocalFileSystem(dir), dir.BaseName,
                                        quietly:=quietly,
                                        env:=env.globalEnvironment)
    End Sub

    ''' <summary>
    ''' serialize target R function expression as byte stream
    ''' </summary>
    ''' <param name="func"></param>
    ''' <returns></returns>
    <ExportAPI("serialize")>
    Public Function serialize(func As Expression) As Object
        Using tmp As New Writer(New MemoryStream)
            Return New MemoryStream(tmp.GetBuffer(func))
        End Using
    End Function

    <ExportAPI("parse")>
    Public Function Parse(rscript As String) As ShellScript
        Return New ShellScript(R.AutoHandleScript(handle:=rscript)).AnalysisAllCommands
    End Function

    ''' <summary>
    ''' attach and hotload of a package.
    ''' </summary>
    ''' <param name="package"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("attach")>
    Public Function attach(package As String,
                           Optional quietly As Boolean = False,
                           Optional env As Environment = Nothing) As Object

        If package.ExtensionSuffix("zip") Then
            If package.FileExists Then
                Return base.attachPackageFile(env.globalEnvironment.Rscript,
                                              zip:=package,
                                              quietly:=quietly)
            ElseIf package.First = "@" Then
                ' source from github
                Return github.hotLoad(package.Substring(1))
            Else
                Return RInternal.debug.stop({$"invalid package source: '{package}'!", $"source: {package}"}, env)
            End If
        ElseIf package.DirectoryExists Then
            ' is dir
            Return PackageLoader2.Hotload(package, env.globalEnvironment)
        ElseIf package.First = "@" Then
            ' source from github
            Return github.hotLoad(package.Substring(1))
        Else
            Return RInternal.debug.stop({$"invalid package source: '{package}'!", $"source: {package}"}, env)
        End If
    End Function

    <ExportAPI("parseRData.raw")>
    Public Function ParseRDataRaw(file As String) As RData
        Return RData.ParseFile(file)
    End Function

    <ExportAPI("unpackRData")>
    Public Function UnpackObjects(rdata As RData) As list
        Return New list(GetType(RObjectInfo)) With {
            .slots = rdata _
                .PullRawData _
                .ToDictionary(Function(t) t.Key,
                              Function(t)
                                  Return CObj(t.Value)
                              End Function)
        }
    End Function

    <ExportAPI("deserialize")>
    Public Function deserialize(rdata As RObject) As Object
        Return ConvertToR.PullRObject(rdata)
    End Function

    <ExportAPI("defineR")>
    Public Function createRDefinition(package_dir As String) As Object
        Dim man_dir As String = $"{package_dir}/man"
        Dim meta As DESCRIPTION = DESCRIPTION.Parse($"{package_dir}/DESCRIPTION")
        Dim Rscript As Rscript

        For Each Rfile As String In $"{package_dir}/R".ListFiles("*.R")
            Rscript = Rscript.AutoHandleScript(handle:=Rfile)

            For Each symbol As Document In RoxygenDocument.ParseDocuments(Rscript)
                Using definition As New StreamWriter($"{package_dir}/".Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False))

                End Using
            Next
        Next

        Return Nothing
    End Function

    ''' <summary>
    ''' parse the clr package module
    ''' </summary>
    ''' <param name="dll"></param>
    ''' <returns></returns>
    <ExportAPI("parseDll")>
    <RApiReturn(GetType(SMRUCC.Rsharp.Development.Package.Package))>
    Public Function parseDll(dll As String) As Object
        Return PackageLoader.ParsePackages(dll:=dll).ToArray
    End Function
End Module
