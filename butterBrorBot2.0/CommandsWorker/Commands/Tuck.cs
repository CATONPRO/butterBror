﻿using static butterBror.BotWorker.FileMng;
using static butterBror.BotWorker;
using butterBib;
using Discord;
using TwitchLib.Client.Enums;

namespace butterBror
{
    public partial class Commands
    {
        public class Tuck
        {
            public static CommandInfo Info = new()
            {
                Name = "Tuck",
                Author = "@ItzKITb",
                AuthorURL = "twitch.tv/itzkitb",
                AuthorImageURL = "https://static-cdn.jtvnw.net/jtv_user_pictures/c3a9af55-d7af-4b4a-82de-39a4d8b296d3-profile_image-70x70.png",
                Description = "При помощи этой команды вы можете уложить чатера спать.",
                UseURL = "https://itzkitb.ru/bot_command/tuck",
                UserCooldown = 5,
                GlobalCooldown = 1,
                aliases = ["tuck", "уложить", "tk", "улож", "тык"],
                ArgsRequired = "(Чатер), (текст)",
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
                ChatColorPresets resultNicknameColor = ChatColorPresets.HotPink;
                if (data.args.Count >= 1)
                {
                    var username = Tools.NicknameFilter(Tools.FilterTextWithoutSpaces(data.args[0]));
                    var isSelectedUserIsNotIgnored = true;
                    var userID = Tools.GetUserID(username.ToLower());
                    try
                    {
                        if (userID != "err")
                        {
                            isSelectedUserIsNotIgnored = !UsersData.UserGetData<bool>(userID, "isIgnored");
                        }
                    }
                    catch (Exception) { }
                    if (username.ToLower() == Bot.BotNick.ToLower())
                    {
                        resultMessage =  TranslationManager.GetTranslation(data.User.Lang, "tuckThanks", data.ChannelID);
                        resultColor = Color.Blue;
                    }
                    else if (isSelectedUserIsNotIgnored)
                    {
                        if (data.args.Count >= 2)
                        {
                            List<string> list = data.args;
                            list.RemoveAt(0);
                            resultMessage = TranslationManager.GetTranslation(data.User.Lang, "tuckUserWithText", data.ChannelID).Replace("%user%", Tools.DontPingUsername(username)).Replace("%text%", string.Join(" ", list));
                        }
                        else
                        {
                            resultMessage = TranslationManager.GetTranslation(data.User.Lang, "tuckUser", data.ChannelID).Replace("%user%", Tools.DontPingUsername(username));
                        }
                    }
                    else
                    {
                        LogWorker.LogInfo($"Пользователь @{data.User.Name} пытался уложить пользователя, который находится в игноре", "CMD/tuck");
                        resultMessage = TranslationManager.GetTranslation(data.User.Lang, "tuckIgnored", data.ChannelID);
                    }
                }
                else
                {
                    resultMessage = TranslationManager.GetTranslation(data.User.Lang, "tuckNone", data.ChannelID);
                    resultColor = Color.LightGrey;
                    resultNicknameColor = ChatColorPresets.DodgerBlue;
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
                    IsEmbed = false,
                    Ephemeral = false,
                    Title = "",
                    Color = resultColor,
                    NickNameColor = resultNicknameColor
                };
            }
        }
    }
}