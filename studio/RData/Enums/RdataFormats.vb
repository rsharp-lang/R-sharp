Imports System.ComponentModel

''' <summary>
''' Format of a R file.
''' </summary>
Public Enum RdataFormats
    Unknown = 0

    <Description("XDR")> XDR
    <Description("ASCII")> ASCII
    <Description("binary")> binary
End Enum
