﻿using static butterBror.BotWorker.FileMng;
using static butterBror.BotWorker;
using TwitchLib.Client.Events;
using butterBib;
using Discord;
using TwitchLib.Client.Enums;

namespace butterBror
{
    public partial class Commands
    {
        public class LastGlobalLine
        {
            public static CommandInfo Info = new()
            {
                Name = "LastGlobalLine",
                Author = "@ItzKITb",
                AuthorURL = "twitch.tv/itzkitb",
                AuthorImageURL = "https://static-cdn.jtvnw.net/jtv_user_pictures/c3a9af55-d7af-4b4a-82de-39a4d8b296d3-profile_image-70x70.png",
                Description = "Эта команда позволяет узнать время и содержание самого последнего сообщения пользователя.",
                UseURL = "https://itzkitb.ru/bot_command/lgl",
                UserCooldown = 10,
                GlobalCooldown = 1,
                aliases = ["lgl", "lastgloballine", "пгс", "последнееглобальноесообщение"],
                ArgsRequired = "[Имя пользователя]",
                ResetCooldownIfItHasNotReachedZero = true,
                CreationDate = DateTime.Parse("07/04/2024"),
                ForAdmins = false,
                ForBotCreator = false,
                ForChannelAdmins = false
            };
            public static CommandReturn Index(CommandData data)
            {
                string resultMessage = "";
                Color resultColor = Color.Green;
                ChatColorPresets resultNicknameColor = ChatColorPresets.YellowGreen;
                string resultMessageTitle = TranslationManager.GetTranslation(data.User.Lang, "dsLGLTitle", data.ChannelID);
                try
                {
                    if (data.args.Count != 0)
                    {
                        var name = Tools.NicknameFilter(data.args.ElementAt(0).ToLower());
                        var userID = Tools.GetUserID(name);
                        if (userID == "err")
                        {
                            resultMessage = TranslationManager.GetTranslation(data.User.Lang, "noneExistUser", data.ChannelID)
                                .Replace("%user%", Tools.DontPingUsername(name));
                            resultMessageTitle = TranslationManager.GetTranslation(data.User.Lang, "Err", data.ChannelID);
                            resultColor = Color.Red;
                            resultNicknameColor = ChatColorPresets.Red;
                        }
                        else
                        {
                            var lastLine = UsersData.UserGetData<string>(userID, "lastSeenMessage");
                            var lastLineDate = UsersData.UserGetData<DateTime>(userID, "lastSeen");
                            DateTime now = DateTime.UtcNow;
                            if (name == Bot.client.TwitchUsername.ToLower())
                            {
                                resultMessage = TranslationManager.GetTranslation(data.User.Lang, "lastGlobalLineWait", data.ChannelID);
                            }
                            else if (name == data.User.Name.ToLower())
                            {
                                resultMessage = TranslationManager.GetTranslation(data.User.Lang, "youRightThere", data.ChannelID);
                            }
                            else
                            {
                                resultMessage = TranslationManager.GetTranslation(data.User.Lang, "lastGlobalLine", data.ChannelID)
                                    .Replace("%user%", Tools.DontPingUsername(Tools.GetUsername(userID, data.User.Name)))
                                    .Replace("&timeAgo&", BotWorker.Tools.FormatTimeSpan(BotWorker.Tools.GetTimeTo(lastLineDate, now, false), data.User.Lang))
                                    .Replace("%message%", lastLine);
                            }
                        }
                    }
                    else
                    {
                        resultMessage = TranslationManager.GetTranslation(data.User.Lang, "youRightThere", data.ChannelID);
                    }
                }
                catch (Exception ex)
                {
                    Tools.ErrorOccured(ex.Message, "cmd2A");
                    resultMessage = TranslationManager.GetTranslation(data.User.Lang, "error", data.ChannelID);
                    resultMessageTitle = TranslationManager.GetTranslation(data.User.Lang, "Err", data.ChannelID);
                    resultColor = Color.Red;
                    resultNicknameColor = ChatColorPresets.Red;
                }
                return new()
                {
                    Message = resultMessage,
                    IsSafeExecute = false,
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
