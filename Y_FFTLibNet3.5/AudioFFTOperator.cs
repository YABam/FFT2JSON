using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace Y_FFTLib
{
    public class AudioFFTOperator
    {
        string filename;
		int _FFTFPS;
		int _FFTLength;
		int _FFTChannel;

		public AudioFFTOperator(string __filename, int __FFTFPS, int __FFTLength, int __FFTChannel)
		{
			filename = __filename;
			_FFTFPS = __FFTFPS;
			_FFTLength = __FFTLength;
			_FFTChannel = __FFTChannel;
		}

		private Mp3Reader _Reader;
        private int _progressTimeMs;
		public List<FFTDataNode> resultList = new List<FFTDataNode>();

		private Complex[] _ReadBuffer;
		public float[] _Buffer;
		
		public void GetSpectrum()
		{
			//read mp3 file
			_Reader = new Mp3Reader(filename);

			//Get FFT Data
			for (_progressTimeMs = 0; _progressTimeMs < _Reader.TotalTimeMs; _progressTimeMs += (int)(1.0 / _FFTFPS * 1000))
			{
				if (_FFTLength < 2) continue;
				resultList.Add(new FFTDataNode(GetFFTData(), _progressTimeMs));
			}
		}

		private float[] GetFFTData()
		{
			Calculate(0, (int)(_progressTimeMs / 1000.0f * 44100), _FFTLength);
			float[] value = new float[_Buffer.Length];
			_Buffer.CopyTo(value, 0);
			return value;
		}

		public void Calculate(int channel, int offset, int len)
		{
			if (_ReadBuffer == null || _ReadBuffer.Length < len)
			{
				_ReadBuffer = new Complex[len];
				_Buffer = new float[len];
			}

			float[] data = channel == 0 ? _Reader.DataL : channel == 1 ? _Reader.DataR : _Reader.DataDiff;

			if (offset + len > data.Length)
			{
				offset = data.Length - len;
			}

			for (int i = 0; i < len; ++i)
			{
				_ReadBuffer[i] = new Complex(data[i + offset], 0);
			}

			Fourier.Forward(_ReadBuffer);
			for (int i = 0; i < len; ++i)
			{
				_Buffer[i] = (float)_ReadBuffer[i].Magnitude;
			}
		}
	}

    public class FFTDataNode
    {
        public float[] _FFTData;
        public int _TimeMs;
        public FFTDataNode(float[] data, int TimeMs)
        {
            _FFTData = data;
            _TimeMs = TimeMs;
        }
    }
}
