Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.ConsolePrinter

    Public Module enumPrinter

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="obj">
        ''' This object should be an enum value.
        ''' </param>
        ''' <returns></returns>
        Public Function printClass(obj As Object) As String
            Dim type As REnum = REnum.GetEnumList(obj.GetType)
            Dim base As Type = type.baseType
            Dim describ$ = DirectCast(obj, [Enum]).Description
            Dim print$ = $"{{{base.Name.ToLower} {type.IntValue(obj)}}} {obj.ToString}"

            If describ = obj.ToString Then
                Return print
            Else
                Return $"{print} #{describ}"
            End If
        End Function
    End Module
End Namespace