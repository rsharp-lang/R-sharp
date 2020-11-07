Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.ValueTypes
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Internal.Object.Converts

    Module castRaw

        <Extension>
        Public Function castToRawRoutine(env As Environment,
                                         obj As Object,
                                         encoding As Encodings,
                                         networkByteOrder As Boolean) As Object

            Dim buffer As New MemoryStream
            Dim encoder As Encoding = encoding.CodePage
            Dim chunk As Byte()
            Dim needReverse As Boolean = BitConverter.IsLittleEndian AndAlso networkByteOrder
            Dim isNumeric As Boolean
            Dim all As Object() = pipeline.TryCreatePipeline(Of Object)(obj, env).populates(Of Object)(env).ToArray

            For Each item As Object In all
                isNumeric = True

                Select Case item.GetType
                    Case GetType(String)
                        If all.Length > 1 Then
                            chunk = (encoder.GetBytes(DirectCast(item, String)).AsList + CByte(0)).ToArray
                        Else
                            chunk = (encoder.GetBytes(DirectCast(item, String)))
                        End If

                        isNumeric = False
                    Case GetType(Integer)
                        chunk = BitConverter.GetBytes(DirectCast(item, Integer))
                    Case GetType(Long)
                        chunk = BitConverter.GetBytes(DirectCast(item, Long))
                    Case GetType(Short)
                        chunk = BitConverter.GetBytes(DirectCast(item, Short))
                    Case GetType(Single)
                        chunk = BitConverter.GetBytes(DirectCast(item, Single))
                    Case GetType(Double)
                        chunk = BitConverter.GetBytes(DirectCast(item, Double))
                    Case GetType(Byte)
                        chunk = BitConverter.GetBytes(DirectCast(item, Byte))
                    Case GetType(Boolean)
                        chunk = {CByte(If(DirectCast(item, Boolean), 1, 0))}
                        isNumeric = False
                    Case GetType(Date)
                        chunk = BitConverter.GetBytes(DirectCast(item, Date).UnixTimeStamp)
                    Case GetType(Char)
                        chunk = encoder.GetBytes({DirectCast(item, Char)})
                        isNumeric = False
                    Case Else
                        Return Internal.debug.stop(Message.InCompatibleType(GetType(String), item.GetType, env), env)
                End Select

                If isNumeric AndAlso needReverse Then
                    Call Array.Reverse(chunk)
                End If

                buffer.Write(chunk, Scan0, chunk.Length)
            Next

            Call buffer.Flush()

            Return buffer
        End Function
    End Module
End Namespace