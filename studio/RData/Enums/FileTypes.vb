Imports System.ComponentModel

''' <summary>
''' Type of file containing a R file.
''' </summary>
Public Enum FileTypes
    Unknown = 0

    <Description("bz2")> bzip2
    <Description("gzip")> gzip
    <Description("xz")> xz
    <Description("rdata version 2 (binary)")> rdata_binary_v2
    <Description("rdata version 3 (binary)")> rdata_binary_v3
End Enum

