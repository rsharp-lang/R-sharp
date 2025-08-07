Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Development.Components
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

<Package("bson")>
Module bson

    <ExportAPI>
    Public Function read_bson(file As String)

    End Function

    ''' <summary>
    ''' cast any object to bson model
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI>
    Public Function as_bson(<RRawVectorArgument> x As Object,
                            Optional maskReadonly As Boolean = False,
                            Optional enumToStr As Boolean = True,
                            Optional unixTimestamp As Boolean = True,
                            <RListObjectArgument>
                            Optional args As list = Nothing,
                            Optional env As Environment = Nothing) As Object

        Dim opts As New JSONSerializerOptions With {
            .indent = False,
            .maskReadonly = maskReadonly,
            .enumToString = enumToStr,
            .unixTimestamp = unixTimestamp
        }
        Dim encoder As Encoder = Encoder.CreateEncoderWithOptions(args, env)
        Dim err As Message = Nothing
        Dim json As JsonElement

        If x Is Nothing Then
            Return RInternal.debug.stop("the given object data can not be nothing!", env)
        Else
            json = opts.GetJsonLiteralRaw(x, encoder, err, env)
        End If

        If Not err Is Nothing Then
            Return err
        ElseIf Not TypeOf json Is JsonObject Then
            Return RInternal.debug.stop($"the given json data model must be an object! ({json.GetType.Name} was given...)", env)
        End If

        Return json
    End Function

    ''' <summary>
    ''' parse the binary JSON data into the raw JSON model or R data object
    ''' </summary>
    ''' <param name="buffer">the binary data package in BSON format</param>
    ''' <param name="raw"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("parse_bson")>
    Public Function parseBSON(<RRawVectorArgument> buffer As Object,
                              Optional raw As Boolean = False,
                              Optional env As Environment = Nothing) As Object

        Dim bytes As pipeline = pipeline.TryCreatePipeline(Of Byte)(buffer, env, suppress:=True)
        Dim bufStream As Stream

        If bytes.isError Then
            If TypeOf buffer Is Stream Then
                bufStream = DirectCast(buffer, Stream)
            Else
                Return bytes.getError
            End If
        Else
            bufStream = New MemoryStream(bytes.populates(Of Byte)(env).ToArray)
        End If

        Dim json As JsonObject = New MIME.application.json.BSON.Decoder(bufStream).decodeDocument

        If raw Then
            Return json
        Else
            Return json.createRObj(env, decodeMetachar:=False)
        End If
    End Function

    ''' <summary>
    ''' save any R object into BSON stream data
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="file">
    ''' the file resource that used for save the BSON data, if this parameter is empty, then
    ''' a binary data stream that contains the BSON data will be returned.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("write_bson")>
    Public Function writeBSON(<RRawVectorArgument>
                              x As Object,
                              Optional file As Object = Nothing,
                              Optional maskReadonly As Boolean = False,
                              Optional enumToStr As Boolean = True,
                              Optional unixTimestamp As Boolean = True,
                              <RListObjectArgument>
                              Optional args As list = Nothing,
                              Optional env As Environment = Nothing) As Object

        Dim stream As Stream

        If file Is Nothing Then
            stream = New MemoryStream
        Else
            If TypeOf file Is Stream Then
                stream = DirectCast(file, Stream)
            ElseIf TypeOf file Is String Then
                stream = DirectCast(file, String).Open
            Else
                Return Message.InCompatibleType(GetType(Stream), file.GetType, env)
            End If
        End If

        Dim json = as_bson(x, maskReadonly, enumToStr, unixTimestamp, args, env)

        Call MIME.application.json.BSON.SafeWriteBuffer(json, stream)

        If file Is Nothing Then
            Return DirectCast(stream, MemoryStream).ToArray
        Else
            Call stream.Flush()
            Call stream.Close()
            Call stream.Dispose()

            Return Nothing
        End If
    End Function

End Module
