// Decompiled with JetBrains decompiler
// Type: Utils.CommandInterpolation.InterpolatedCommandFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.CommandInterpolation
{
  public class InterpolatedCommandFormatter
  {
    private readonly Stack<InterpolatedCommandFormatter.InterpolatedCommandFormatterContext> availableContexts;
    private Dictionary<string, Func<List<string>, string>> commands;

    public char StartClosure { get; set; }

    public char EndClosure { get; set; }

    public char Escape { get; set; }

    public char ArgumentSplitter { get; set; }

    public Dictionary<string, Func<List<string>, string>> Commands
    {
      get
      {
        return this.commands;
      }
      set
      {
        Dictionary<string, Func<List<string>, string>> dictionary = value;
        if (dictionary == null)
          throw new ArgumentNullException(nameof (value));
        this.commands = dictionary;
      }
    }

    public InterpolatedCommandFormatter(int initialContexts = 4)
    {
      this.availableContexts = new Stack<InterpolatedCommandFormatter.InterpolatedCommandFormatterContext>();
      for (int index = 0; index < initialContexts; ++index)
        this.availableContexts.Push(new InterpolatedCommandFormatter.InterpolatedCommandFormatterContext());
      this.Commands = new Dictionary<string, Func<List<string>, string>>();
    }

    private void ProcessInterpolation(
      string raw,
      InterpolatedCommandFormatter.InterpolatedCommandFormatterContext context)
    {
      bool flag = false;
      int num = 0;
      int startIndex = 0;
      for (int index = 0; index < raw.Length; ++index)
      {
        char ch = raw[index];
        if ((int) ch == (int) this.Escape && !flag)
          flag = true;
        else if (flag)
        {
          flag = false;
          switch (ch)
          {
            case '\\':
              context.Builder.Append('\\');
              continue;
            case 'n':
              context.Builder.Append('\n');
              continue;
            default:
              if ((int) ch != (int) this.StartClosure && (int) ch != (int) this.EndClosure && ((int) ch != (int) this.ArgumentSplitter && (int) ch != (int) this.Escape))
                throw new InvalidOperationException(string.Format("Unrecognized escape character at column {0}.", (object) index));
              continue;
          }
        }
        else if ((int) ch == (int) this.StartClosure)
        {
          if (num++ == 0)
            startIndex = index + 1;
        }
        else if ((int) ch == (int) this.EndClosure)
        {
          if (num-- == 0)
            throw new InvalidOperationException(string.Format("Unmatched closing character at column {0}.", (object) index));
          if (num == 0)
          {
            string processed = raw.Substring(startIndex, index - startIndex);
            if (!this.ProcessInterpolatedCommand(processed, context.Arguments, out processed))
              throw new InvalidOperationException(string.Format("Invalid command at column {0}: {1}", (object) startIndex, (object) string.Join(", ", context.Arguments.Select<string, string>((Func<string, string>) (x => "\"" + x + "\"")))));
            context.Arguments.Clear();
            context.Builder.Append(processed);
          }
        }
        else if (num == 0)
          context.Builder.Append(ch);
      }
      if (flag)
        throw new InvalidOperationException("Unable to end string with an escape character.");
      if (num > 0)
        throw new InvalidOperationException("Unmatched opening character(s).");
    }

    private bool ProcessInterpolatedCommand(
      string raw,
      List<string> argumentBuffer,
      out string processed)
    {
      this.ProcessArguments(raw, (ICollection<string>) argumentBuffer);
      Func<List<string>, string> func;
      if (this.Commands.TryGetValue(argumentBuffer[0], out func))
      {
        processed = func(argumentBuffer);
        return true;
      }
      processed = (string) null;
      return false;
    }

    private void ProcessArguments(string raw, ICollection<string> arguments)
    {
      int startIndex = 0;
      int num = 0;
      bool flag = false;
      for (int index = 0; index < raw.Length; ++index)
      {
        char ch = raw[index];
        if (flag)
          flag = false;
        else if ((int) ch == (int) this.Escape)
          flag = true;
        else if ((int) ch == (int) this.StartClosure)
          ++num;
        else if ((int) ch == (int) this.EndClosure)
          --num;
        else if ((int) ch == (int) this.ArgumentSplitter && num == 0)
        {
          arguments.Add(raw.Substring(startIndex, index - startIndex));
          startIndex = index + 1;
        }
      }
      arguments.Add(raw.Substring(startIndex, raw.Length - startIndex));
    }

    private InterpolatedCommandFormatter.InterpolatedCommandFormatterContext SafePopContext()
    {
      return this.availableContexts.Count != 0 ? this.availableContexts.Pop() : new InterpolatedCommandFormatter.InterpolatedCommandFormatterContext();
    }

    public bool TryProcessExpression(string raw, string source, out string result)
    {
      bool flag = false;
      try
      {
        result = this.ProcessExpression(raw);
        flag = true;
      }
      catch (InvalidOperationException ex)
      {
        result = "Command interpolation (" + source + ") threw an error: " + ex.Message;
      }
      catch (CommandInputException ex)
      {
        string str = ex.ArgumentValue is IEnumerable<object> argumentValue ? string.Join(", ", argumentValue.Select<object, string>((Func<object, string>) (x => string.Format("\"{0}\"", x)))) : ex.ArgumentValue.ToString();
        result = "A command errored in " + source + " command interpolation: " + ex.Message + "\nArgument name: " + ex.ArgumentName + "\nArgument value: " + str;
      }
      return flag;
    }

    public string ProcessExpression(string raw)
    {
      InterpolatedCommandFormatter.InterpolatedCommandFormatterContext context = this.SafePopContext();
      this.ProcessInterpolation(raw, context);
      string str = context.Builder.ToString();
      context.Clear();
      this.availableContexts.Push(context);
      return str;
    }

    private class InterpolatedCommandFormatterContext
    {
      public List<string> Arguments { get; }

      public StringBuilder Builder { get; }

      public InterpolatedCommandFormatterContext()
      {
        this.Arguments = new List<string>();
        this.Builder = new StringBuilder();
      }

      public void Clear()
      {
        this.Arguments.Clear();
        this.Builder.Clear();
      }
    }
  }
}
