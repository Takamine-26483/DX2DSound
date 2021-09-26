using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using DxLibDLL;

namespace Sample
{
	static class Program
	{
		const string FILEPATH = "ぼくは夏色、きみは夢の中、.mp3";
		

		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		static void Main()
		{
			DX.SetWaitVSyncFlag(1);
			DX.ChangeWindowMode(1);
			DX.DxLib_Init();



			var t1_s = DX.GetNowHiPerformanceCount();
			DX.LoadSoundMem(FILEPATH);
			var t1_e = DX.GetNowHiPerformanceCount();


			DX.InitSoundMem();


			DX.SetCreateSoundPitchRate(0);
			DX.SetCreateSoundTimeStretchRate(1.0f);
			DX.SetCreateSoundDataType(DX.DX_SOUNDDATATYPE_MEMNOPRESS);
			var t2_s = DX.GetNowHiPerformanceCount();
			DX.LoadSoundMem(FILEPATH);
			var t2_e = DX.GetNowHiPerformanceCount();



			while (DX.ProcessMessage() != -1)
			{
				DX.ClearDrawScreen();


				
				DX.DrawString(0, 0, "t1:" + (t1_e - t1_s), DX.GetColor(255, 0, 0));
				DX.DrawString(0, 20, "t2:" + (t2_e - t2_s), DX.GetColor(255, 0, 0));



				DX.ScreenFlip();
			}


			DX.DxLib_End();
		}
	}
}
