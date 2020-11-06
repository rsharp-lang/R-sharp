Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
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

        Public ReadOnly Property underlyingType As Type
            Get
                Return Global.System.Type.GetType(type)
            End Get
        End Property

        Public Function GetVector() As vector
            Dim rtype As RType = RType.GetRSharpType(underlyingType)
            Dim unit As New unit With {.name = Me.unit}
            Dim vec As New vector(names, vector, rtype, unit)

            Return vec
        End Function

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

                raw = New Byte(3) {}
                bytes.Read(raw, Scan0, raw.Length)
                name_size = BitConverter.ToInt32(raw, Scan0)
                raw = New Byte(name_size - 1) {}
                bytes.Read(raw, Scan0, raw.Length)

                Using ms As New MemoryStream(raw)
                    Dim vector As Array = RawStream.GetData(ms, type.PrimitiveTypeCode)

                    Return New vectorBuffer With {
                        .type = type.FullName,
                        .names = names,
                        .unit = unit,
                        .vector = vector
                    }
                End Using
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

                Dim raw As Byte() = RawStream.GetBytes(vector)

                output.Write(raw.Length)
                buffer.Write(raw, Scan0, raw.Length)
                output.Flush()
                buffer.Flush()

                Return buffer.ToArray
            End Using
        End Function
    End Class
End Namespace