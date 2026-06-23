using System;
using UnityEngine;

public class StampToolPreviewContext
{
	public Transform previewParent;

	public InterfaceTool tool;

	public TemplateContainer stampTemplate;

	public System.Action frameAfterSetupFn = null;

	public Action<int> refreshFn = null;

	public System.Action onPlaceFn = null;

	public Action<string> onErrorChangeFn = null;

	public System.Action cleanupFn = null;
}
