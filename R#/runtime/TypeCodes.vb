''' <summary>
''' The R# types
''' </summary>
Public Enum TypeCodes
    ''' <summary>
    ''' <see cref="Integer"/> vector
    ''' </summary>
    [integer]
    ''' <summary>
    ''' <see cref="ULong"/> vector
    ''' </summary>
    [uinteger]
    ''' <summary>
    ''' <see cref="Double"/> numeric vector
    ''' </summary>
    [double]
    ''' <summary>
    ''' <see cref="String"/> vector
    ''' </summary>
    [string]
    ''' <summary>
    ''' <see cref="Char"/> vector
    ''' </summary>
    [char]
    ''' <summary>
    ''' <see cref="Boolean"/> vector
    ''' </summary>
    [boolean]
    ''' <summary>
    ''' Class type in R#
    ''' </summary>
    [list]
    ''' <summary>
    ''' Object type in R#
    ''' </summary>
    [generic]
End Enum
