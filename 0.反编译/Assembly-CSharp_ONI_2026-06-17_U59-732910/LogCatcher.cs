using System;
using UnityEngine;

public class LogCatcher : ILogHandler
{
	private ILogHandler def;

	public LogCatcher(ILogHandler old)
	{
		def = old;
	}

	void ILogHandler.LogException(Exception exception, UnityEngine.Object context)
	{
		string text = exception.ToString();
		string text2 = ((context != null) ? context.ToString() : null);
		if (text == "False" || text2 == "False")
		{
			Debug.LogError("False only message!");
		}
		def.LogException(exception, context);
	}

	void ILogHandler.LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
	{
		if (string.Format(format, args) == "False")
		{
			Debug.LogError("False only message!");
		}
		def.LogFormat(logType, context, format, args);
	}
}
