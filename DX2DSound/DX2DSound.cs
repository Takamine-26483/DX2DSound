using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DxLibDLL;


namespace TakamineProduction
{
	/// <summary>DXライブラリのSoundMem系をまとめたクラス。</summary>
	public class DX2DSound : BaseVolumeObject
	{
		private int Handle { get; set; }

		/// <summary>読み込んだ音声ファイルへのパス</summary>
		public string FilePath { get; }
		/// <summary>ピッチレート(100fで半音)</summary>
		public float Pitch { get; }
		/// <summary>タイムストレッチ倍率</summary>
		public float TimeStretch { get; }
		/// <summary>音声データの形式</summary>
		public SoundDataType DataType { get; }
		/// <summary>再生形式</summary>
		public SoundPlayType PlayType { get; }
		/// <summary>再生中かを表す</summary>
		public bool IsPlaying
		{
			get => DX.CheckSoundMem(Handle) == 1;
		}
		/// <summary>再生周波数</summary>
		public int Freq
		{
			get => DX.GetFrequencySoundMem(Handle);
			set => DX.SetFrequencySoundMem(value, Handle);
		}
		/// <summary>パン(-10000～10000：左のみ～右のみ)</summary>
		public int Pan
		{
			get => DX.GetPanSoundMem(Handle);
			set => DX.SetPanSoundMem(value, Handle);
		}
		/// <summary>ループ範囲（ミリ秒単位。ループ範囲を指定すると自動的にループします）</summary>
		public (long start, long end) LoopArea
		{
			get
			{
				DX.GetLoopAreaTimePosSoundMem(out long start, out long end, Handle);
				return (start, end);
			}
			set => DX.SetLoopAreaTimePosSoundMem(value.start, value.end, Handle);
		}
		/// <summary>ループ範囲（サンプル単位。ループ範囲を指定すると自動的にループします）</summary>
		public (long start, long end) LoopAreaSample
		{
			get
			{
				DX.GetLoopAreaSamplePosSoundMem(out long start, out long end, Handle);
				return (start, end);
			}
			set => DX.SetLoopAreaSamplePosSoundMem(value.start, value.end, Handle);
		}
		/// <summary>音声の再生位置（サンプル単位。再生中の変更に対応しています）</summary>
		public long CurrentPosition
		{
			get => DX.GetCurrentPositionSoundMem(Handle);
			set
			{
				int isPlaying = DX.CheckSoundMem(Handle);
				DX.StopSoundMem(Handle);
				DX.SetCurrentPositionSoundMem(value, Handle);
				if (isPlaying == 1)
					Play(false);
			}
		}
		/// <summary>Playの度にStopSoundMemを行わないフラグ</summary>
		public bool PermitMultiple { get; set; } = false;
		/// <summary>追加先のVolumeObject</summary>
		public VolumeObject AddedDestVolumeObject { get; }



		private DX2DSound(DX2DSound copy_from, string filepath, VolumeObject volume, float pitch, float timeStretch, SoundDataType soundDataType, SoundPlayType soundPlayType)
		{
			FilePath = filepath;
			Pitch = pitch;
			TimeStretch = timeStretch;
			DataType = soundDataType;
			PlayType = soundPlayType;
			AddedDestVolumeObject = volume;

			volume?.Sounds.Add(this);
			MakeHandle(copy_from);
		}

		/// <summary>コンストラクタ。音量ミキサーへの登録も行う（新規作成）</summary>
		/// <param name="filepath">ファイルパス</param>
		/// <param name="volume">所属するVolumeObjectのインスタンス（ここで設定したインスタンスの音量の影響を受ける）</param>
		/// <param name="soundPlayType">再生形式</param>
		/// <param name="pitch">ピッチレート(100fで半音)</param>
		/// <param name="timeStretch">タイムストレッチ倍率</param>
		/// <param name="soundDataType">音声データの形式</param>
		public DX2DSound(string filepath, VolumeObject volume, SoundPlayType soundPlayType = SoundPlayType.Back, float pitch = 0f, float timeStretch = 1.0f, SoundDataType soundDataType = SoundDataType.MemNoPress)
			: this(null, filepath, volume, pitch, timeStretch, soundDataType, soundPlayType)
		{ }
		/// <summary>コンストラクタ。音量ミキサーへの登録も行う（ハンドル以外は全てコピー）</summary>
		/// <param name="copy_from">コピー元</param>
		public DX2DSound(DX2DSound copy_from)
			: this(copy_from, copy_from.FilePath, copy_from.AddedDestVolumeObject, copy_from.Pitch, copy_from.TimeStretch, copy_from.DataType, copy_from.PlayType)
		{ }
		/// <summary>デストラクタ（ハンドル削除・VolumeObjectのSoundsから自身を削除）</summary>
		~DX2DSound()
		{
			DX.DeleteSoundMem(Handle);
			AddedDestVolumeObject?.Sounds.Remove(this);
		}

		/// <summary>音声を再生する</summary>
		/// <param name="topPositionFlag">再生位置を0に戻すフラグ</param>
		public void Play(bool topPositionFlag = true)
		{
			if (!PermitMultiple)
				Stop();
			DX.PlaySoundMem(Handle, (int)PlayType, topPositionFlag ? 1 : 0);
		}
		/// <summary>再生を停止する</summary>
		public void Stop() => DX.StopSoundMem(Handle);
		/// <summary>ハンドルを作り直す（※　音量以外の全てのパラメータは再設定が必要です）</summary>
		public void RemakeHandle() => MakeHandle();


		private void MakeHandle(DX2DSound copy_from = null)
		{
			var pitchBuf = DX.GetCreateSoundPitchRate();
			var tsBuf = DX.GetCreateSoundTimeStretchRate();
			var sdtBuf = DX.GetCreateSoundDataType();

			DX.SetCreateSoundPitchRate(Pitch);
			DX.SetCreateSoundTimeStretchRate(TimeStretch);
			DX.SetCreateSoundDataType((int)DataType);

			DX.DeleteSoundMem(Handle);
			Handle = copy_from == null ? DX.LoadSoundMem(FilePath) : DX.DuplicateSoundMem(copy_from.Handle);

			DX.SetCreateSoundPitchRate(pitchBuf);
			DX.SetCreateSoundTimeStretchRate(tsBuf);
			DX.SetCreateSoundDataType(sdtBuf);

			VolumeChangeEvent();
		}
		/// <summary>音量を更新</summary>
		protected override void VolumeChangeEvent() => DX.SetVolumeSoundMem((int)(Volume * (AddedDestVolumeObject?.EffectedVolumeMag ?? 1d)), Handle);//VolumeObjectを登録してない場合は計算を行わないため、1になる



		//*****************************************************************************************
		//* 内部クラス ****************************************************************************
		//*****************************************************************************************

		/// <summary>音量クラス</summary>
		public class VolumeObject : BaseVolumeObject
		{
			/// <summary>このVolumeObjectに影響を与えるMixer</summary>
			public Mixer EffectingMixer { get; }
			/// <summary>Mixerの影響を加味した音量数値(10000 = 100%)</summary>
			public int EffectedVolume { get => (int)(Volume * EffectingMixer.VolumeMag); }
			/// <summary>Mixerの影響を加味した音量数値(1.0 = 100%)</summary>
			public double EffectedVolumeMag { get => VolumeMag * EffectingMixer.VolumeMag; }
			/// <summary>音量を変更した際、変更が適用される音声郡</summary>
			public List<DX2DSound> Sounds { get; } = new List<DX2DSound>();


			/// <summary>コンストラクタ</summary>
			/// <param name="mixer">このVolumeObjectに影響を与えるMixer</param>
			public VolumeObject(Mixer mixer) => EffectingMixer = mixer;

			/// <summary>全ての音声の音量を再設定する</summary>
			protected override void VolumeChangeEvent()
			{
				foreach (var i in Sounds.Select(x => x))
					i.VolumeChangeEvent();
			}
		}



		//*****************************************************************************************
		//* 列挙体 ********************************************************************************
		//*****************************************************************************************

		/// <summary>データ形式</summary>
		public enum SoundDataType
		{
			/// <summary>無圧縮状態でメモリ上に保存</summary>
			MemNoPress = DX.DX_SOUNDDATATYPE_MEMNOPRESS,
			/// <summary>圧縮した状態でメモリ上に保存</summary>
			MemPress = DX.DX_SOUNDDATATYPE_MEMPRESS,
			/// <summary>読み込みと再生を同時に行う（ストリーミング再生）</summary>
			File = DX.DX_SOUNDDATATYPE_FILE
		}
		/// <summary>再生形式</summary>
		public enum SoundPlayType
		{
			/// <summary>再生終了まで処理を止める</summary>
			Normal = DX.DX_PLAYTYPE_NORMAL,
			/// <summary>鳴らし始めるとすぐ次の処理へ移る</summary>
			Back = DX.DX_PLAYTYPE_BACK,
			/// <summary>DX_PLAYTYPE_BACK + ループ再生</summary>
			Loop = DX.DX_PLAYTYPE_LOOP
		}
	}
	
	/// <summary>基底音量クラス</summary>
	public abstract class BaseVolumeObject
	{
		private int innerVolume = 10000;

		/// <summary>このObject自体の音量数値(10000 = 100%)</summary>
		public int Volume
		{
			get => innerVolume;
			set
			{
				innerVolume = value;
				VolumeChangeEvent();
			}
		}
		/// <summary>このObject自体の音量数値(倍率単位)(1.0 = 100%)</summary>
		public double VolumeMag
		{
			get => innerVolume / 10000d;
			set
			{
				innerVolume = (int)(value * 10000d);
				VolumeChangeEvent();
			}
		}
		/// <summary>Volumeが変更された際に呼び出される</summary>
		protected abstract void VolumeChangeEvent();




		//*****************************************************************************************
		//* 内部クラス ****************************************************************************
		//*****************************************************************************************

		/// <summary>音量ミキサー。このオブジェクトのVolume値はミキサー内の全てのVolumeObjectの音量に影響を与える</summary>
		public class Mixer : BaseVolumeObject
		{
			/// <summary>VolumeObject郡。Mixerの影響を受ける</summary>
			public Dictionary<string, DX2DSound.VolumeObject> VolObjs { get; } = new Dictionary<string, DX2DSound.VolumeObject>();
			/// <summary>VolObjsを検索</summary>
			/// <param name="key">キー</param>
			/// <returns></returns>
			public DX2DSound.VolumeObject this[string key] => VolObjs[key];

			/// <summary>全てのVolumeObjectのイベントを発生させる</summary>
			protected override void VolumeChangeEvent()
			{
				foreach (var i in VolObjs.Select(x => x.Value))
					i.VolumeChangeEvent();
			}
			
		}
	}
}
