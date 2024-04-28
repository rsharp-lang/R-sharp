#Region "Microsoft.VisualBasic::806d575402c88c4a2dc7ac7bb3d10b3c, E:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/PackageModel.vb"

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

    '   Total Lines: 66
    '    Code Lines: 34
    ' Comment Lines: 24
    '   Blank Lines: 8
    '     File Size: 2.39 KB


    '     Class PackageModel
    ' 
    '         Properties: assembly, clr, dataSymbols, info, loading
    '                     pkg_dir, symbols, tsd, unixman, vignettes
    ' 
    '         Function: GetSymbols, ToString
    ' 
    '         Sub: Flush
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Development.Package.File

    Public Class PackageModel

        Public Property info As DESCRIPTION

        ''' <summary>
        ''' only allows function and constant.
        ''' </summary>
        ''' <returns></returns>
        Public Property symbols As Dictionary(Of String, Expression)
        ''' <summary>
        ''' the file names in ``data/`` directory.
        ''' </summary>
        ''' <returns></returns>
        Public Property dataSymbols As Dictionary(Of String, String)
        Public Property loading As Dependency()
        Public Property assembly As AssemblyPack
        Public Property unixman As List(Of String)
        Public Property vignettes As List(Of String)
        Public Property tsd As New Dictionary(Of String, String)
        ''' <summary>
        ''' documents about the clr data types
        ''' </summary>
        ''' <returns></returns>
        Public Property clr As New Dictionary(Of String, String)
        Public Property pkg_dir As String

        ''' <summary>
        ''' get all <see cref="SymbolExpression"/> from <see cref="symbols"/>
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSymbols() As SymbolExpression()
            Return symbols.Values _
                .Select(Function(si) DirectCast(si, SymbolExpression)) _
                .ToArray
        End Function

        Public Overrides Function ToString() As String
            Return info.ToString
        End Function

        ''' <summary>
        ''' generate package file from here
        ''' </summary>
        ''' <param name="outfile"></param>
        ''' <param name="assets"></param>
        ''' <remarks>
        ''' generate nuget package format zip archive file.
        ''' </remarks>
        Public Sub Flush(outfile As Stream, assets As Dictionary(Of String, String))
            Using zip As New ZipArchive(outfile, ZipArchiveMode.Create)
                Call New NuGetZip(zip, Me).CreateNuGetPackage(assets)
            End Using
        End Sub

    End Class
End Namespace
