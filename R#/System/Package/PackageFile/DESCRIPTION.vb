#Region "Microsoft.VisualBasic::99e95fcac1bc921dbed08ee0b29ff19d, G:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/DESCRIPTION.vb"

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

    '   Total Lines: 92
    '    Code Lines: 73
    ' Comment Lines: 8
    '   Blank Lines: 11
    '     File Size: 3.58 KB


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
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language.[Default]
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop.CType

Namespace Development.Package.File

    Public Class DESCRIPTION : Implements ICTypeList, IsEmpty

        ''' <summary>
        ''' the package name
        ''' </summary>
        ''' <returns></returns>
        Public Property Package As String
        Public Property Type As String
        Public Property Title As String
        ''' <summary>
        ''' the package version string
        ''' </summary>
        ''' <returns></returns>
        Public Property Version As String
        Public Property [Date] As String
        Public Property Author As String
        Public Property Maintainer As String
        Public Property Description As String
        Public Property License As String
        Public Property meta As Dictionary(Of String, String)

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

        Public Shared Function Parse(file As String) As DESCRIPTION
            Dim meta As Dictionary(Of String, String) = file.SolveListStream.ParseTagData
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
