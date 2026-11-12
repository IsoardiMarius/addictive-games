using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Game.Core;
using Game.Core.Engine;

namespace Headless;

/// <summary>
/// Enregistre une vraie séquence de jeu (images et son) sans Unity : le robot joue, chaque image est
/// rendue par le banc d'essai et encodée par ffmpeg ; le son est mixé à partir des mêmes sons synthétisés.
/// Usage : dotnet run -c Release -- video sortie.mp4   (ffmpeg doit être installé)
/// </summary>
public static class Video
{
    private sealed class Voice { public float[] Clip; public double Pos; public float Pitch, Vol; }

    private sealed class Mixer
    {
        private readonly float[][] _clips = new float[(int)Sfx.Count][];
        private readonly float[] _drill = SoundBank.BuildDrillLoop(), _music = SoundBank.BuildMusicLoop();
        private readonly List<Voice> _voices = new List<Voice>();
        private readonly List<float> _out = new List<float>();
        private double _drillPos, _musicPos, _carry;

        public Mixer() { for (int i = 0; i < _clips.Length; i++) _clips[i] = SoundBank.Build((Sfx)i); }

        private static float At(float[] d, double pos, bool loop)
        {
            int i0 = (int)pos;
            if (loop) { i0 %= d.Length; } else if (i0 >= d.Length - 1) return 0f;
            int i1 = loop ? (i0 + 1) % d.Length : i0 + 1;
            float f = (float)(pos - Math.Floor(pos));
            return d[i0] * (1f - f) + d[i1] * f;
        }

        public void Frame(AudioBus bus, float dt)
        {
            foreach (AudioBus.Shot s in bus.Shots)
                _voices.Add(new Voice { Clip = _clips[(int)s.Id], Pitch = Math.Clamp(s.Pitch, 0.3f, 3f), Vol = Math.Clamp(s.Vol, 0f, 1f) });
            _carry += dt * SoundBank.Rate;
            int n = (int)_carry; _carry -= n;
            float drillVol = bus.SfxOn ? bus.DrillVol : 0f, musicVol = bus.MusicOn ? bus.MusicVol : 0f;
            for (int k = 0; k < n; k++)
            {
                float v = At(_drill, _drillPos, true) * drillVol + At(_music, _musicPos, true) * musicVol;
                _drillPos += bus.DrillPitch; _musicPos += 1.0;
                for (int i = _voices.Count - 1; i >= 0; i--)
                {
                    Voice vo = _voices[i];
                    v += At(vo.Clip, vo.Pos, false) * vo.Vol;
                    vo.Pos += vo.Pitch;
                    if (vo.Pos >= vo.Clip.Length - 1) _voices.RemoveAt(i);
                }
                _out.Add(v);
            }
        }

        public void SaveWav(string path)
        {
            using var bw = new BinaryWriter(File.Create(path));
            int bytes = _out.Count * 2;
            bw.Write(new[] { 'R', 'I', 'F', 'F' }); bw.Write(36 + bytes); bw.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
            bw.Write(16); bw.Write((short)1); bw.Write((short)1); bw.Write(SoundBank.Rate); bw.Write(SoundBank.Rate * 2); bw.Write((short)2); bw.Write((short)16);
            bw.Write(new[] { 'd', 'a', 't', 'a' }); bw.Write(bytes);
            // Limiteur doux, comme le ferait la sortie audio d'un téléphone.
            foreach (float v in _out) bw.Write((short)(Math.Tanh(v * 0.9f) * 30000f));
        }
    }

    public static int Make(string outPath)
    {
        const float scale = 2f;
        const int wPt = 390, hPt = 844;
        int W = (int)(wPt * scale), H = (int)(hPt * scale);
        string dir = Path.GetDirectoryName(Path.GetFullPath(outPath))!;
        Directory.CreateDirectory(dir);
        string silent = Path.Combine(dir, "_images.mp4"), wav = Path.Combine(dir, "_son.wav");

        var ff = Process.Start(new ProcessStartInfo("ffmpeg",
            $"-y -loglevel error -f rawvideo -pix_fmt rgb24 -s {W}x{H} -r 30 -i - -c:v libx264 -preset veryfast -crf 21 -pix_fmt yuv420p \"{silent}\"")
        { RedirectStandardInput = true, UseShellExecute = false })!;
        Stream pipe = ff.StandardInput.BaseStream;

        var d = new AppDriver(dir, "", wPt, hPt, 47f, 34f, scale);
        var mixer = new Mixer();
        byte[] rgb = new byte[W * H * 3];
        int frame = 0;
        d.OnFrame = dt =>
        {
            mixer.Frame(d.App.Audio, dt);
            if (frame++ % 2 != 0) return;
            d.Fb.Clear(0f, 0f, 0f);
            d.Fb.Draw(d.App.List, d.App.Atlas);
            d.Fb.CopyRgb24(rgb);
            pipe.Write(rgb, 0, rgb.Length);
        };

        var sw = Stopwatch.StartNew();
        d.Wait(1.6f);                       // premier lancement : « glisse pour diriger »
        d.PlayRun(true);                    // première descente, jusqu'à l'écran de fin
        d.Wait(2.6f);
        d.Tap(Loc.Get("garage")); d.Wait(0.8f);
        for (int i = 0; i < 4; i++)
            for (int u = 0; u < 6; u++)
            {
                int lvl = d.App.P.Up[u];
                long cost = Balance.Cost((Upgrade)u, lvl);
                if (lvl < Balance.MaxLevel((Upgrade)u) && cost <= d.App.P.Coins && d.Tap(Fmt.Short(cost, true))) { d.Wait(0.45f); break; }
            }
        d.Wait(0.6f);
        d.Tap(Loc.Get("play")); d.Wait(0.2f);
        d.PlayRun(true, null, false, 11f);  // début de la deuxième descente
        d.OnFrame = null;
        pipe.Flush(); pipe.Close();
        ff.WaitForExit();
        mixer.SaveWav(wav);
        Console.WriteLine($"{frame} images simulées en {sw.Elapsed.TotalSeconds:0.0} s");

        var mux = Process.Start(new ProcessStartInfo("ffmpeg",
            $"-y -loglevel error -i \"{silent}\" -i \"{wav}\" -c:v copy -c:a aac -b:a 128k -shortest \"{outPath}\"") { UseShellExecute = false })!;
        mux.WaitForExit();
        File.Delete(silent);
        Console.WriteLine("écrit : " + outPath + " (son seul : " + wav + ")");
        return mux.ExitCode;
    }
}
