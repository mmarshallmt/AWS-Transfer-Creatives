Imports System.Configuration
Imports System.IO
Public Class AwsLog
    Public Sub AddtoLogFile(Message As String, ErrorPage As String, OutPutLocation As String)

        Dim LogPath As String = OutPutLocation
        Dim filename As String = “Log_” + DateTime.Now.ToString(“dd-MM-yyyy”) + “.txt”
        Dim filepath As String = LogPath + filename
        Dim writeFile As StreamWriter
        If File.Exists(filepath) Then

            Using writer As StreamWriter = New StreamWriter(filepath, True)

                writer.WriteLine(“——————-START————-“ + DateTime.Now)
                writer.WriteLine(“Source :” + ErrorPage)
                writer.WriteLine(Message)
                writer.WriteLine(“——————-END————-“ + DateTime.Now)

            End Using
        Else

            writeFile = File.CreateText(filepath)
            writeFile.WriteLine(“——————-START————-“ + DateTime.Now)
            writeFile.WriteLine(“Source :” + ErrorPage)
            writeFile.WriteLine(Message)
            writeFile.WriteLine(“——————-END————-“ + DateTime.Now)
            writeFile.Close()

        End If
    End Sub

    Public Function checkLogFile(fileName As String) As Boolean
        Dim fs As FileStream
        Try
            If Not File.Exists(fileName) Then ' No Then File? Create

                fs = File.Create(fileName)
                fs.Close()
            End If
            Dim fi As New System.IO.FileInfo(fileName)
            If (fi.Length >= 100 * 1024 * 1024) Then ' (100mB) File to big? Create New

                Dim filenamebase As String = "myLogFile" 'Insert the base form Of the log file, the same as the 1st filename without .log at the end
                If (fileName.Contains("-")) Then 'Check Then If older Then Log contained - x

                    Dim lognumber As Integer = Int32.Parse(fileName.Substring(fileName.LastIndexOf("-") + 1, fileName.Length - 4)) 'Get old number, Can cause exception if the last digits aren't numbers
                    lognumber = lognumber + 1  'Increment lognumber by 1
                    fileName = filenamebase + "-" + lognumber + ".log"  'Override filename

                Else

                    fileName = filenamebase + "-1.log" 'Override filename
                End If
                fs = File.Create(fileName)
                fs.Close()
            End If

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
End Class
