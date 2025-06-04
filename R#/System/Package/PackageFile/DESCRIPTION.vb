#Region "Microsoft.VisualBasic::af3e9b98c18d4e929c97639d5b232674, R#\System\Package\PackageFile\DESCRIPTION.vb"

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

    '   Total Lines: 130
    '    Code Lines: 73 (56.15%)
    ' Comment Lines: 46 (35.38%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (8.46%)
    '     File Size: 4.92 KB


    '     Class DESCRIPTION
    ' 
    '         Properties: [Date], Author, Description, isEmpty, License
    '                     Maintainer, meta, Package, Title, Type
    '                     Version
    ' 
    '         Function: Parse, toList, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language.[Default]
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop.CType

Namespace Development.Package.File

    ''' <summary>
    ''' the metadata of the R# package file
    ''' </summary>
    Public Class DESCRIPTION : Implements ICTypeList, IsEmpty

        ''' <summary>
        ''' the package name
        ''' </summary>
        ''' <remarks>
        ''' this package name could be used for load the package via ``require`` or ``library`` function.
        ''' </remarks>
        ''' <returns></returns>
        Public Property Package As String
        ''' <summary>
        ''' the type of the current R# package.
        ''' </summary>
        ''' <returns></returns>
        Public Property Type As String
        ''' <summary>
        ''' the title display of the current package.
        ''' </summary>
        ''' <returns></returns>
        Public Property Title As String
        ''' <summary>
        ''' the package version string
        ''' </summary>
        ''' <returns></returns>
        Public Property Version As String
        ''' <summary>
        ''' the date time for build this package
        ''' </summary>
        ''' <returns></returns>
        Public Property [Date] As String
        ''' <summary>
        ''' the author names for write this packages
        ''' </summary>
        ''' <returns></returns>
        Public Property Author As String
        Public Property Maintainer As String
        ''' <summary>
        ''' A long text for make notes about this package its function and usage.
        ''' </summary>
        ''' <returns></returns>
        Public Property Description As String
        ''' <summary>
        ''' the license name
        ''' </summary>
        ''' <returns></returns>
        Public Property License As String
        ''' <summary>
        ''' other additional metadata about this package
        ''' </summary>
        ''' <returns></returns>
        Public Property meta As Dictionary(Of String, String)

        ''' <summary>
        ''' check of the content data of current package metadata is empty or not?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isEmpty As Boolean Implements IsEmpty.IsEmpty
            Get
                Return Package.StringEmpty
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{Package}_{Version}] {Title}"
        End Function

        Public Function toList() As list Implements ICTypeList.toList
            Return New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {NameOf(Package).ToLower, Package},
                    {NameOf(Type).ToLower, Type},
                    {NameOf(Title).ToLower, Title},
                    {NameOf(Version).ToLower, Version},
                    {NameOf([Date]).ToLower, [Date]},
                    {NameOf(Author).ToLower, Author},
                    {NameOf(Maintainer).ToLower, Maintainer},
                    {NameOf(Description).ToLower, Description},
                    {NameOf(License).ToLower, License},
                    {NameOf(meta), New list With {
                        .slots = meta _
                            .SafeQuery _
                            .ToDictionary(Function(a) a.Key,
                                          Function(a)
                                              Return CObj(a.Value)
                                          End Function)
                        }
                    }
                }
            }
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="file">the file path to the package document description file</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Parse(file As String) As DESCRIPTION
            Return ParseDocument(file.ReadAllText)
        End Function

        Public Shared Function ParseDocument(str As String) As DESCRIPTION
            Dim meta As Dictionary(Of String, String) = str.LineTokens.ParseTagData
            Dim index As New DESCRIPTION With {
                .meta = New Dictionary(Of String, String)
            }

            Static writer As Dictionary(Of String, PropertyInfo) = DataFramework.Schema(Of DESCRIPTION)(
                flag:=PropertyAccess.Writeable,
                nonIndex:=True,
                primitive:=True
            ).ToUpper

            For Each tag As KeyValuePair(Of String, String) In meta
                Dim writeName As String = tag.Key.ToUpper

                If writer.ContainsKey(writeName) Then
                    writer(writeName).SetValue(index, tag.Value)
                Else
                    index.meta(tag.Key) = tag.Value
                End If
            Next

            Return index
        End Function
    End Class
End Namespace
