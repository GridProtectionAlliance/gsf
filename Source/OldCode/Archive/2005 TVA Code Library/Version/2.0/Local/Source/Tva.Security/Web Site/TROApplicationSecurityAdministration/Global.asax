
<%@ Application Language="VB" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="Tva.Data.Common" %>

<script runat="server">
    
    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs on application startup
    End Sub
    
    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs on application shutdown
    End Sub
        
    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        
        Dim ex As Exception = HttpContext.Current.Server.GetLastError
        
        If TypeOf ex Is HttpUnhandledException AndAlso ex.InnerException IsNot Nothing Then
            ex = ex.InnerException
        End If
        
        If ex IsNot Nothing Then
            Dim conn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("WebSecurityConnectionString").ConnectionString)
            
            Try
                If Not conn.State = ConnectionState.Open Then
                    conn.Open()
                End If
                
                ExecuteNonQuery("LogError", conn, "TRO_APP_SEC", "An unhandled exception occured.", ex.Message)
                Server.Transfer("ErrorPage.aspx", True)
                
            Catch
                'Do nothing. May be a good place to write to the EventLog.
            Finally
                If conn.State = ConnectionState.Open Then
                    conn.Close()
                End If
                conn.Dispose()
            End Try
        End If
        
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs when a new session is started
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs when a session ends. 
        ' Note: The Session_End event is raised only when the sessionstate mode
        ' is set to InProc in the Web.config file. If session mode is set to StateServer 
        ' or SQLServer, the event is not raised.
    End Sub
       
</script>