Public Class KendoGridPost

    Public Sub New(ByVal queryString As Dictionary(Of String, String), ByVal postVariables As Dictionary(Of String, String))
        Me.Page = GetDictValue(Of Integer)("page", postVariables, 1)
        Me.PageSize = GetDictValue(Of Integer)("pageSize", postVariables, 5)
        Me.Skip = GetDictValue(Of Integer)("skip", postVariables, 0)
        Me.Take = GetDictValue(Of Integer)("take", postVariables, 5)

        Me.SortOrd = GetDictValue(Of String)("sort[0][dir]", postVariables, String.Empty)
        Me.SortOn = GetDictValue(Of String)("sort[0][field]", postVariables, String.Empty)

        Dim filterLogic As String = GetDictValue(Of String)("filter[logic]", queryString, String.Empty)

        If Not String.IsNullOrWhiteSpace(filterLogic) Then
            Me.FilterLogic = filterLogic

            For i As Integer = 0 To queryString.Count

                If Not String.IsNullOrWhiteSpace(GetDictValue(Of String)("filter[filters][" & i & "][field]", queryString, String.Empty)) Then
                    Dim filter As New KendoGridFilter()
                    filter.Field = queryString("filter[filters][" & i & "][field]")
                    filter.[Operator] = queryString("filter[filters][" & i & "][operator]")
                    filter.Value = queryString("filter[filters][" & i & "][value]")

                    Me.Filters.Add(filter)
                End If
            Next
        End If
    End Sub

    Private Function GetDictValue(Of T)(ByVal keyName As String, ByVal dict As Dictionary(Of String, String), ByVal defaultVal As T) As T
        Dim tempVal As String = Nothing
        Dim retVal As T = defaultVal
        If dict.TryGetValue(keyName, tempVal) Then
            retVal = CType(Convert.ChangeType(tempVal, GetType(T)), T)
        End If
        Return retVal
    End Function

    Public Property Page As Integer
    Public Property PageSize As Integer
    Public Property Skip As Integer
    Public Property Take As Integer
    Public Property SortOrd As String
    Public Property SortOn As String
    Public Property Filters As New List(Of KendoGridFilter)
    Public Property FilterLogic As String
End Class

Public Class KendoGridFilter
    Public Property Field As String
    Public Property [Operator] As String
    Public Property Value As String

    Public Shared Function BuildWhereClause(Of T)(index As Integer, logic As String, filter As KendoGridFilter, parameters As List(Of Object)) As String
        Dim entityType = (GetType(T))
        Dim [property] = entityType.GetProperty(filter.Field)

        Select Case filter.[Operator].ToLower()
            Case "eq", "neq", "gte", "gt", "lte", "lt"
                If GetType(DateTime).IsAssignableFrom([property].PropertyType) Then
                    Dim dateFilterValue As Date

                    DateTime.TryParse(filter.Value, dateFilterValue)
                    dateFilterValue = DateTime.SpecifyKind(dateFilterValue.[Date], DateTimeKind.Local)
                    parameters.Add(dateFilterValue)
                    Return String.Format("EntityFunctions.TruncateTime({0}){1}@{2}", filter.Field, ToLinqOperator(filter.[Operator]), index)
                End If
                If GetType(Integer).IsAssignableFrom([property].PropertyType) Then
                    parameters.Add(Integer.Parse(filter.Value))
                    Return String.Format("{0}{1}@{2}", filter.Field, ToLinqOperator(filter.[Operator]), index)
                End If
                If GetType(String).IsAssignableFrom([property].PropertyType) Then
                    parameters.Add(filter.Value)
                    Return String.Format("{0}.ToLower(){1}@{2}.ToLower()", filter.Field, ToLinqOperator(filter.[Operator]), index)
                End If
                parameters.Add(filter.Value)
                Return String.Format("{0}{1}@{2}", filter.Field, ToLinqOperator(filter.[Operator]), index)
            Case "startswith"
                parameters.Add(filter.Value)
                Return String.Format("{0}.ToLower().StartsWith(" & "@{1}.ToLower())", filter.Field, index)
            Case "endswith"
                parameters.Add(filter.Value)
                Return String.Format("{0}.ToLower().EndsWith(" & "@{1}.ToLower())", filter.Field, index)
            Case "contains"
                parameters.Add(filter.Value)
                Return String.Format("{0}.ToLower().Contains(" & "@{1}.ToLower())", filter.Field, index)
            Case "doesnotcontain"
                parameters.Add(filter.Value)
                Return String.Format("Not {0}.ToLower().Contains(" & "@{1}.ToLower())", filter.Field, index)
            Case Else
                Throw New ArgumentException("This operator is not yet supported for this Grid", filter.[Operator])
        End Select
    End Function

    Public Shared Function ToLinqOperator([operator] As String) As String
        Select Case [operator].ToLower()
            Case "eq"
                Return " == "
            Case "neq"
                Return " != "
            Case "gte"
                Return " >= "
            Case "gt"
                Return " > "
            Case "lte"
                Return " <= "
            Case "lt"
                Return " < "
            Case "or"
                Return " || "
            Case "and"
                Return " && "
            Case Else
                Return Nothing
        End Select
    End Function
    
End Class

