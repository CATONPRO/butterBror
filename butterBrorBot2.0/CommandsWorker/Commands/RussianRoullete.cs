﻿using static butterBror.BotWorker.FileMng;
using static butterBror.BotWorker;
using butterBib;
using Discord;
using TwitchLib.Client.Enums;

namespace butterBror
{
    public partial class Commands
    {
        public class RussianRoullete
        {
            public static CommandInfo Info = new()
            {
                Name = "RussianRoullete",
                Author = "@ItzKITb",
                AuthorURL = "twitch.tv/itzkitb",
                AuthorImageURL = "https://static-cdn.jtvnw.net/jtv_user_pictures/c3a9af55-d7af-4b4a-82de-39a4d8b296d3-profile_image-70x70.png",
                Description = "Хотите проверить свою удачу? Вы можете проверить её тут!",
                UseURL = "https://itzkitb.ru/bot_command/tuck",
                UserCooldown = 5,
                GlobalCooldown = 1,
                aliases = ["rr", "russianroullete", "русскаярулетка", "рр"],
                ArgsRequired = "(Нету)",
                ResetCooldownIfItHasNotReachedZero = true,
                CreationDate = DateTime.Parse("08/08/2024"),
                ForAdmins = false,
                ForBotCreator = false,
                ForChannelAdmins = false
            };
            public static CommandReturn Index(CommandData data)
            {
                string resultMessage = "";
                Color resultColor = Color.Green;
                ChatColorPresets resultNicknameColor = ChatColorPresets.YellowGreen;
                Random rand = new Random();
                int win = rand.Next(1, 6);
                int page2 = rand.Next(1, 4);
                string translationParam = "russianRoullete";
                if (Tools.GetBalance(data.UserUUID) > 4)
                {
                    if (win < 3)
                    {
                        // WIN
                        translationParam += "Win" + page2;
                        Tools.SaveBalance(data.UserUUID, 1, 0);
                    }
                    else
                    {
                        // GAME OVER
                        translationParam += "Over" + page2;
                        if (page2 == 4)
                        {
                            Tools.SaveBalance(data.UserUUID, -1, 0);
                        }
                        else
                        {
                            Tools.SaveBalance(data.UserUUID, -5, 0);
                        }
                        resultNicknameColor = ChatColorPresets.Red;
                        resultColor = Color.Red;
                    }
                    resultMessage = "🔫 " + TranslationManager.GetTranslation(data.User.Lang, translationParam, data.ChannelID);
                }
                else
                {
                    resultMessage = TranslationManager.GetTranslation(data.User.Lang, "russianRoulleteNoMoney", data.ChannelID);
                }
                return new()
                {
                    Message = resultMessage,
                    IsSafeExecute = true,
                    Description = "",
                    Author = "",
                    ImageURL = "",
                    ThumbnailUrl = "",
                    Footer = "",
                    IsEmbed = true,
                    Ephemeral = false,
                    Title = "",
                    Color = resultColor,
                    NickNameColor = resultNicknameColor
                };
            }
        }
    }
}