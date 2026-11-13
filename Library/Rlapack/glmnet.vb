Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports GlmnetLib = Microsoft.VisualBasic.MachineLearning.GLMNet
Imports Mtx = Microsoft.VisualBasic.Math
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal
Imports std = System.Math

''' <summary>
''' ### Lasso and Elastic-Net Regularized Generalized Linear Models
'''
''' R# wrapper of the ``GLMNet`` kernel. It mirrors the GNU R ``glmnet`` package:
''' ``glmnet()`` fits a whole regularization path, ``cv.glmnet()`` runs K-fold
''' cross validation, and ``predict.glmnet()`` / ``coef.glmnet()`` /
''' ``deviance.glmnet()`` evaluate the fitted model.
'''
''' Results are returned as R-style named lists (``fit$lambda``, ``fit$beta``,
''' ``cv$lambda.min``, ...). The native CLR fit object is kept in a private slot
''' so that the prediction helpers do not have to re-materialize the path.
''' </summary>
<Package("glmnet", Category:=APICategories.ResearchTools, Publisher:="xie.guigang@live.com")>
<RTypeExport("glmnet.fit", GetType(GlmnetLib.GlmnetFit))>
<RTypeExport("cv.glmnet.fit", GetType(GlmnetLib.GlmnetCVFit))>
Module glmnet

    ''' <summary>private slot holding the native <see cref="GlmnetLib.GlmnetFit"/>.</summary>
    Private Const FitSlot As String = ".__glmnet_fit__"
    ''' <summary>private slot holding the native <see cref="GlmnetLib.GlmnetCVFit"/>.</summary>
    Private Const CvSlot As String = ".__glmnet_cv__"

    ' ------------------------------------------------------------------
    '  Input conversion helpers
    ' ------------------------------------------------------------------

    ''' <summary>
    ''' Convert a R# dataframe / matrix / list / numeric vector into a row-major
    ''' ``Double()()`` matrix. Follows the conventions used by
    ''' ``RMatrix.vb::matrix_extractor`` and ``stats.vb::getMatrix``.
    ''' </summary>
    Private Function toMatrix(x As Object, env As Environment, ByRef err As Message) As Double()()
        If x Is Nothing Then
            err = RInternal.debug.stop("the input matrix is missing!", env)
            Return Nothing
        End If

        If TypeOf x Is Double()() Then
            Return DirectCast(x, Double()())
        End If

        If TypeOf x Is Rdataframe Then
            Dim df As Rdataframe = DirectCast(x, Rdataframe)
            Dim names As String() = df.colnames

            If names Is Nothing OrElse names.Length = 0 Then
                err = RInternal.debug.stop("the input dataframe has no numeric columns!", env)
                Return Nothing
            End If

            Dim cols As New List(Of Double())

            For Each name As String In names
                cols.Add(CLRVector.asNumeric(df.getColumnVector(name)))
            Next

            Dim n As Integer = cols(0).Length
            Dim p As Integer = cols.Count
            Dim mat As Double()() = New Double(n - 1)() {}

            For i As Integer = 0 To n - 1
                Dim row As Double() = New Double(p - 1) {}
                For j As Integer = 0 To p - 1
                    If i < cols(j).Length Then
                        row(j) = cols(j)(i)
                    End If
                Next
                mat(i) = row
            Next

            Return mat
        End If

        If TypeOf x Is Mtx.INumericMatrix Then
            Return DirectCast(x, Mtx.INumericMatrix).ArrayPack()
        End If

        If TypeOf x Is list Then
            Dim rows As New List(Of Double())

            For Each slot As KeyValuePair(Of String, Object) In DirectCast(x, list).slots
                rows.Add(CLRVector.asNumeric(slot.Value))
            Next

            If rows.Count = 0 Then
                err = RInternal.debug.stop("the input list is empty!", env)
                Return Nothing
            End If

            Return rows.ToArray()
        End If

        If TypeOf x Is vector Then
            Return toMatrix(DirectCast(x, vector).data, env, err)
        End If

        If TypeOf x Is Double() Then
            Dim v As Double() = DirectCast(x, Double())
            Dim mat1 As Double()() = New Double(v.Length - 1)() {}

            For i As Integer = 0 To v.Length - 1
                mat1(i) = New Double() {v(i)}
            Next

            Return mat1
        End If

        If TypeOf x Is Array Then
            Dim rows As New List(Of Double())

            For Each item As Object In DirectCast(x, Array)
                If item Is Nothing Then
                    err = RInternal.debug.stop("the input matrix contains an empty row!", env)
                    Return Nothing
                End If
                rows.Add(CLRVector.asNumeric(item))
            Next

            If rows.Count = 0 Then
                err = RInternal.debug.stop("the input matrix is empty!", env)
                Return Nothing
            End If

            Return rows.ToArray()
        End If

        err = Message.InCompatibleType(GetType(Rdataframe), x.GetType, env,
                                       message:="the input should be a dataframe, a numeric matrix or a numeric vector!")
        Return Nothing
    End Function

    ''' <summary>Convert a R# value into a plain numeric vector (or NULL).</summary>
    Private Function toVector(x As Object, env As Environment, ByRef err As Message) As Double()
        If x Is Nothing Then Return Nothing

        Dim v As Double() = CLRVector.asNumeric(x)

        If v Is Nothing OrElse v.Length = 0 Then
            err = RInternal.debug.stop("the numeric vector is empty!", env)
            Return Nothing
        End If
        For i As Integer = 0 To v.Length - 1
            If Not Double.IsFinite(v(i)) Then
                err = RInternal.debug.stop($"the numeric vector contains a non-finite value at index {i}!", env)
                Return Nothing
            End If
        Next

        Return v
    End Function

    ''' <summary>Convert a R# value into a character vector (or NULL).</summary>
    Private Function toNames(x As Object) As String()
        If x Is Nothing Then Return Nothing

        Dim names As String() = CLRVector.asCharacter(x)
        If names Is Nothing OrElse names.Length = 0 Then Return Nothing
        Return names
    End Function

    ''' <summary>
    ''' R uses 1-based ``foldid``/``exclude`` while the kernel uses 0-based
    ''' indices; keep the conversion in one place.
    ''' </summary>
    Private Function toZeroBased(x As Object, env As Environment, ByRef err As Message) As Integer()
        Dim v As Double() = toVector(x, env, err)
        If v Is Nothing Then Return Nothing

        Dim ids As Integer() = New Integer(v.Length - 1) {}

        For i As Integer = 0 To v.Length - 1
            Dim r As Double = std.Round(v(i), MidpointRounding.AwayFromZero)
            If r <> v(i) OrElse r < 1 Then
                err = RInternal.debug.stop($"the index at position {i} must be a positive integer (R is 1-based), but {v(i)} was given!", env)
                Return Nothing
            End If
            ids(i) = CInt(r) - 1
        Next

        Return ids
    End Function

    ''' <summary>Build the kernel option object shared by glmnet() and cv.glmnet().</summary>
    Private Function buildOptions(env As Environment, ByRef err As Message,
                                  alpha As Double,
                                  nlambda As Integer,
                                  lambda_min_ratio As Double,
                                  lambda As Object,
                                  weights As Object,
                                  offset As Object,
                                  standardize As Boolean,
                                  intercept As Boolean,
                                  penalty_factor As Object,
                                  lower_limits As Object,
                                  upper_limits As Object,
                                  exclude As Object,
                                  dfmax As Integer,
                                  pmax As Integer,
                                  thresh As Double,
                                  maxit As Integer,
                                  type_gaussian As String,
                                  type_multinomial As String,
                                  type_logistic As String,
                                  standardize_response As Boolean,
                                  relax As Boolean,
                                  feature_names As Object,
                                  response_names As Object) As GlmnetLib.GlmnetOptions

        Dim opts As New GlmnetLib.GlmnetOptions With {
            .Alpha = alpha,
            .NLambda = nlambda,
            .LambdaMinRatio = lambda_min_ratio,
            .Standardize = standardize,
            .Intercept = intercept,
            .DfMax = dfmax,
            .PMax = pmax,
            .Thresh = thresh,
            .MaxIt = maxit,
            .StandardizeResponse = standardize_response,
            .Relax = relax
        }

        If lambda IsNot Nothing Then
            Dim grid As Double() = toVector(lambda, env, err)
            If grid Is Nothing Then Return Nothing
            For i As Integer = 1 To grid.Length - 1
                If grid(i) <= 0 OrElse grid(i) >= grid(i - 1) Then
                    err = RInternal.debug.stop("the custom lambda grid must be positive and strictly decreasing!", env)
                    Return Nothing
                End If
            Next
            If grid(0) <= 0 Then
                err = RInternal.debug.stop("the custom lambda grid must be positive and strictly decreasing!", env)
                Return Nothing
            End If
            opts.Lambda = grid
        End If

        If weights IsNot Nothing Then
            opts.Weights = toVector(weights, env, err)
            If opts.Weights Is Nothing Then Return Nothing
        End If
        If penalty_factor IsNot Nothing Then
            opts.PenaltyFactor = toVector(penalty_factor, env, err)
            If opts.PenaltyFactor Is Nothing Then Return Nothing
        End If
        If lower_limits IsNot Nothing Then
            opts.LowerLimits = toVector(lower_limits, env, err)
            If opts.LowerLimits Is Nothing Then Return Nothing
        End If
        If upper_limits IsNot Nothing Then
            opts.UpperLimits = toVector(upper_limits, env, err)
            If opts.UpperLimits Is Nothing Then Return Nothing
        End If
        If exclude IsNot Nothing Then
            opts.Exclude = toZeroBased(exclude, env, err)
            If opts.Exclude Is Nothing Then Return Nothing
        End If
        If offset IsNot Nothing Then
            opts.Offset = toMatrix(offset, env, err)
            If opts.Offset Is Nothing Then Return Nothing
        End If

        opts.FeatureNames = toNames(feature_names)
        opts.ResponseNames = toNames(response_names)

        Select Case If(type_gaussian, "covariance").Trim().ToLowerInvariant()
            Case "covariance" : opts.TypeGaussian = GlmnetLib.GaussianAlgorithm.Covariance
            Case "naive" : opts.TypeGaussian = GlmnetLib.GaussianAlgorithm.Naive
            Case Else
                err = RInternal.debug.stop($"unsupported type.gaussian: '{type_gaussian}' (expect 'covariance' or 'naive')!", env)
                Return Nothing
        End Select

        Select Case If(type_multinomial, "ungrouped").Trim().ToLowerInvariant()
            Case "ungrouped" : opts.TypeMultinomial = GlmnetLib.MultinomialGrouping.Ungrouped
            Case "grouped" : opts.TypeMultinomial = GlmnetLib.MultinomialGrouping.Grouped
            Case Else
                err = RInternal.debug.stop($"unsupported type.multinomial: '{type_multinomial}' (expect 'grouped' or 'ungrouped')!", env)
                Return Nothing
        End Select

        Select Case If(type_logistic, "newton").Trim().ToLowerInvariant().Replace(".", "").Replace("-", "")
            Case "newton" : opts.TypeLogistic = GlmnetLib.LogisticAlgorithm.Newton
            Case "modifiednewton" : opts.TypeLogistic = GlmnetLib.LogisticAlgorithm.ModifiedNewton
            Case Else
                err = RInternal.debug.stop($"unsupported type.logistic: '{type_logistic}'!", env)
                Return Nothing
        End Select

        Return opts
    End Function

    ''' <summary>Parse a ``type.measure`` name used by cv.glmnet().</summary>
    Private Function parseMeasure(name As String) As GlmnetLib.CVMeasure?
        If String.IsNullOrWhiteSpace(name) Then Return Nothing

        Select Case name.Trim().ToLowerInvariant().Replace("-", "").Replace("_", "")
            Case "mse", "meansquarederror" : Return GlmnetLib.CVMeasure.Mse
            Case "mae", "meanabsoluteerror" : Return GlmnetLib.CVMeasure.Mae
            Case "deviance" : Return GlmnetLib.CVMeasure.Deviance
            Case "class", "misclassification", "misclassificationerror" : Return GlmnetLib.CVMeasure.[Class]
            Case "auc" : Return GlmnetLib.CVMeasure.Auc
            Case "cindex", "c" : Return GlmnetLib.CVMeasure.CIndex
            Case Else : Return Nothing
        End Select
    End Function

    ''' <summary>Validate and normalize the response matrix before calling the kernel.</summary>
    Private Function toResponse(y As Object, n As Integer, env As Environment, ByRef err As Message) As Double()()
        Dim mat As Double()() = toMatrix(y, env, err)
        If mat Is Nothing Then Return Nothing

        If mat.Length <> n Then
            err = RInternal.debug.stop($"x has {n} rows but y has {mat.Length} rows!", env)
            Return Nothing
        End If

        Return mat
    End Function

    ''' <summary>
    ''' ``FamilyRegistry`` is Friend to the kernel assembly, so the family name
    ''' is mirrored here for the R-style result metadata.
    ''' </summary>
    Private Function familyName(kind As GlmnetLib.FamilyKind) As String
        Select Case kind
            Case GlmnetLib.FamilyKind.Gaussian : Return "gaussian"
            Case GlmnetLib.FamilyKind.Binomial : Return "binomial"
            Case GlmnetLib.FamilyKind.Poisson : Return "poisson"
            Case GlmnetLib.FamilyKind.Multinomial : Return "multinomial"
            Case GlmnetLib.FamilyKind.Cox : Return "cox"
            Case GlmnetLib.FamilyKind.MGaussian : Return "mgaussian"
            Case Else : Return kind.ToString().ToLowerInvariant()
        End Select
    End Function

    ' ------------------------------------------------------------------
    '  Result assembly
    ' ------------------------------------------------------------------

    ''' <summary>One row per lambda step: path diagnostics of a glmnet fit.</summary>
    Private Function pathFrame(fit As GlmnetLib.GlmnetFit) As Rdataframe
        Dim steps As Integer = fit.Count
        Dim cols As New Dictionary(Of String, Array)
        Dim index As Integer() = New Integer(steps - 1) {}

        For s As Integer = 0 To steps - 1
            index(s) = s
        Next

        cols.Add("step", index)
        cols.Add("lambda", fit.lambda)
        cols.Add("df", fit.df)
        cols.Add("dev.ratio", fit.devRatio)
        cols.Add("nzero", fit.nzero)
        cols.Add("deviance", fit.deviance)
        cols.Add("converged", fit.converged)
        cols.Add("iterations", fit.iterations)
        cols.Add("nulldev", Enumerable.Repeat(fit.nulldev, steps).ToArray())
        cols.Add("jerr", Enumerable.Repeat(CInt(fit.jerr), steps).ToArray())

        Dim k As Integer = fit.nresponses

        For c As Integer = 0 To k - 1
            Dim name As String = If(k = 1, "a0", $"a0.{c + 1}")
            Dim col As Double() = New Double(steps - 1) {}

            For s As Integer = 0 To steps - 1
                col(s) = fit.a0(s)(c)
            Next

            cols.Add(name, col)
        Next

        Return New Rdataframe With {.columns = cols}
    End Function

    ''' <summary>One row per lambda step: the flattened coefficient matrix.</summary>
    Private Function betaFrame(fit As GlmnetLib.GlmnetFit) As Rdataframe
        Dim steps As Integer = fit.Count
        Dim p As Integer = fit.nvars
        Dim k As Integer = fit.nresponses
        Dim cols As New Dictionary(Of String, Array)
        Dim index As Integer() = New Integer(steps - 1) {}

        For s As Integer = 0 To steps - 1
            index(s) = s
        Next
        cols.Add("step", index)

        For c As Integer = 0 To k - 1
            For j As Integer = 0 To p - 1
                Dim name As String = If(k = 1, $"V{j + 1}", $"V{j + 1}.{c + 1}")
                Dim col As Double() = New Double(steps - 1) {}

                For s As Integer = 0 To steps - 1
                    col(s) = fit.beta(s)(j)(c)
                Next

                cols.Add(name, col)
            Next
        Next

        Return New Rdataframe With {.columns = cols}
    End Function

    ''' <summary>One row per lambda step: the cross validation curve.</summary>
    Private Function cvFrame(cv As GlmnetLib.GlmnetCVFit) As Rdataframe
        Dim steps As Integer = cv.lambda.Length
        Dim cols As New Dictionary(Of String, Array)
        Dim index As Integer() = New Integer(steps - 1) {}

        For s As Integer = 0 To steps - 1
            index(s) = s
        Next

        cols.Add("step", index)
        cols.Add("lambda", cv.lambda)
        cols.Add("cvm", cv.cvm)
        cols.Add("cvsd", cv.cvsd)
        cols.Add("cvup", cv.cvup)
        cols.Add("cvlo", cv.cvlo)
        cols.Add("nzero", cv.nzero)
        cols.Add("lambda.min", Enumerable.Repeat(cv.lambdaMin, steps).ToArray())
        cols.Add("lambda.1se", Enumerable.Repeat(cv.lambda1se, steps).ToArray())

        ' foldid is per sample (length n), not per lambda step (length steps),
        ' so it is deliberately kept out of the tabular view; it remains
        ' available as the `foldid` slot of the cv.glmnet list.
        Return New Rdataframe With {.columns = cols}
    End Function

    ''' <summary>Assemble the R-style list for a glmnet fit.</summary>
    Private Function fitToList(fit As GlmnetLib.GlmnetFit, env As Environment) As list
        Dim result As New list With {.slots = New Dictionary(Of String, Object)}

        result.add("a0", fit.a0)
        result.add("beta", betaFrame(fit))
        result.add("lambda", fit.lambda)
        result.add("df", fit.df)
        result.add("dev.ratio", fit.devRatio)
        result.add("deviance", fit.deviance)
        result.add("nzero", fit.nzero)
        result.add("converged", fit.converged)
        result.add("iterations", fit.iterations)
        result.add("nulldev", fit.nulldev)
        result.add("nobs", fit.nobs)
        result.add("nvars", fit.nvars)
        result.add("nresponses", fit.nresponses)
        result.add("nlambda", fit.Count)
        result.add("jerr", CInt(fit.jerr))
        result.add("family", familyName(fit.Family))
        result.add("feature.names", fit.featureNames)
        result.add("response.names", fit.responseNames)
        result.add("class.labels", fit.classLabels)
        result.add("path", pathFrame(fit))
        result.add("class", "glmnet")
        result.add(FitSlot, fit)

        Return result
    End Function

    ''' <summary>Assemble the R-style list for a cv.glmnet fit.</summary>
    Private Function cvToList(cv As GlmnetLib.GlmnetCVFit, env As Environment) As list
        Dim result As New list With {.slots = New Dictionary(Of String, Object)}

        result.add("lambda", cv.lambda)
        result.add("cvm", cv.cvm)
        result.add("cvsd", cv.cvsd)
        result.add("cvup", cv.cvup)
        result.add("cvlo", cv.cvlo)
        result.add("nzero", cv.nzero)
        result.add("lambda.min", cv.lambdaMin)
        result.add("lambda.1se", cv.lambda1se)
        result.add("index.min", cv.indexMin)
        result.add("index.1se", cv.index1se)
        result.add("name", cv.name)
        result.add("measure", cv.measure.ToString())
        result.add("foldid", If(cv.foldid Is Nothing, Nothing, cv.foldid.Select(Function(i) CDbl(i + 1)).ToArray()))
        result.add("glmnet.fit", fitToList(cv.glmnetFit, env))
        result.add("cv.curve", cvFrame(cv))
        result.add("class", "cv.glmnet")
        result.add(FitSlot, cv.glmnetFit)
        result.add(CvSlot, cv)

        Return result
    End Function

    ''' <summary>Recover the native fit from a R# list or a raw CLR object.</summary>
    Private Function unwrapFit(obj As Object) As GlmnetLib.GlmnetFit
        If obj Is Nothing Then Return Nothing
        If TypeOf obj Is GlmnetLib.GlmnetFit Then Return DirectCast(obj, GlmnetLib.GlmnetFit)
        If TypeOf obj Is GlmnetLib.GlmnetCVFit Then Return DirectCast(obj, GlmnetLib.GlmnetCVFit).glmnetFit

        If TypeOf obj Is list Then
            Dim slots As Dictionary(Of String, Object) = DirectCast(obj, list).slots
            Dim raw As Object = Nothing

            If slots.TryGetValue(FitSlot, raw) Then
                Return TryCast(raw, GlmnetLib.GlmnetFit)
            End If
        End If

        Return Nothing
    End Function

    ''' <summary>Recover the native CV fit from a R# list or a raw CLR object.</summary>
    Private Function unwrapCv(obj As Object) As GlmnetLib.GlmnetCVFit
        If obj Is Nothing Then Return Nothing
        If TypeOf obj Is GlmnetLib.GlmnetCVFit Then Return DirectCast(obj, GlmnetLib.GlmnetCVFit)

        If TypeOf obj Is list Then
            Dim slots As Dictionary(Of String, Object) = DirectCast(obj, list).slots
            Dim raw As Object = Nothing

            If slots.TryGetValue(CvSlot, raw) Then
                Return TryCast(raw, GlmnetLib.GlmnetCVFit)
            End If
        End If

        Return Nothing
    End Function

    ' ------------------------------------------------------------------
    '  glmnet() and cv.glmnet()
    ' ------------------------------------------------------------------

    ''' <summary>
    ''' ### Fit a generalized linear model via penalized maximum likelihood
    '''
    ''' Fits the entire lasso or elastic-net regularization path for the six
    ''' families supported by GNU R ``glmnet``: ``gaussian``, ``binomial``,
    ''' ``poisson``, ``multinomial``, ``cox`` and ``mgaussian``.
    ''' </summary>
    ''' <param name="x">input matrix or dataframe, each row is an observation.</param>
    ''' <param name="y">response vector (or a ``n x 2`` matrix for cox / binomial).</param>
    ''' <param name="family">model family name, default ``gaussian``.</param>
    ''' <param name="alpha">elastic-net mixing parameter, 1 = lasso, 0 = ridge.</param>
    ''' <param name="nlambda">number of lambda values in the auto-generated grid.</param>
    ''' <param name="lambda">an optional user supplied, strictly decreasing lambda grid.</param>
    ''' <param name="lambda_min_ratio">smallest lambda as a fraction of lambda.max.</param>
    ''' <param name="env"></param>
    ''' <returns>a R-style list with ``lambda``, ``df``, ``dev.ratio``, ``beta``, ...</returns>
    <ExportAPI("glmnet")>
    Public Function glmnet_fit(<RRawVectorArgument> x As Object,
                               <RRawVectorArgument> y As Object,
                               Optional family As String = "gaussian",
                               Optional alpha As Double = 1.0R,
                               Optional nlambda As Integer = 100,
                               <RRawVectorArgument> Optional lambda As Object = Nothing,
                               Optional lambda_min_ratio As Double = -1.0R,
                               <RRawVectorArgument> Optional weights As Object = Nothing,
                               <RRawVectorArgument> Optional offset As Object = Nothing,
                               Optional standardize As Boolean = True,
                               Optional intercept As Boolean = True,
                               <RRawVectorArgument> Optional penalty_factor As Object = Nothing,
                               <RRawVectorArgument> Optional lower_limits As Object = Nothing,
                               <RRawVectorArgument> Optional upper_limits As Object = Nothing,
                               <RRawVectorArgument> Optional exclude As Object = Nothing,
                               Optional dfmax As Integer = -1,
                               Optional pmax As Integer = -1,
                               Optional thresh As Double = 0.0000001R,
                               Optional maxit As Integer = 100000,
                               Optional type_gaussian As String = "covariance",
                               Optional type_multinomial As String = "ungrouped",
                               Optional type_logistic As String = "Newton",
                               Optional standardize_response As Boolean = False,
                               Optional relax As Boolean = False,
                               <RRawVectorArgument> Optional feature_names As Object = Nothing,
                               <RRawVectorArgument> Optional response_names As Object = Nothing,
                               Optional env As Environment = Nothing) As Object

        Dim err As Message = Nothing
        Dim xMat As Double()() = toMatrix(x, env, err)
        If xMat Is Nothing Then Return err
        Dim yMat As Double()() = toResponse(y, xMat.Length, env, err)
        If yMat Is Nothing Then Return err

        Dim opts As GlmnetLib.GlmnetOptions = buildOptions(env, err,
                                                     alpha, nlambda, lambda_min_ratio, lambda,
                                                     weights, offset, standardize, intercept,
                                                     penalty_factor, lower_limits, upper_limits, exclude,
                                                     dfmax, pmax, thresh, maxit,
                                                     type_gaussian, type_multinomial, type_logistic,
                                                     standardize_response, relax,
                                                     feature_names, response_names)
        If opts Is Nothing Then Return err

        Try
            Dim fit As GlmnetLib.GlmnetFit = GlmnetLib.GlmnetSolver.Fit(xMat, yMat, family, opts)
            Return fitToList(fit, env)
        Catch ex As Exception
            Return RInternal.debug.stop($"glmnet(x, y, family='{family}') failed: {ex.Message}", env)
        End Try
    End Function

    ''' <summary>
    ''' ### Cross-validation for glmnet
    '''
    ''' Runs K-fold cross validation on the full lambda path and returns the
    ''' mean cross-validated error together with ``lambda.min`` and ``lambda.1se``.
    ''' </summary>
    ''' <param name="foldid">optional 1-based fold assignment (GNU R convention).</param>
    ''' <param name="type_measure">mse / mae / deviance / class / auc / cindex.</param>
    ''' <param name="env"></param>
    ''' <returns>a R-style list with ``cvm``, ``cvsd``, ``lambda.min``, ``lambda.1se``, ...</returns>
    <ExportAPI("cv.glmnet")>
    Public Function cv_glmnet(<RRawVectorArgument> x As Object,
                              <RRawVectorArgument> y As Object,
                              Optional family As String = "gaussian",
                              Optional alpha As Double = 1.0R,
                              Optional nlambda As Integer = 100,
                              <RRawVectorArgument> Optional lambda As Object = Nothing,
                              Optional lambda_min_ratio As Double = -1.0R,
                              <RRawVectorArgument> Optional weights As Object = Nothing,
                              <RRawVectorArgument> Optional offset As Object = Nothing,
                              Optional standardize As Boolean = True,
                              Optional intercept As Boolean = True,
                              <RRawVectorArgument> Optional penalty_factor As Object = Nothing,
                              <RRawVectorArgument> Optional lower_limits As Object = Nothing,
                              <RRawVectorArgument> Optional upper_limits As Object = Nothing,
                              <RRawVectorArgument> Optional exclude As Object = Nothing,
                              Optional dfmax As Integer = -1,
                              Optional pmax As Integer = -1,
                              Optional thresh As Double = 0.0000001R,
                              Optional maxit As Integer = 100000,
                              Optional type_gaussian As String = "covariance",
                              Optional type_multinomial As String = "ungrouped",
                              Optional type_logistic As String = "Newton",
                              Optional standardize_response As Boolean = False,
                              Optional relax As Boolean = False,
                              <RRawVectorArgument> Optional feature_names As Object = Nothing,
                              <RRawVectorArgument> Optional response_names As Object = Nothing,
                              Optional nfolds As Integer = 10,
                              <RRawVectorArgument> Optional foldid As Object = Nothing,
                              Optional type_measure As String = Nothing,
                              Optional grouped As Boolean = True,
                              Optional keep As Boolean = False,
                              Optional seed As Integer = 1,
                              Optional env As Environment = Nothing) As Object

        Dim err As Message = Nothing
        Dim xMat As Double()() = toMatrix(x, env, err)
        If xMat Is Nothing Then Return err
        Dim yMat As Double()() = toResponse(y, xMat.Length, env, err)
        If yMat Is Nothing Then Return err

        Dim opts As GlmnetLib.GlmnetOptions = buildOptions(env, err,
                                                     alpha, nlambda, lambda_min_ratio, lambda,
                                                     weights, offset, standardize, intercept,
                                                     penalty_factor, lower_limits, upper_limits, exclude,
                                                     dfmax, pmax, thresh, maxit,
                                                     type_gaussian, type_multinomial, type_logistic,
                                                     standardize_response, relax,
                                                     feature_names, response_names)
        If opts Is Nothing Then Return err

        opts.NFolds = nfolds
        opts.Grouped = grouped
        opts.Keep = keep
        opts.Seed = seed

        If Not String.IsNullOrWhiteSpace(type_measure) Then
            Dim measure As GlmnetLib.CVMeasure? = parseMeasure(type_measure)
            If Not measure.HasValue Then
                Return RInternal.debug.stop($"unsupported type.measure: '{type_measure}'!", env)
            End If
            opts.TypeMeasure = measure.Value
        End If

        If foldid IsNot Nothing Then
            opts.FoldId = toZeroBased(foldid, env, err)
            If opts.FoldId Is Nothing Then Return err
        End If

        Try
            Dim cv As GlmnetLib.GlmnetCVFit = GlmnetLib.GlmnetSolver.CrossValidate(xMat, yMat, family, opts)
            Return cvToList(cv, env)
        Catch ex As Exception
            Return RInternal.debug.stop($"cv.glmnet(x, y, family='{family}') failed: {ex.Message}", env)
        End Try
    End Function

    ' ------------------------------------------------------------------
    '  Package initialization
    ' ------------------------------------------------------------------

    ''' <summary>
    ''' Register the native CLR result types with the R# generics. Called by the
    ''' runtime when ``imports "glmnet" from "Rlapack"`` is executed.
    ''' </summary>
    <RInitialize>
    Sub Main()
        Call RInternal.Object.Converts.makeDataframe.addHandler(GetType(GlmnetLib.GlmnetFit), AddressOf nativeFitToFrame)
        Call RInternal.Object.Converts.makeDataframe.addHandler(GetType(GlmnetLib.GlmnetCVFit), AddressOf nativeCvToFrame)
        Call RInternal.generic.add("summary", GetType(GlmnetLib.GlmnetFit), AddressOf nativeFitSummary)
        Call RInternal.generic.add("summary", GetType(GlmnetLib.GlmnetCVFit), AddressOf nativeCvSummary)
        Call RInternal.ConsolePrinter.AttachConsoleFormatter(Of GlmnetLib.GlmnetFit)(Function(o) DirectCast(o, GlmnetLib.GlmnetFit).ToString())
        Call RInternal.ConsolePrinter.AttachConsoleFormatter(Of GlmnetLib.GlmnetCVFit)(Function(o) DirectCast(o, GlmnetLib.GlmnetCVFit).ToString())
    End Sub

    Private Function nativeFitToFrame(x As Object, args As list, env As Environment) As Rdataframe
        Dim fit As GlmnetLib.GlmnetFit = TryCast(x, GlmnetLib.GlmnetFit)
        If fit Is Nothing Then Return Nothing
        Return pathFrame(fit)
    End Function

    Private Function nativeCvToFrame(x As Object, args As list, env As Environment) As Rdataframe
        Dim cv As GlmnetLib.GlmnetCVFit = TryCast(x, GlmnetLib.GlmnetCVFit)
        If cv Is Nothing Then Return Nothing
        Return cvFrame(cv)
    End Function

    Private Function nativeFitSummary(x As Object, args As list, env As Environment) As Object
        Return summary_glmnet(x, env)
    End Function

    Private Function nativeCvSummary(x As Object, args As list, env As Environment) As Object
        Dim cv As GlmnetLib.GlmnetCVFit = unwrapCv(x)
        If cv Is Nothing Then Return RInternal.debug.stop("not a cv.glmnet object!", env)
        Return cvFrame(cv)
    End Function

    ' ------------------------------------------------------------------
    '  predict / coef / deviance helpers
    ' ------------------------------------------------------------------

    ''' <summary>Resolve ``s`` to a numeric lambda or NULL (last path step).</summary>
    Private Function resolveLambda(s As Object, cv As GlmnetLib.GlmnetCVFit,
                                   env As Environment, ByRef err As Message) As Double?
        If s Is Nothing Then Return Nothing

        If TypeOf s Is String Then
            Dim text As String = DirectCast(s, String).Trim()

            Select Case text.ToLowerInvariant().Replace("_", ".")
                Case "lambda.min", "lambda.min.lambda"
                    If cv Is Nothing Then
                        err = RInternal.debug.stop("s='lambda.min' requires a cv.glmnet object!", env)
                        Return Nothing
                    End If
                    Return cv.lambdaMin
                Case "lambda.1se", "lambda1se"
                    If cv Is Nothing Then
                        err = RInternal.debug.stop("s='lambda.1se' requires a cv.glmnet object!", env)
                        Return Nothing
                    End If
                    Return cv.lambda1se
                Case Else
                    Dim parsed As Double
                    If Double.TryParse(text,
                                       Globalization.NumberStyles.Float,
                                       Globalization.CultureInfo.InvariantCulture,
                                       parsed) AndAlso parsed > 0 Then
                        Return parsed
                    End If

                    err = RInternal.debug.stop($"invalid s value: '{text}' (expect a positive lambda, 'lambda.min' or 'lambda.1se')!", env)
                    Return Nothing
            End Select
        End If

        Dim v As Double() = toVector(s, env, err)
        If v Is Nothing Then Return Nothing
        If v(0) <= 0 Then
            err = RInternal.debug.stop($"s must be a positive lambda, but {v(0)} was given!", env)
            Return Nothing
        End If

        Return v(0)
    End Function

    ''' <summary>Parse the ``type`` argument of predict.glmnet().</summary>
    Private Function parsePredictionType(type As String, env As Environment, ByRef err As Message) As GlmnetLib.PredictionType
        Select Case If(type, "link").Trim().ToLowerInvariant()
            Case "link" : Return GlmnetLib.PredictionType.Link
            Case "response" : Return GlmnetLib.PredictionType.Response
            Case "coefficients", "coef" : Return GlmnetLib.PredictionType.Coefficients
            Case "nonzero" : Return GlmnetLib.PredictionType.NonZero
            Case "class" : Return GlmnetLib.PredictionType.[Class]
            Case Else
                err = RInternal.debug.stop($"unsupported prediction type: '{type}' (expect link/response/coefficients/nonzero/class)!", env)
                Return GlmnetLib.PredictionType.Link
        End Select
    End Function

    ''' <summary>Turn a ``n x k`` prediction matrix into a R# dataframe.</summary>
    Private Function valuesFrame(values As Double()(), prefix As String) As Rdataframe
        Dim n As Integer = values.Length
        Dim k As Integer = If(n = 0, 0, values(0).Length)
        Dim cols As New Dictionary(Of String, Array)

        For c As Integer = 0 To k - 1
            Dim name As String = If(k = 1, prefix, $"{prefix}.{c + 1}")
            Dim col As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                col(i) = values(i)(c)
            Next
            cols.Add(name, col)
        Next

        Return New Rdataframe With {.columns = cols}
    End Function

    ''' <summary>Turn a ``(p+1) x k`` coefficient matrix into a R# dataframe.</summary>
    Private Function coefFrame(coefficients As Double()(), fit As GlmnetLib.GlmnetFit) As Rdataframe
        Dim rows As Integer = coefficients.Length
        Dim k As Integer = coefficients(0).Length
        Dim cols As New Dictionary(Of String, Array)
        Dim names As String() = New String(rows - 1) {}

        names(0) = "(Intercept)"
        For j As Integer = 1 To rows - 1
            names(j) = If(fit.featureNames IsNot Nothing AndAlso j - 1 < fit.featureNames.Length,
                          fit.featureNames(j - 1), $"V{j}")
        Next
        cols.Add("name", names)

        For c As Integer = 0 To k - 1
            Dim name As String = If(k = 1, "coef", $"coef.{c + 1}")
            Dim col As Double() = New Double(rows - 1) {}

            For r As Integer = 0 To rows - 1
                col(r) = coefficients(r)(c)
            Next
            cols.Add(name, col)
        Next

        Return New Rdataframe With {.columns = cols}
    End Function

    ''' <summary>Assemble the prediction result into a R-style list.</summary>
    Private Function bundleToList(bundle As GlmnetLib.PredictionBundle, fit As GlmnetLib.GlmnetFit) As list
        Dim result As New list With {.slots = New Dictionary(Of String, Object)}

        result.add("type", bundle.Type.ToString().ToLowerInvariant())
        result.add("lambda", bundle.Lambda)

        If bundle.Values IsNot Nothing AndAlso bundle.Values.Length > 0 Then
            result.add("values", valuesFrame(bundle.Values, bundle.Type.ToString().ToLowerInvariant()))
        Else
            result.add("values", Nothing)
        End If

        If bundle.Coefficients IsNot Nothing Then
            result.add("coefficients", coefFrame(bundle.Coefficients, fit))
        Else
            result.add("coefficients", Nothing)
        End If

        If bundle.Indices IsNot Nothing Then
            result.add("indices", bundle.Indices.Select(Function(i) i + 1).ToArray())
        Else
            result.add("indices", Nothing)
        End If

        If bundle.ClassIndex IsNot Nothing Then
            result.add("class.index", bundle.ClassIndex.Select(Function(i) i + 1).ToArray())

            If bundle.ClassLabels IsNot Nothing Then
                result.add("prediction", bundle.ClassIndex _
                    .Select(Function(i) If(i >= 0 AndAlso i < bundle.ClassLabels.Length,
                                           bundle.ClassLabels(i), CStr(i))) _
                    .ToArray())
            End If
        Else
            result.add("class.index", Nothing)
            result.add("prediction", Nothing)
        End If

        result.add("class", "predict.glmnet")
        Return result
    End Function

    ' ------------------------------------------------------------------
    '  predict.glmnet / coef.glmnet / deviance.glmnet
    ' ------------------------------------------------------------------

    ''' <summary>
    ''' ### Extract predictions from a fitted glmnet object
    '''
    ''' ``type`` accepts ``link``, ``response``, ``coefficients``, ``nonzero``
    ''' and ``class``; ``s`` accepts a numeric lambda, ``"lambda.min"`` or
    ''' ``"lambda.1se"`` when a cv.glmnet object is supplied.
    ''' </summary>
    <ExportAPI("predict.glmnet")>
    Public Function predict_glmnet(<RRawVectorArgument> fit As Object,
                                   <RRawVectorArgument> Optional newx As Object = Nothing,
                                   Optional type As String = "link",
                                   Optional s As Object = Nothing,
                                   Optional exact As Boolean = False,
                                   <RRawVectorArgument> Optional newoffset As Object = Nothing,
                                   Optional env As Environment = Nothing) As Object

        Dim nativeFit As GlmnetLib.GlmnetFit = unwrapFit(fit)
        If nativeFit Is Nothing Then
            Return Message.InCompatibleType(GetType(GlmnetLib.GlmnetFit), If(fit Is Nothing, GetType(Object), fit.GetType), env,
                                            message:="the 'fit' argument should be a glmnet or cv.glmnet result!")
        End If

        Dim err As Message = Nothing
        Dim nativeCv As GlmnetLib.GlmnetCVFit = unwrapCv(fit)
        Dim lambda As Double? = resolveLambda(s, nativeCv, env, err)
        If err IsNot Nothing Then Return err

        Dim ptype As GlmnetLib.PredictionType = parsePredictionType(type, env, err)
        If err IsNot Nothing Then Return err

        Dim xMat As Double()() = Nothing
        If newx Is Nothing Then
            ' coefficients/nonzero do not touch newx, but the kernel still
            ' requires a non-null placeholder.
            xMat = New Double()() {New Double(nativeFit.nvars - 1) {}}
        Else
            xMat = toMatrix(newx, env, err)
            If xMat Is Nothing Then Return err
        End If

        Dim offMat As Double()() = Nothing
        If newoffset IsNot Nothing Then
            offMat = toMatrix(newoffset, env, err)
            If offMat Is Nothing Then Return err
        End If

        Try
            Dim bundle As GlmnetLib.PredictionBundle = GlmnetLib.GlmnetPredict.Predict(nativeFit, xMat, ptype, lambda, exact, offMat)
            Return bundleToList(bundle, nativeFit)
        Catch ex As Exception
            Return RInternal.debug.stop($"predict.glmnet(type='{type}') failed: {ex.Message}", env)
        End Try
    End Function

    ''' <summary>R-style generic alias of ``predict.glmnet``.</summary>
    <ExportAPI("predict")>
    Public Function predict_alias(<RRawVectorArgument> fit As Object,
                                  <RRawVectorArgument> Optional newx As Object = Nothing,
                                  Optional type As String = "link",
                                  Optional s As Object = Nothing,
                                  Optional exact As Boolean = False,
                                  <RRawVectorArgument> Optional newoffset As Object = Nothing,
                                  Optional env As Environment = Nothing) As Object
        Return predict_glmnet(fit, newx, type, s, exact, newoffset, env)
    End Function

    ''' <summary>
    ''' ### Extract the coefficient matrix from a fitted glmnet object
    ''' </summary>
    <ExportAPI("coef.glmnet")>
    Public Function coef_glmnet(<RRawVectorArgument> fit As Object,
                                Optional s As Object = Nothing,
                                Optional exact As Boolean = False,
                                Optional env As Environment = Nothing) As Object

        Dim nativeFit As GlmnetLib.GlmnetFit = unwrapFit(fit)
        If nativeFit Is Nothing Then
            Return Message.InCompatibleType(GetType(GlmnetLib.GlmnetFit), If(fit Is Nothing, GetType(Object), fit.GetType), env,
                                            message:="the 'fit' argument should be a glmnet or cv.glmnet result!")
        End If

        Dim err As Message = Nothing
        Dim lambda As Double? = resolveLambda(s, unwrapCv(fit), env, err)
        If err IsNot Nothing Then Return err

        Try
            Dim coefficients As Double()() = GlmnetLib.GlmnetPredict.Coef(nativeFit, lambda, exact)
            Return coefFrame(coefficients, nativeFit)
        Catch ex As Exception
            Return RInternal.debug.stop($"coef.glmnet failed: {ex.Message}", env)
        End Try
    End Function

    ''' <summary>R-style generic alias of ``coef.glmnet``.</summary>
    <ExportAPI("coef")>
    Public Function coef_alias(<RRawVectorArgument> fit As Object,
                               Optional s As Object = Nothing,
                               Optional exact As Boolean = False,
                               Optional env As Environment = Nothing) As Object
        Return coef_glmnet(fit, s, exact, env)
    End Function

    ''' <summary>
    ''' ### Compute the model deviance at a given lambda on new data
    ''' </summary>
    <ExportAPI("deviance.glmnet")>
    Public Function deviance_glmnet(<RRawVectorArgument> fit As Object,
                                    <RRawVectorArgument> newx As Object,
                                    <RRawVectorArgument> newy As Object,
                                    Optional s As Object = Nothing,
                                    Optional env As Environment = Nothing) As Object

        Dim nativeFit As GlmnetLib.GlmnetFit = unwrapFit(fit)
        If nativeFit Is Nothing Then
            Return Message.InCompatibleType(GetType(GlmnetLib.GlmnetFit), If(fit Is Nothing, GetType(Object), fit.GetType), env,
                                            message:="the 'fit' argument should be a glmnet or cv.glmnet result!")
        End If

        Dim err As Message = Nothing
        Dim xMat As Double()() = toMatrix(newx, env, err)
        If xMat Is Nothing Then Return err
        Dim yMat As Double()() = toResponse(newy, xMat.Length, env, err)
        If yMat Is Nothing Then Return err

        Dim lambda As Double? = resolveLambda(s, unwrapCv(fit), env, err)
        If err IsNot Nothing Then Return err

        Try
            Return GlmnetLib.GlmnetPredict.Deviance(nativeFit, xMat, yMat, lambda)
        Catch ex As Exception
            Return RInternal.debug.stop($"deviance.glmnet failed: {ex.Message}", env)
        End Try
    End Function

    ''' <summary>R-style generic alias of ``deviance.glmnet``.</summary>
    <ExportAPI("deviance")>
    Public Function deviance_alias(<RRawVectorArgument> fit As Object,
                                   <RRawVectorArgument> newx As Object,
                                   <RRawVectorArgument> newy As Object,
                                   Optional s As Object = Nothing,
                                   Optional env As Environment = Nothing) As Object
        Return deviance_glmnet(fit, newx, newy, s, env)
    End Function

    ' ------------------------------------------------------------------
    '  print / summary / as.data.frame and result field helpers
    ' ------------------------------------------------------------------

    ''' <summary>Compact R-style print of a glmnet or cv.glmnet result.</summary>
    <ExportAPI("print.glmnet")>
    Public Function print_glmnet(<RRawVectorArgument> fit As Object,
                                 Optional env As Environment = Nothing) As Object
        Dim cv As GlmnetLib.GlmnetCVFit = unwrapCv(fit)
        If cv IsNot Nothing Then Return cv.ToString()

        Dim native As GlmnetLib.GlmnetFit = unwrapFit(fit)
        If native Is Nothing Then
            Return Message.InCompatibleType(GetType(GlmnetLib.GlmnetFit), If(fit Is Nothing, GetType(Object), fit.GetType), env)
        End If

        Return native.ToString()
    End Function

    ''' <summary>Summary of a fitted path as a named list.</summary>
    <ExportAPI("summary.glmnet")>
    Public Function summary_glmnet(<RRawVectorArgument> fit As Object,
                                   Optional env As Environment = Nothing) As Object
        Dim cv As GlmnetLib.GlmnetCVFit = unwrapCv(fit)
        If cv IsNot Nothing Then
            Dim cvInfo As New list With {.slots = New Dictionary(Of String, Object)}
            cvInfo.add("class", "cv.glmnet")
            cvInfo.add("name", cv.name)
            cvInfo.add("nlambda", cv.lambda.Length)
            cvInfo.add("lambda.min", cv.lambdaMin)
            cvInfo.add("lambda.1se", cv.lambda1se)
            cvInfo.add("cvm.min", cv.cvm(cv.indexMin))
            cvInfo.add("cv.curve", cvFrame(cv))
            Return cvInfo
        End If

        Dim native As GlmnetLib.GlmnetFit = unwrapFit(fit)
        If native Is Nothing Then
            Return Message.InCompatibleType(GetType(GlmnetLib.GlmnetFit), If(fit Is Nothing, GetType(Object), fit.GetType), env)
        End If

        Dim result As New list With {.slots = New Dictionary(Of String, Object)}
        result.add("class", "glmnet")
        result.add("family", familyName(native.Family))
        result.add("nobs", native.nobs)
        result.add("nvars", native.nvars)
        result.add("nlambda", native.Count)
        result.add("lambda.max", native.lambda(0))
        result.add("lambda.min", native.lambda(native.Count - 1))
        result.add("nulldev", native.nulldev)
        result.add("dev.ratio.max", native.devRatio.Max())
        result.add("df.max", native.df.Max())
        result.add("jerr", CInt(native.jerr))
        result.add("path", pathFrame(native))
        Return result
    End Function

    ''' <summary>
    ''' Convert a glmnet/cv.glmnet result into a dataframe with one row per
    ''' lambda step. This is the primary export used by the offline comparison
    ''' test scripts.
    ''' </summary>
    <ExportAPI("as.data.frame.glmnet")>
    Public Function as_data_frame_glmnet(<RRawVectorArgument> fit As Object,
                                         Optional env As Environment = Nothing) As Object
        Dim cv As GlmnetLib.GlmnetCVFit = unwrapCv(fit)
        If cv IsNot Nothing Then Return cvFrame(cv)

        Dim native As GlmnetLib.GlmnetFit = unwrapFit(fit)
        If native Is Nothing Then
            Return Message.InCompatibleType(GetType(GlmnetLib.GlmnetFit), If(fit Is Nothing, GetType(Object), fit.GetType), env)
        End If

        Return pathFrame(native)
    End Function

    ''' <summary>Return the flattened coefficient path as a dataframe.</summary>
    <ExportAPI("glmnet.beta")>
    Public Function glmnet_beta(<RRawVectorArgument> fit As Object,
                                Optional env As Environment = Nothing) As Object
        Dim native As GlmnetLib.GlmnetFit = unwrapFit(fit)
        If native Is Nothing Then
            Return Message.InCompatibleType(GetType(GlmnetLib.GlmnetFit), If(fit Is Nothing, GetType(Object), fit.GetType), env)
        End If
        Return betaFrame(native)
    End Function

    ''' <summary>Return the cross validation curve as a dataframe.</summary>
    <ExportAPI("glmnet.cv")>
    Public Function glmnet_cv(<RRawVectorArgument> fit As Object,
                              Optional env As Environment = Nothing) As Object
        Dim cv As GlmnetLib.GlmnetCVFit = unwrapCv(fit)
        If cv Is Nothing Then
            Return Message.InCompatibleType(GetType(GlmnetLib.GlmnetCVFit), If(fit Is Nothing, GetType(Object), fit.GetType), env)
        End If
        Return cvFrame(cv)
    End Function

End Module
