Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System.ComponentModel
Namespace SIS.PAK
  <DataObject()>
  Partial Public Class pakQCListDIRef
    Public Property SerialNo As Integer = 0
    Public Property QCLNo As Integer = 0
    Public Property ItemReference As String = ""
    Public Property SubItem As String = ""
    Public Property TotalWeight As Decimal = 0
    Public Property ProgressPercent As Decimal = 0
    Public Property POWeight As Decimal = 0

    Public Shared Function GetOfferedQCListDIref(ByVal QCRequestNo As Integer) As List(Of SIS.PAK.pakQCListDIRef)
      Dim Results As New List(Of SIS.PAK.pakQCListDIRef)
      Dim Sql As String = ""
      Sql &= " select "
      Sql &= "   qcl.SerialNo, "
      Sql &= "   qcl.QCLNo, "
      Sql &= "   sum(qcl.TotalWeight) as TotalWeight, "
      Sql &= "   itm.ItemReference, "
      Sql &= "   itm.SubItem, "
      Sql &= "   po.POWeight "
      Sql &= " from PAK_QCListD as qcl "
      Sql &= "   inner join PAK_POBItems as itm "
      Sql &= "      on qcl.SerialNo = itm.SerialNo "
      Sql &= " 	   and qcl.BOMNo = itm.BOMNo "
      Sql &= " 	   and qcl.ItemNo = itm.ItemNo "
      Sql &= "   inner join PAK_QCListH as qch "
      Sql &= "      on qcl.SerialNo = qch.SerialNo "
      Sql &= "     and qcl.QCLNo = qch.QCLNo "
      Sql &= "   inner join PAK_PO as po "
      Sql &= "      oN po.SerialNo = qch.SerialNo "
      Sql &= " where "
      Sql &= "   qcl.InspectionStageID in (2,3) "
      Sql &= "   and qch.QCRequestNo = " & QCRequestNo
      Sql &= " group by "
      Sql &= "   qcl.SerialNo, "
      Sql &= "   qcl.QCLNo, "
      Sql &= "   itm.ItemReference, "
      Sql &= "   itm.SubItem, "
      Sql &= "   po.POWeight "
      Using Con As SqlConnection = New SqlConnection(SIS.SYS.SQLDatabase.DBCommon.GetConnectionString())
        Using Cmd As SqlCommand = Con.CreateCommand()
          Cmd.CommandType = CommandType.Text
          Cmd.CommandText = Sql
          Con.Open()
          Dim Reader As SqlDataReader = Cmd.ExecuteReader()
          While (Reader.Read())
            Dim tmp As New SIS.PAK.pakQCListDIRef(Reader)
            Try
              tmp.ProgressPercent = (tmp.TotalWeight / tmp.POWeight) * 100
            Catch ex As Exception
            End Try
            Results.Add(tmp)
          End While
          Reader.Close()
        End Using
      End Using
      Return Results
    End Function
    'Public Shared Function GetClearedQCListDIref(ByVal SerialNo As Integer, ByVal QCLNo As Integer) As List(Of SIS.PAK.pakQCListDIRef)
    '  Dim Results As New List(Of SIS.PAK.pakQCListDIRef)
    '  Dim Sql As String = ""
    '  Sql &= " select "
    '  Sql &= "   qcl.SerialNo, "
    '  Sql &= "   qcl.QCLNo, "
    '  Sql &= "   sum(qcl.QualityClearedWt) as TotalWeight, "
    '  Sql &= "   itm.ItemReference, "
    '  Sql &= "   itm.SubItem "
    '  Sql &= " from PAK_QCListD as qcl "
    '  Sql &= "   inner join PAK_POBItems as itm "
    '  Sql &= "      on qcl.SerialNo = itm.SerialNo "
    '  Sql &= " 	   and qcl.BOMNo = itm.BOMNo "
    '  Sql &= " 	   and qcl.ItemNo = itm.ItemNo "
    '  Sql &= " where "
    '  Sql &= "   qcl.InspectionStageID in (2,3) "
    '  Sql &= "   and qcl.SerialNo = " & SerialNo
    '  Sql &= "   and qcl.QCLNo = " & QCLNo
    '  Sql &= " group by "
    '  Sql &= "   qcl.SerialNo, "
    '  Sql &= "   qcl.QCLNo, "
    '  Sql &= "   itm.ItemReference, "
    '  Sql &= "   itm.SubItem "
    '  Using Con As SqlConnection = New SqlConnection(SIS.SYS.SQLDatabase.DBCommon.GetConnectionString())
    '    Using Cmd As SqlCommand = Con.CreateCommand()
    '      Cmd.CommandType = CommandType.Text
    '      Cmd.CommandText = Sql
    '      Con.Open()
    '      Dim Reader As SqlDataReader = Cmd.ExecuteReader()
    '      While (Reader.Read())
    '        Dim tmp As New SIS.PAK.pakQCListDIRef(Reader)
    '        Try
    '          tmp.ProgressPercent = (tmp.TotalWeight / POWeight) * 100
    '        Catch ex As Exception
    '        End Try
    '        Results.Add(tmp)
    '      End While
    '      Reader.Close()
    '    End Using
    '  End Using
    '  Return Results
    'End Function
    Public Sub New(ByVal Reader As SqlDataReader)
      Try
        For Each pi As System.Reflection.PropertyInfo In Me.GetType.GetProperties
          If pi.MemberType = Reflection.MemberTypes.Property Then
            Try
              Dim Found As Boolean = False
              For I As Integer = 0 To Reader.FieldCount - 1
                If Reader.GetName(I).ToLower = pi.Name.ToLower Then
                  Found = True
                  Exit For
                End If
              Next
              If Found Then
                If Convert.IsDBNull(Reader(pi.Name)) Then
                  Select Case Reader.GetDataTypeName(Reader.GetOrdinal(pi.Name))
                    Case "decimal"
                      CallByName(Me, pi.Name, CallType.Let, "0.00")
                    Case "bit"
                      CallByName(Me, pi.Name, CallType.Let, Boolean.FalseString)
                    Case Else
                      CallByName(Me, pi.Name, CallType.Let, String.Empty)
                  End Select
                Else
                  CallByName(Me, pi.Name, CallType.Let, Reader(pi.Name))
                End If
              End If
            Catch ex As Exception
            End Try
          End If
        Next
      Catch ex As Exception
      End Try
    End Sub
    Public Sub New()
    End Sub
  End Class
End Namespace
