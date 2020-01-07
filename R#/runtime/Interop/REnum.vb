Imports System.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Runtime.Interop

    ''' <summary>
    ''' VB.NET enum type wrapper in R#
    ''' </summary>
    Public Class REnum

        Public Property raw As Type

        ReadOnly namedValues As New Dictionary(Of String, Object)
        ReadOnly intValues As New Dictionary(Of String, Object)
        ReadOnly namedIntegers As New Dictionary(Of String, Long)

        Public ReadOnly Iterator Property values As IEnumerable(Of Object)
            Get
                For Each item As Object In namedValues.Values
                    Yield item
                Next
            End Get
        End Property

        Public ReadOnly Property name As String
            Get
                Return raw.Name
            End Get
        End Property

        Public ReadOnly Property baseType As Type

        Private Sub New(type As Type)
            raw = type

            ' parsing enum type values for 
            ' named values and 
            ' int values
            Call doEnumParser()

            baseType = DirectCast(namedValues.Values.First, [Enum]) _
                .GetTypeCode() _
                .CreatePrimitiveType()
        End Sub

        Public Function IntValue(val As Object) As Long
            Dim key As String = val.ToString.ToLower

            If namedIntegers.ContainsKey(key) Then
                Return namedIntegers(key)
            Else
                Return Conversion.CTypeDynamic(val, GetType(Long))
            End If
        End Function

        Private Sub doEnumParser()
            Dim values As [Enum]() = raw _
                .GetEnumValues _
                .AsObjectEnumerator _
                .Select(Function(flag) DirectCast(flag, [Enum])) _
                .ToArray
            Dim members As Dictionary(Of String, FieldInfo) = raw.GetFields _
                .Where(Function(field) field.FieldType Is raw) _
                .ToDictionary(Function(flag)
                                  Return flag.GetValue(Nothing).ToString
                              End Function)
            Dim int As Long

            For Each flag As [Enum] In values
                int = CLng(members(flag.ToString).GetValue(Nothing))
                intValues.Add("T" & int, flag)
                namedValues.Add(flag.ToString.ToLower, flag)
                namedIntegers.Add(flag.ToString.ToLower, int)
            Next
        End Sub

        Public Function GetByName(name As String) As Object
            Return namedValues.TryGetValue(name.ToLower)
        End Function

        Public Function getByIntVal(int As Long) As Object
            Dim key = "T" & int

            If intValues.ContainsKey(key) Then
                Return intValues(key)
            Else
                ' is a flags combination
                Return Conversion.CTypeDynamic(int, baseType)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"({baseType.Name}) {raw.FullName}"
        End Function

        Public Shared Function GetEnumList(type As Type) As REnum
            If Not type.IsEnum Then
                Throw New InvalidCastException(type.FullName)
            End If

            Static enumCache As New Dictionary(Of Type, REnum)
            Return enumCache.ComputeIfAbsent(type, Function() New REnum(type))
        End Function
    End Class
End Namespace