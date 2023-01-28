using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace CleannetCode_bot.Features.Forwards
{
    internal interface IForwardHandler
    {
        Task HandleAsync(Message message, CancellationToken ct);
    }
}
