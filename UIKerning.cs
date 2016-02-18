using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UGUICustom
{
	/// <summary>
	/// uGUIのtextにカーニング機能を適用します。インスペクターの_kerningに入れた値の分だけ字間が移動します。左揃え・中央・右揃えに適応しています.
	/// </summary>
	[AddComponentMenu("UI/Effects/Kerning")]
	[RequireComponent(typeof(Text))]
	#if UNITY_4_6 || UNITY_5_0 || UNITY_5_1
	public class UIKerning : BaseVertexEffect
	#else
	public class UIKerning : BaseMeshEffect
	#endif
	{
		[SerializeField] private float _kerning;

		private Text _textComponent;
		private int _textLength;

		#if UNITY_4_6 || UNITY_5_0 || UNITY_5_1
		private const int _ONE_CHAR_VERTEX = 4;
		#else
		private const int _ONE_CHAR_VERTEX = 6;
		#endif

		protected override void OnEnable()
		{
			_textComponent = GetComponent<Text>();
		}

		#if UNITY_4_6 || UNITY_5_0 || UNITY_5_1
		public override void ModifyVertices(List<UIVertex> vertexList)
		{
			// GameComponentとComponentのアクティブチェック・頂点リストの有無・頂点数が0ではないか・文字が1文字じゃないかチェック.
			if (IsActive() == false || vertexList == null || vertexList.Count == 0 || vertexList.Count < _ONE_CHAR_VERTEX)
			{
				return;
			}

		#else
		public override void ModifyMesh(VertexHelper vh)
		{
			// GameComponentとComponentのアクティブチェック
			if (IsActive() == false)
			{
				return;
			}

			List<UIVertex> vertexList = new List<UIVertex>();
			vh.GetUIVertexStream(vertexList);

			//頂点リストの有無・頂点数が0ではないか・文字が1文字じゃないかチェック.
			if (vertexList == null || vertexList.Count == 0 || vertexList.Count < _ONE_CHAR_VERTEX)
			{
				return;
			}

		#endif
			_textLength = _textComponent.text.Length;

			//NOTE:iは頂点カウント、jは文字カウント.
			for (int i = 0, j = 0; i < vertexList.Count; i++)
			{
				UIVertex uiVertex = vertexList[i];

				//NOTE:4頂点毎に文字が繰り上がる.
				if (i > 0 && i % _ONE_CHAR_VERTEX == 0)
				{
					j++;
				}

				switch (_textComponent.alignment)
				{
				case TextAnchor.UpperLeft:
				case TextAnchor.MiddleLeft:
				case TextAnchor.LowerLeft:
					_KerningLeftPivot(ref uiVertex, j);
					break;

				case TextAnchor.UpperRight:
				case TextAnchor.MiddleRight:
				case TextAnchor.LowerRight:
					_KerningRightPivot(ref uiVertex, j);
					break;

				case TextAnchor.UpperCenter:
				case TextAnchor.MiddleCenter:
				case TextAnchor.LowerCenter:
					_KerningCenterPivot(ref uiVertex, j);
					break;
				}

				vertexList[i] = uiVertex;
			}

			#if !UNITY_4_6 && !UNITY_5_0 && !UNITY_5_1
			vh.Clear();
			vh.AddUIVertexTriangleStream(vertexList);
			#endif
		}

		/// <summary>
		/// 左揃えの場合のカーニング.
		/// </summary>
		/// <param name="uiVertex">vertex.</param>
		/// <param name="charIndex">Char index.</param>
		private void _KerningLeftPivot(ref UIVertex uiVertex, int charIndex)
		{
			//NOTE:一番左の文字座標は動かさない.
			if (charIndex < 1)
			{
				return;
			}

			uiVertex.position.x += (float)charIndex * _kerning;
		}

		/// <summary>
		/// 右揃えの場合のカーニング.
		/// </summary>
		/// <param name="uiVertex">vertex.</param>
		/// <param name="charIndex">Char index.</param>
		private void _KerningRightPivot(ref UIVertex uiVertex, int charIndex)
		{
			//NOTE:一番右の文字座標は動かさない
			if (charIndex >= _textLength - 1)
			{
				return;
			}

			uiVertex.position.x -= (float)((_textLength - 1) - charIndex) * _kerning;
		}

		/// <summary>
		/// 中央揃えの場合のカーニング.
		/// </summary>
		/// <param name="uiVertex">vertex.</param>
		/// <param name="charIndex">Char index.</param>
		private void _KerningCenterPivot(ref UIVertex uiVertex, int charIndex)
		{
			//NOTE:文字数が偶数の場合.
			if (_textLength % 2 == 0)
			{
				//NOTE:中央から左の文字は左へ.
				if (charIndex < _textLength / 2)
				{
					//NOTE:最も中央に近い文字だけ移動幅を半分に.
					if (_textLength / 2 - charIndex == 1)
					{
						uiVertex.position.x -= (float)(_textLength / 2 - charIndex) * (_kerning / 2f);
					}
					else
					{
						uiVertex.position.x -= (float)(_textLength / 2 - charIndex) * _kerning - (_kerning / 2f);
					}
				}
				//NOTE:中央から右の文字は右へ.
				else
				{
					//NOTE:最も中央に近い文字だけ移動幅を半分に.
					if (charIndex + 1 - _textLength / 2 == 1)
					{
						uiVertex.position.x += (float)(charIndex + 1 - _textLength / 2) * (_kerning / 2f);
					}
					else
					{
						uiVertex.position.x += (float)(charIndex + 1 - _textLength / 2) * _kerning - (_kerning / 2f);
					}
				}
			}
			//NOTE:文字数が奇数の場合.
			else
			{
				//NOTE:中央の文字は固定.
				if (charIndex == (_textLength - 1) / 2)
				{
					return;
				}
				//NOTE:中央文字より左側は左へ.
				else if (charIndex < (_textLength - 1) / 2)
				{
					uiVertex.position.x -= (float)((_textLength - 1) / 2 - charIndex) * _kerning;
				}
				//NOTE:中央文字より右側は右へ.
				else
				{
					uiVertex.position.x += (float)(charIndex - (_textLength - 1) / 2) * _kerning;
				}
			}
		}
	}
}
