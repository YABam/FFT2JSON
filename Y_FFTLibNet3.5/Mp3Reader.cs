using NAudio.Wave;
using System;
using System.IO;

namespace Y_FFTLib
{
	class Mp3Reader
    {
        public float[] DataL, DataR, DataDiff;

        public byte[] RawData;
        public WaveFormat RawFormat;
        public long TotalTimeMs;


        public Mp3Reader(string filename)
        {
            byte[] convertedData;
            int size;
            WaveFormat f = new WaveFormat(44100, 16, 2);
            //read mp3 file
            using (Mp3FileReader r = new Mp3FileReader(filename))
            {
                using (var conv = new WaveFormatConversionStream(f, r))
                {
                    int len = (int)conv.Length;
                    size = len / 4;
                    convertedData = new byte[len];
                    conv.Read(convertedData, 0, len);
                }
            }

            //data to left and right
            DataL = new float[size];
            DataR = new float[size];
            DataDiff = new float[size];
            using (var ms = new MemoryStream(convertedData))
            {
                using (var br = new BinaryReader(ms))
                {
                    for (int i = 0; i < size; ++i)
                    {
                        DataL[i] = br.ReadInt16() / (float)Int16.MaxValue;
                        DataR[i] = br.ReadInt16() / (float)Int16.MaxValue;
                        DataDiff[i] = DataL[i] - DataR[i];
                    }
                }
            }

            RawData = convertedData;
            RawFormat = f;
            TotalTimeMs = convertedData.Length * 1000L / f.AverageBytesPerSecond;
        }
    }
}
