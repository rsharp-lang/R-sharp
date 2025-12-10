#Region "Microsoft.VisualBasic::e58d2219a3f2bbb7a710d923334cd734, Library\base\base.vb"

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

    '   Total Lines: 177
    '    Code Lines: 107 (60.45%)
    ' Comment Lines: 43 (24.29%)
    '    - Xml Docs: 93.02%
    ' 
    '   Blank Lines: 27 (15.25%)
    '     File Size: 6.51 KB


    ' Module base
    ' 
    '     Function: class_labeled, impute, loadMsgPack, ParseTtl, readSasXptDataframe
    '               string_motif
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.IO.MessagePack
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Matrix
Imports Microsoft.VisualBasic.MIME.application.rdf_xml.Turtle
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.Patterns
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports DataFrame = Microsoft.VisualBasic.Data.Framework.DataFrame
Imports RDataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

''' <summary>
''' #### The R Base Package
''' 
''' This package contains the basic functions which let R function as a language: 
''' arithmetic, input/output, basic programming support, etc. Its contents are 
''' available through inheritance from any environment.
'''
''' For a complete list of functions, use ``ls("base")``.
''' </summary>
<Package("base", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@gcmodeller.org")>
Public Module base

    ''' <summary>
    ''' impute for missing values
    ''' </summary>
    ''' <param name="rawMatrix"></param>
    ''' <param name="byRow"></param>
    ''' <param name="infer"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("impute")>
    Public Function impute(rawMatrix As DataSet(),
                           Optional byRow As Boolean = True,
                           Optional infer As InferMethods = InferMethods.Average) As DataSet()

        Return rawMatrix.SimulateMissingValues(byRow, infer).ToArray
    End Function

    ''' <summary>
    ''' parse the RDF Turtle document
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("parseTtl")>
    <RApiReturn(GetType(Triple))>
    Public Function ParseTtl(<RRawVectorArgument> file As Object,
                             Optional lazy As Boolean = True,
                             Optional env As Environment = Nothing) As Object

        Dim stream = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env)

        If stream Like GetType(Message) Then
            Return stream.TryCast(Of Message)
        End If

        Dim reader As New TurtleFile(stream)

        If lazy Then
            Return pipeline.CreateFromPopulator(reader.ReadObjects)
        Else
            Return reader.ReadObjects.ToArray
        End If
    End Function

    ''' <summary>
    ''' A helper function for load the messagepack dataset
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="[typeof]"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("load.msgpack")>
    Public Function loadMsgPack(<RRawVectorArgument> file As Object, [typeof] As Object, Optional env As Environment = Nothing) As Object
        Dim stream = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env)

        If stream Like GetType(Message) Then
            Return stream.TryCast(Of Message)
        End If

        Dim rtype As RType = env.globalEnvironment.GetType([typeof])

        If rtype.raw Is GetType(Object) Then
            Return RInternal.debug.stop("a valid type schema must be provided to construct the .net clr data set!", env)
        End If

        Dim type As Type = rtype.raw

        If Not type.IsArray Then
            type = type.MakeArrayType
        End If

        Dim value As Object = MsgPackSerializer.Deserialize(type, stream.TryCast(Of Stream))

        If TypeOf file Is String Then
            Call stream.TryCast(Of Stream).Dispose()
        End If

        Return value
    End Function

    ''' <summary>
    ''' get common string motif
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="threshold"></param>
    ''' <param name="any"></param>
    ''' <returns></returns>
    <ExportAPI("string_motif")>
    Public Function string_motif(<RRawVectorArgument> x As Object,
                                 Optional threshold As Double = 0.3,
                                 Optional any As Char = "-"c,
                                 Optional trim As Boolean = True) As Object

        Dim motif As String = CommonTagParser.StringMotif(CLRVector.asCharacter(x), threshold, any)
        If trim Then
            motif = motif.Trim(any)
            motif = motif.Replace(any & any, any)
            motif = motif.Replace(any & any, any)
            motif = motif.Replace(any & any, any)
        End If
        Return motif
    End Function

    ''' <summary>
    ''' read the SAS XPT file as dataframe
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("read.sas_xpt")>
    Public Function readSasXptDataframe(<RRawVectorArgument> file As Object,
                                        Optional row_names As Object = Nothing,
                                        Optional env As Environment = Nothing) As Object
        Dim is_file As Boolean = False
        Dim s = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env, is_filepath:=is_file)
        Dim data As DataFrame

        If s Like GetType(Message) Then
            Return s.TryCast(Of Message)
        Else
            data = FrameReader.ReadSasXPT(s.TryCast(Of Stream))
        End If

        Dim table As RDataframe = data.toDataframe(list.empty, env)

        If is_file Then
            Call s.TryCast(Of Stream).Close()
            Call s.TryCast(Of Stream).Dispose()
        End If

        Return table
    End Function

    <ExportAPI("class_labeled")>
    Public Function class_labeled(classSet As list, Optional env As Environment = Nothing) As list
        Dim classList As Dictionary(Of String, String()) = classSet.AsGeneric(Of String())(env)
        Dim labels As New Dictionary(Of String, Object)

        For Each [class] In classList
            For Each id As String In [class].Value.SafeQuery
                labels(id) = [class].Key
            Next
        Next

        Return New list() With {.slots = labels}
    End Function
End Module
