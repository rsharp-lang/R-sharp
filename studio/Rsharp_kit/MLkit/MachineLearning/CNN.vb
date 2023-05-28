#Region "Microsoft.VisualBasic::a9bff2c6c1fbb09fb7035b756237c991, F:/GCModeller/src/R-sharp/studio/Rsharp_kit/MLkit//MachineLearning/CNN.vb"

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

    '   Total Lines: 71
    '    Code Lines: 40
    ' Comment Lines: 22
    '   Blank Lines: 9
    '     File Size: 2.44 KB


    ' Module CNN
    ' 
    '     Function: detectObject, loadModel, saveModel
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MachineLearning.Convolutional
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

''' <summary>
''' feed-forward phase of deep Convolutional Neural Networks
''' </summary>
<Package("CNN")>
Module CNN

    ''' <summary>
    ''' load a CNN model from file
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <ExportAPI("CeNiN")>
    Public Function loadModel(file As String) As CeNiN
        Return New CeNiN(file)
    End Function

    ''' <summary>
    ''' classify a object from a given image data
    ''' </summary>
    ''' <param name="model"></param>
    ''' <param name="target"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("detectObject")>
    Public Function detectObject(model As CeNiN, target As Bitmap, Optional env As Environment = Nothing) As dataframe
        Dim result As NamedValue(Of Double)() = model.DetectObject(target, dev:=env.globalEnvironment.stdout)
        Dim classes As String() = result.Select(Function(v) v.Name).ToArray
        Dim probs As Double() = result.Select(Function(v) v.Value).ToArray

        Return New dataframe With {
            .columns = New Dictionary(Of String, Array) From {
                {"class", classes},
                {"probs", probs}
            }
        }
    End Function

    ''' <summary>
    ''' save the CNN model into a binary data file
    ''' </summary>
    ''' <param name="model"></param>
    ''' <param name="file"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("saveModel")>
    Public Function saveModel(model As CeNiN, file As Object, Optional env As Environment = Nothing) As Object
        Dim buffer = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Write, env)

        If buffer Like GetType(Message) Then
            Return buffer.TryCast(Of Message)
        End If

        Dim result As Boolean = model.Save(buffer)

        If TypeOf file Is String Then
            Call buffer.TryCast(Of Stream).Dispose()
        End If

        Return result
    End Function
End Module
