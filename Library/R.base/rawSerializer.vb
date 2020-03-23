Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports cdfAttribute = Microsoft.VisualBasic.Data.IO.netCDF.Components.attribute

Module rawSerializer

    <Extension>
    Public Sub writeObject(cdf As CDFWriter, symbolRef$, obj As Object)
        If obj Is Nothing Then
            ' skip null?
            Return
        ElseIf TypeOf obj Is list Then
            Call cdf.writeList(symbolRef, DirectCast(obj, list).slots)
        ElseIf TypeOf obj Is Dictionary(Of String, Object) Then
            Call cdf.writeList(symbolRef, obj)
        ElseIf obj.GetType.ImplementInterface(Of IDictionary) Then
            Dim list As New Dictionary(Of String, Object)
            Dim raw As IDictionary = obj

            For Each key As Object In raw.Keys
                list(Scripting.ToString(key)) = raw.Item(key)
            Next

            Call cdf.writeList(symbolRef, list)
        Else
            Dim vector As Array = Runtime.asVector(Of Object)(obj)
            Dim elTypes = vector.AsObjectEnumerator _
                .Select(Function(o) o.GetType) _
                .GroupBy(Function(t) t.FullName) _
                .ToArray _
                .OrderByDescending(Function(g) g.Count) _
                .First _
                .First

            If elTypes Is GetType(String) Then
                Call cdf.writeString(symbolRef, Runtime.asVector(Of String)(vector))
            Else
                Dim value As CDFData = (CObj(vector), elTypes.GetCDFTypeCode)
                Dim attributes As cdfAttribute() = {
                    New cdfAttribute With {.name = "length", .type = CDFDataTypes.INT, .value = vector.Length},
                    New cdfAttribute With {.name = "type", .type = CDFDataTypes.INT, .value = CInt(elTypes.GetRTypeCode)}
                }

                cdf.AddVariable(obj.Name, value, {cdf.getDimension(elTypes.FullName)}, attributes)
            End If
        End If
    End Sub

    <Extension>
    Private Sub writeString(cdf As CDFWriter, symbolRef$, strings As String())
        Using buffer As New MemoryStream, sb As New BinaryDataWriter(buffer)
            For Each str As String In strings
                Call sb.Write(str, BinaryStringFormat.DwordLengthPrefix)
            Next

            Call sb.Flush()

            Dim value As New CDFData With {.chars = buffer.ToArray.ToBase64String}
            Dim attributes As cdfAttribute() = {
                New cdfAttribute With {.name = "length", .type = CDFDataTypes.INT, .value = strings.Length},
                New cdfAttribute With {.name = "type", .type = CDFDataTypes.INT, .value = CInt(TypeCodes.string)}
            }

            cdf.AddVariable(symbolRef, value, {cdf.getDimension(GetType(String).FullName)}, attributes)
        End Using
    End Sub

    <Extension>
    Private Sub writeList(cdf As CDFWriter, symbolRef$, list As Dictionary(Of String, Object))
        ' write symbol
        Dim attributes As cdfAttribute() = {
            New cdfAttribute With {.name = "length", .type = CDFDataTypes.INT, .value = list.Count}，
            New cdfAttribute With {.name = "type", .type = CDFDataTypes.INT, .value = CInt(TypeCodes.list)}
        }
        Dim symbolVal As CDFData = {CInt(TypeCodes.list)}

        Call cdf.AddVariable(symbolRef, symbolVal, {cdf.getDimension(GetType(Integer).FullName)}, attributes)

        ' write names
        Call cdf.writeString($"{symbolRef}\names", list.Keys.ToArray)

        ' write values
        For Each name As String In list.Keys
            Call cdf.writeObject($"{symbolRef}\slots\{name}", list(name))
        Next
    End Sub
End Module
