Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Object.serialize

    Public Class vectorBuffer : Inherits RawStream

        ''' <summary>
        ''' the full name of the <see cref="Global.System.Type"/> for create <see cref="RType"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property type As String
        Public Property vector As Array
        Public Property names As String()
        Public Property unit As String

        Public Shared Function CreateBuffer(vector As vector, env As Environment) As Object
            Dim buffer As New vectorBuffer With {
                .names = If(vector.getNames, {}),
                .type = vector.elementType.raw.FullName,
                .unit = If(vector.unit?.name, "")
            }
            Dim generic = REnv.asVector(vector.data, vector.elementType.raw, env)

            If TypeOf generic Is Message Then
                Return generic
            Else
                buffer.vector = generic
            End If

            Return buffer
        End Function

        Public Shared Function CreateBuffer(bytes As Stream) As vectorBuffer
            Dim raw As Byte() = New Byte(2 * Marshal.SizeOf(GetType(Integer)) - 1) {}

            bytes.Read(raw, Scan0, raw.Length)

            Dim name_size As Integer = BitConverter.ToInt32(raw, Scan0)
            Dim vector_size As Integer = BitConverter.ToInt32(raw, Marshal.SizeOf(GetType(Integer)))

            Using reader As New StreamReader(bytes)
                Dim type As Type = Type.GetType(reader.ReadLine)
                Dim unit As String = reader.ReadLine

                Dim names As String() = New String(name_size - 1) {}

                For i As Integer = 0 To names.Length - 1
                    names(i) = reader.ReadLine
                Next

                name_size = Marshal.SizeOf(GetType(Type))
                raw = New Byte(vector_size * name_size - 1) {}

                bytes.Read(raw, Scan0, raw.Length)



                Return New vectorBuffer With {
                    .type = type.FullName,
                    .names = names,
                    .unit = unit
                }
            End Using
        End Function

        Public Overrides Function Serialize() As Byte()
            Dim type As Type = Type.GetType(Me.type)

            Using buffer As New MemoryStream, output As New StreamWriter(buffer)
                output.Write(names.Length)
                output.Write(vector.Length)

                output.WriteLine(type.FullName)
                output.WriteLine(unit)

                For i As Integer = 0 To names.Length - 1
                    output.WriteLine(names(i))
                Next

                output.Flush()

                Dim raw As Byte()

                Select Case type
                    Case GetType(Integer)
                        raw = DirectCast(vector, Integer()) _
                            .Select(Function(s) BitConverter.GetBytes(s)) _
                            .IteratesALL _
                            .ToArray
                    Case GetType(Long)
                        raw = DirectCast(vector, Long()) _
                            .Select(Function(s) BitConverter.GetBytes(s)) _
                            .IteratesALL _
                            .ToArray
                    Case GetType(Double)
                        raw = DirectCast(vector, Double()) _
                            .Select(Function(s) BitConverter.GetBytes(s)) _
                            .IteratesALL _
                            .ToArray
                    Case GetType(Single)
                        raw = DirectCast(vector, Single()) _
                            .Select(Function(s) BitConverter.GetBytes(s)) _
                            .IteratesALL _
                            .ToArray
                    Case GetType(Boolean)
                        raw = DirectCast(vector, Boolean()) _
                            .Select(Function(b) If(b, CByte(1), CByte(0))) _
                            .ToArray
                    Case GetType(Byte)
                        raw = DirectCast(vector, Byte())
                    Case Else
                        Throw New NotImplementedException(type.FullName)
                End Select

                buffer.Write(raw, Scan0, raw.Length)
                output.Flush()
                buffer.Flush()

                Return buffer.ToArray
            End Using
        End Function
    End Class
End Namespace