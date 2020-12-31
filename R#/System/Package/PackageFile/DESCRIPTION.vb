#Region "Microsoft.VisualBasic::012598f5c7a874ac974188f0a02b574d, R#\System\Package\PackageFile\DESCRIPTION.vb"

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

    '     Class DESCRIPTION
    ' 
    '         Properties: [Date], Author, Description, License, Maintainer
    '                     meta, Package, Title, Type, Version
    ' 
    '         Function: Parse, toList, ToString
    ' 
    '         Sub: ParserLoopStep
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop.CType

Namespace System.Package.File

    Public Class DESCRIPTION : Implements ICTypeList

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
            Dim lines As String() = file.SolveListStream.ToArray
            Dim index As New DESCRIPTION With {
                .meta = New Dictionary(Of String, String)
            }
            Dim lastTag As String = Nothing

            For Each line As String In lines
                Call ParserLoopStep(line, lastTag, index)
            Next

            Return index
        End Function

        Private Shared Sub ParserLoopStep(line As String, ByRef lastTag$, index As DESCRIPTION)
            Dim tag As NamedValue(Of String) = line.GetTagValue(":", trim:=True)
            Dim continuteLine As String
            Dim valueStr As String

            Static writer As Dictionary(Of String, PropertyInfo) = DataFramework.Schema(Of DESCRIPTION)(
                flag:=PropertyAccess.Writeable,
                nonIndex:=True,
                primitive:=True
            ).ToUpper

            If tag.Name.StringEmpty Then
                If lastTag.StringEmpty Then
                    Throw New SyntaxErrorException("invalid content format of the 'DESCRIPTION' meta data file!")
                ElseIf index.meta.ContainsKey(lastTag) Then
                    continuteLine = index.meta(lastTag) & vbCrLf & line
                    writer(lastTag).SetValue(index, continuteLine)
                Else
                    valueStr = writer(lastTag).GetValue(index)?.ToString
                    continuteLine = valueStr & vbCrLf & line
                    writer(lastTag).SetValue(index, continuteLine)
                End If
            ElseIf Not writer.ContainsKey(tag.Name.ToUpper) Then
                lastTag = tag.Name
                index.meta(lastTag) = tag.Value
            Else
                lastTag = tag.Name.ToUpper
                writer(lastTag).SetValue(index, tag.Value)
            End If
        End Sub
    End Class
End Namespace
