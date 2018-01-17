Imports System.ComponentModel
Imports System.Threading
Imports System.IO

Public Class ConvertVideoFiles
    Public Property FileToConvert As String
    Private WithEvents worker As New BackgroundWorker()
    Private Event BackgroundWorkFinished As EventHandler
    Private proc As New Process
    Private Sub Init()


    End Sub
    Public Function StartConversion() As Boolean
        Try
            'AddHandler worker.DoWork, AddressOf worker_DoWork
            'AddHandler worker.RunWorkerCompleted, AddressOf worker_RunWorkerCompleted
            'AddHandler worker.ProgressChanged, AddressOf worker_ProgressChanged
            'worker.WorkerReportsProgress = True
            'worker.WorkerSupportsCancellation = True

            'If worker.IsBusy = False Then
            '    Console.WriteLine("Starting Converting...")
            '    worker.RunWorkerAsync()
            'End If

            Call Converter()

        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function
    Private Sub worker_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles worker.ProgressChanged
        Console.WriteLine(e.ProgressPercentage.ToString())
    End Sub

    Private Sub worker_DoWork(sender As Object, e As DoWorkEventArgs) Handles worker.DoWork
        Try
            Call Converter()
        Catch ex As Exception
            Console.WriteLine("Backgroundworker {0}", ex.Message)
        End Try

    End Sub

    Private Sub worker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles worker.RunWorkerCompleted
        Console.WriteLine("Done now...")
    End Sub

    Private Sub Converter()
        Dim OutFile As String
        Dim watch As New Stopwatch

        ' WriteOnBottomLine("File " + FileToConvert + " converting to MP4")
        OutFile = ChangeFileToMP4(FileToConvert)

        Try
            If File.Exists(OutFile) = False Then
                'Dim spinner = New Spinner(20, 1)
                'spinner.Start()
                Dim ffMpeg = New NReco.VideoConverter.FFMpegConverter()
                ffMpeg.ConvertMedia(FileToConvert, OutFile.ToUpper(), "MP4")


                'startFFmConversion(FileToConvert, OutFile.ToUpper())
                ' spinner.[Stop]()
            End If
            'If IsFileLocked(FileToConvert) = True Then
            '    Thread.Sleep(3000)
            '    StartConversion()
            'End If
        Catch ex As Exception
            Console.WriteLine("Error Converting File {0}", ex.Message)
        End Try

    End Sub

    Private Function ChangeFileToMP4(ByVal fileName As String) As String
        Dim newFileName As String
        Select Case Path.GetExtension(fileName).ToLower()
            Case ".mpg"
                newFileName = Path.ChangeExtension(fileName, ".MP4")

        End Select
        Return newFileName
    End Function
    Public Function FileInUse(ByVal sFile As String) As Boolean
        Dim thisFileInUse As Boolean = False
        If System.IO.File.Exists(sFile) Then
            Try
                Using f As New IO.FileStream(sFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                    ' thisFileInUse = False
                End Using
            Catch
                thisFileInUse = True
            End Try
        End If
        Return thisFileInUse
    End Function

    Public Function IsFileLocked(ByVal sFile As String) As Boolean
        If System.IO.File.Exists(sFile) Then
            Try
                Dim F As Short = FreeFile()
                FileOpen(F, sFile, OpenMode.Binary, OpenAccess.ReadWrite, OpenShare.LockReadWrite)
                FileClose(F)
            Catch
                Return True
            End Try
        End If
        Return False
    End Function

    Function startFFmConversion(Input As String, Output As String)

        Dim exepath As String = My.Application.Info.DirectoryPath + "\ffmpeg.exe"
        Dim quality As Integer = 2

        Dim startinfo As New System.Diagnostics.ProcessStartInfo
        Dim sr As StreamReader
        Dim cmd As String = " -i """ + input + """ -ar 22050 -qscale " & quality & " -y """ + output + """" 'ffmpeg commands -y replace

        Dim ffmpegOutput As String

        ' all parameters required to run the process
        startinfo.FileName = exepath
        startinfo.Arguments = cmd
        startinfo.UseShellExecute = False
        startinfo.WindowStyle = ProcessWindowStyle.Hidden
        startinfo.RedirectStandardError = True
        startinfo.RedirectStandardOutput = True
        startinfo.CreateNoWindow = True
        proc.StartInfo = startinfo
        proc.Start() ' start the process
        sr = proc.StandardError 'standard error is used by ffmpeg

        Do
            If worker.CancellationPending Then 'check if a cancellation request was made
                Exit Function
            End If
            ffmpegOutput = sr.ReadLine

        Loop Until proc.HasExited And ffmpegOutput = Nothing Or ffmpegOutput = ""

        Return 0
    End Function
    Private Sub WriteOnBottomLine(text As String)
        Dim x As Integer = Console.CursorLeft
        Dim y As Integer = Console.CursorTop
        Console.CursorTop = Console.WindowTop + Console.WindowHeight - 1
        Console.Write(text)
        ' Restore previous position
        Console.SetCursorPosition(x, y)
    End Sub

End Class
