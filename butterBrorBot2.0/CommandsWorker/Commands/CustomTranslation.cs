﻿using butterBib;
using Discord;
using TwitchLib.Client.Enums;
using static butterBror.BotWorker.TranslationManager;

namespace butterBror
{
    public partial class Commands
    {
        public class CustomTranslation
        {
            public static CommandInfo Info = new()
            {
                Name = "CustomTranslation",
                Author = "@ItzKITb",
                AuthorURL = "twitch.tv/itzkitb",
                AuthorImageURL = "https://static-cdn.jtvnw.net/jtv_user_pictures/c3a9af55-d7af-4b4a-82de-39a4d8b296d3-profile_image-70x70.png",
                Description = "При помощи этой команды вы можете сделать кастомный перевод сообщений бота для канала.",
                UseURL = "NONE",
                UserCooldown = 10,
                GlobalCooldown = 5,
                aliases = ["CustomTranslation", "ct", "кастомныйперевод", "кп"],
                ArgsRequired = "(set [paramName] [en/ru] [text]/get [paramName] [en/ru]/original [paramName] [en/ru]/reset [paramName] [en/ru])",
                ResetCooldownIfItHasNotReachedZero = true,
                CreationDate = DateTime.Parse("08/05/2024"),
                ForAdmins = false,
                ForBotCreator = false,
                ForChannelAdmins = false
            };
            public static CommandReturn Index(CommandData data)
            {
                Color resultColor = Color.Green;
                ChatColorPresets resultNicknameColor = ChatColorPresets.YellowGreen;

                string resultMessage = "";
                string[] setAliases = ["set", "s", "установить", "сет", "с", "у"];
                string[] getAliases = ["get", "g", "гет", "получить", "п", "г"];
                string[] originalAliases = ["original", "оригинал", "о", "o"];
                string[] deleteAliases = ["delete", "del", "d", "remove", "reset", "сбросить", "удалить", "с"];

                string[] langs = ["ru", "en"];

                string[] uneditableitems = ["botInfo", "cantSend", "lowArgs", "lang", "wrongArgs", "changedLang", "commandDoesntWork", "noneExistUser", "noAccess", "userBanned", 
                    "userPardon", "rejoinedChannel", "joinedChannel", "leavedChannel", "modAdded", "modDel", "addedChannel", "delChannel", "welcomeChannel", "error", "botVerified",
                    "unhandledError", "Err"];

                if (data.args.Count >= 3)
                {
                    try
                    {
                        string arg1 = data.args[0];
                        string paramName = data.args[1];
                        string lang = data.args[2];
                        if (langs.Contains(lang))
                        {
                            if (setAliases.Contains(arg1) && ((bool)data.User.IsChannelAdmin || (bool)data.User.IsChannelBroadcaster || (bool)data.User.IsBotAdmin))
                            {
                                if (data.args.Count > 3)
                                {
                                    // Скипнуть все лишнее
                                    List<string> textArgs = data.args;
                                    textArgs = textArgs.Skip(3).ToList();
                                    string text = string.Join(' ', textArgs);
                                    // Установить кастомный перевод
                                    if (uneditableitems.Contains(paramName))
                                    {
                                        // ТЫ НЕ ПРОЙДЕШЬ!
                                        resultMessage = GetTranslation(data.User.Lang, "customTranslationSecured", "");
                                        resultNicknameColor = ChatColorPresets.Red;
                                        resultColor = Color.Red;
                                    }
                                    else
                                    {
                                        if (TranslateContains(paramName))
                                        {
                                            if (SetCustomTranslation(paramName, text, data.ChannelID, lang))
                                            {
                                                UpdateTranslation(lang, data.ChannelID);
                                                // Ура
                                                resultMessage = GetTranslation(data.User.Lang, "customTranslationSetted", "").Replace("%key%", paramName);
                                            }
                                            else
                                            {
                                                // Не ура
                                                resultMessage = GetTranslation(data.User.Lang, "customTranslationSettingError", "");
                                                resultNicknameColor = ChatColorPresets.Red;
                                                resultColor = Color.Red;
                                            }
                                        }
                                        else
                                        {
                                            // И че ты изменить хочешь?
                                            resultMessage = GetTranslation(data.User.Lang, "customTranslationSettingWhat", "");
                                            resultNicknameColor = ChatColorPresets.GoldenRod;
                                            resultColor = Color.Gold;
                                        }
                                    }
                                }
                                else
                                {
                                    // Почему
                                    resultMessage = GetTranslation(data.User.Lang, "lowArgs", "").Replace("%commandWorks%", "#ct set [paramName] [en/ru] [text]");
                                    resultNicknameColor = ChatColorPresets.Red;
                                    resultColor = Color.Red;
                                }
                            }
                            else if (getAliases.Contains(arg1) && ((bool)data.User.IsChannelAdmin || (bool)data.User.IsChannelBroadcaster || (bool)data.User.IsBotAdmin))
                            {
                                // Получить перевод
                                if (TranslateContains(paramName))
                                {
                                    resultMessage = GetTranslation(data.User.Lang, "customTranslationGetted", "").Replace("%key%", paramName).Replace("%translation%", GetTranslation(lang, paramName, data.ChannelID));
                                }
                                else
                                {
                                    // И че ты изменить хочешь?
                                    resultMessage = GetTranslation(data.User.Lang, "customTranslationGettingWhat", "");
                                    resultNicknameColor = ChatColorPresets.GoldenRod;
                                    resultColor = Color.Gold;
                                }
                            }
                            else if (originalAliases.Contains(arg1))
                            {
                                resultMessage = GetTranslation(data.User.Lang, "customTranslationOriginal", "").Replace("%key%", paramName).Replace("%translation%", GetTranslation(lang, paramName, ""));
                            }
                            else if (deleteAliases.Contains(arg1) && ((bool)data.User.IsChannelAdmin || (bool)data.User.IsChannelBroadcaster || (bool)data.User.IsBotAdmin))
                            {
                                // Удалить кастомный перевод
                                if (TranslateContains(paramName))
                                {
                                    if (DeleteCustomTranslation(paramName, data.ChannelID, lang, data.Platform))
                                    {
                                        // Ура
                                        resultMessage = GetTranslation(data.User.Lang, "customTranslationDeleted", "").Replace("%key%", paramName);
                                        UpdateTranslation(lang, data.ChannelID);
                                    }
                                    else
                                    {
                                        // Не ура
                                        resultMessage = GetTranslation(data.User.Lang, "customTranslationDeletingError", "");
                                        resultNicknameColor = ChatColorPresets.Red;
                                        resultColor = Color.Red;
                                    }
                                }
                                else
                                {
                                    // И че ты изменить хочешь?
                                    resultMessage = GetTranslation(data.User.Lang, "customTranslationDeletingWhat", "");
                                    resultNicknameColor = ChatColorPresets.GoldenRod;
                                    resultColor = Color.Gold;
                                }
                            }
                        }
                        else
                        {
                            // Нет такого языка!
                            resultMessage = GetTranslation(data.User.Lang, "customTranslationWrongLang", "").Replace("%lang%", lang).Replace("%langs%", string.Join(", ", langs));
                            resultNicknameColor = ChatColorPresets.Red;
                            resultColor = Color.Red;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message + " | " + ex.StackTrace);
                        resultMessage = GetTranslation(data.User.Lang, "error", data.Channel);
                        resultNicknameColor = ChatColorPresets.Red;
                        resultColor = Color.Red;
                    }
                }
                else
                {
                    // If args lenght < 2
                    resultMessage = GetTranslation(data.User.Lang, "lowArgs", "").Replace("%commandWorks%", "#ct set [paramName] [en/ru] [text]");
                    resultNicknameColor = ChatColorPresets.Red;
                    resultColor = Color.Red;
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