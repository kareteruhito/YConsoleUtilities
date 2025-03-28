using System.CommandLine;
using OpenCvSharp;

// dotnet.exe publish -c Release -r win-x64 /p:PublishSingleFile=true --self-contained true -o (Convert-Path ~/Bin)

namespace YResize;
class Program
{
    static void Resize(string srcFile, string dstFile, double scale)
    {
        if (!File.Exists(srcFile)) return;

        // サフィックス
        string defaultSuffix = $"_yresize_{scale}";

        srcFile = Path.GetFullPath(srcFile);
        if ("" == dstFile)
        {
            // 出力パス生成
            var dirname = Path.GetDirectoryName(srcFile);
            var basename = Path.GetFileNameWithoutExtension(srcFile);
            var ext = Path.GetExtension(srcFile);
            dstFile = Path.Join(dirname, $"{basename}{defaultSuffix}{ext}");
        }

        try
        {
            // 画像を読み込む
            using Mat src = new (srcFile);
            // 画像をリサイズする
            using Mat dst = new Mat();

            // 補完方法
            var InterpolationFlag = scale < 0 ? InterpolationFlags.Area : InterpolationFlags.Cubic;
            // リサイズ
            Cv2.Resize(src, dst, new Size(), scale, scale, InterpolationFlag);

            // リサイズ後の画像を保存する
            Cv2.ImWrite(dstFile, dst);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"エラーが発生しました: {ex.Message}");
        }
    }
    static async Task<int> Main(string[] args)
    {
        var srcFileOption = new Option<string>(
            name: "--srcFile",
            description: "変換元の画像ファイル"
        ){ IsRequired = true };
        var dstFileOption = new Option<string>(
            name: "--dstFile",
            description: "変換先の画像ファイルト",
            getDefaultValue: () => ""
        ){ IsRequired = false };
        var scaleOption = new Option<double>(
            name: "--sclae",
            description: "拡大縮小倍率",
            getDefaultValue: () => 0.25
        ){ IsRequired = false };
        var rootCommand = new RootCommand(
            "画像サイズを拡大縮小するコマンド"
        );
        rootCommand.AddOption(srcFileOption);
        rootCommand.AddOption(dstFileOption);
        rootCommand.AddOption(scaleOption);

        rootCommand.SetHandler((srcFile, dstFile, scale) => 
            {
                Resize(srcFile, dstFile, scale);
            },
            srcFileOption, dstFileOption, scaleOption);

        return await rootCommand.InvokeAsync(args);
    }
}