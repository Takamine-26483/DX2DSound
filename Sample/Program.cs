using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using TakamineProduction;
using DxLibDLL;

namespace Sample
{
	static class Program
	{
		const string VOLUMEKEY_BGM = "BGM";
		const string VOLUMEKEY_SE = "SE";


		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		static void Main()
		{
			DX.SetWaitVSyncFlag(1);
			DX.ChangeWindowMode(1);
			DX.DxLib_Init();

			var key = new byte[256];

			//★　音量ミキサーを作成
			var mixer = new BaseVolumeObject.Mixer();

			//★　音量ミキサーにボリュームを追加
			mixer.VolObjs.Add(VOLUMEKEY_BGM, new DX2DSound.VolumeObject(mixer));
			mixer.VolObjs.Add(VOLUMEKEY_SE,new DX2DSound.VolumeObject(mixer));

			//★　サウンドを作成・音量ミキサーのVolumeObjectに登録
			var bgm = new DX2DSound("bgm.mp3", mixer.VolObjs[VOLUMEKEY_BGM], DX2DSound.SoundPlayType.Loop);
			var se = new DX2DSound("se.mp3", mixer.VolObjs[VOLUMEKEY_SE]) { PermitMultiple = true };
			//★　音量ミキサーに登録しない
			//var se = new DX2DSound("se.mp3", null) { PermitMultiple = true };

			//★　再生する
			bgm.Play();

			while (DX.ProcessMessage() != -1)
			{
				DX.ClearDrawScreen();

				
				DX.GetHitKeyStateAll(key);

				if (key[DX.KEY_INPUT_Z] == 1)
					se.Play();//★　再生する
				if (key[DX.KEY_INPUT_X] == 1)
					se.Stop();//★　停止する

				//★　ミキサーの音量を変更
				if (key[DX.KEY_INPUT_UP] == 1)
					mixer.Volume += 100;
				if (key[DX.KEY_INPUT_DOWN] == 1)
					mixer.Volume -= 100;

				//★　ボリュームの音量を変更
				if (key[DX.KEY_INPUT_RIGHT] == 1)
					mixer[VOLUMEKEY_BGM].Volume += 100;
				if (key[DX.KEY_INPUT_LEFT] == 1)
					mixer[VOLUMEKEY_BGM].Volume -= 100;

				DX.DrawString(0, 0, "Mixer:" + mixer.Volume.ToString(), DX.GetColor(255, 0, 0));
				DX.DrawString(0, 20, "VOLUME_BGM:" + mixer[VOLUMEKEY_BGM].Volume.ToString(), DX.GetColor(255, 0, 0));
				DX.DrawString(0, 40, "VOLUME_SE:" + mixer[VOLUMEKEY_SE].Volume.ToString(), DX.GetColor(255, 0, 0));
				DX.DrawString(0, 60, "bgm:" + bgm.Volume.ToString(), DX.GetColor(255, 0, 0));
				DX.DrawString(0, 80, "se:" + se.Volume.ToString(), DX.GetColor(255, 0, 0));


				DX.ScreenFlip();
			}


			DX.DxLib_End();
		}
	}
}
