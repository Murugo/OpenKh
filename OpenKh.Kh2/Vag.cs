//Resources:
//https://github.com/ColdSauce/psxsdk/blob/master/tools/vag2wav.c
//https://github.com/losnoco/vgmstream/blob/master/src/meta/vag.c
//VAG+VAS Previewer

using System;
using System.IO;
using System.Text;
using Xe.IO;

namespace OpenKh.Kh2
{
    public class Vag
    {
        private const uint MagicCode = 0x70474156U;
        private static readonly double[][] f = new double[5][]
        {
            new double[2],
            new double[2]{ 15.0 / 16.0, 0.0 },
            new double[2]{ 115.0 / 64.0, -13.0 / 16.0 },
            new double[2]{ 49.0 / 32.0, -55.0 / 64.0 },
            new double[2]{ 61.0 / 32.0, -15.0 / 16.0 }
        };

        private readonly Stream _vagStream;
        private Stream _wavStream;

        public int Version { get; }
        public long FileSize { get; }
        public int ChannelSize { get; }
        public int SampleRate { get; }
        public byte Channels { get; }
        public byte Volume { get; }
        public string StreamName { get; }

        public Stream WaveStream
        {
            get
            {
                if (_wavStream == null)
                    _wavStream = Decode();
                return _wavStream;
            }
        }

        public bool LoopFlag { get; }
        public int LoopStartSample { get; }
        public int LoopEndSample { get; }

        public TimeSpan Duration { get; }

        public Vag(Stream stream)
        {
            _vagStream = stream;

            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            var reader = new BinaryReader(stream);
            if (stream.Length < 16L || reader.ReadUInt32() != MagicCode)
                throw new InvalidDataException("Invalid header");

            FileSize = stream.Length;

            Version = reader.ReadInt32Swap();

            reader.BaseStream.Position = 0xC;

            ChannelSize = reader.ReadInt32Swap();
            SampleRate = reader.ReadInt32Swap();

            reader.BaseStream.Position = 0x1C;

            if (Version == 4 && ChannelSize == FileSize - 0x60 && reader.ReadInt32Swap() != 0)
            {
                //KH VAG / VAS file
                //Beware: VAGs converted from MFAudio have Version 3
                reader.BaseStream.Position = 0x14;
                LoopStartSample = reader.ReadInt32Swap();
                LoopEndSample = reader.ReadInt32Swap();
                LoopFlag = (LoopEndSample > 0);

                reader.BaseStream.Position = 0x1E;
                Channels = reader.ReadByte();
                Volume = reader.ReadByte();
            }
            else
                Channels = 1;

            reader.BaseStream.Position = 0x20;
            StreamName = Encoding.UTF8.GetString(reader.ReadBytes(0x10)).Replace("\0", string.Empty);

            var seconds = (double)((ChannelSize / Channels / 0x10 * 28) / SampleRate);
            Duration = new TimeSpan(0, 0, (int)seconds);

            _wavStream = Decode();
            _wavStream.Position = 0L;
        }

        public void ToWav(string outputName)
        {
            using (FileStream fs = new FileStream(outputName, FileMode.OpenOrCreate))
            {
                _wavStream.Position = 0L;
                _wavStream.CopyTo(fs);
                fs.Flush();
            }
        }

        private Stream Decode()
        {
            _vagStream.Position = 0L;

            Stream wavStream = new MemoryStream();
            using (BinaryReader vagReader = new BinaryReader(_vagStream, Encoding.Default, true))
            using (BinaryWriter wavWriter = new BinaryWriter(wavStream, Encoding.Default, true))
            {
                wavWriter.Write(0x46464952);    //RIFF
                wavWriter.Write(0);             //Length of Chunk, filled in later
                wavWriter.Write(0x45564157);    //WAVE
                wavWriter.Write(0x20746D66);    //fmt
                wavWriter.Write(16);
                wavWriter.Write((short)1);
                wavWriter.Write((short)Channels);
                wavWriter.Write(SampleRate);
                wavWriter.Write(SampleRate * Channels * 2);
                wavWriter.Write((short)(Channels * 2U));
                wavWriter.Write((short)16);
                wavWriter.Write(0x61746164);
                wavWriter.Write(0);             //Length of SubChunk, filled in later

                vagReader.BaseStream.Position = 48 + 16 * Channels;
                int bytesLeft = ChannelSize - 16 * Channels;
                if (Channels < 2)
                {
                    double s_1 = 0.0;
                    double s_2 = 0.0;
                    while (bytesLeft > 0U)
                    {
                        byte[] buffer = vagReader.ReadBytes(16);
                        bytesLeft -= 16;
                        if (buffer.Length >= 16 && buffer[1] != 7)
                        {
                            byte predict_nr = buffer[0];
                            int shift_factor = predict_nr & 15;
                            predict_nr >>= 4;
                            for (int i = 2; i < 16; ++i)
                            {
                                int d = (buffer[i] & 15) << 12;
                                if ((d & 0x8000) == 0x8000)
                                    d |= -65536;
                                s_2 = (d >> shift_factor) + s_1 * f[predict_nr][0] + s_2 * f[predict_nr][1];
                                wavWriter.Write((ushort)(s_2 + 0.5));

                                int s = (buffer[i] & 240) << 8;
                                if ((s & 0x8000) == 0x8000)
                                    s |= -65536;
                                s_1 = (s >> shift_factor) + s_2 * f[predict_nr][0] + s_1 * f[predict_nr][1];
                                wavWriter.Write((ushort)(s_1 + 0.5));
                            }
                            if (buffer[1] == 1)
                                break;
                        }
                        else
                            break;
                    }
                }
                else if (Channels == 2)
                {
                    double s_1 = 0.0;
                    double s_2 = 0.0;
                    double s_3 = 0.0;
                    double s_4 = 0.0;
                    while (bytesLeft > 0U)
                    {
                        byte[] buffer = vagReader.ReadBytes(32);
                        bytesLeft -= 32;
                        if (buffer.Length >= 32 && buffer[1] != 7 && buffer[17] != 7)
                        {
                            byte predict_nr1 = buffer[0];
                            byte predict_nr2 = buffer[16];
                            int shift_factor1 = predict_nr1 & 15;
                            predict_nr1 >>= 4;
                            int shift_factor2 = predict_nr2 & 15;
                            predict_nr2 >>= 4;
                            for (int i = 2; i < 16; ++i)
                            {
                                byte sam1 = buffer[i];
                                byte sam2 = buffer[16 + i];
                                int sam1_shift = (sam1 & 15) << 12;
                                if ((sam1_shift & 32768) == 32768)
                                    sam1_shift |= -65536;
                                s_2 = (sam1_shift >> shift_factor1) + s_1 * f[predict_nr1][0] + s_2 * f[predict_nr1][1];
                                wavWriter.Write((ushort)(s_2 + 0.5));

                                int sam2_shift = (sam2 & 15) << 12;
                                if ((sam2_shift & 32768) == 32768)
                                    sam2_shift |= -65536;
                                s_4 = (sam2_shift >> shift_factor2) + s_3 * f[predict_nr2][0] + s_4 * f[predict_nr2][1];
                                wavWriter.Write((ushort)(s_4 + 0.5));

                                int sam1_shift2 = (sam1 & 240) << 8;
                                if ((sam1_shift2 & 32768) == 32768)
                                    sam1_shift2 |= -65536;
                                s_1 = (sam1_shift2 >> shift_factor1) + s_2 * f[predict_nr1][0] + s_1 * f[predict_nr1][1];
                                wavWriter.Write((ushort)(s_1 + 0.5));

                                int sam2_shift2 = (sam2 & 240) << 8;
                                if ((sam2_shift2 & 32768) == 32768)
                                    sam2_shift2 |= -65536;
                                s_3 = (sam2_shift2 >> shift_factor2) + s_4 * f[predict_nr2][0] + s_3 * f[predict_nr2][1];
                                wavWriter.Write((ushort)(s_3 + 0.5));
                            }
                            if (buffer[1] == 1 || buffer[17] == 1)
                                break;
                        }
                        else
                            break;
                    }
                }
                wavWriter.BaseStream.Position = 4L;
                uint len = (uint)wavStream.Length - 8U;
                wavWriter.Write(len);
                wavWriter.BaseStream.Position = 40L;
                wavWriter.Write(len - 36U);
                wavWriter.Flush();
            }
            return wavStream;
        }
    }
}