using System;
using System.IO;
using System.IO.Compression;
using System.Drawing;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;

public static class FfmpegTests
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static void Fails<T>(Action action) where T:Exception {try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
    static string Hash(string path){using(SHA256 hash=SHA256.Create())using(Stream input=File.OpenRead(path))return BitConverter.ToString(hash.ComputeHash(input)).Replace("-","");}
    static void Zip(string path,params string[] names){using(ZipArchive zip=ZipFile.Open(path,ZipArchiveMode.Create))foreach(string name in names)using(StreamWriter writer=new StreamWriter(zip.CreateEntry(name).Open()))writer.Write("fixture");}
    static void Capture(Form form,string path){form.Show();Application.DoEvents();using(Bitmap bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(Point.Empty,bitmap.Size));bitmap.Save(path);}form.Close();}
    [STAThread] public static int Main(string[] args) {
        try {Run(args);return 0;}catch(Exception e){Console.Error.WriteLine(e);return 1;}
    }
    static void Run(string[] args) {
        string root=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"case-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        Fails<FfmpegMissingException>(delegate{Decoder.Locate(new string[0],"");});
        string local=Path.Combine(root,"ffmpeg.exe");File.WriteAllText(local,"fixture");
        Check(Decoder.Locate(new string[]{local},"")==local,"Local executable ignored");
        Check(Decoder.Locate(new string[0],"\""+root+"\"")==local,"Quoted PATH ignored");
        string archive=Path.Combine(root,"good.zip");Zip(archive,"release/bin/ffmpeg.exe","release/LICENSE","../escape.txt");
        FfmpegSetup.VerifyHash(archive,Hash(archive),CancellationToken.None);
        Fails<IOException>(delegate{FfmpegSetup.VerifyHash(archive,new string('0',64),CancellationToken.None);});
        Fails<IOException>(delegate{FfmpegSetup.VerifyHash(archive,"invalid",CancellationToken.None);});
        Fails<OperationCanceledException>(delegate{FfmpegSetup.VerifyHash(archive,Hash(archive),new CancellationToken(true));});
        string extracted=Path.Combine(root,"extracted");FfmpegSetup.Extract(archive,extracted,CancellationToken.None);
        Check(File.Exists(Path.Combine(extracted,"ffmpeg.exe")) && File.Exists(Path.Combine(extracted,"LICENSE")),"Missing binary or license");
        Check(!File.Exists(Path.Combine(root,"escape.txt")),"Archive escaped destination");
        string empty=Path.Combine(root,"empty.zip");Zip(empty,"README.txt");
        Fails<IOException>(delegate{FfmpegSetup.Extract(empty,extracted,CancellationToken.None);});
        string duplicate=Path.Combine(root,"duplicate.zip");Zip(duplicate,"a/bin/ffmpeg.exe","b/bin/ffmpeg.exe");
        Fails<IOException>(delegate{FfmpegSetup.Extract(duplicate,extracted,CancellationToken.None);});
        Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
        foreach(string lang in new string[]{"zh-CN","en"}){L.Language=lang;using(FfmpegDownloadDialog dialog=new FfmpegDownloadDialog())Capture(dialog,Path.Combine(root,"download-"+lang+".png"));}
        Console.WriteLine("PASS: missing/local/PATH lookup, SHA256 validation, cancellation, bounded extraction, license, missing/duplicate executable, bilingual download dialog.");
        if(Array.IndexOf(args,"--download")>=0) {
            string destination=Path.Combine(root,"installed","ffmpeg.exe");
            using(CancellationTokenSource cancel=new CancellationTokenSource()) {
                Fails<OperationCanceledException>(delegate {FfmpegSetup.Install(delegate(int percent,string text){if(percent>=0 && percent<90)cancel.Cancel();},cancel.Token,destination);});
            }
            Check(!File.Exists(destination),"Cancelled download installed executable");
            Check(Directory.GetDirectories(Path.GetDirectoryName(destination),"download-*").Length==0,"Cancelled download left temporary data");
            Console.WriteLine("PASS: live download cancellation and cleanup. Retrying...");
            DateTime last=DateTime.MinValue;
            FfmpegSetup.Install(delegate(int percent,string text){if((DateTime.UtcNow-last).TotalSeconds>3 || percent>=92){Console.WriteLine(text);last=DateTime.UtcNow;}},CancellationToken.None,destination);
            Check(File.Exists(destination),"Download not installed");
            // The real downloaded executable must decode a still image without serial hardware.
            File.Copy(destination,Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"ffmpeg.exe"),true);
            string image=Path.Combine(root,"sample.png");using(Bitmap bitmap=new Bitmap(160,80)){using(Graphics graphics=Graphics.FromImage(bitmap))graphics.Clear(Color.Red);bitmap.Save(image);}
            using(Decoder decoder=new Decoder(image,5,0,false,false,false)) {byte[] frame=decoder.Next();Check(frame!=null && frame.Length==25600 && frame[0]==0xf8 && frame[1]==0,"Downloaded decoder produced wrong RGB565 frame");Check(decoder.Next()==null,"Still image did not end");}
            Check(Directory.GetDirectories(Path.GetDirectoryName(destination),"download-*").Length==0,"Successful download left temporary data");
            Console.WriteLine("PASS: real provider download, checksum, extraction, executable startup, decode, retry and cleanup.");
        }
        Console.WriteLine("Artifacts: "+root);
    }
}
