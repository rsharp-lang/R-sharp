#Region "Microsoft.VisualBasic::56f8e042d53751c2b6bede8081bbb0a2, R#\Runtime\Internal\objects\dataset\pipeline.vb"

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

    '     Class pipeline
    ' 
    '         Properties: isError, isMessage
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: CreateFromPopulator, createVector, getError, populates, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' The R# pipeline
    ''' </summary>
    Public Class pipeline : Inherits RsharpDataObject

        ReadOnly pipeline As IEnumerable

        Public Property [pipeFinalize] As Action

        Public ReadOnly Property isError As Boolean
            Get
                Return TypeOf pipeline Is Message AndAlso DirectCast(pipeline, Message).level = MSG_TYPES.ERR
            End Get
        End Property

        Public ReadOnly Property isMessage As Boolean
            Get
                Return TypeOf pipeline Is Message AndAlso GetType(Message) Is elementType
            End Get
        End Property

        Sub New(input As IEnumerable, type As Type)
            pipeline = input
            elementType = RType.GetRSharpType(type)
        End Sub

        Sub New(input As IEnumerable, type As RType)
            pipeline = input
            elementType = type
        End Sub

        Public Function getError() As Message
            If isError Then
                Return DirectCast(CObj(pipeline), Message)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' Only suite for small set of data
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' If the data in current pipeline is super large, then
        ''' the program will be crashed.
        ''' </remarks>
        Public Function createVector(env As Environment) As vector
            Return New vector(elementType, populates(Of Object), env)
        End Function

        Public Iterator Function populates(Of T)() As IEnumerable(Of T)
            For Each obj As Object In pipeline
                Yield DirectCast(obj, T)
            Next

            If Not _pipeFinalize Is Nothing Then
                Call _pipeFinalize()
            End If
        End Function

        Public Overrides Function ToString() As String
            If isError Then
                Return DirectCast(pipeline, Message).ToString
            Else
                Return $"pipeline[{elementType.ToString}]"
            End If
        End Function

        <DebuggerStepThrough>
        Public Shared Function CreateFromPopulator(Of T)(upstream As IEnumerable(Of T), Optional finalize As Action = Nothing) As pipeline
            Return New pipeline(upstream, GetType(T)) With {
                .pipeFinalize = finalize
            }
        End Function

        Public Shared Widening Operator CType([error] As Message) As pipeline
            Return New pipeline([error], GetType(Message))
        End Operator

    End Class
End Namespace
