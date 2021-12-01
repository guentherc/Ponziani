using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiComponents
{
    internal enum LogEntryType { EngineOutput, EngineInput };

    internal record LogEntry
    {
        public LogEntryType Type { get; init; }
        public string Message { get; init; }
    }

    public partial class EngineLog
    {
        /// <summary>
        /// Other HTML Attributes, which are applied to the root element of the rendered scoresheet.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> OtherAttributes { get; set; }

        private IJSObjectReference module;
        private DotNetObjectReference<EngineLog> objRef;
        private readonly List<LogEntry> Log = new();

        private string LogText => String.Join(Environment.NewLine, Log.Select((e) => e.Type == LogEntryType.EngineOutput ? "<= " + e.Message : "=> " + e.Message).Reverse());


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender || module == null)
            {
                module = await js.InvokeAsync<IJSObjectReference>("import", "./_content/PonzianiComponents/ponziani.js");
                if (module != null)
                {
                    objRef = DotNetObjectReference.Create(this);
                    await module.InvokeVoidAsync("initEngine", new object[] { objRef });
                }
            }
        }

        [JSInvokable]
        public async Task EngineMessageAsync(string msg)
        {
            Log.Add(new LogEntry { Type = LogEntryType.EngineOutput, Message = msg });
            StateHasChanged();
        }

        internal void AddEngineInputMessage(string msg)
        {
            Log.Add(new LogEntry { Type = LogEntryType.EngineInput, Message = msg });
            StateHasChanged();
        }
    }
}
