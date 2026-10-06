using Brigadier.NET;
using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using MinecraftClient.CommandHandler;
using System;
using static MinecraftClient.Settings;

namespace MinecraftClient.Commands
{
    public class QueueCommand : Command
    {
        static string lang=Config.Main.Advanced.Language;
        string in_queue = lang switch
        {
            "zh_cn" => "你目前在队列中 ",
            _ => "You are in queue."
        };
        string Show_queue_status = lang switch
        {
            "zh_cn" => "显示队列状态",
            _ => "Show queue status"
        };
        string in_main = lang switch
        {
            "zh_cn" => "你目前在主服",
            _ => "You are currently in the main server."
        };
        string Position = lang switch
        {
            "zh_cn" => "队列位置",
            _ => "Position"
        };
        string Total_queue = lang switch
        {
            "zh_cn" => "队列总长",
            _ => "Total queue"
        };
        string no_data_for_eta = lang switch
        {
            "zh_cn" => "没有足够的信息计算剩余时长",
            _ => "Not enough data to estimate ETA."
        };
        string eta = lang switch
        {
            "zh_cn" => "预计剩余时长",
            _ => "ETA"
        };
        string Invalid_number = lang switch
        {
            "zh_cn" => "无效数字",
            _ => "Invalid number"
        };
        public override string CmdName => "queue";
        public override string CmdDesc => Show_queue_status;
        public override string CmdUsage => "queue [total|eta <-number>]";
        public override void RegisterCommand(CommandDispatcher<CmdResult> dispatcher)
        {
            dispatcher.Register(l => l.Literal("help")
                .Then(l => l.Literal(CmdName)
                    .Executes(r => GetUsage(r.Source, string.Empty))
                )
            );

            dispatcher.Register(l => l.Literal(CmdName)

                // /queue
                .Executes(r =>
                {
                    var handler = CmdResult.currentHandler;  // ⭐ 正確來源

                    if (handler == null)
                        return r.Source.SetAndReturn("Handler not available.");

                    if (handler.isinqueue())
                    {
                        return r.Source.SetAndReturn(
                    $"{in_queue} {Position}: {handler.GetqueuePosition()}"
                        );
                    }
                    else
                    {
                        return r.Source.SetAndReturn(
                            $"{in_main}"
                        );
                    }
                })

                // /queue total
                .Then(l => l.Literal("total")
                    .Executes(r =>
                    {
                        var handler = CmdResult.currentHandler;

                        if (handler == null)
                            return r.Source.SetAndReturn("Handler not available.");

                        return r.Source.SetAndReturn(
                            $"{Total_queue}: {handler.Gettotalqueue()}"
                        );
                    })
                )
                .Then(l => l.Literal("eta")

                    // /queue eta
                    .Executes(r => HandleEta(r, null))

                    // /queue eta <anything>
                    .Then(l => l.Argument("arg", Arguments.String())
                        .Executes(r =>
                        {
                            string arg = r.GetArgument<string>("arg");
                            return HandleEta(r, arg);
                        })
                    )
                )
                .Then(l => l.Literal("_help")
                    .Executes(r => GetUsage(r.Source, string.Empty))
                    .Redirect(dispatcher.GetRoot().GetChild("help").GetChild(CmdName)))
            );
        }
        private int HandleEta(CommandContext<CmdResult> r, string? arg)
        {
            var handler = CmdResult.currentHandler;

            if (handler == null)
                return r.Source.SetAndReturn("Handler not available.");

            int targetPosition;
            double etaSeconds;
            double speed = handler.GetAvgSpeed();
            if (arg != null)
            {
                if (!int.TryParse(arg, out targetPosition))
                    return r.Source.SetAndReturn($"{Invalid_number}");
            }
            else
            {
                if (!handler.isinqueue())
                    return r.Source.SetAndReturn($"{in_main}");
                etaSeconds = handler.GetqueuePosition() / speed;
                return r.Source.SetAndReturn($"{eta}: {TimeSpan.FromSeconds(etaSeconds):hh\\:mm\\:ss}");
            }


            if (speed <= 0)
                return r.Source.SetAndReturn("{no_data_for_eta}");

            etaSeconds = targetPosition / speed;

            return r.Source.SetAndReturn(
                lang switch
                {
                    "zh_cn" => $"队列位置 = {targetPosition} 处的预计剩余时长:{TimeSpan.FromSeconds(etaSeconds):hh\\:mm\\:ss}",
                    _ => $"ETA for position {targetPosition}: {TimeSpan.FromSeconds(etaSeconds):hh\\:mm\\:ss}"
                }
            );
        }
        private int GetUsage(CmdResult r, string? cmd)
        {
            return r.SetAndReturn(cmd switch
            {
                _ => GetCmdDescTranslated(),
            });
        }
    }
}