Option Strict On
Option Explicit On

Public Class GridFilters

    Public Shared Function Filter(Of T)(ByVal results As IQueryable(Of T), ByVal queryString As Specialized.NameValueCollection, ByVal postVariables As Specialized.NameValueCollection) As GridResults(Of T)
        Dim query As Dictionary(Of String, String) = queryString.Cast(Of String).ToDictionary(Function(p) p, Function(p) queryString(p))
        Dim postVar As Dictionary(Of String, String) = postVariables.Cast(Of String).ToDictionary(Function(p) p, Function(p) postVariables(p))

        Return Filter(results, query, postVar)
    End Function
    Public Shared Function Filter(Of T)(ByVal results As IQueryable(Of T), ByVal queryString As Dictionary(Of String, String), ByVal postVariables As Dictionary(Of String, String)) As GridResults(Of T)
        Dim requestParams As New KendoGridPost(queryString, postVariables)

        Dim parameters As New List(Of Object)
        Dim filterString As String = String.Empty
        For i As Integer = 0 To requestParams.Filters.Count - 1
            If (i = 0) Then
                filterString &= String.Format(" {0}",
                    KendoGridFilter.BuildWhereClause(Of T)(i, requestParams.FilterLogic, requestParams.Filters(i), parameters))
            Else
                filterString &= String.Format(" {0} {1}",
                    KendoGridFilter.ToLinqOperator(requestParams.FilterLogic),
                    KendoGridFilter.BuildWhereClause(Of T)(i, requestParams.FilterLogic, requestParams.Filters(i), parameters))
            End If
        Next
        Dim filteredResults As IQueryable(Of T)
        If Not String.IsNullOrWhiteSpace(filterString) Then
            filteredResults = results.Where(filterString, parameters.ToArray())
        Else
            filteredResults = results
        End If

        Dim resultsCount As Integer = PageAndSort(Of T)(requestParams, filteredResults)

        Return New GridResults(Of T)() With {.TotalResults = resultsCount, .Data = filteredResults}

    End Function

    Private Shared Function PageAndSort(Of T)(ByVal gridPost As KendoGridPost, ByRef results As IQueryable(Of T)) As Integer
        Dim page As Integer = gridPost.Page
        Dim rows As Integer = gridPost.PageSize

        Dim pageIndex As Integer = Convert.ToInt32(page) - 1
        Dim pageSize As Integer = rows
        Dim totalRecords As Integer = results.Count()

        'If the sort Order is provided perform a sort on the specified column
        If Not String.IsNullOrEmpty(gridPost.SortOrd) Then
            results = CType(results.OrderBy(gridPost.SortOn & " " & gridPost.SortOrd).Skip(pageIndex * pageSize).Take(pageSize), IQueryable(Of T))
        Else
            results = CType(results.Skip(pageIndex * pageSize).Take(pageSize), IQueryable(Of T))
        End If

        Return totalRecords
    End Function


End Class

Public Class GridResults(Of T)
    Public Property TotalResults As Integer
    Public Property Data As IQueryable(Of T)
End Class