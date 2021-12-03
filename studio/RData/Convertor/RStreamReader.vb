Imports System.Runtime.CompilerServices
Imports REnv = SMRUCC.Rsharp.Runtime

Public Class RStreamReader

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function ReadString(robj As RObject) As String
        Return robj.DecodeCharacters
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function ReadNumbers(robj As RObject) As Double()
        Return REnv.asVector(Of Double)(robj.value.data)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function ReadIntegers(robj As RObject) As Long()
        Return REnv.asVector(Of Long)(robj.value.data)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function ReadLogicals(robj As RObject) As Boolean()
        Return REnv.asVector(Of Boolean)(robj.value.data)
    End Function

    ''' <summary>
    ''' read R vector in any element data type
    ''' </summary>
    ''' <param name="robj"></param>
    ''' <returns></returns>
    Public Shared Function ReadVector(robj As RObject) As Array
        Select Case robj.info.type
            Case RObjectType.CHAR : Return ReadString(robj).ToArray
            Case RObjectType.STR : Return ReadStrings(robj)
            Case RObjectType.INT : Return ReadIntegers(robj)
            Case RObjectType.REAL : Return ReadNumbers(robj)
            Case RObjectType.LGL : Return ReadLogicals(robj)
            Case Else
                Throw New NotImplementedException(robj.info.ToString)
        End Select
    End Function

    Friend Shared Function ReadStrings(robj As Object) As String()
        If TypeOf robj Is RList Then
            Return ReadStrings(DirectCast(robj, RList).CAR)
        ElseIf DirectCast(robj, RObject).info.type = RObjectType.LIST Then
            Return ReadStrings(DirectCast(robj, RObject).value)
        Else
            Dim obj As RObject = DirectCast(robj, RObject)

            If obj.info.type = RObjectType.STR Then
                Return DirectCast(obj.value.data, RObject()) _
                    .Select(AddressOf ReadString) _
                    .ToArray
            Else
                Return {ReadString(obj)}
            End If
        End If
    End Function
End Class
