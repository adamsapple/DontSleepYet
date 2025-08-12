using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DontSleepYet.Contracts.Services;
using Microsoft.Extensions.Options;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DontSleepYet.Services;
public class GithubUnpackagedUpdateService: IUpdateService
{
    #region Dependencies.
    private readonly GithubBaseOption baseOption;
    private IUpdateCheckService updateCheckService;
    #endregion Dependencies.

    private UpdateCheckData? latestCheckData = null;

    public GithubUnpackagedUpdateService(IOptions<GithubBaseOption> baseOption,
                                         IUpdateCheckService updateCheckService)
    {
        this.baseOption         = baseOption?.Value ?? throw new ArgumentNullException(nameof(baseOption));
        this.updateCheckService = updateCheckService;
    }

    public async Task<UpdateCheckData> CheckUpdateAsync()
    {
        var data = await updateCheckService.CheckUpdateAsync();

        latestCheckData = data;

        return data;
    }

    public async Task UpdateAsync()
    {
        var data = latestCheckData;
        if (data == null)
        {
            data = await updateCheckService.CheckUpdateAsync();
        }

        if (data == null || !data.IsUpdateAvailable)
        {
            return;
        }

        await UpdateAsync(data.ArchiveUrl!);
    }

    public async Task UpdateAsync(Uri uri)
    {
        var client   = new HttpClient();
        var request  = new HttpRequestMessage(HttpMethod.Get, uri);
        var fileName = uri.Segments.LastOrDefault() ?? "data.zip";
        
        baseOption.AddHeader(request);

        var response = await client.SendAsync(request);

        var downloadPath     = Path.GetTempPath();      // %APPDATA%\..\Local\Temp
        var downloadFilePath = Path.Combine(downloadPath, fileName);
        var extractPath      = Path.Combine(downloadPath, Path.GetFileNameWithoutExtension(downloadFilePath));
        var moveToPath       = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var exePath          = Path.ChangeExtension(System.Reflection.Assembly.GetExecutingAssembly().Location, "exe");

        /// Response to File.
        using (var zipdata = await response.Content.ReadAsStreamAsync())
        using (FileStream outout = File.Create(downloadFilePath))
        {
            await zipdata.CopyToAsync(outout);
        }

        /// Zipファイルの展開
        {
            Directory.CreateDirectory(extractPath);
            

            using (ZipArchive archive = ZipFile.OpenRead(downloadFilePath))
            {
                archive.ExtractToDirectory(extractPath, true);
            }
        }

        //string basePath = GetAllFileInFolder(PathBase, "exe")[0];
        //string pathAft;
        //pathAft = GetFileNameInPath(basePath, false);
        //pathAft = GetNowFolderPath() + "\\" + pathAft;
        //File.Copy(basePath, pathAft, true);

        
        // Batファイル出力
        var bat = $@"
@echo バージョンアップ中...@echo off

timeout /t 10 /nobreak > nul

del {exePath} > nul
REM move /y {extractPath}\* {moveToPath}\
robocopy ""{extractPath}\"" ""{moveToPath}\"" * /MOVE


start {exePath}
REM del update.bat > nul

pause
exit
";
        var batchFile = "update.bat";
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance); // memo: Shift-JISを扱うためのおまじない
        var sw = new StreamWriter(batchFile, false, Encoding.GetEncoding("Shift_JIS"));
        sw.Write(bat);
        sw.Close();
        
        // exe更新
        var p = new Process();
        p.StartInfo.FileName = batchFile;
        p.StartInfo.CreateNoWindow = false;
        p.Start();
        Exit();
        //p.Close();
    }


    private void Exit()
    {
        var app = Microsoft.UI.Xaml.Application.Current as App;
        if (app == null)
        {
            return;
        }

        App.HandleClosedEvents = false;

        app.CloseWindow();
        app.Exit();
    }

}
