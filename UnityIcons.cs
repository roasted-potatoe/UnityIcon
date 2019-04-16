using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class UnityIcons : EditorWindow
{
	[MenuItem("Window/Unity Icons")]
	private static void OpenWindow()
	{
		GetWindow<UnityIcons>("Unity Icons");
	}

	private static readonly Vector2 ICON_ITEM_SIZE = new Vector2(100f, 100f);
	private static readonly Vector2 STYLE_ITEM_SIZE = new Vector2(300f, 50f);

	private string m_SearchFilter = "";
	private State m_State;
	private Vector2 m_StyleScroll;
	private Vector2 m_IconScroll;

	private List<Texture> m_AllIcons = new List<Texture>();
	private List<Texture> m_FilteredIcons;

	private List<GUIStyle> m_AllStyles = new List<GUIStyle>();
	private List<GUIStyle> m_FilteredStyles;

	private enum State
	{
		Icons,
		Styles,
	}

	private GUIStyle m_BoxStyle = null;

	private GUIStyle boxStyle
	{
		get
		{
			return m_BoxStyle ?? (m_BoxStyle = new GUIStyle("HelpBox")
			{
				alignment = TextAnchor.MiddleCenter
			});
		}
	}

	private GUIStyle m_CenteredStyle = null;

	private GUIStyle centeredStyle
	{
		get
		{
			return m_CenteredStyle ?? (m_CenteredStyle = new GUIStyle("Label")
			{
				alignment = TextAnchor.MiddleCenter
			});
		}
	}

	private GUIStyle m_RightStyle = null;

	private GUIStyle rightStyle
	{
		get
		{
			return m_RightStyle ?? (m_RightStyle = new GUIStyle("Label")
			{
				alignment = TextAnchor.MiddleRight
			});
		}
	}

	private void OnFocus()
	{
		UpdateLists();
		FilterLists();
	}

	private void OnGUI()
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Toggle(m_State == State.Styles, "Styles", EditorStyles.toolbarButton))
		{
			m_State = State.Styles;
		}

		if (GUILayout.Toggle(m_State == State.Icons, "Icons", EditorStyles.toolbarButton))
		{
			m_State = State.Icons;
		}

		if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(75f)))
		{
			UpdateLists();
			FilterLists();
		}

		GUILayout.EndHorizontal();

		UpdateLists();

		string result = SearchField(m_SearchFilter);
		if (result != m_SearchFilter || m_AllIcons.Any(x => x == null) || m_AllStyles.Any(x => x == null))
		{
			m_SearchFilter = result;
			FilterLists();
		}

		switch (m_State)
		{
			case State.Styles:
				DrawList(m_FilteredStyles, DrawStyle, STYLE_ITEM_SIZE, ref m_StyleScroll);
				break;

			case State.Icons:
				DrawList(m_FilteredIcons, DrawIcon, ICON_ITEM_SIZE, ref m_IconScroll);
				break;
		}
	}

	private void FilterLists()
	{
		m_FilteredIcons = m_AllIcons
			.Where(x => x != null && x.name.ToLower().Contains(m_SearchFilter))
			.ToList();

		m_FilteredStyles = m_AllStyles
			.Where(x => x != null && x.name.ToLower().Contains(m_SearchFilter))
			.ToList();
	}

	private void UpdateLists()
	{
		if (m_AllStyles.Count == 0)
		{
			m_AllStyles = GUI.skin.customStyles
				.OrderBy(x => x.name)
				.ToList();
		}

		if (m_FilteredStyles == null)
		{
			m_FilteredStyles = m_AllStyles;
		}

		if (m_AllIcons.Count == 0)
		{
			m_AllIcons = Resources.FindObjectsOfTypeAll<Texture>()
				.OrderBy(x => x.name)
				.ToList();
		}

		if (m_FilteredIcons == null)
		{
			m_FilteredIcons = m_AllIcons;
		}
	}

	private void DrawList<T>(IEnumerable<T> _list, Action<Rect, T> _drawCallback, Vector2 _itemSize, ref Vector2 _scroll)
	{
		_scroll = GUILayout.BeginScrollView(_scroll);
		using (GridScope grid = new GridScope(Mathf.FloorToInt(position.width / _itemSize.x), _itemSize))
		{
			foreach (T item in _list)
			{
				if (item == null)
				{
					continue;
				}

				Rect rect = grid.Next();
				if (!IsInScreen(rect.y, m_StyleScroll, _itemSize))
				{
					continue;
				}

				_drawCallback(rect, item);
			}
		}

		GUILayout.EndScrollView();
	}

	private void DrawStyle(Rect _rect, GUIStyle _style)
	{
		_rect.x += 5f;
		_rect.y += 5f;
		_rect.width -= 10f;
		_rect.height -= 10f;

		Rect bg = new Rect(_rect)
		{
			height = _rect.height - EditorGUIUtility.singleLineHeight,
		};

		GUI.Label(bg, "", boxStyle);

		if (Event.current.type == EventType.Repaint)
		{
			Rect rectInactive = new Rect(_rect)
			{
				width = _rect.width / 3f,
			};

			Rect rectActive = new Rect(_rect)
			{
				x = rectInactive.x + rectInactive.width,
			};

			Rect rectPressed = new Rect(_rect)
			{
				x = rectActive.x + rectActive.width,
			};

			_style.Draw(rectInactive, "Inactive", false, false, false, false);
			_style.Draw(rectActive, "Active", false, true, false, false);
			_style.Draw(rectPressed, "Pressed", false, false, true, false);
		}

		Rect label = new Rect(_rect)
		{
			height = EditorGUIUtility.singleLineHeight,
			y = _rect.y + (_rect.height - EditorGUIUtility.singleLineHeight)
		};

		if (GUI.Button(label, _style.name))
		{
			GUIUtility.systemCopyBuffer = string.Format("(GUIStyle)\"{0}\"", _style.name);
			Debug.Log("Copied to buffer : " + GUIUtility.systemCopyBuffer);
		}
	}

	private void DrawIcon(Rect _rect, Texture _resource)
	{
		if (_resource == null)
		{
			return;
		}

		Rect texture = new Rect(_rect)
		{
			height = _rect.height - EditorGUIUtility.singleLineHeight,
		};

		GUI.Label(texture, _resource, boxStyle);

		Rect size = new Rect(texture)
		{
			height = EditorGUIUtility.singleLineHeight,
			y = texture.y + (texture.height - EditorGUIUtility.singleLineHeight)
		};

		GUI.Label(size, _resource.width + "x" + _resource.height, rightStyle);

		Rect label = new Rect(_rect)
		{
			height = EditorGUIUtility.singleLineHeight,
			y = _rect.y + (_rect.height - EditorGUIUtility.singleLineHeight)
		};

		if (GUI.Button(label, _resource.name))
		{
			const string findTextureText = "EditorGUIUtility.FindTexture(\"{0}\")";
			const string loadTextureText = "(Texture)EditorGUIUtility.Load(\"{0}\")";

			Texture2D findTexture = EditorGUIUtility.FindTexture(_resource.name);
			string toCopy = string.Format(findTexture == null ? loadTextureText : findTextureText, _resource.name);
			GUIUtility.systemCopyBuffer = toCopy;
		}

		if ((_resource.width > ICON_ITEM_SIZE.x || _resource.height > ICON_ITEM_SIZE.y) && texture.Contains(Event.current.mousePosition))
		{
			Rect overview = new Rect(texture)
			{
				x = texture.x - (_resource.width / 2f),
				y = texture.y - (_resource.height / 2f),
				width = _resource.width,
				height = _resource.height,
			};

			GUI.Label(overview, _resource, centeredStyle);
		}
	}

	private bool IsInScreen(float _itemYPos, Vector2 _scrollPos, Vector2 _itemSize)
	{
		return _itemYPos > _scrollPos.y - _itemSize.y || _itemYPos < _scrollPos.y + position.height;
	}

	private void Update()
	{
		Repaint();
	}

	public static string SearchField(string _content)
	{
		Rect rect = GUILayoutUtility.GetRect(GUIContent.none, "SearchTextField");

		Rect fieldRect = new Rect(rect)
		{
			width = rect.width - 25f,
		};

		Rect btnRect = new Rect(rect)
		{
			x = fieldRect.x + fieldRect.width,
			width = 25f,
		};

		EditorGUI.BeginChangeCheck();
		_content = EditorGUI.TextField(fieldRect, _content, (GUIStyle)"SearchTextField");
		if (EditorGUI.EndChangeCheck())
		{
			GUI.changed = true;
		}

		if (string.IsNullOrEmpty(_content))
		{
			GUI.Button(btnRect, "", (GUIStyle)"SearchCancelButtonEmpty");
		}
		else
		{
			if (GUI.Button(btnRect, "", (GUIStyle)"SearchCancelButton"))
			{
				_content = "";
				GUI.changed = true;
				EditorGUIUtility.editingTextField = false;
			}
		}

		return _content;
	}

	public class GridScope : GUI.Scope
	{
		private Vector2 m_Size;
		private GUILayoutOption[] m_GetRectOptions;
		private readonly int m_MaxRows;
		private int m_CurrentRow;

		public GridScope(int _rows, Vector2 _itemSize)
		{
			m_Size = _itemSize;
			m_MaxRows = _rows;

			m_GetRectOptions = new[]
			{
				GUILayout.MaxWidth(m_Size.x),
				GUILayout.MaxHeight(m_Size.y)
			};

			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
		}

		public Rect Next()
		{
			if (m_CurrentRow >= m_MaxRows)
			{
				m_CurrentRow = 0;
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
			}

			m_CurrentRow++;

			return GUILayoutUtility.GetRect(m_Size.x, m_Size.y, m_GetRectOptions);
		}

		protected override void CloseScope()
		{
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}
	}
}