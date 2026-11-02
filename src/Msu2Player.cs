using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;

// UI resources. Storage paths and protocol bytes never depend on language.
internal static class L
{
    public static volatile string Language = DefaultLanguage;
    public static string DefaultLanguage { get { return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName=="zh" ? "zh-CN" : "en"; } }
    public static bool Supported(string value) { return value=="zh-CN" || value=="en"; }
    static readonly Dictionary<string,string> English = new Dictionary<string,string> {
        { "需要 FFmpeg", "FFmpeg required" },
        { "未找到 FFmpeg。图片、GIF、视频和离线动画生成需要它；其他显示模式可继续使用。", "FFmpeg was not found. Images, GIFs, videos and offline animation generation need it. Other display modes remain available." },
        { "将从 FFmpeg 官网列出的 Gyan 下载 Windows 64 位版本，校验后安装到当前用户目录，无需管理员权限。", "Download the Windows 64-bit build from Gyan, listed on the FFmpeg website. It will be verified and installed for the current user without administrator access." },
        { "下载并继续", "Download" },
        { "取消", "Cancel" },
        { "打开官网下载页", "Open FFmpeg downloads" },
        { "正在获取下载信息…", "Getting download details..." },
        { "正在下载 FFmpeg：", "Downloading FFmpeg: " },
        { "正在校验并解压…", "Verifying and extracting..." },
        { "正在检查 FFmpeg…", "Checking FFmpeg..." },
        { "下载失败，可重试或打开官网下载页手动下载：", "Download failed. Retry or open the FFmpeg website to download manually: " },
        { "下载已取消，可以重试。", "Download cancelled. You can retry." },
        { "缺少 FFmpeg，请打开主窗口下载后继续。", "FFmpeg is missing. Open the main window to download it and continue." },
        { "未安装 FFmpeg，选择媒体或点击开始显示可重试。", "FFmpeg was not installed. Choose media or click Start display to retry." },
        { "自动下载需要 64 位 Windows。请从官网下载兼容版本并将 ffmpeg.exe 放在程序旁。", "Automatic download requires 64-bit Windows. Download a compatible build and place ffmpeg.exe beside the player." },
        { "下载校验信息无效。", "Invalid download checksum." },
        { "下载文件校验失败，请重试。", "Download checksum mismatch. Please retry." },
        { "下载包中没有唯一的 ffmpeg.exe。", "The download must contain exactly one ffmpeg.exe." },
        { "下载的 FFmpeg 无法运行。请从官网下载兼容版本。", "The downloaded FFmpeg could not run. Download a compatible build from the FFmpeg website." },
        { "屏幕回复异常，请重新连接。", "Unexpected screen reply. Please reconnect." },
        { "屏幕型号确认失败。", "Could not verify the screen model." },
        { "请选择已识别的 MSU2 屏幕端口。", "Select a detected MSU2 screen port." },
        { "屏幕未确认连接。", "The screen did not confirm the connection." },
        { "屏幕未响应，请重新插拔后重试。", "The screen is not responding. Unplug it, reconnect, and retry." },
        { "本程序适用于 160×80 的 MSU2 MINI 屏幕。", "This player supports 160 x 80 MSU2 MINI screens." },
        { "屏幕连接中断，请重新连接。", "Connection lost. Please reconnect." },
        { "存储读取回复异常，已停止操作。", "Unexpected storage reply. Operation stopped." },
        { "屏幕离线布局与 36 帧 / 100ms 的标准配置不符，未执行擦除或写入。", "The offline layout is not the standard 36 frames / 100 ms. Nothing was erased or written." },
        { "存储写入未确认。请保留备份，重新连接后恢复。", "Storage write was not confirmed. Keep the backup and restore it after reconnecting." },
        { "离线动画必须是 36 帧 160×80 RGB565。", "Offline animation must contain 36 frames of 160 x 80 RGB565." },
        { "备份原动画 ", "Backing up original animation " },
        { "%（备份和校验总计可能约 15 分钟）", "% (backup and verification may take about 15 minutes)" },
        { "备份已保存，开始写入离线动画；请保持供电", "Backup saved. Writing animation; keep USB power connected." },
        { "写入第 ", "Writing page " },
        { " 页时回复异常。备份：", " failed: unexpected reply. Backup: " },
        { "写入动画 ", "Writing animation " },
        { "写后校验失败，地址 ", "Verification failed at address " },
        { "。备份：", ". Backup: " },
        { "校验动画 ", "Verifying animation " },
        { "写入及完整校验完成。关闭播放器并重新上电，即可离线播放。", "Write and full verification complete. Exit the player and reconnect power for offline playback." },
        { "图像数据大小不正确。", "Invalid image data size." },
        { "找不到视频解码器。请将 ffmpeg.exe 放到程序旁边后重试。", "Decoder not found. Place ffmpeg.exe beside the player and retry." },
        { "媒体无法解码。", "Cannot decode this media." },
        { "选择媒体，开始预览", "Choose media to preview" },
        { "内存", "Memory" },
        { "电量 / 电源", "Power" },
        { "系统盘", "Disk" },
        { "上传  ", "Up  " },
        { "下载  ", "Down  " },
        { "0 · 屏幕内动图", "0 · Stored animation" },
        { "1 · 电脑状态 · 蓝色", "1 · PC status · Blue" },
        { "2 · 电脑状态 · 红色", "2 · PC status · Red" },
        { "3 · 屏幕内照片", "3 · Stored photo" },
        { "4 · 时钟", "4 · Clock" },
        { "5 · 桌面投屏", "5 · Desktop mirror" },
        { "6 · 网速", "6 · Network speed" },
        { "7 · 图片 / GIF / 视频", "7 · Image / GIF / Video" },
        { "MSU2 小屏播放器 v1.5.5", "MSU2 Mini Player v1.5.5" },
        { "已开启：登录 Windows 后自动实时播放上次的媒体", "Enabled: resume the last content at Windows login" },
        { "已关闭开机启动", "Start at login disabled" },
        { "开机启动设置未保存：", "Could not save startup setting: " },
        { "开机时隐藏窗口、在托盘运行并自动实时显示", "Start hidden in the tray and resume content at login" },
        { "开机时最小化显示主窗口", "Start with the main window minimized" },
        { "请先选择图片、GIF 或视频。", "Choose an image, GIF, or video first." },
        { "请先选择屏幕端口。", "Select a screen port first." },
        { "上次的媒体文件找不到，请重新选择。", "Last media file not found. Choose another file." },
        { "开机实时显示：等待内容和屏幕就绪…", "Startup playback: waiting for content and screen..." },
        { "开机实时播放未开始：请先选择媒体文件。", "Startup playback needs a media file. Choose one first." },
        { "小屏播放器", "Mini Player" },
        { "实时显示 · 状态监测 · 离线播放   /   关闭窗口后继续在托盘运行", "Live display · PC stats · Offline playback / Close to tray" },
        { "小屏精灵图标", "Player mascot icon" },
        { "实时预览", "Live preview" },
        { "媒体文件", "Media file" },
        { "拖入图片、GIF 或视频", "Drop an image, GIF, or video here" },
        { "选择媒体…", "Open file..." },
        { "保存到屏幕…", "Save offline..." },
        { "屏幕连接", "Screen connection" },
        { "刷新", "Refresh" },
        { "连接屏幕", "Connect" },
        { "功能状态", "Display mode" },
        { "上一项", "Previous" },
        { "下一项", "Next" },
        { "轻触屏幕切换状态", "Tap screen to switch modes" },
        { "显示选项", "Display options" },
        { "完整显示 / 留边", "Fit / Letterbox" },
        { "铺满屏幕 / 裁切", "Fill / Crop" },
        { "拉伸铺满", "Stretch" },
        { "循环播放", "Loop" },
        { "旋转 180°", "Rotate 180°" },
        { "启动设置", "Startup options" },
        { "开机启动", "Start at login" },
        { "开机静默运行到托盘", "Start hidden in tray" },
        { "开始显示", "Start display" },
        { "暂停", "Pause" },
        { "先选择功能状态，再连接屏幕", "Choose a mode, then connect the screen" },
        { "本地预览", "Local preview" },
        { "离线操作尚未完成，请等待后从托盘退出。", "Offline operation in progress. Wait, then exit from the tray." },
        { "MSU2 小屏播放器", "MSU2 Mini Player" },
        { "打开主窗口", "Open main window" },
        { "暂停 / 继续显示", "Pause / Resume" },
        { "退出播放器", "Exit player" },
        { "断开连接", "Disconnect" },
        { "时 : 分", "Hours : Minutes" },
        { "时 : 分 : 秒", "Hours : Minutes : Seconds" },
        { "时 : 分 : 秒 . 毫秒", "Hours : Minutes : Seconds . ms" },
        { "显示器 ", "Monitor " },
        { "屏幕内置动图\n直接读取屏幕已存储的 36 帧", "Stored animation\nPlays the 36 frames stored on the screen" },
        { "屏幕内置照片\n直接显示屏幕照片槽位", "Stored photo\nDisplays the photo stored on the screen" },
        { "选择图片、GIF 或视频", "Choose an image, GIF, or video" },
        { "正在生成预览…", "Generating preview..." },
        { "正在显示：", "Displaying: " },
        { "已选择：", "Selected: " },
        { "找不到选择的文件。", "Selected file not found." },
        { "正在载入媒体…", "Loading media..." },
        { "媒体已就绪", "Media ready" },
        { "载入失败：", "Could not load media: " },
        { "未找到 MSU2 屏幕，请插入屏幕后刷新。", "No MSU2 screen found. Plug it in, then refresh." },
        { "正在连接 ", "Connecting to " },
        { " 已连接 · 160 × 80", " connected · 160 x 80" },
        { "播放完成，屏幕保留当前画面", "Playback finished. The screen keeps the last frame." },
        { "连接失败：", "Connection failed: " },
        { "屏幕已断开", "Screen disconnected" },
        { "请插入屏幕后刷新并选择端口。", "Plug in the screen, refresh, and select its port." },
        { "请等待内容载入完成。", "Wait for the content to finish loading." },
        { "正在发送，暂停后可调整画面", "Sending. Pause to adjust the image." },
        { "已暂停发送，屏幕保留当前画面", "Paused. The screen keeps the current frame." },
        { "预览失败：", "Preview failed: " },
        { "预览就绪，点击发送 / 播放", "Preview ready. Click Start display." },
        { "开机实时播放：等待屏幕连接，正在自动重试…", "Startup playback: waiting for screen, retrying..." },
        { "已发送 ", "Sent " },
        { " 帧  ·  ", " frames · " },
        { "本地预览 · 160 × 80", "Local preview · 160 x 80" },
        { "已连接", "Connected" },
        { "等待连接", "Waiting" },
        { "素材不足以生成动画，请选择其他开始位置。", "Not enough frames. Choose another start position." },
        { "保存到屏幕 · 离线播放", "Save to screen · Offline playback" },
        { "关闭软件也能播放，屏幕需保持 USB 供电。\r\n图片会固定显示；GIF / 视频保存为 36 帧、3.6 秒的循环动画。", "Plays with the app closed; keep USB power connected.\r\nImages stay still; GIF / video becomes a 36-frame, 3.6-second loop." },
        { "截取开始（秒）", "Start (seconds)" },
        { "将替换原有离线动画；写入前自动备份，完成后逐字节校验。", "Replaces the stored animation. Auto backup, then full verification." },
        { "生成并预览", "Build preview" },
        { "写入屏幕", "Write to screen" },
        { "恢复备份…", "Restore..." },
        { "取消备份", "Cancel backup" },
        { "正在生成离线动画…", "Building offline animation..." },
        { "预览已生成：160 × 80 · 36 帧 · 3.6 秒", "Preview ready: 160 x 80 · 36 frames · 3.6 seconds" },
        { "生成失败：", "Build failed: " },
        { "选择自动备份的原离线动画.bin", "Select the original animation backup (.bin)" },
        { "离线动画备份", "Offline animation backup" },
        { "备份必须是 921600 字节的离线动画文件。", "Backup must be a 921600-byte offline animation file." },
        { "恢复失败：", "Restore failed: " },
        { "正在停止备份，尚未改动屏幕内容…", "Stopping backup. The screen content has not changed..." },
        { "正在写入或校验，请保持连接，等待完成后关闭。", "Writing or verifying. Keep connected and wait before closing." },
        { "将替换屏幕原有离线动画。原内容会先备份，写入后完整校验。\r\n读取速度有限，整个过程可能约 15 分钟。备份阶段可以取消。\r\n\r\n写入期间请保持 USB 供电。现在开始？", "This replaces the stored animation after backing it up, followed by full verification.\r\nThe process may take about 15 minutes. You can cancel during backup.\r\n\r\nKeep USB power connected while writing. Start now?" },
        { "写入离线动画", "Write offline animation" },
        { "已保存并校验。退出所有播放器后重新上电；若显示其他页面，可轻触屏幕切回动画。", "Saved and verified. Exit all players and reconnect power. Tap the screen to return to animation if needed." },
        { "离线动画已写入，并通过完整校验。\r\n\r\n请退出所有播放器，重新插拔屏幕。之后保持 USB 供电即可自动播放。\r\n\r\n原动画备份：\r\n", "Animation written and fully verified.\r\n\r\nExit all players, unplug the screen, and reconnect it. Keep USB power connected for playback.\r\n\r\nOriginal animation backup:\r\n" },
        { "保存完成", "Save complete" },
        { "备份已取消，未擦除或写入屏幕。", "Backup cancelled. Nothing was erased or written." },
        { "未完成：", "Not completed: " },
        { "\r\n\r\n备份目录：\r\n", "\r\n\r\nBackup folder:\r\n" },
        { "离线保存未完成", "Offline save incomplete" },
        { "图片、GIF 和视频", "Images, GIFs and videos" },
        { "所有文件", "All files" },
    };
    public static string T(string source) {
        string result;
        return Language=="en" && English.TryGetValue(source,out result) ? result : source;
    }
    public static string Source(string text) {
        foreach(KeyValuePair<string,string> item in English) if(item.Value==text) return item.Key;
        return text;
    }
    public static string Retext(string text, string previous) {
        if(previous=="en") foreach(KeyValuePair<string,string> item in English) if(item.Value==text) return T(item.Key);
        return T(text);
    }
    public static void Apply(Control control, string previous) {
        // User filenames and port names are data, not resources.
        if(control.Name!="MediaFilename" && !(control is ComboBox)) control.Text=Retext(control.Text,previous);
        if(control.AccessibleName!=null) control.AccessibleName=Retext(control.AccessibleName,previous);
        ComboBox combo=control as ComboBox;
        if(combo!=null && combo.Name!="LanguagePicker") {
            int selected=combo.SelectedIndex;
            for(int i=0;i<combo.Items.Count;i++) combo.Items[i]=Retext(combo.Items[i].ToString(),previous);
            combo.SelectedIndex=selected;
        }
        foreach(Control child in control.Controls) Apply(child,previous);
        control.Invalidate();
    }
}

internal sealed class ScreenPort : IDisposable
{
    SerialPort port;
    public event Action TouchReleased;
    int touchBaseline, touchSamples; bool touchDown; long lastTouch;
    void Trace(string value) { try { File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"连接记录.log"),DateTime.Now.ToString("HH:mm:ss.fff")+" "+value+"\r\n"); } catch { } }
    public static string[] FindPorts()
    {
        List<string> found = new List<string>();
        using (RegistryKey root = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB\VID_1A86&PID_FE0C&MI_00")) {
            if (root != null) foreach (string child in root.GetSubKeyNames())
                using (RegistryKey key = root.OpenSubKey(child + @"\Device Parameters")) {
                    string name = key == null ? null : key.GetValue("PortName") as string;
                    if (name != null && Array.IndexOf(SerialPort.GetPortNames(), name) >= 0) found.Add(name);
                }
        }
        return found.ToArray();
    }
    byte[] Read(int n)
    {
        byte[] b = new byte[n]; int off = 0;
        while (off < n) off += port.Read(b, off, n - off);
        return b;
    }
    void Write(params byte[] b) { port.Write(b, 0, b.Length); }
    void Ack(params byte[] b)
    {
        Write(b); byte[] reply = Read(6);
        for (int i = 0; i < 6; i++) if (reply[i] != b[i]) throw new IOException(L.T("屏幕回复异常，请重新连接。"));
    }
    int U16(byte index)
    {
        Write(0, 0x30, 0x20, index, 0, 0); byte[] b = Read(6);
        if (b[0] != 0 || b[1] != 0x30 || b[2] != 0x20 || b[3] != index) throw new IOException(L.T("屏幕型号确认失败。"));
        return b[4] * 256 + b[5];
    }
    bool Handshake()
    {
        int timeout=port.ReadTimeout;port.ReadTimeout=400;
        try {
            for(int attempt=0;attempt<2;attempt++) {
                port.DiscardInBuffer();Write(0,77,83,78,67,78);
                try { for(int i=0;i<12;i++) { byte[] b=Read(6);Trace("HELLO RX "+BitConverter.ToString(b));if(Encoding.ASCII.GetString(b)=="\0MSNCN")return true; } } catch(TimeoutException) { }
                // A busy broadcast endpoint can lose the HELLO ACK after accepting the connection.
                // Verify the read-only dictionary signature instead of sending display commands blindly.
                port.DiscardInBuffer();Write(0,0x30,0,1,0,0);
                try { for(int i=0;i<12;i++) {byte[] b=Read(6);if(BitConverter.ToString(b)=="00-30-00-01-00-4C"){Trace("HELLO dictionary confirmation");return true;}} } catch(TimeoutException) { }
                Thread.Sleep(100);
            }
            return false;
        } finally {port.ReadTimeout=timeout;}
    }
    public ScreenPort(string name)
    {
        if (Array.IndexOf(FindPorts(), name) < 0) throw new IOException(L.T("请选择已识别的 MSU2 屏幕端口。"));
        port = new SerialPort(name, 19200, Parity.None, 8, StopBits.One);
        port.DtrEnable = true; port.RtsEnable = false; port.ReadTimeout = 1500; port.WriteTimeout = 3000;
        try {
            port.Open(); Thread.Sleep(1000);
            string broadcast = port.ReadExisting();
            Trace("Open RX "+BitConverter.ToString(Encoding.ASCII.GetBytes(broadcast)));
            if (broadcast.Contains("MSN")) {
                if (!Handshake()) throw new IOException(L.T("屏幕未确认连接。"));
            } else {
                Write(0, 0x30, 0, 1, 0, 0); byte[] b = Read(6);
                if (BitConverter.ToString(b) != "00-30-00-01-00-4C") throw new IOException(L.T("屏幕未响应，请重新插拔后重试。"));
            }
            Trace("Checking dimensions");
            if (U16(0) != 160 || U16(1) != 80) throw new IOException(L.T("本程序适用于 160×80 的 MSU2 MINI 屏幕。"));
            Trace("Setting orientation");
            Ack(2, 3, 10, 0, 0, 0);
            Trace("Connected");
        } catch { Dispose(); throw; }
    }
    public void KeepAlive()
    {
        Write(8, 9, 0, 0, 0, 0); byte[] b = Read(6);
        if (b[0] != 8 || b[1] != 9) throw new IOException(L.T("屏幕连接中断，请重新连接。"));
        int adc=b[4]*256+b[5];
        if(touchSamples<3) { touchBaseline=Math.Max(touchBaseline,adc); touchSamples++; }
        else if(adc<touchBaseline-125) touchDown=true;
        else if(touchDown) { touchDown=false; long now=DateTime.UtcNow.Ticks;
            if(now-lastTouch>TimeSpan.TicksPerMillisecond*350) { lastTouch=now; Action notify=TouchReleased; if(notify!=null) notify(); }
        }
    }
    public void Orientation(bool turn) { Ack(2,3,10,turn ? (byte)1 : (byte)0,0,0); }
    public void ShowStoredPhoto(int page)
    {
        if(page<0 || page>3996) throw new ArgumentOutOfRangeException("page");
        Write(2,0,0,0,0,0); Write(2,1,0,160,0,80);
        Ack(2,3,0,(byte)(page>>8),(byte)page,0);
    }
    public byte ReadFlash(int address)
    {
        if (address < 0 || address >= 1048576) throw new ArgumentOutOfRangeException("address");
        byte[] cmd = { 3, 0, (byte)(address >> 16), (byte)(address >> 8), (byte)address, 0 };
        Write(cmd); byte[] reply = Read(6);
        for (int i = 0; i < 5; i++) if (reply[i] != cmd[i]) throw new IOException(L.T("存储读取回复异常，已停止操作。"));
        return reply[5];
    }
    public byte[] ReadRange(int address, int count)
    {
        if(address<0 || count<0 || address>1048576-count) throw new ArgumentOutOfRangeException("address");
        byte[] result=new byte[count];
        // Firmware drops replies when read requests are pipelined. Keep one outstanding request.
        for(int pos=0;pos<count;pos++) result[pos]=ReadFlash(address+pos);
        return result;
    }
    public byte[] OfflineLayout()
    {
        byte[] cfg = new byte[32]; for (int i = 0; i < cfg.Length; i++) cfg[i] = ReadFlash(0xffe00 + i);
        if (cfg[0] != 0 || cfg[1] != 0 || cfg[2] != 0 || cfg[3] != 0 || cfg[4] != 0 || cfg[5] != 100 ||
            cfg[6] != 0 || cfg[7] != 36 || cfg[8] != 0 || cfg[9] != 0 || cfg[10] != 0 ||
            cfg[20] != 0 || cfg[21] != 160 || cfg[22] != 0 || cfg[23] != 80)
            throw new IOException(L.T("屏幕离线布局与 36 帧 / 100ms 的标准配置不符，未执行擦除或写入。"));
        return cfg;
    }
    void FlashAck(byte[] command)
    {
        Write(command); byte[] reply = Read(6);
        if (reply[0] != command[0] || reply[1] != command[1]) throw new IOException(L.T("存储写入未确认。请保留备份，重新连接后恢复。"));
    }
    public void SaveOffline(byte[] animation, string folder, Action<int, string> progress, CancellationToken cancel)
    {
        const int size = 921600;
        if (animation == null || animation.Length != size) throw new ArgumentException(L.T("离线动画必须是 36 帧 160×80 RGB565。"));
        byte[] layout = OfflineLayout(); // Never infer or rewrite panel settings.
        Directory.CreateDirectory(folder); string backup = Path.Combine(folder, "原离线动画.bin");
        File.WriteAllBytes(Path.Combine(folder, "离线目录-只读.bin"), layout);
        // Finish and close the backup before sending any erase command.
        using (FileStream output = new FileStream(backup + ".partial", FileMode.CreateNew, FileAccess.Write)) {
            byte[] chunk = new byte[1024];
            for (int pos = 0; pos < size; pos += chunk.Length) {
                cancel.ThrowIfCancellationRequested();
                chunk = ReadRange(pos, chunk.Length);
                output.Write(chunk, 0, chunk.Length);
                progress(pos * 40 / size, L.T("备份原动画 ") + (pos * 100 / size) + L.T("%（备份和校验总计可能约 15 分钟）"));
            }
            output.Flush(true);
        }
        File.Move(backup + ".partial", backup);
        using (SHA256 hash = SHA256.Create()) using (FileStream input = File.OpenRead(backup))
            File.WriteAllText(Path.Combine(folder, "备份校验SHA256.txt"), BitConverter.ToString(hash.ComputeHash(input)).Replace("-", ""));
        File.WriteAllBytes(Path.Combine(folder, "本次写入动画.bin"), animation);
        cancel.ThrowIfCancellationRequested();
        progress(40, L.T("备份已保存，开始写入离线动画；请保持供电"));
        int previousTimeout = port.ReadTimeout; port.ReadTimeout = 60000;
        try { FlashAck(new byte[] { 3, 2, 0, 0, 0x0e, 0x10 }); } finally { port.ReadTimeout = previousTimeout; }
        byte[] packet = new byte[390];
        for (int page = 0; page < 3600; page++) {
            for (int i = 0; i < 64; i++) {
                packet[i * 6] = 4; packet[i * 6 + 1] = (byte)i;
                Buffer.BlockCopy(animation, page * 256 + i * 4, packet, i * 6 + 2, 4);
            }
            packet[384] = 3; packet[385] = 3; packet[386] = 0; packet[387] = (byte)(page >> 8); packet[388] = (byte)page; packet[389] = 1;
            port.Write(packet, 0, packet.Length); byte[] reply = Read(6);
            if (reply[0] != 3 || reply[1] != 3) throw new IOException(L.T("写入第 ") + page + L.T(" 页时回复异常。备份：") + backup);
            if (page % 16 == 0) progress(40 + page * 35 / 3600, L.T("写入动画 ") + (page * 100 / 3600) + "%");
        }
        // Full readback, not a sampled check. Layout and other resources are outside the erased range.
        for (int pos = 0; pos < size; pos+=1024) {
            byte[] check=ReadRange(pos,1024);
            for(int i=0;i<check.Length;i++) if(check[i]!=animation[pos+i]) throw new IOException(L.T("写后校验失败，地址 ") + (pos+i) + L.T("。备份：") + backup);
            progress(75 + pos * 25 / size, L.T("校验动画 ") + (pos * 100 / size) + "%");
        }
        progress(100, L.T("写入及完整校验完成。关闭播放器并重新上电，即可离线播放。"));
    }
    public void SendFrame(byte[] pixels, CancellationToken cancel)
    {
        if (pixels.Length != 25600) throw new ArgumentException(L.T("图像数据大小不正确。"));
        byte[] packet = new byte[390];
        for (int y = 0; y < 80; y++) {
            cancel.ThrowIfCancellationRequested();
            Write(2, 0, 0, 0, 0, (byte)y); Write(2, 1, 0, 160, 0, 1); Ack(2, 3, 7, 0, 0, 0);
            for (int pos = 0; pos < 320; pos += 256) {
                int count = Math.Min(256, 320 - pos);
                for (int i = 0; i < 64; i++) {
                    packet[i * 6] = 4; packet[i * 6 + 1] = (byte)i;
                    for (int j = 0; j < 4; j++) {
                        int k = i * 4 + j;
                        packet[i * 6 + 2 + j] = k < count ? pixels[y * 320 + pos + k] : (byte)255;
                    }
                }
                packet[384] = 2; packet[385] = 3; packet[386] = 8;
                packet[387] = (byte)(count >> 8); packet[388] = (byte)count; packet[389] = 0;
                port.Write(packet, 0, packet.Length); KeepAlive();
            }
        }
    }
    public void Dispose() { if (port != null) { port.Dispose(); port = null; } }
}

internal sealed class FfmpegMissingException : FileNotFoundException
{
    public FfmpegMissingException() : base(L.T("缺少 FFmpeg，请打开主窗口下载后继续。")) { }
}

internal static class FfmpegSetup
{
    public const string Website="https://ffmpeg.org/download.html";
    const string ChecksumUrl="https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip.sha256";
    public static string InstalledPath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"MSU2MiniPlayer","ffmpeg","ffmpeg.exe"); } }
    static HttpWebRequest Request(string url) {
        HttpWebRequest request=(HttpWebRequest)WebRequest.Create(url);
        request.UserAgent="MSU2MiniPlayer";request.Timeout=20000;request.ReadWriteTimeout=30000;
        return request;
    }
    public static void VerifyHash(string archive,string expected,CancellationToken token) {
        if(expected==null || expected.Length!=64 || !System.Text.RegularExpressions.Regex.IsMatch(expected,"\\A[0-9a-fA-F]{64}\\z"))throw new IOException(L.T("下载校验信息无效。"));
        using(SHA256 hash=SHA256.Create())using(Stream input=File.OpenRead(archive)) {
            byte[] buffer=new byte[65536];int count;
            while((count=input.Read(buffer,0,buffer.Length))>0) {token.ThrowIfCancellationRequested();hash.TransformBlock(buffer,0,count,buffer,0);}
            hash.TransformFinalBlock(new byte[0],0,0);
            if(!String.Equals(BitConverter.ToString(hash.Hash).Replace("-",""),expected,StringComparison.OrdinalIgnoreCase))throw new IOException(L.T("下载文件校验失败，请重试。"));
        }
    }
    // Extract only explicitly selected files, never paths supplied by the archive.
    public static void Extract(string archive,string folder,CancellationToken token) {
        using(ZipArchive zip=ZipFile.OpenRead(archive)) {
            ZipArchiveEntry executable=null;
            foreach(ZipArchiveEntry entry in zip.Entries)if(entry.FullName.EndsWith("/bin/ffmpeg.exe",StringComparison.OrdinalIgnoreCase)) {
                if(executable!=null)throw new IOException(L.T("下载包中没有唯一的 ffmpeg.exe。"));executable=entry;
            }
            if(executable==null)throw new IOException(L.T("下载包中没有唯一的 ffmpeg.exe。"));
            Directory.CreateDirectory(folder);
            using(Stream input=executable.Open())using(Stream output=File.Create(Path.Combine(folder,"ffmpeg.exe"))) {
                byte[] buffer=new byte[65536];int count;
                while((count=input.Read(buffer,0,buffer.Length))>0) {token.ThrowIfCancellationRequested();output.Write(buffer,0,count);}
            }
            foreach(ZipArchiveEntry entry in zip.Entries) {
                string name=entry.Name;
                if(name.Equals("LICENSE",StringComparison.OrdinalIgnoreCase) || name.Equals("LICENSE.txt",StringComparison.OrdinalIgnoreCase) || name.Equals("README.txt",StringComparison.OrdinalIgnoreCase)) {
                    token.ThrowIfCancellationRequested();entry.ExtractToFile(Path.Combine(folder,name),true);
                }
            }
        }
    }
    static void CheckExecutable(string path) {
        ProcessStartInfo info=new ProcessStartInfo(path,"-version");info.UseShellExecute=false;info.CreateNoWindow=true;info.RedirectStandardOutput=true;info.RedirectStandardError=true;
        bool recognized=false;
        using(Process process=new Process()) {
            process.StartInfo=info;
            process.OutputDataReceived+=delegate(object sender,DataReceivedEventArgs e){if(e.Data!=null && e.Data.StartsWith("ffmpeg version ",StringComparison.OrdinalIgnoreCase))recognized=true;};
            process.ErrorDataReceived+=delegate { };
            process.Start();process.BeginOutputReadLine();process.BeginErrorReadLine();
            if(!process.WaitForExit(10000)) {try{process.Kill();}catch{}throw new IOException(L.T("下载的 FFmpeg 无法运行。请从官网下载兼容版本。"));}
            process.WaitForExit();
            if(process.ExitCode!=0 || !recognized)throw new IOException(L.T("下载的 FFmpeg 无法运行。请从官网下载兼容版本。"));
        }
    }
    public static string Install(Action<int,string> progress,CancellationToken token) {
        return Install(progress,token,InstalledPath);
    }
    public static string Install(Action<int,string> progress,CancellationToken token,string installedPath) {
        if(!Environment.Is64BitOperatingSystem)throw new PlatformNotSupportedException(L.T("自动下载需要 64 位 Windows。请从官网下载兼容版本并将 ffmpeg.exe 放在程序旁。"));
        ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
        string root=Path.GetDirectoryName(installedPath);Directory.CreateDirectory(root);
        string work=Path.Combine(root,"download-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(work);
        try {
            string checksum,downloadUrl;
            progress(-1,L.T("正在获取下载信息…"));
            HttpWebRequest request=Request(ChecksumUrl);
            using(token.Register(request.Abort))using(WebResponse response=request.GetResponse())using(StreamReader reader=new StreamReader(response.GetResponseStream())) {
                string[] fields=reader.ReadToEnd().Trim().Split(new char[]{' ','\t','\r','\n'},StringSplitOptions.RemoveEmptyEntries);
                if(fields.Length==0)throw new IOException(L.T("下载校验信息无效。"));checksum=fields[0];
                // Pin the ZIP to the version of the checksum, rather than a moving release alias.
                Uri uri=response.ResponseUri;
                if(uri.Scheme!="https" || uri.Host!="www.gyan.dev" || !uri.AbsolutePath.EndsWith(".zip.sha256",StringComparison.OrdinalIgnoreCase))throw new IOException(L.T("下载校验信息无效。"));
                downloadUrl=uri.AbsoluteUri.Substring(0,uri.AbsoluteUri.Length-7);
            }
            string archive=Path.Combine(work,"ffmpeg.zip");request=Request(downloadUrl);
            using(token.Register(request.Abort))using(WebResponse response=request.GetResponse())using(Stream input=response.GetResponseStream())using(Stream output=File.Create(archive)) {
                byte[] buffer=new byte[65536];int count;long received=0,total=response.ContentLength;Stopwatch clock=Stopwatch.StartNew();
                while((count=input.Read(buffer,0,buffer.Length))>0) {
                    token.ThrowIfCancellationRequested();output.Write(buffer,0,count);received+=count;
                    if(clock.ElapsedMilliseconds>200) {progress(total>0?(int)Math.Min(90,received*90/total):-1,L.T("正在下载 FFmpeg：")+(received/1048576.0).ToString("0.0")+" MB"+(total>0?" / "+(total/1048576.0).ToString("0.0")+" MB":""));clock.Restart();}
                }
            }
            token.ThrowIfCancellationRequested();progress(92,L.T("正在校验并解压…"));VerifyHash(archive,checksum,token);
            string extracted=Path.Combine(work,"extracted");Extract(archive,extracted,token);
            progress(98,L.T("正在检查 FFmpeg…"));CheckExecutable(Path.Combine(extracted,"ffmpeg.exe"));token.ThrowIfCancellationRequested();
            foreach(string file in Directory.GetFiles(extracted))if(Path.GetFileName(file)!="ffmpeg.exe")File.Copy(file,Path.Combine(root,Path.GetFileName(file)),true);
            // A partial download is never visible to Locate(). Preserve a concurrent completed install.
            if(!File.Exists(installedPath))File.Move(Path.Combine(extracted,"ffmpeg.exe"),installedPath);
            return installedPath;
        } catch {token.ThrowIfCancellationRequested();throw;}
        finally {try{Directory.Delete(work,true);}catch{} }
    }
}

internal sealed class FfmpegDownloadDialog : Form
{
    public static bool Ensure(IWin32Window owner) {
        try {Decoder.Locate();return true;}catch(FfmpegMissingException) {
            using(FfmpegDownloadDialog dialog=new FfmpegDownloadDialog())return dialog.ShowDialog(owner)==DialogResult.OK;
        }
    }
    readonly Button download=new Button(),cancelButton=new Button();
    readonly Label message=new Label();readonly ProgressBar progress=new ProgressBar();
    CancellationTokenSource cancellation;bool busy,closeWhenCancelled;
    public FfmpegDownloadDialog() {
        Text=L.T("需要 FFmpeg");ClientSize=new Size(640,360);MinimumSize=new Size(656,399);StartPosition=FormStartPosition.CenterParent;
        Font=new Font("Microsoft YaHei UI",10);BackColor=PlayerTheme.Canvas;ForeColor=PlayerTheme.Text;MaximizeBox=false;MinimizeBox=false;
        TableLayoutPanel layout=new TableLayoutPanel();layout.Dock=DockStyle.Fill;layout.Padding=new Padding(24);layout.RowCount=6;layout.ColumnCount=1;
        foreach(int height in new int[]{66,78,32,36,52,48})layout.RowStyles.Add(new RowStyle(SizeType.Absolute,height));
        Label explanation=new Label();explanation.Text=L.T("未找到 FFmpeg。图片、GIF、视频和离线动画生成需要它；其他显示模式可继续使用。");explanation.Dock=DockStyle.Fill;layout.Controls.Add(explanation);
        Label source=new Label();source.Text=L.T("将从 FFmpeg 官网列出的 Gyan 下载 Windows 64 位版本，校验后安装到当前用户目录，无需管理员权限。");source.Dock=DockStyle.Fill;layout.Controls.Add(source);
        LinkLabel website=new LinkLabel();website.Text=L.T("打开官网下载页");website.AutoSize=true;website.LinkClicked+=delegate {try{Process.Start(new ProcessStartInfo(FfmpegSetup.Website){UseShellExecute=true});}catch(Exception e){message.Text=e.Message;}};layout.Controls.Add(website);
        progress.Dock=DockStyle.Fill;layout.Controls.Add(progress);message.Dock=DockStyle.Fill;message.AutoEllipsis=true;layout.Controls.Add(message);
        FlowLayoutPanel buttons=new FlowLayoutPanel();buttons.Dock=DockStyle.Fill;
        download.Text=L.T("下载并继续");cancelButton.Text=L.T("取消");
        foreach(Button button in new Button[]{download,cancelButton}) {button.Size=new Size(160,36);PlayerTheme.StyleButton(button,button==download);buttons.Controls.Add(button);}
        layout.Controls.Add(buttons);Controls.Add(layout);AcceptButton=download;CancelButton=cancelButton;
        download.Click+=async delegate {
            if(busy)return;busy=true;download.Enabled=false;closeWhenCancelled=false;cancellation=new CancellationTokenSource();
            Progress<Tuple<int,string>> updates=new Progress<Tuple<int,string>>(delegate(Tuple<int,string> update){if(IsDisposed || !busy)return;progress.Style=update.Item1<0?ProgressBarStyle.Marquee:ProgressBarStyle.Continuous;if(update.Item1>=0)progress.Value=update.Item1;message.Text=update.Item2;});
            bool completed=false;
            try {await Task.Run(delegate {FfmpegSetup.Install(delegate(int value,string text){((IProgress<Tuple<int,string>>)updates).Report(Tuple.Create(value,text));},cancellation.Token);});completed=true;}
            catch(OperationCanceledException){message.Text=L.T("下载已取消，可以重试。");}
            catch(Exception e){message.Text=L.T("下载失败，可重试或打开官网下载页手动下载：")+e.Message;}
            finally {busy=false;cancellation.Dispose();cancellation=null;download.Enabled=true;progress.Style=ProgressBarStyle.Continuous;}
            if(completed){DialogResult=DialogResult.OK;Close();}else if(closeWhenCancelled)Close();
        };
        cancelButton.Click+=delegate {if(busy){closeWhenCancelled=true;cancellation.Cancel();}else Close();};
        FormClosing+=delegate(object sender,FormClosingEventArgs e){if(busy){e.Cancel=true;closeWhenCancelled=true;cancellation.Cancel();}};
    }
}

internal sealed class Decoder : IDisposable
{
    Process process;
    StringBuilder errors = new StringBuilder();
    public static string Locate()
    {
        string[] known = { Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe"),
            FfmpegSetup.InstalledPath, @"C:\ProgramData\chocolatey\bin\ffmpeg.exe" };
        return Locate(known,Environment.GetEnvironmentVariable("PATH") ?? "");
    }
    internal static string Locate(string[] known,string searchPath)
    {
        foreach (string f in known) if (File.Exists(f)) return f;
        foreach (string dir in searchPath.Split(';')) {
            if(String.IsNullOrWhiteSpace(dir))continue;
            try { string f = Path.Combine(dir.Trim('"'), "ffmpeg.exe"); if (File.Exists(f)) return f; } catch { }
        }
        throw new FfmpegMissingException();
    }
    public static bool IsStill(string file)
    {
        return Array.IndexOf(new string[] { ".png", ".jpg", ".jpeg", ".bmp", ".webp", ".tif", ".tiff" }, Path.GetExtension(file).ToLowerInvariant()) >= 0;
    }
    public Decoder(string path, int fps, int fit, bool rotate, bool loop, bool realtime)
        : this(path, fps, fit, rotate, loop, realtime, 0) { }
    public Decoder(string path, int fps, int fit, bool rotate, bool loop, bool realtime, double start)
    {
        string filter = fit == 0 ? "scale=160:80:force_original_aspect_ratio=decrease,pad=160:80:(ow-iw)/2:(oh-ih)/2:color=black" :
            fit == 1 ? "scale=160:80:force_original_aspect_ratio=increase,crop=160:80" : "scale=160:80";
        if (!IsStill(path)) filter = "fps=" + fps.ToString(CultureInfo.InvariantCulture) + "," + filter;
        if (rotate) filter += ",hflip,vflip";
        bool still = IsStill(path);
        string input = (realtime && !still ? "-re " : "") + (loop && !still ? "-stream_loop -1 " : "");
        string arguments = "-hide_banner -loglevel error -nostdin " + input + (start > 0 ? "-ss " + start.ToString("0.###", CultureInfo.InvariantCulture) + " " : "") + "-i \"" + path + "\" -map 0:v:0 -an -sn -dn -vf \"" + filter +
            "\" " + (still ? "-frames:v 1 " : "") + "-pix_fmt rgb565be -threads 1 -f rawvideo pipe:1";
        ProcessStartInfo psi = new ProcessStartInfo(Locate(), arguments);
        psi.UseShellExecute = false; psi.CreateNoWindow = true; psi.RedirectStandardOutput = true; psi.RedirectStandardError = true;
        process = new Process(); process.StartInfo = psi;
        process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e) { if (e.Data != null) lock (errors) { if (errors.Length < 4000) errors.AppendLine(e.Data); } };
        process.Start(); process.BeginErrorReadLine();
    }
    public byte[] Next()
    {
        Process active = process;
        if (active == null) throw new OperationCanceledException();
        byte[] frame = new byte[25600]; int off = 0;
        while (off < frame.Length) {
            int n = active.StandardOutput.BaseStream.Read(frame, off, frame.Length - off);
            if (n == 0) {
                active.WaitForExit();
                if (active.ExitCode != 0 || off != 0) throw new IOException(L.T("媒体无法解码。") + Environment.NewLine + errors.ToString());
                return null;
            }
            off += n;
        }
        return frame;
    }
    public void Dispose()
    {
        Process active = Interlocked.Exchange(ref process, null);
        if (active != null) {
            try { if (!active.HasExited) active.Kill(); } catch { }
            try { active.StandardOutput.Close(); } catch { }
            active.Dispose();
        }
    }
}

internal static class PlayerTheme
{
    public static readonly Color Primary=Color.FromArgb(124,58,237), Hover=Color.FromArgb(109,40,217), Pressed=Color.FromArgb(91,33,182);
    public static readonly Color Canvas=Color.FromArgb(247,244,252), Text=Color.FromArgb(48,35,67), Muted=Color.FromArgb(127,113,141);
    public static readonly Color Border=Color.FromArgb(222,210,239), Soft=Color.FromArgb(242,235,252), Preview=Color.FromArgb(32,22,47);
    public static void StyleButton(Button button,bool primary)
    {
        button.FlatStyle=FlatStyle.Flat;button.UseVisualStyleBackColor=false;
        button.FlatAppearance.BorderColor=primary?Primary:Border;button.BackColor=primary?Primary:Color.White;button.ForeColor=primary?Color.White:Text;
        button.FlatAppearance.MouseOverBackColor=primary?Hover:Soft;button.FlatAppearance.MouseDownBackColor=primary?Pressed:Border;
    }
    public static void StyleInputs(Control root)
    {
        foreach(Control child in root.Controls) {
            CheckBox box=child as CheckBox;
            if(box!=null){box.FlatStyle=FlatStyle.Flat;box.ForeColor=Text;box.FlatAppearance.BorderColor=Primary;box.FlatAppearance.CheckedBackColor=Soft;box.FlatAppearance.MouseOverBackColor=Soft;}
            ComboBox combo=child as ComboBox;if(combo!=null){combo.ForeColor=Text;combo.BackColor=Color.White;}
            StyleInputs(child);
        }
    }
}

internal sealed class Preview : Control
{
    Bitmap frame;
    public Preview() { DoubleBuffered = true; BackColor = PlayerTheme.Preview; Dock = DockStyle.Fill; }
    public void SetFrame(byte[] bytes)
    {
        Bitmap next = new Bitmap(160, 80, PixelFormat.Format24bppRgb);
        BitmapData bd = next.LockBits(new Rectangle(0, 0, 160, 80), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
        byte[] bgr = new byte[bd.Stride * 80];
        for (int y = 0; y < 80; y++) for (int x = 0; x < 160; x++) {
            int i = (y * 160 + x) * 2; int v = (bytes[i] << 8) | bytes[i + 1]; int o = y * bd.Stride + x * 3;
            bgr[o] = (byte)((v & 31) * 255 / 31); bgr[o + 1] = (byte)(((v >> 5) & 63) * 255 / 63); bgr[o + 2] = (byte)(((v >> 11) & 31) * 255 / 31);
        }
        Marshal.Copy(bgr, 0, bd.Scan0, bgr.Length); next.UnlockBits(bd);
        Bitmap old = frame; frame = next; if (old != null) old.Dispose(); Invalidate();
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); int w = Math.Min(Width - 40, (Height - 40) * 2); w = Math.Max(w, 2);
        Rectangle rect = new Rectangle((Width - w) / 2, (Height - w / 2) / 2, w, w / 2);
        if (frame != null) { e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor; e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half; e.Graphics.DrawImage(frame, rect); }
        else TextRenderer.DrawText(e.Graphics, L.T(hint), Font, rect, Color.FromArgb(196,179,218), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
    protected override void Dispose(bool disposing) { if (disposing && frame != null) frame.Dispose(); base.Dispose(disposing); }
    string hint="选择媒体，开始预览";
    public void Hint(string text) { hint=L.Source(text); Bitmap old=frame;frame=null;if(old!=null)old.Dispose();Invalidate(); }
}

internal sealed class PlayerSettings
{
    public string Media = "", Port = "COM13";
    public string Language = L.DefaultLanguage;
    public int Fit = 0, Fps = 5;
    public bool Rotate = false, Loop = true;
    public int Mode=7, ClockStyle=1, Monitor=0;
    public bool Touch=true;
    public bool SilentStartup=false;
    public static string FilePath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"settings.xml"); } }
    public static PlayerSettings Load(string path)
    {
        PlayerSettings s = new PlayerSettings();
        if (!File.Exists(path)) return s;
        try {
            XmlReaderSettings safe = new XmlReaderSettings(); safe.DtdProcessing = DtdProcessing.Prohibit; safe.XmlResolver = null;
            XmlDocument doc = new XmlDocument(); doc.XmlResolver = null;
            using(XmlReader reader=XmlReader.Create(path,safe)) doc.Load(reader);
            XmlElement root=doc.DocumentElement;
            if(L.Supported(root.GetAttribute("language"))) s.Language=root.GetAttribute("language");
            s.Media=root.GetAttribute("media"); s.Port=root.GetAttribute("port");
            int number;
            if(Int32.TryParse(root.GetAttribute("fit"),out number) && number>=0 && number<=2) s.Fit=number;
            if(Int32.TryParse(root.GetAttribute("fps"),out number) && Array.IndexOf(new int[]{1,2,5,10,15},number)>=0) s.Fps=number;
            s.Rotate=root.GetAttribute("rotate")=="true"; s.Loop=root.GetAttribute("loop")!="false";
            if(Int32.TryParse(root.GetAttribute("state"),out number) && number>=0 && number<8) s.Mode=number;
            if(Int32.TryParse(root.GetAttribute("clock"),out number) && number>=0 && number<3) s.ClockStyle=number;
            if(Int32.TryParse(root.GetAttribute("monitor"),out number) && number>=0 && number<64) s.Monitor=number;
            s.Touch=root.GetAttribute("touch")!="false";
            s.SilentStartup=root.GetAttribute("silentStartup")=="true";
        } catch { }
        return s;
    }
    public void Save(string path)
    {
        string temp=path+".tmp";
        XmlWriterSettings format=new XmlWriterSettings(); format.Indent=true; format.Encoding=new UTF8Encoding(false);
        using(XmlWriter w=XmlWriter.Create(temp,format)) {
            w.WriteStartElement("player"); w.WriteAttributeString("language",Language); w.WriteAttributeString("media",Media ?? ""); w.WriteAttributeString("port",Port ?? "");
            w.WriteAttributeString("fit",Fit.ToString(CultureInfo.InvariantCulture)); w.WriteAttributeString("fps",Fps.ToString(CultureInfo.InvariantCulture));
            w.WriteAttributeString("rotate",Rotate ? "true":"false"); w.WriteAttributeString("loop",Loop ? "true":"false");
            w.WriteAttributeString("state",Mode.ToString());w.WriteAttributeString("clock",ClockStyle.ToString());w.WriteAttributeString("monitor",Monitor.ToString());w.WriteAttributeString("touch",Touch ? "true":"false");w.WriteAttributeString("silentStartup",SilentStartup ? "true":"false");w.WriteEndElement();
        }
        if(File.Exists(path)) File.Replace(temp,path,null); else File.Move(temp,path);
    }
}

internal static class StartupOption
{
    const string RunKey=@"Software\Microsoft\Windows\CurrentVersion\Run";
    const string ValueName="MSU2MiniPlayer";
    public static string Command(string exe) { return "\""+exe+"\" --startup"; }
    public static bool Enabled()
    {
        using(RegistryKey key=Registry.CurrentUser.OpenSubKey(RunKey))
            return key!=null && String.Equals(key.GetValue(ValueName) as string,Command(Application.ExecutablePath),StringComparison.OrdinalIgnoreCase);
    }
    public static void Set(bool enabled)
    {
        using(RegistryKey key=Registry.CurrentUser.CreateSubKey(RunKey)) {
            if(enabled) key.SetValue(ValueName,Command(Application.ExecutablePath),RegistryValueKind.String);
            else key.DeleteValue(ValueName,false);
        }
    }
}

internal sealed class Dashboard
{
    [StructLayout(LayoutKind.Sequential)] class MemoryInfo {
        public uint Length=(uint)Marshal.SizeOf(typeof(MemoryInfo)), Load;
        public ulong TotalPhysical,AvailablePhysical,TotalPage,AvailablePage,TotalVirtual,AvailableVirtual,AvailableExtended;
    }
    [DllImport("kernel32.dll")] static extern bool GlobalMemoryStatusEx([In,Out] MemoryInfo memory);
    [DllImport("kernel32.dll")] static extern bool GetSystemTimes(out ulong idle,out ulong kernel,out ulong user);
    ulong oldIdle,oldTotal; float cpu;
    long oldRx,oldTx; DateTime oldNetwork=DateTime.MinValue; double down,up;
    readonly List<double> history=new List<double>();
    static readonly Color Ink=Color.FromArgb(223,236,250), Muted=Color.FromArgb(129,153,180);
    public static byte[] Pixels(Bitmap image)
    {
        byte[] bytes=new byte[25600];
        for(int y=0;y<80;y++)for(int x=0;x<160;x++) {
            Color c=image.GetPixel(x,y); int v=((c.R>>3)<<11)|((c.G>>2)<<5)|(c.B>>3);int i=(y*160+x)*2;bytes[i]=(byte)(v>>8);bytes[i+1]=(byte)v;
        }
        return bytes;
    }
    void SampleCpu() {
        ulong idle,kernel,user;
        if(GetSystemTimes(out idle,out kernel,out user)) {
            ulong total=kernel+user;
            if(oldTotal!=0 && total>oldTotal) cpu=(float)Math.Max(0,Math.Min(100,100.0*(1.0-(idle-oldIdle)/(double)(total-oldTotal))));
            oldIdle=idle;oldTotal=total;
        }
    }
    void SampleNetwork() {
        long rx=0,tx=0;
        foreach(NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces()) {
            if(nic.OperationalStatus!=OperationalStatus.Up || (nic.NetworkInterfaceType!=NetworkInterfaceType.Ethernet && nic.NetworkInterfaceType!=NetworkInterfaceType.Wireless80211))continue;
            try { IPInterfaceStatistics stats=nic.GetIPStatistics();rx+=stats.BytesReceived;tx+=stats.BytesSent; }catch{}
        }
        DateTime now=DateTime.UtcNow;
        if(oldNetwork!=DateTime.MinValue) { double seconds=(now-oldNetwork).TotalSeconds; if(seconds>0){down=Math.Max(0,rx-oldRx)/seconds;up=Math.Max(0,tx-oldTx)/seconds;} }
        oldNetwork=now;oldRx=rx;oldTx=tx;history.Add(Math.Max(up,down));if(history.Count>76)history.RemoveAt(0);
    }
    static string Speed(double value) { return value>=1048576?(value/1048576).ToString("0.0")+" MB/s":value>=1024?(value/1024).ToString("0.0")+" KB/s":value.ToString("0")+" B/s"; }
    public byte[] Render(int mode,int clock,int monitor,bool turn,int fit)
    {
        using(Bitmap bitmap=new Bitmap(160,80,PixelFormat.Format24bppRgb)) {
            using(Graphics g=Graphics.FromImage(bitmap)) {
                g.Clear(Color.FromArgb(9,18,31));g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                using(Font small=new Font("Microsoft YaHei UI",10,FontStyle.Regular,GraphicsUnit.Pixel))
                using(Font number=new Font("Segoe UI",20,FontStyle.Bold,GraphicsUnit.Pixel))
                using(SolidBrush ink=new SolidBrush(Ink))using(SolidBrush muted=new SolidBrush(Muted)) {
                    if(mode==1 || mode==2) {
                        SampleCpu();MemoryInfo mem=new MemoryInfo();bool memOk=GlobalMemoryStatusEx(mem);
                        string disk="--",power="AC";
                        try { DriveInfo d=new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory));disk=(100.0*(d.TotalSize-d.AvailableFreeSpace)/d.TotalSize).ToString("0")+"%";}catch{}
                        PowerStatus ps=SystemInformation.PowerStatus;if(ps.BatteryChargeStatus!=BatteryChargeStatus.NoSystemBattery && ps.BatteryLifePercent>=0 && ps.BatteryLifePercent<=1)power=(ps.BatteryLifePercent*100).ToString("0")+"%";
                        string[] names={"CPU",L.T("内存"),L.T("电量 / 电源"),L.T("系统盘")};string[] values={cpu.ToString("0")+"%",memOk?mem.Load+"%":"--",power,disk};
                        using(SolidBrush accent=new SolidBrush(mode==1?Color.FromArgb(63,164,255):Color.FromArgb(255,97,111)))
                        using(Pen line=new Pen(Color.FromArgb(36,53,75))) {
                            g.DrawLine(line,80,3,80,77);g.DrawLine(line,3,40,157,40);
                            for(int i=0;i<4;i++){int x=(i%2)*80+5,y=(i/2)*40;g.DrawString(names[i],small,muted,x,y+1);g.DrawString(values[i],number,accent,x,y+12);}
                        }
                    } else if(mode==4) {
                        DateTime now=DateTime.Now;string time=now.ToString(clock==0?"HH:mm":clock==1?"HH:mm:ss":"HH:mm:ss.fff");
                        using(Font big=new Font("Consolas",clock==0?38:clock==1?28:19,FontStyle.Bold,GraphicsUnit.Pixel)) {
                            SizeF size=g.MeasureString(time,big);g.DrawString(time,big,ink,(160-size.Width)/2,18);
                        }
                        string date=now.ToString("yyyy-MM-dd ddd",CultureInfo.GetCultureInfo(L.Language));SizeF ds=g.MeasureString(date,small);g.DrawString(date,small,muted,(160-ds.Width)/2,61);
                    } else if(mode==6) {
                        SampleNetwork();g.DrawString(L.T("上传  ")+Speed(up),small,ink,5,3);g.DrawString(L.T("下载  ")+Speed(down),small,ink,5,20);
                        double max=1;foreach(double h in history)max=Math.Max(max,h);
                        using(Pen line=new Pen(Color.FromArgb(53,190,168),1.5f))for(int i=1;i<history.Count;i++)g.DrawLine(line,5+(i-1)*2,74-(float)(history[i-1]/max*29),5+i*2,74-(float)(history[i]/max*29));
                    } else if(mode==5) {
                        Screen[] screens=Screen.AllScreens;Rectangle area=screens[Math.Min(Math.Max(0,monitor),screens.Length-1)].Bounds;
                        using(Bitmap desktop=new Bitmap(area.Width,area.Height)) {
                            using(Graphics capture=Graphics.FromImage(desktop))capture.CopyFromScreen(area.Location,Point.Empty,area.Size);
                            double scale=fit==1?Math.Max(160.0/area.Width,80.0/area.Height):Math.Min(160.0/area.Width,80.0/area.Height);
                            int w=fit==2?160:(int)Math.Round(area.Width*scale),h=fit==2?80:(int)Math.Round(area.Height*scale);
                            g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;g.DrawImage(desktop,new Rectangle((160-w)/2,(80-h)/2,w,h));
                        }
                    }
                }
            }
            if(turn)bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
            return Pixels(bitmap);
        }
    }
}

internal static class WindowSignal
{
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int RegisterWindowMessage(string name);
    [DllImport("user32.dll")] static extern bool SendNotifyMessage(IntPtr window,int message,IntPtr wParam,IntPtr lParam);
    public static readonly int ShowMessage=RegisterWindowMessage("MSU2MiniPlayer_ShowWindow_v14");
    public static void ShowExisting() { SendNotifyMessage(new IntPtr(0xffff),ShowMessage,IntPtr.Zero,IntPtr.Zero); }
}

internal sealed class PlayerForm : Form
{
    ComboBox language = new ComboBox();
    bool changingLanguage,decoderMissing,ffmpegPromptActive;
    ComboBox ports = new ComboBox(), fit = new ComboBox(), fps = new ComboBox();
    Button connect, choose, play, stop, refresh, offline, prev, next;
    Label filename, status, info;
    CheckBox loop = new CheckBox(), rotate = new CheckBox(), startup = new CheckBox(), silent=new CheckBox();
    NotifyIcon tray;
    Icon playerIcon;
    bool suppressStartupShow, initialized, exitRequested;
    Bitmap headerLogo;
    Action initialize;
    ComboBox states=new ComboBox(), detail=new ComboBox();CheckBox touch=new CheckBox();
    readonly Dashboard dashboard=new Dashboard();
    volatile int currentMode=7,modeEpoch;
    volatile bool outputRotate,touchEnabled=true;
    bool changingMode;
    DateTime nextPanel=DateTime.MinValue;
    static string[] StateNames { get { return new string[]{L.T("0 · 屏幕内动图"),L.T("1 · 电脑状态 · 蓝色"),L.T("2 · 电脑状态 · 红色"),L.T("3 · 屏幕内照片"),L.T("4 · 时钟"),L.T("5 · 桌面投屏"),L.T("6 · 网速"),L.T("7 · 图片 / GIF / 视频")}; } }
    PlayerSettings settings = PlayerSettings.Load(PlayerSettings.FilePath);
    bool autoResume, settingStartup;
    DateTime nextAutoAttempt=DateTime.MinValue;
    Preview preview = new Preview();
    readonly object gate = new object();
    byte[] latest; long sequence, shown; bool ended;
    volatile bool sending, connected, closing;
    CancellationTokenSource linkCancel;
    Task linkTask;
    Decoder decoder; int generation;
    string media;
    long sentFrames, previousFrames; Stopwatch rate = Stopwatch.StartNew();
    System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
    public PlayerForm(string initial, bool startedAtLogin)
    {
        L.Language=settings.Language;
        suppressStartupShow=startedAtLogin && settings.SilentStartup;
        ShowInTaskbar=!suppressStartupShow;
        Text = L.T("MSU2 小屏播放器 v1.5.5"); ClientSize = new Size(1080, 780); MinimumSize = new Size(960, 680);
        using(Stream source=System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("player-icon.ico")) {
            if(source==null)playerIcon=(Icon)SystemIcons.Application.Clone();
            else using(Icon loaded=new Icon(source,new Size(32,32)))playerIcon=(Icon)loaded.Clone();
        }
        Icon=playerIcon;
        Font = new Font("Microsoft YaHei UI", 10); AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = PlayerTheme.Canvas; ForeColor = PlayerTheme.Text;
        BuildLayout();
        language.SelectedIndexChanged+=delegate { SwitchLanguage(); };
        PlayerTheme.StyleInputs(this);
        refresh.Click += delegate { RefreshPorts(); }; connect.Click += async delegate { if (connected) await Disconnect(); else Connect(); };
        choose.Click += delegate { Browse(); }; play.Click += delegate { Play(); }; stop.Click += delegate { Pause(); };
        prev.Click+=delegate { states.SelectedIndex=(states.SelectedIndex+7)%8; };next.Click+=delegate { states.SelectedIndex=(states.SelectedIndex+1)%8; };
        states.SelectedIndexChanged+=delegate { if(!changingLanguage) ChangeState(); };
        detail.SelectedIndexChanged+=delegate { if(changingMode || changingLanguage)return;if(currentMode==4)settings.ClockStyle=detail.SelectedIndex;if(currentMode==5)settings.Monitor=detail.SelectedIndex;nextPanel=DateTime.MinValue;SaveSettings(); };
        touch.CheckedChanged+=delegate { touchEnabled=touch.Checked;SaveSettings(); };
        startup.CheckedChanged += delegate {
            if(settingStartup) return;
            try {
                StartupOption.Set(startup.Checked); SaveSettings();
                silent.Enabled=startup.Checked;
                status.Text=startup.Checked ? L.T("已开启：登录 Windows 后自动实时播放上次的媒体") : L.T("已关闭开机启动");
            } catch(Exception e) {
                settingStartup=true; startup.Checked=!startup.Checked; settingStartup=false;
                status.Text=L.T("开机启动设置未保存：")+e.Message;
            }
        };
        silent.CheckedChanged+=delegate {SaveSettings();status.Text=silent.Checked?L.T("开机时隐藏窗口、在托盘运行并自动实时显示"):L.T("开机时最小化显示主窗口");};
        ports.SelectedIndexChanged += delegate { if(!changingLanguage && ports.SelectedItem!=null) SaveSettings(); };
        offline.Click += async delegate {
            if(currentMode!=7)return;
            if (media == null) { status.Text = L.T("请先选择图片、GIF 或视频。"); return; }
            if (ports.SelectedItem == null) { status.Text = L.T("请先选择屏幕端口。"); return; }
            string target = ports.Text; Pause(); await Disconnect();
            using (OfflineDialog d = new OfflineDialog(media, target, fit.SelectedIndex, rotate.Checked)) d.ShowDialog(this);
        };
        fit.SelectedIndexChanged += delegate { RestartPreview(); }; fps.SelectedIndexChanged += delegate { RestartPreview(); }; rotate.CheckedChanged += delegate { RestartPreview(); }; loop.CheckedChanged += delegate { RestartPreview(); };
        AllowDrop = currentMode==7; DragEnter += delegate(object s, DragEventArgs e) { e.Effect=currentMode==7 && e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None; };
        DragDrop += delegate(object s, DragEventArgs e) { if(currentMode!=7)return;string[] files = e.Data.GetData(DataFormats.FileDrop) as string[]; if (files != null && files.Length > 0) LoadFile(files[0]); };
        timer.Interval = 80; timer.Tick += delegate { TickPreview(); }; timer.Start();
        FormClosing += async delegate(object sender, FormClosingEventArgs e) {
            if(e.CloseReason==CloseReason.UserClosing && !exitRequested && !closing) {e.Cancel=true;HideToTray();return;}
            if (closing) return; e.Cancel = true; closing = true; Enabled = false; timer.Stop();
            Interlocked.Increment(ref generation); Decoder old = decoder; decoder = null; if (old != null) old.Dispose();
            await Disconnect(); Close();
        };
        SetupTray();
        FormClosed+=delegate {if(tray!=null){tray.Visible=false;if(tray.ContextMenuStrip!=null)tray.ContextMenuStrip.Dispose();tray.Dispose();}if(headerLogo!=null)headerLogo.Dispose();if(playerIcon!=null)playerIcon.Dispose();};
        RefreshPorts(); ChangeState(); UpdateButtons();
        initialize=delegate {
            string path=!String.IsNullOrEmpty(initial) ? initial : settings.Media;
            if(!String.IsNullOrEmpty(path)) {
                if(File.Exists(path)) {
                    if(String.IsNullOrEmpty(initial) && currentMode!=7) {media=Path.GetFullPath(path);filename.Text=Path.GetFileName(media);UpdateButtons();}
                    else LoadFile(path);
                }
                else status.Text=L.T("上次的媒体文件找不到，请重新选择。");
            }
            if(startedAtLogin) {
                autoResume=true; nextAutoAttempt=DateTime.MinValue;if(!suppressStartupShow)WindowState=FormWindowState.Minimized;
                if(media!=null || currentMode!=7) status.Text=L.T("开机实时显示：等待内容和屏幕就绪…");
                else status.Text=L.T("开机实时播放未开始：请先选择媒体文件。");
            }
        };
        Shown+=delegate {InitializeOnce();};
    }
    void BuildLayout()
    {
        TableLayoutPanel root=new TableLayoutPanel();root.Dock=DockStyle.Fill;root.Padding=new Padding(24,16,24,14);root.ColumnCount=1;root.RowCount=3;
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,92));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,32));Controls.Add(root);
        TableLayoutPanel heading=new TableLayoutPanel();heading.Dock=DockStyle.Fill;heading.ColumnCount=3;heading.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));heading.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,154));heading.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,76));
        TableLayoutPanel titles=new TableLayoutPanel();titles.Dock=DockStyle.Fill;titles.RowCount=2;titles.RowStyles.Add(new RowStyle(SizeType.Absolute,44));titles.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        Label title=Label(L.T("小屏播放器"),24,true);title.Dock=DockStyle.Fill;title.Margin=new Padding(0,0,0,0);titles.Controls.Add(title);
        Label subtitle=Label(L.T("实时显示 · 状态监测 · 离线播放   /   关闭窗口后继续在托盘运行"),9,false);subtitle.Dock=DockStyle.Fill;subtitle.Margin=new Padding(1,0,0,0);subtitle.ForeColor=PlayerTheme.Muted;titles.Controls.Add(subtitle);heading.Controls.Add(titles,0,0);
        // Use the original PNG for the header. Icon.ToBitmap on a large ICO frame
        // can yield corrupted pixels on .NET Framework; clone to own the image data.
        using(Stream source=System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("player-logo.png"))
        using(Image image=Image.FromStream(source))headerLogo=new Bitmap(image);
        PictureBox logo=new PictureBox();logo.Name="HeaderLogo";logo.AccessibleName=L.T("小屏精灵图标");logo.Image=headerLogo;logo.SizeMode=PictureBoxSizeMode.Zoom;logo.Size=new Size(64,64);logo.Anchor=AnchorStyles.Top|AnchorStyles.Right;logo.Margin=new Padding(6,0,0,0);heading.Controls.Add(logo,2,0);
        FlowLayoutPanel languages=new FlowLayoutPanel();languages.Dock=DockStyle.Fill;languages.FlowDirection=FlowDirection.TopDown;languages.WrapContents=false;languages.Margin=new Padding(0,4,0,0);
        Label languageLabel=Label("语言 / Language",9,false);languageLabel.Margin=new Padding(0,0,0,4);languages.Controls.Add(languageLabel);
        language.Name="LanguagePicker";language.DropDownStyle=ComboBoxStyle.DropDownList;language.Width=144;language.Items.AddRange(new object[]{"简体中文","English"});language.SelectedIndex=settings.Language=="en"?1:0;languages.Controls.Add(language);heading.Controls.Add(languages,1,0);root.Controls.Add(heading,0,0);
        TableLayoutPanel body=new TableLayoutPanel();body.Dock=DockStyle.Fill;body.ColumnCount=2;body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,320));body.Margin=Padding.Empty;root.Controls.Add(body,0,1);
        TableLayoutPanel left=new TableLayoutPanel();left.Dock=DockStyle.Fill;left.RowCount=2;left.RowStyles.Add(new RowStyle(SizeType.Percent,100));left.RowStyles.Add(new RowStyle(SizeType.Absolute,112));left.Margin=new Padding(0,0,18,0);body.Controls.Add(left,0,0);
        TableLayoutPanel previewCard=new TableLayoutPanel();previewCard.Dock=DockStyle.Fill;previewCard.BackColor=Color.White;previewCard.Padding=new Padding(14);previewCard.RowCount=2;previewCard.RowStyles.Add(new RowStyle(SizeType.Absolute,36));previewCard.RowStyles.Add(new RowStyle(SizeType.Percent,100));previewCard.Margin=new Padding(0,0,0,14);
        TableLayoutPanel previewTitle=new TableLayoutPanel();previewTitle.Dock=DockStyle.Fill;previewTitle.ColumnCount=2;previewTitle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));previewTitle.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,112));
        Label live=Label(L.T("实时预览"),11,true);live.Margin=Padding.Empty;previewTitle.Controls.Add(live,0,0);Label resolution=Label("160 × 80",9,false);resolution.Dock=DockStyle.Fill;resolution.TextAlign=ContentAlignment.TopRight;resolution.Margin=Padding.Empty;resolution.ForeColor=PlayerTheme.Muted;previewTitle.Controls.Add(resolution,1,0);previewCard.Controls.Add(previewTitle,0,0);preview.Margin=Padding.Empty;previewCard.Controls.Add(preview,0,1);left.Controls.Add(previewCard,0,0);
        TableLayoutPanel mediaCard=new TableLayoutPanel();mediaCard.Dock=DockStyle.Fill;mediaCard.BackColor=Color.White;mediaCard.Padding=new Padding(14,12,8,12);mediaCard.ColumnCount=3;mediaCard.RowCount=2;mediaCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));mediaCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,124));mediaCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,124));mediaCard.RowStyles.Add(new RowStyle(SizeType.Absolute,25));mediaCard.RowStyles.Add(new RowStyle(SizeType.Percent,100));mediaCard.Margin=Padding.Empty;
        Label mediaTitle=Label(L.T("媒体文件"),10,true);mediaTitle.Margin=Padding.Empty;mediaCard.Controls.Add(mediaTitle,0,0);
        filename=Label(L.T("拖入图片、GIF 或视频"),9,false);filename.Name="MediaFilename";filename.AutoSize=false;filename.AutoEllipsis=true;filename.Dock=DockStyle.Fill;filename.Margin=new Padding(0,0,8,0);filename.ForeColor=PlayerTheme.Muted;filename.TextAlign=ContentAlignment.MiddleLeft;mediaCard.Controls.Add(filename,0,1);
        choose=Button(L.T("选择媒体…"),116,false);offline=Button(L.T("保存到屏幕…"),116,false);choose.Anchor=offline.Anchor=AnchorStyles.None;choose.Margin=offline.Margin=new Padding(0,0,6,0);mediaCard.Controls.Add(choose,1,0);mediaCard.SetRowSpan(choose,2);mediaCard.Controls.Add(offline,2,0);mediaCard.SetRowSpan(offline,2);left.Controls.Add(mediaCard,0,1);
        TableLayoutPanel right=new TableLayoutPanel();right.Dock=DockStyle.Fill;right.BackColor=Color.White;right.Margin=Padding.Empty;right.RowCount=2;right.RowStyles.Add(new RowStyle(SizeType.Percent,100));right.RowStyles.Add(new RowStyle(SizeType.Absolute,54));body.Controls.Add(right,1,0);
        FlowLayoutPanel side=new FlowLayoutPanel();side.Dock=DockStyle.Fill;side.BackColor=Color.White;side.Padding=new Padding(16,10,12,8);side.FlowDirection=FlowDirection.TopDown;side.WrapContents=false;side.AutoScroll=true;side.Margin=Padding.Empty;right.Controls.Add(side,0,0);
        side.Controls.Add(Section(L.T("屏幕连接")));FlowLayoutPanel connection=SideRow();ports.DropDownStyle=ComboBoxStyle.DropDownList;ports.Width=194;ports.Margin=Padding.Empty;connection.Controls.Add(ports);refresh=Button(L.T("刷新"),74,false);refresh.Margin=new Padding(8,0,0,0);connection.Controls.Add(refresh);side.Controls.Add(connection);connect=Button(L.T("连接屏幕"),276,false);connect.Margin=new Padding(0,0,0,10);side.Controls.Add(connect);
        side.Controls.Add(Section(L.T("功能状态")));states.Font=Font;states.DropDownStyle=ComboBoxStyle.DropDownList;states.Width=276;states.Items.AddRange(StateNames);states.SelectedIndex=settings.Mode;currentMode=settings.Mode;states.Margin=new Padding(0,0,0,12);side.Controls.Add(states);
        FlowLayoutPanel switchRow=SideRow();switchRow.Margin=new Padding(0,6,0,10);prev=Button(L.T("上一项"),134,false);next=Button(L.T("下一项"),134,false);switchRow.Controls.Add(prev);switchRow.Controls.Add(next);side.Controls.Add(switchRow);
        detail.Font=Font;detail.DropDownStyle=ComboBoxStyle.DropDownList;detail.Width=276;detail.Margin=new Padding(0,0,0,10);side.Controls.Add(detail);
        touch.Text=L.T("轻触屏幕切换状态");touch.Checked=settings.Touch;touchEnabled=touch.Checked;touch.AutoSize=true;touch.Margin=new Padding(0,0,0,10);side.Controls.Add(touch);
        side.Controls.Add(Section(L.T("显示选项")));TableLayoutPanel display=new TableLayoutPanel();display.Size=new Size(276,36);display.Margin=new Padding(0,0,0,5);display.Padding=Padding.Empty;display.ColumnCount=4;display.RowCount=1;
        display.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,178));display.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,8));display.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,54));display.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));display.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        fit.Font=Font;fit.DropDownStyle=ComboBoxStyle.DropDownList;fit.Margin=Padding.Empty;fit.Anchor=AnchorStyles.Left|AnchorStyles.Right;fit.Items.AddRange(new object[]{L.T("完整显示 / 留边"),L.T("铺满屏幕 / 裁切"),L.T("拉伸铺满")});fit.SelectedIndex=settings.Fit;display.Controls.Add(fit,0,0);
        fps.Font=Font;fps.DropDownStyle=ComboBoxStyle.DropDownList;fps.Margin=Padding.Empty;fps.Anchor=AnchorStyles.Left|AnchorStyles.Right;fps.Items.AddRange(new object[]{"1","2","5","10","15"});fps.SelectedItem=settings.Fps.ToString();display.Controls.Add(fps,2,0);Label fpsLabel=Label("fps",9,false);fpsLabel.AutoSize=false;fpsLabel.Dock=DockStyle.Fill;fpsLabel.TextAlign=ContentAlignment.MiddleLeft;fpsLabel.Margin=new Padding(6,0,0,0);display.Controls.Add(fpsLabel,3,0);side.Controls.Add(display);
        FlowLayoutPanel toggles=SideRow();toggles.Height=30;loop.Text=L.T("循环播放");loop.Checked=settings.Loop;loop.AutoSize=true;loop.Margin=new Padding(0,1,18,0);toggles.Controls.Add(loop);rotate.Text=L.T("旋转 180°");rotate.Checked=settings.Rotate;rotate.AutoSize=true;rotate.Margin=new Padding(0,1,0,0);toggles.Controls.Add(rotate);side.Controls.Add(toggles);
        side.Controls.Add(Section(L.T("启动设置")));startup.Text=L.T("开机启动");startup.AutoSize=true;startup.Margin=new Padding(0,0,0,8);startup.Checked=StartupOption.Enabled();side.Controls.Add(startup);
        silent.Text=L.T("开机静默运行到托盘");silent.AutoSize=true;silent.Margin=new Padding(0,0,0,14);silent.Checked=settings.SilentStartup;silent.Enabled=startup.Checked;side.Controls.Add(silent);
        FlowLayoutPanel actions=SideRow();actions.Dock=DockStyle.Fill;actions.Margin=new Padding(16,2,12,8);play=Button(L.T("开始显示"),174,true);stop=Button(L.T("暂停"),94,false);actions.Controls.Add(play);actions.Controls.Add(stop);right.Controls.Add(actions,0,1);
        TableLayoutPanel footer=new TableLayoutPanel();footer.Dock=DockStyle.Fill;footer.ColumnCount=2;footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,245));footer.Margin=new Padding(0,7,0,0);
        status=Label(L.T("先选择功能状态，再连接屏幕"),9,false);status.AutoSize=false;status.AutoEllipsis=true;status.Dock=DockStyle.Fill;status.Margin=Padding.Empty;status.ForeColor=PlayerTheme.Muted;info=Label(L.T("本地预览"),9,false);info.Dock=DockStyle.Fill;info.TextAlign=ContentAlignment.TopRight;info.Margin=Padding.Empty;info.ForeColor=status.ForeColor;footer.Controls.Add(status,0,0);footer.Controls.Add(info,1,0);root.Controls.Add(footer,0,2);
    }
    Label Section(string text) {Label label=Label(text,10,true);label.AutoSize=false;label.Size=new Size(276,24);label.Margin=new Padding(0,4,0,5);return label;}
    FlowLayoutPanel SideRow(){FlowLayoutPanel row=new FlowLayoutPanel();row.Size=new Size(276,36);row.Padding=Padding.Empty;row.Margin=new Padding(0,0,0,5);row.WrapContents=false;return row;}
    void InitializeOnce() {if(initialized)return;initialized=true;if(initialize!=null)initialize();}
    protected override void SetVisibleCore(bool value)
    {
        if(value && suppressStartupShow) {
            if(!IsHandleCreated)CreateHandle();
            base.SetVisibleCore(false);
            if(!initialized)BeginInvoke((Action)InitializeOnce);
            return;
        }
        base.SetVisibleCore(value);
    }
    protected override void WndProc(ref System.Windows.Forms.Message m)
    {
        if(WindowSignal.ShowMessage!=0 && m.Msg==WindowSignal.ShowMessage && !closing)ShowMainWindow();
        base.WndProc(ref m);
    }
    void ShowMainWindow() {
        if(closing)return;suppressStartupShow=false;ShowInTaskbar=true;Show();WindowState=FormWindowState.Normal;Activate();
        if(decoderMissing && !ffmpegPromptActive)RestartPreview();
    }
    void HideToTray() {
        if(closing)return;
        // ShowInTaskbar recreates the native handle. Hide first so that handle
        // recreation cannot briefly show and repaint the still-visible form.
        Hide();ShowInTaskbar=false;if(tray!=null)tray.Visible=true;
    }
    void RequestExit() {
        foreach(Form child in OwnedForms) {OfflineDialog offlineDialog=child as OfflineDialog;if(offlineDialog!=null && offlineDialog.Busy){ShowMainWindow();child.Activate();status.Text=L.T("离线操作尚未完成，请等待后从托盘退出。");return;}}
        exitRequested=true;Close();
    }
    void SetupTray()
    {
        tray=new NotifyIcon();tray.Icon=playerIcon;tray.Text=L.T("MSU2 小屏播放器");
        ContextMenuStrip menu=new ContextMenuStrip();menu.Items.Add(L.T("打开主窗口"),null,delegate {ShowMainWindow();});
        menu.Items.Add(L.T("暂停 / 继续显示"),null,delegate {if(sending)Pause();else Play();});
        menu.Items.Add(new ToolStripSeparator());menu.Items.Add(L.T("退出播放器"),null,delegate {RequestExit();});
        tray.ContextMenuStrip=menu;tray.DoubleClick+=delegate {ShowMainWindow();};tray.Visible=true;
    }
    Label Label(string text, float size, bool bold)
    {
        Label l = new Label(); l.Text = text; l.Font = new Font("Microsoft YaHei UI", size, bold ? FontStyle.Bold : FontStyle.Regular);
        l.AutoSize = true; l.Margin = new Padding(0, 7, 8, 0); return l;
    }
    Button Button(string text, int width, bool primary)
    {
        Button b = new Button(); b.Text = text; b.Width = width; b.Height = 34; b.Margin = new Padding(0, 0, 8, 0); b.FlatStyle = FlatStyle.Flat;
        PlayerTheme.StyleButton(b,primary);return b;
    }
    FlowLayoutPanel Flow() { FlowLayoutPanel p = new FlowLayoutPanel(); p.Dock = DockStyle.Fill; p.Padding = new Padding(0, 5, 0, 0); return p; }
    void UI(Action action) { if (!IsDisposed && !closing && IsHandleCreated) try { BeginInvoke(action); } catch (InvalidOperationException) { } }
    void Message(string text) { UI(delegate { status.Text = text; }); }
    void UpdateButtons()
    {
        choose.Enabled=currentMode==7;
        offline.Enabled=currentMode==7 && media!=null;
        AllowDrop=currentMode==7;
        play.Enabled = (media != null || currentMode!=7) && !sending; stop.Enabled = sending; connect.Text = connected ? L.T("断开连接") : L.T("连接屏幕");
        ports.Enabled = !connected && linkTask == null; refresh.Enabled = ports.Enabled;
        fit.Enabled = fps.Enabled = loop.Enabled = !sending;rotate.Enabled=true;
    }
    void ChangeState()
    {
        if(states.SelectedIndex<0)return;
        currentMode=states.SelectedIndex;outputRotate=rotate.Checked;modeEpoch++;
        int id=Interlocked.Increment(ref generation);Decoder old=decoder;decoder=null;if(old!=null)old.Dispose();
        lock(gate){latest=null;ended=false;sequence++;} shown=-1;nextPanel=DateTime.MinValue;
        changingMode=true;detail.Items.Clear();
        if(currentMode==4){detail.Items.AddRange(new object[]{L.T("时 : 分"),L.T("时 : 分 : 秒"),L.T("时 : 分 : 秒 . 毫秒")});detail.SelectedIndex=settings.ClockStyle;}
        if(currentMode==5){Screen[] screens=Screen.AllScreens;for(int i=0;i<screens.Length;i++)detail.Items.Add(L.T("显示器 ")+(i+1)+" · "+screens[i].Bounds.Width+"×"+screens[i].Bounds.Height);detail.SelectedIndex=Math.Min(settings.Monitor,screens.Length-1);}
        detail.Visible=currentMode==4||currentMode==5;changingMode=false;
        if(currentMode==0)preview.Hint(L.T("屏幕内置动图\n直接读取屏幕已存储的 36 帧"));
        else if(currentMode==3)preview.Hint(L.T("屏幕内置照片\n直接显示屏幕照片槽位"));
        else if(currentMode==7){preview.Hint(L.T("选择图片、GIF 或视频"));if(media!=null)RestartPreview();}
        else preview.Hint(L.T("正在生成预览…"));
        SaveSettings();UpdateButtons();status.Text=(sending?L.T("正在显示："):L.T("已选择："))+StateNames[currentMode];
    }
    void SwitchLanguage()
    {
        if(changingLanguage || language.SelectedIndex<0)return;
        string previous=L.Language;
        string selected=language.SelectedIndex==1?"en":"zh-CN";
        if(previous==selected)return;
        changingLanguage=true;SuspendLayout();
        try {
            settings.Language=selected;L.Language=selected;
            L.Apply(this,previous);
            if(media==null)filename.Text=L.T("拖入图片、GIF 或视频");
            // Monitor labels combine a resource with a resolution.
            if(currentMode==5) {
                int index=detail.SelectedIndex;detail.Items.Clear();
                Screen[] screens=Screen.AllScreens;
                for(int i=0;i<screens.Length;i++)detail.Items.Add(L.T("显示器 ")+(i+1)+" · "+screens[i].Bounds.Width+"×"+screens[i].Bounds.Height);
                detail.SelectedIndex=index;
            }
            foreach(ToolStripItem item in tray.ContextMenuStrip.Items) item.Text=L.Retext(item.Text,previous);
            tray.Text=L.T("MSU2 小屏播放器");
            status.Text=(sending?L.T("正在显示："):L.T("已选择："))+StateNames[currentMode];
            info.Text=connected?L.T("已连接"):L.T("本地预览 · 160 × 80");
            nextPanel=DateTime.MinValue;rate.Restart();UpdateButtons();SaveSettings();
        } finally { ResumeLayout(true);changingLanguage=false; }
    }
    void RefreshPorts()
    {
        string current = ports.Text; ports.Items.Clear(); ports.Items.AddRange(ScreenPort.FindPorts());
        if (ports.Items.Contains(current)) ports.SelectedItem = current;
        else if (ports.Items.Contains(settings.Port)) ports.SelectedItem = settings.Port;
        else if (ports.Items.Contains("COM13")) ports.SelectedItem = "COM13"; else if (ports.Items.Count > 0) ports.SelectedIndex = 0;
    }
    void SaveSettings()
    {
        try {
            settings.Media=media ?? settings.Media;
            if(ports.SelectedItem!=null) settings.Port=ports.Text;
            settings.Fit=fit.SelectedIndex; int value; if(Int32.TryParse(fps.Text,out value)) settings.Fps=value;
            settings.Rotate=rotate.Checked; settings.Loop=loop.Checked;
            settings.SilentStartup=silent.Checked;
            settings.Mode=currentMode;settings.Touch=touch.Checked;settings.Save(PlayerSettings.FilePath);
        } catch(Exception e) { Log("Settings: "+e); }
    }
    void Browse()
    {
        if(currentMode!=7)return;
        using (OpenFileDialog d = new OpenFileDialog()) {
            d.Title = L.T("选择图片、GIF 或视频");
            d.Filter = L.T("图片、GIF 和视频") + "|" + "*.png;*.jpg;*.jpeg;*.bmp;*.webp;*.tif;*.tiff;*.gif;*.mp4;*.mkv;*.mov;*.avi;*.webm;*.wmv;*.m4v;*.ts" + "|" + L.T("所有文件") + "|" + "*.*";
            if (d.ShowDialog(this) == DialogResult.OK && currentMode==7) LoadFile(d.FileName);
        }
    }
    void LoadFile(string path)
    {
        if (!File.Exists(path)) { status.Text = L.T("找不到选择的文件。"); return; }
        Pause(); media = Path.GetFullPath(path); filename.Text = Path.GetFileName(media);
        if(states.SelectedIndex!=7)states.SelectedIndex=7;else RestartPreview(); UpdateButtons();
    }
    void RestartPreview()
    {
        if(changingLanguage)return;
        SaveSettings();
        outputRotate=rotate.Checked;
        if(currentMode!=7){modeEpoch++;nextPanel=DateTime.MinValue;return;}
        if (media == null || closing) return;
        int id = Interlocked.Increment(ref generation); Decoder old = decoder; decoder = null; if (old != null) old.Dispose();
        lock (gate) { latest = null; ended = false; sequence++; }
        if(!EnsureDecoder())return;
        int targetFps = Int32.Parse(fps.Text); int mode = fit.SelectedIndex; bool turn = rotate.Checked, repeat = loop.Checked; string path = media;
        status.Text = L.T("正在载入媒体…");
        Task.Run(delegate {
            Decoder active = null;
            try {
                active = new Decoder(path, targetFps, mode, turn, repeat, true);
                lock (gate) { if (id == generation) decoder = active; }
                while (id == generation && !closing) {
                    byte[] frame = active.Next(); if (frame == null) break;
                    lock (gate) { if (id != generation) break; latest = frame; sequence++; }
                }
                if (id == generation) { lock (gate) { ended = true; } Message(L.T("媒体已就绪")); }
            } catch (FfmpegMissingException) {UI(delegate {if(id==generation && !closing)RestartPreview();});}
            catch (Exception e) { if (id == generation && !closing) { lock (gate) { ended = true; } Message(L.T("载入失败：") + e.Message.Split('\n')[0]); Log(e.ToString()); } }
            finally { if (active != null) active.Dispose(); lock (gate) { if (decoder == active) decoder = null; } }
        });
    }
    bool EnsureDecoder()
    {
        try {Decoder.Locate();decoderMissing=false;return true;}catch(FfmpegMissingException) {decoderMissing=true;}
        if(ffmpegPromptActive)return false;
        if(suppressStartupShow || !Visible) {
            status.Text=L.T("缺少 FFmpeg，请打开主窗口下载后继续。");
            if(tray!=null)tray.ShowBalloonTip(6000,L.T("需要 FFmpeg"),status.Text,ToolTipIcon.Info);
            return false;
        }
        ffmpegPromptActive=true;
        try {
            if(FfmpegDownloadDialog.Ensure(this)){decoderMissing=false;return true;}
            status.Text=L.T("未安装 FFmpeg，选择媒体或点击开始显示可重试。");preview.Hint(status.Text);return false;
        } finally {ffmpegPromptActive=false;}
    }
    void Connect()
    {
        if (linkTask != null) return;
        if (ports.SelectedItem == null) { status.Text = L.T("未找到 MSU2 屏幕，请插入屏幕后刷新。"); return; }
        string name = ports.Text; status.Text = L.T("正在连接 ") + name + "…";
        linkCancel = new CancellationTokenSource(); CancellationToken token = linkCancel.Token;
        linkTask = Task.Run(delegate {
            try {
                using (ScreenPort device = new ScreenPort(name)) {
                    device.TouchReleased+=delegate { if(touchEnabled)UI(delegate { states.SelectedIndex=(states.SelectedIndex+1)%8; }); };
                    connected = true; UI(delegate { status.Text = name + L.T(" 已连接 · 160 × 80"); UpdateButtons(); });
                    long last = -1;
                    int lastEpoch=-1,gif=0;DateTime nextStored=DateTime.MinValue;
                    while (!token.IsCancellationRequested) {
                        int mode=currentMode;
                        if(lastEpoch!=modeEpoch) { lastEpoch=modeEpoch;device.Orientation((mode==0||mode==3)&&outputRotate);gif=0;last=-1;nextStored=DateTime.MinValue; }
                        if(sending && (mode==0||mode==3)) {
                            if(DateTime.UtcNow>=nextStored) {
                                device.ShowStoredPhoto(mode==0?(gif++%36)*100:3926);Interlocked.Increment(ref sentFrames);
                                nextStored=mode==0?DateTime.UtcNow.AddMilliseconds(100):DateTime.MaxValue;
                            }
                            device.KeepAlive();token.WaitHandle.WaitOne(15);continue;
                        }
                        byte[] frame; long seq; bool done;
                        lock (gate) { frame = latest; seq = sequence; done = ended; }
                        if (sending && frame != null && seq != last) { device.SendFrame(frame, token); last = seq; Interlocked.Increment(ref sentFrames); }
                        else { device.KeepAlive(); token.WaitHandle.WaitOne(140); }
                        if (mode==7 && sending && done && frame != null && last == seq) { sending = false; UI(delegate { status.Text = L.T("播放完成，屏幕保留当前画面"); UpdateButtons(); }); }
                    }
                }
            } catch (OperationCanceledException) { }
            catch (Exception e) { if (!token.IsCancellationRequested) { Message(L.T("连接失败：") + e.Message); Log(e.ToString()); } }
            finally { connected = false; sending = false; UI(delegate { linkTask = null; UpdateButtons(); }); }
        }); UpdateButtons();
    }
    async Task Disconnect()
    {
        autoResume=false;
        sending = false; if (linkCancel != null) linkCancel.Cancel(); Task task = linkTask;
        if (task != null) await task; linkTask = null; connected = false;
        if (linkCancel != null) { linkCancel.Dispose(); linkCancel = null; }
        if (!closing) { status.Text = L.T("屏幕已断开"); UpdateButtons(); }
    }
    void Play()
    {
        if(currentMode==7 && decoderMissing){RestartPreview();return;}
        if (!connected && ports.SelectedItem == null) { status.Text = L.T("请插入屏幕后刷新并选择端口。"); return; }
        lock (gate) { if (latest == null && currentMode!=0 && currentMode!=3) { status.Text = L.T("请等待内容载入完成。"); return; } }
        if (currentMode==7 && ended && !Decoder.IsStill(media)) RestartPreview();
        sending = true; status.Text = L.T("正在发送，暂停后可调整画面");
        if (!connected) Connect(); UpdateButtons();
    }
    void Pause() { autoResume=false; sending = false; status.Text = connected ? L.T("已暂停发送，屏幕保留当前画面") : L.T("本地预览"); UpdateButtons(); }
    void TickPreview()
    {
        if(currentMode!=0 && currentMode!=3 && currentMode!=7 && DateTime.UtcNow>=nextPanel) {
            nextPanel=DateTime.UtcNow.AddMilliseconds(currentMode==1||currentMode==2||currentMode==6?1000:Math.Max(100,1000/settings.Fps));
            try {
                byte[] panel=dashboard.Render(currentMode,settings.ClockStyle,settings.Monitor,rotate.Checked,fit.SelectedIndex);
                lock(gate){latest=panel;sequence++;ended=false;}
            } catch(Exception e){status.Text=L.T("预览失败：")+e.Message;nextPanel=DateTime.UtcNow.AddSeconds(3);}
        }
        byte[] pixels = null; long seq;
        lock (gate) { seq = sequence; if (seq != shown && latest != null) pixels = latest; }
        if (pixels != null) { preview.SetFrame(pixels); shown = seq; if (status.Text == L.T("正在载入媒体…")) status.Text = L.T("预览就绪，点击发送 / 播放"); }
        if(autoResume && !connected && linkTask==null && (media!=null || currentMode!=7) && DateTime.UtcNow>=nextAutoAttempt) {
            bool ready; lock(gate) { ready=latest!=null || currentMode==0 || currentMode==3; }
            if(ready) {
                nextAutoAttempt=DateTime.UtcNow.AddSeconds(5); RefreshPorts();
                if(ports.SelectedItem!=null) { sending=true; Connect(); }
                else status.Text=L.T("开机实时播放：等待屏幕连接，正在自动重试…");
            }
        }
        if (rate.ElapsedMilliseconds > 1000) {
            long frames = Interlocked.Read(ref sentFrames); double speed = (frames - previousFrames) * 1000.0 / rate.ElapsedMilliseconds;
            info.Text = connected ? L.T("已发送 ") + frames + L.T(" 帧  ·  ") + speed.ToString("0.0") + " fps" : L.T("本地预览 · 160 × 80");
            previousFrames = frames; rate.Restart();
            if(tray!=null)tray.Text="MSU2 · "+(connected?L.T("已连接"):L.T("等待连接"))+" · "+StateNames[currentMode];
        }
    }
    static readonly object logGate=new object();
    static void Log(string text) {
        try { lock(logGate) {
            string path=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"运行记录.log");
            string line=DateTime.Now.ToString("s")+" "+text+Environment.NewLine;
            if(File.Exists(path) && new FileInfo(path).Length>1048576) File.WriteAllText(path,line); else File.AppendAllText(path,line);
        } } catch { }
    }
}

internal static class OfflineClip
{
    public static byte[] Create(string path, int fit, bool turn, double start)
    {
        byte[] result = new byte[921600];
        using (Decoder d = new Decoder(path, 10, fit, turn, true, false, start)) {
            byte[] first = null;
            for (int i = 0; i < 36; i++) {
                byte[] frame = Decoder.IsStill(path) && first != null ? first : d.Next();
                if (frame == null) throw new IOException(L.T("素材不足以生成动画，请选择其他开始位置。"));
                if (first == null) first = frame;
                Buffer.BlockCopy(frame, 0, result, i * 25600, 25600);
            }
        }
        return result;
    }
}

internal sealed class OfflineDialog : Form
{
    public bool Busy {get{return busy;}}
    readonly string media, port;
    readonly int fit;
    readonly bool rotate;
    byte[] prepared;
    bool busy;
    volatile bool writing;
    CancellationTokenSource cancel;
    Label message = new Label(), summary = new Label();
    NumericUpDown start = new NumericUpDown();
    Button prepare = new Button(), save = new Button(), restore = new Button(), cancelButton = new Button();
    ProgressBar progress = new ProgressBar();
    Preview preview = new Preview();
    System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
    int frameIndex;
    public OfflineDialog(string path, string target, int crop, bool turn)
    {
        media = path; port = target; fit = crop; rotate = turn;
        Text = L.T("保存到屏幕 · 离线播放"); ClientSize = new Size(680, 560); MinimumSize = new Size(680, 560);
        StartPosition = FormStartPosition.CenterParent; Font = new Font("Microsoft YaHei UI", 10);
        BackColor = PlayerTheme.Canvas; ForeColor = PlayerTheme.Text;
        TableLayoutPanel root = new TableLayoutPanel(); root.Dock = DockStyle.Fill; root.Padding = new Padding(24); root.ColumnCount = 1; root.RowCount = 7;
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Percent,100)); root.RowStyles.Add(new RowStyle(SizeType.Absolute,40));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,36)); root.RowStyles.Add(new RowStyle(SizeType.Absolute,54)); root.RowStyles.Add(new RowStyle(SizeType.Absolute,48));
        summary.Text = L.T("关闭软件也能播放，屏幕需保持 USB 供电。\r\n图片会固定显示；GIF / 视频保存为 36 帧、3.6 秒的循环动画。");
        summary.Dock = DockStyle.Fill; root.Controls.Add(summary,0,0);
        FlowLayoutPanel seek = new FlowLayoutPanel(); seek.Dock = DockStyle.Fill;
        seek.Controls.Add(new Label { Text = L.T("截取开始（秒）"), AutoSize = true, Margin = new Padding(0,6,8,0) });
        start.DecimalPlaces = 1; start.Maximum = 86400; start.Width = 110; start.Enabled = !Decoder.IsStill(media); seek.Controls.Add(start);
        start.ValueChanged += delegate { prepared = null; save.Enabled = false; }; root.Controls.Add(seek,0,1);
        root.Controls.Add(preview,0,2);
        Label note = new Label(); note.Text = L.T("将替换原有离线动画；写入前自动备份，完成后逐字节校验。"); note.Dock = DockStyle.Fill; note.TextAlign = ContentAlignment.MiddleLeft; root.Controls.Add(note,0,3);
        progress.Dock = DockStyle.Fill; progress.Margin = new Padding(0,6,0,6); root.Controls.Add(progress,0,4);
        FlowLayoutPanel buttons = new FlowLayoutPanel(); buttons.Dock = DockStyle.Fill;
        prepare.Text = L.T("生成并预览"); save.Text = L.T("写入屏幕"); restore.Text = L.T("恢复备份…"); cancelButton.Text = L.T("取消备份");
        foreach (Button b in new Button[]{prepare, save, restore, cancelButton}) { b.Width = 125; b.Height = 36;PlayerTheme.StyleButton(b,b==save);buttons.Controls.Add(b); }
        save.Enabled = false; cancelButton.Enabled = false; root.Controls.Add(buttons,0,5);
        message.Text = Path.GetFileName(media); message.AutoEllipsis = true; message.Dock = DockStyle.Fill; root.Controls.Add(message,0,6); Controls.Add(root);
        prepare.Click += async delegate {
            if(busy || !FfmpegDownloadDialog.Ensure(this)) return; SetBusy(true); message.Text = L.T("正在生成离线动画…"); double offset = (double)start.Value;
            try { prepared = await Task.Run(delegate { return OfflineClip.Create(media,fit,rotate,offset); }); frameIndex=0; message.Text = L.T("预览已生成：160 × 80 · 36 帧 · 3.6 秒"); }
            catch(Exception e) { prepared = null; message.Text = L.T("生成失败：")+e.Message; }
            finally { SetBusy(false); }
        };
        save.Click += async delegate { if(prepared != null) await WriteAnimation(prepared); };
        restore.Click += async delegate {
            using(OpenFileDialog dialog = new OpenFileDialog()) {
                dialog.Title = L.T("选择自动备份的原离线动画.bin"); dialog.Filter = L.T("离线动画备份") + "|" + "*.bin";
                if(dialog.ShowDialog(this) != DialogResult.OK) return;
                try { byte[] data = File.ReadAllBytes(dialog.FileName); if(data.Length != 921600) throw new IOException(L.T("备份必须是 921600 字节的离线动画文件。")); prepared=data; frameIndex=0; await WriteAnimation(data); }
                catch(Exception e) { message.Text = L.T("恢复失败：")+e.Message; }
            }
        };
        cancelButton.Click += delegate { if(!writing && cancel != null) { cancel.Cancel(); message.Text = L.T("正在停止备份，尚未改动屏幕内容…"); } };
        FormClosing += delegate(object s,FormClosingEventArgs e) { if(busy) { e.Cancel=true; if(!writing && cancel!=null) cancel.Cancel(); else message.Text=L.T("正在写入或校验，请保持连接，等待完成后关闭。"); } };
        timer.Interval=100; timer.Tick += delegate { if(prepared==null) return; byte[] frame=new byte[25600]; Buffer.BlockCopy(prepared,(frameIndex++ % 36)*25600,frame,0,25600); preview.SetFrame(frame); }; timer.Start();
        FormClosed += delegate { timer.Stop(); timer.Dispose(); if(cancel!=null) cancel.Dispose(); };
    }
    void SetBusy(bool value) { busy=value; prepare.Enabled=restore.Enabled=!value; start.Enabled=!value&&!Decoder.IsStill(media); save.Enabled=!value&&prepared!=null; }
    async Task WriteAnimation(byte[] bytes)
    {
        if(busy) return;
        if(MessageBox.Show(this,L.T("将替换屏幕原有离线动画。原内容会先备份，写入后完整校验。\r\n读取速度有限，整个过程可能约 15 分钟。备份阶段可以取消。\r\n\r\n写入期间请保持 USB 供电。现在开始？"),L.T("写入离线动画"),MessageBoxButtons.OKCancel,MessageBoxIcon.Information)!=DialogResult.OK) return;
        SetBusy(true); writing=false; cancel=new CancellationTokenSource(); cancelButton.Enabled=true; progress.Value=0;
        string folder=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"离线备份",DateTime.Now.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N").Substring(0,6));
        try {
            await Task.Run(delegate {
                using(ScreenPort device=new ScreenPort(port)) {
                    device.SaveOffline(bytes,folder,delegate(int p,string text) {
                        if(p>=40) writing=true;
                        if(!IsDisposed) BeginInvoke((Action)delegate { progress.Value=Math.Min(100,p); message.Text=text; cancelButton.Enabled=!writing; });
                    },cancel.Token);
                }
            });
            message.Text=L.T("已保存并校验。退出所有播放器后重新上电；若显示其他页面，可轻触屏幕切回动画。");
            MessageBox.Show(this,L.T("离线动画已写入，并通过完整校验。\r\n\r\n请退出所有播放器，重新插拔屏幕。之后保持 USB 供电即可自动播放。\r\n\r\n原动画备份：\r\n")+folder,L.T("保存完成"),MessageBoxButtons.OK,MessageBoxIcon.Information);
        } catch(OperationCanceledException) { message.Text=L.T("备份已取消，未擦除或写入屏幕。"); }
        catch(Exception e) { message.Text=L.T("未完成：")+e.Message; MessageBox.Show(this,e.Message+L.T("\r\n\r\n备份目录：\r\n")+folder,L.T("离线保存未完成"),MessageBoxButtons.OK,MessageBoxIcon.Error); }
        finally { writing=false; cancelButton.Enabled=false; cancel.Dispose(); cancel=null; SetBusy(false); }
    }
}

internal static class Program
{
    [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
    [STAThread] static int Main(string[] args)
    {
        if(args.Length>0 && args[0]=="--state-hardware-test") {
            string report=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"状态验证记录.txt");
            try { using(ScreenPort device=new ScreenPort("COM13")) {
                Dashboard panels=new Dashboard();device.ShowStoredPhoto(0);device.KeepAlive();device.ShowStoredPhoto(100);device.KeepAlive();
                File.AppendAllText(report,"Stored animation pages 0/100: ACK OK\r\n");
                device.Orientation(true);device.ShowStoredPhoto(3926);device.KeepAlive();device.Orientation(false);
                File.AppendAllText(report,"Stored photo page 3926 and orientation: ACK OK\r\n");
                foreach(int mode in new int[]{1,2,6,4}){device.SendFrame(panels.Render(mode,1,0,false,0),CancellationToken.None);File.AppendAllText(report,"State "+mode+": frame sent and ADC responses OK\r\n");}
            }return 0;}catch(Exception e){File.AppendAllText(report,e+"\r\n");return 1;}
        }
        if(args.Length>0 && args[0]=="--offline-inspect") {
            string report=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"存储读取验证.txt");
            try { using(ScreenPort p=new ScreenPort("COM13")) {
                byte[] cfg=p.OfflineLayout(); File.WriteAllText(report,"Layout: "+BitConverter.ToString(cfg)+"\r\n");
                byte[] reference=new byte[256]; Stopwatch sw=Stopwatch.StartNew();for(int i=0;i<256;i++)reference[i]=p.ReadFlash(i);
                File.AppendAllText(report,"Sequential 256 bytes: "+sw.ElapsedMilliseconds+"ms\r\n");sw.Restart();
                byte[] batch=p.ReadRange(0,256);for(int i=0;i<256;i++)if(batch[i]!=reference[i])throw new Exception("Batch mismatch at "+i);
                File.AppendAllText(report,"ReadRange 256 bytes: "+sw.ElapsedMilliseconds+"ms; all bytes equal\r\n");
            } return 0; } catch(Exception e) {File.AppendAllText(report,e+"\r\n");return 1;}
        }
        if(args.Length>1 && args[0]=="--offline-prepare-test") {
            try { byte[] b=OfflineClip.Create(args[1],0,false,0); if(b.Length!=921600) return 1;
                if(Decoder.IsStill(args[1])) for(int i=25600;i<b.Length;i++) if(b[i]!=b[i%25600]) return 1;
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"离线验证记录.txt"),"Prepared "+Path.GetFileName(args[1])+" 36 frames / "+b.Length+" bytes\r\n"); return 0;
            } catch(Exception e) { File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"离线验证记录.txt"),e+"\r\n"); return 1; }
        }
        if (args.Length > 1 && (args[0] == "--decode-test" || args[0] == "--hardware-test")) {
            try {
                using (Decoder d = new Decoder(args[1], 5, 0, false, false, false)) {
                    using (ScreenPort port = args[0] == "--hardware-test" ? new ScreenPort("COM13") : null) {
                        int count = 0; Stopwatch sw = Stopwatch.StartNew(); byte[] b;
                        while (count < 10 && (b = d.Next()) != null) {
                            if (b.Length != 25600) throw new Exception("Bad frame size");
                            if (port != null) port.SendFrame(b, CancellationToken.None); count++;
                        }
                        if (count == 0) throw new Exception("No decoded frames");
                        File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "验证记录.txt"), args[0] + " " + Path.GetFileName(args[1]) + " frames=" + count + " elapsed=" + sw.ElapsedMilliseconds + "ms\r\n");
                    }
                }
                return 0;
            } catch (Exception e) { File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "验证记录.txt"), e.ToString() + "\r\n"); return 1; }
        }
        SetProcessDPIAware(); Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
        bool startup=Array.IndexOf(args,"--startup")>=0;
        string initial=args.Length>0 && !args[0].StartsWith("--") ? args[0] : null;
        using(Mutex instance=new Mutex(false,"Local\\MSU2MiniPlayer")) {
            bool owned=false;
            try { try { owned=instance.WaitOne(0); } catch(AbandonedMutexException) { owned=true; }
                if(!owned) { if(!startup)WindowSignal.ShowExisting(); return 0; }
                Application.Run(new PlayerForm(initial,startup));
            } finally { if(owned) instance.ReleaseMutex(); }
        }
        return 0;
    }
}
