﻿using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using butterBror.Utils.DataManagers;
using butterBror.Utils;

namespace butterBror
{
    public class BotEngine
    {
        static Bot bot = new();
        public static int restartedTimes = 0;
        public static int CompletedCommands = 0;
        public static int buttersTotalUsers = 0;
        public static DateTime StartTime = new();
        public static bool isNeedRestart = false;
        public static bool isTwitchReady = false;
        public static float buttersTotalAmount = 0;
        public static string botVersion = "2.09.1";
        public static DataManager currencyWorker = new();
        public static int buttersTotalDollarsInTheBank = 0;
        private static DateTime statSenderTimer = DateTime.UtcNow;
        private static float previousSaveButtersAmount = 0;

        static void Main(string[] args)
        {
            try
            {
                DebugUtil.IsDebugEnabled = false;
                statSenderTimer = DateTime.UtcNow;
                StartTime = DateTime.Now;

                Task task = Task.Run(() =>
                {
                    BotR(args);
                });

                Timer TitleUpdateTimer = new(callback: timerTitle, null, 0, 1000);
                while (true)
                {
                    Thread.Sleep(1000);
                    if (isNeedRestart)
                    {
                        restartedTimes++;
                        isNeedRestart = false;
                        ConsoleUtil.LOG("Перезапуск...");
                        Console.WriteLine(" +");
                        Console.WriteLine("");
                        try { task.Dispose(); } catch (Exception) { }
                        task = Task.Run(() =>
                        {
                            BotR(args);
                        });
                    }
                }
            }
            catch (Exception e)
            {
                string ErrorText = $"ФАТАЛЬНАЯ ОШИБКА ДВИЖКА: {e.Message} : {e.Source}";
                LogWorker.Log(ErrorText, LogWorker.LogTypes.Err, "BotEngine\\Main");
                ConsoleUtil.LOG(ErrorText, ConsoleColor.Black, ConsoleColor.Red);
                ConsoleUtil.LOG("Перезапуск...");
                Task.Delay(1000);
                Main(args);
            }
        }
        public static void timerTitle(object timer)
        {
            TitleUpdate();
        }
        public static async Task TitleUpdate()
        {
            if (!isNeedRestart && buttersTotalAmount != 0 && buttersTotalUsers != 0 && previousSaveButtersAmount != buttersTotalAmount)
            {
                Task task = Task.Run(() =>
                {
                    var date = DateTime.UtcNow;
                    Dictionary<string, dynamic> currencyData = new Dictionary<string, dynamic>();
                    currencyData.Add("amount", buttersTotalAmount);
                    currencyData.Add("users", buttersTotalUsers);
                    currencyData.Add("dollars", buttersTotalDollarsInTheBank);
                    currencyData.Add("cost", buttersTotalDollarsInTheBank / buttersTotalAmount);
                    currencyData.Add("middleBalance", buttersTotalAmount / buttersTotalUsers);
                    DataManager.SaveData(Bot.MainPath + "currencyReadable.json", $"{date.Day}.{date.Month}.{date.Year}", currencyData);

                    DataManager.SaveData(Bot.CurrencyPath, "totalAmount", buttersTotalAmount, false);
                    DataManager.SaveData(Bot.CurrencyPath, "totalUsers", buttersTotalUsers, false);
                    DataManager.SaveData(Bot.CurrencyPath, "totalDollarsInTheBank", buttersTotalDollarsInTheBank, false);

                    DataManager.SaveData(Bot.CurrencyPath, $"{date.Day}.{date.Month}.{date.Year}butter'sCost", float.Parse((buttersTotalDollarsInTheBank / buttersTotalAmount).ToString("0.00")), false);
                    DataManager.SaveData(Bot.CurrencyPath, $"{date.Day}.{date.Month}.{date.Year}totalAmount", buttersTotalAmount, false);
                    DataManager.SaveData(Bot.CurrencyPath, $"{date.Day}.{date.Month}.{date.Year}totalUsers", buttersTotalUsers, false);
                    DataManager.SaveData(Bot.CurrencyPath, $"{date.Day}.{date.Month}.{date.Year}totalDollarsInTheBank", buttersTotalDollarsInTheBank, false);

                    DataManager.SaveData(Bot.CurrencyPath);
                    previousSaveButtersAmount = buttersTotalAmount;
                });
            }
            if (DateTime.UtcNow.Minute % 10 == 0 && (DateTime.UtcNow - statSenderTimer).TotalMinutes >= 1 && isTwitchReady)
            {
                statSenderTimer = DateTime.UtcNow;
                Task task2 = Task.Run(() =>
                {
                    bot.StatusSender();
                });
            }
            var workTime = DateTime.Now - StartTime;
            var TimeNow = DateTime.UtcNow;
            ConsoleUtil.Title(botVersion, CompletedCommands, Bot.ReadedMessages, workTime);
            Random rand = new();
            Ping ping = new();
            long pingSpeed = 0;
            try
            {
                PingReply reply = await ping.SendPingAsync("twitch.tv", 1000);
                if (reply.Status == IPStatus.Success)
                {
                    pingSpeed = reply.RoundtripTime;
                }
                if (pingSpeed >= 60 && pingSpeed <= 70 && rand.Next(0, 2) == 1)
                {
                    pingSpeed = 69;
                }
            }
            catch{}
            Process process = Process.GetCurrentProcess();
            long workingAppSet = process.WorkingSet64 / (1024 * 1024);
            ConsoleServer.SendConsoleMessage("data", $"Время работы: {workTime.ToString(@"dd\.hh\:mm")}, Пинг с twitch: {pingSpeed}ms, Памяти занято процессом: {workingAppSet}мб, Бутеров в банке: {buttersTotalAmount}, Зарегистрированно пользователей: {buttersTotalUsers}");
        } // Таймер для обновления заголовка консоли
        private static void BotR(string[] args)
        {
            try
            {
                bot = new();
                bot.Start(args, restartedTimes);
            }
            catch (Exception)
            {
                isNeedRestart = true;
            }
        }
    }
    // #BOT
    public class Bot
    {
        public static readonly string ProgramPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\ItzKITb\\";
        public static readonly string MainPath = ProgramPath + "butterBror\\";
        public static readonly string ChannelsPath = MainPath + "CHNLS\\";
        public static readonly string UsersDataPath = MainPath + "USERSDB\\";
        public static readonly string UsersBankDataPath = MainPath + "BANKDB\\";
        public static readonly string ConvertorsPath = MainPath + "CONVRT\\";
        public static readonly string NicknameToIDPath = ConvertorsPath + "N2I\\";
        public static readonly string IDToNicknamePath = ConvertorsPath + "I2N\\";
        public static readonly string SettingsPath = MainPath + "SETTINGS.json";
        public static readonly string CookiesPath = MainPath + "COOKIES.MDS";
        public static readonly string TranslatePath = MainPath + "TRNSLT\\";
        public static readonly string TranslateDefualtPath = MainPath + "TRNSLT\\DEFAULT\\";
        public static readonly string TranslateCustomPath = MainPath + "TRNSLT\\CUSTOM\\";
        public static readonly string BanWordsPath = MainPath + "BNWORDS.txt";
        public static readonly string BanWordsReplacementPath = MainPath + "BNWORDSREP.txt";
        public static readonly string APIUseDataPath = MainPath + "API.json";
        public static readonly string LogsPath = MainPath + "LOGS.log";
        public static readonly string ErrorsPath = MainPath + "ERRORS.log";
        public static readonly string LocationsCachePath = MainPath + "LOC.cache";
        public static readonly string CurrencyPath = MainPath + "CURR.json";
        public static TwitchTokenUtil tokenGetter = new(ClientID, Secret, "database.db");
        public static readonly CultureInfo LOL = new("fr-fr");
        public static readonly string ReserveCopyPath = ProgramPath + "bbRESERVE\\" + DateTime.UtcNow.Year + DateTime.UtcNow.Month + DateTime.UtcNow.Day + "/";
        public static Dictionary<string, string[]> EmotesByChannel = new();
        public static TwitchClient client = new();
        public static string BotNick = "";
        public static string BotToken = "";
        private static string BotDiscordToken = "";
        public static string[] Channels = [""];
        public static int CommandsActive = 24;
        public static int ServersConnected = 0;
        public static bool is_connected = false;
        public static string last_channel_connected = "";
        public static bool timerStarted = false;
        public static bool botAlreadyConnected = false;
        public static string imgurAPIkey = "";
        public static int previousCheckTimersMinute = DateTime.UtcNow.Minute;
        public static string UID = "";
        public static string ClientID = "";
        public static string Secret = "";
        public static DiscordSocketClient discordClient;
        public static CommandService discordCommands;
        public static IServiceProvider discordServices;
        public static string nowColor = "";
        public static string[] connectionAnnounceChannels = [];
        public static string[] reconnectionAnnounceChannels = [];
        public static int ReadedMessages = 0;

        // #BOT 0A
        public void Start(string[] args, int ThreadID)
        {
            // Бот запускается...
            // Установка ID для Thread потока
            Thread.CurrentThread.Name = ThreadID.ToString();
            Console.OutputEncoding = Encoding.Unicode;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Clear();
            Console.Title = "butterBror · Загрузка...";
            Console.WriteLine("    _______  ________  ________  ________  ________  ________   _______  ________  ________  ________ ");
            Console.WriteLine("  //      / /    /   \\/        \\/        \\/        \\/        \\//      / /        \\/        \\/        \\");
            Console.WriteLine(" //       \\/         /        _/        _/         /         //       \\/         /         /         /");
            Console.WriteLine("/         /         //       / /       //        _/        _/         /        _/         /        _/ ");
            Console.WriteLine("\\________/\\________/ \\______/  \\______/ \\________/\\____/___/\\________/\\____/___/\\________/\\____/___/  ");
            Console.WriteLine();
            Console.ResetColor();

            Maintrance();
        }

        public static async void Maintrance()
        {
            BotEngine.isTwitchReady = false;

            Console.WriteLine($" ButterBror {BotEngine.botVersion}");
            Console.WriteLine(" +");

            if (File.Exists(CurrencyPath))
            {
                BotEngine.buttersTotalAmount = DataManager.GetData<float>(CurrencyPath, "totalAmount");
                BotEngine.buttersTotalDollarsInTheBank = DataManager.GetData<int>(CurrencyPath, "totalDollarsInTheBank");
                BotEngine.buttersTotalUsers = DataManager.GetData<int>(CurrencyPath, "totalUsers");
            }

            try
            {
                ConsoleServer.Main();
                await Task.Delay(1000);
                ConsoleUtil.LOG("БОТ");
                ConsoleUtil.LOG("- Проверка и создание директорий", WrapLine: false);
                FileUtil.CreateDirectory(ProgramPath); p();
                FileUtil.CreateDirectory(MainPath); p();
                FileUtil.CreateDirectory(ChannelsPath); p();
                FileUtil.CreateDirectory(UsersDataPath); p();
                FileUtil.CreateDirectory(ConvertorsPath); p();
                FileUtil.CreateDirectory(NicknameToIDPath); p();
                FileUtil.CreateDirectory(IDToNicknamePath); p();
                FileUtil.CreateDirectory(TranslateDefualtPath); p();
                FileUtil.CreateDirectory(TranslateCustomPath); p();
                FileUtil.CreateDirectory(UsersBankDataPath); p();

                ConsoleUtil.LOG("\n- Проверка и создание файлов", WrapLine: false);
                if (!File.Exists(SettingsPath))
                {
                    FileUtil.CreateFile(SettingsPath); p();
                    DataManager.SaveData(SettingsPath, "nickname", ""); p();
                    DataManager.SaveData(SettingsPath, "token", ""); p();
                    DataManager.SaveData(SettingsPath, "discordToken", ""); p();
                    DataManager.SaveData(SettingsPath, "imgurAPI", ""); p();
                    DataManager.SaveData(SettingsPath, "UID", ""); p();
                    DataManager.SaveData(SettingsPath, "ClientID", ""); p();
                    DataManager.SaveData(SettingsPath, "Secret", ""); p();
                    string[] channels = ["1channel", "2channel"]; p();
                    DataManager.SaveData(SettingsPath, "connectionInfoChannels", (string[])[]); p();
                    DataManager.SaveData(SettingsPath, "reconnectionInfoChannels", (string[])[]); p();
                    DataManager.SaveData(SettingsPath, "channels", channels); p();
                    string[] apis = ["1 api", "2 api"]; p();
                    DataManager.SaveData(SettingsPath, "weatherApis", apis); p();
                    DataManager.SaveData(SettingsPath, "gptApis", apis); p();
                    ConsoleUtil.LOG($"\nФайл настроек создан! Заполните его! (Путь к файлу: {SettingsPath})", ConsoleColor.Black, ConsoleColor.Cyan);
                    Thread.Sleep(-1);
                }
                else
                {
                    p();
                    FileUtil.CreateFile(CookiesPath); p();
                    FileUtil.CreateFile(BanWordsPath); p();
                    FileUtil.CreateFile(BanWordsReplacementPath); p();
                    FileUtil.CreateFile(CurrencyPath); p();
                    FileUtil.CreateFile(LocationsCachePath); p();
                    FileUtil.CreateFile(LogsPath); p();
                    FileUtil.CreateFile(APIUseDataPath); p();
                    FileUtil.CreateFile(TranslateDefualtPath + "ru.txt"); p();
                    FileUtil.CreateFile(TranslateDefualtPath + "en.txt"); p();

                    ConsoleUtil.LOG("\n- Чтение информации", WrapLine: false);
                    BotNick = DataManager.GetData<string>(SettingsPath, "nickname"); p();
                    Channels = DataManager.GetData<string[]>(SettingsPath, "channels"); p();
                    reconnectionAnnounceChannels = DataManager.GetData<string[]>(SettingsPath, "reconnectionInfoChannels"); p();
                    connectionAnnounceChannels = DataManager.GetData<string[]>(SettingsPath, "connectionInfoChannels"); p();
                    BotDiscordToken = DataManager.GetData<string>(SettingsPath, "discordToken"); p();
                    imgurAPIkey = DataManager.GetData<string>(SettingsPath, "imgurAPI"); p();
                    UID = DataManager.GetData<string>(SettingsPath, "UID"); p();
                    ClientID = DataManager.GetData<string>(SettingsPath, "ClientID"); p();
                    Secret = DataManager.GetData<string>(SettingsPath, "Secret"); p();
                    ConsoleUtil.LOG($" {ClientID} {Secret} ");
                    ConsoleUtil.LOG("\n- Генерируем/получаем токен...");
                    tokenGetter = new(ClientID, Secret, "database.db");
                    var token = await tokenGetter.GetTokenAsync();
                    if (token != null)
                    {
                        BotToken = token; p();
                        connect();
                    }
                    else
                    {
                        RestartPlease();
                    }
                    CommandUtil.ChangeNicknameColorAsync(TwitchLib.Client.Enums.ChatColorPresets.YellowGreen);
                }
            }
            catch (Exception ex)
            {
                LogWorker.Log(ex.Message, LogWorker.LogTypes.Err, "bot_maintrance");
                Console.ForegroundColor = ConsoleColor.Red;
                ConsoleUtil.LOG("Ошибка подключения! Рестарт...", ConsoleColor.Red);
                LogWorker.Log($"Ошибка подключения! Рестарт...", LogWorker.LogTypes.Err, "bot_maintrance");
                Console.ResetColor();
                RestartPlease();
            }
        }

        public static void p()
        {
            // Это точка?
            Console.Write(".");
        } // Точка
          // #BOT 1A
        public static async Task connect()
        {
            try
            {
                ConsoleUtil.LOG("\nТВИЧ");
                ConsoleUtil.LOG("- Подключение к Twitch.tv", WrapLine: false);
                ConnectionCredentials credentials = new(BotNick, "oauth:" + BotToken); p();
                var clientOptions = new ClientOptions
                {
                    MessagesAllowedInPeriod = 750,
                    ThrottlingPeriod = TimeSpan.FromSeconds(30)
                }; p();
                var webSocketClient = new WebSocketClient(clientOptions); p();
                client = new TwitchClient(webSocketClient); p();
                client.Initialize(credentials, BotNick, '#'); p();
                client.OnJoinedChannel += TwitchEventHandler.OnJoin; p();
                client.OnChatCommandReceived += Commands.TwitchCommand; p();
                client.OnMessageReceived += TwitchEventHandler.OnMessageReceived; p();
                client.OnMessageThrottled += TwitchEventHandler.OnMessageThrottled; p();
                client.OnMessageSent += TwitchEventHandler.OnMessageSend; p();
                client.OnAnnouncement += TwitchEventHandler.OnAnnounce; p();
                client.OnBanned += TwitchEventHandler.OnBanned; p();
                client.OnConnectionError += TwitchEventHandler.OnConnectionError; p();
                client.OnContinuedGiftedSubscription += TwitchEventHandler.OnContinuedGiftedSubscription; p();
                client.OnChatCleared += TwitchEventHandler.OnChatCleared; p();
                client.OnDisconnected += TwitchEventHandler.OnTwitchDisconnected; p();
                client.OnReconnected += TwitchEventHandler.OnReconnected; p();
                client.OnError += TwitchEventHandler.OnError; p();
                client.OnIncorrectLogin += TwitchEventHandler.OnIncorrectLogin; p();
                client.OnLeftChannel += TwitchEventHandler.OnLeftChannel; p();
                client.OnRaidNotification += TwitchEventHandler.OnRaidNotification; p();
                client.OnNewSubscriber += TwitchEventHandler.OnNewSubscriber; p();
                client.OnGiftedSubscription += TwitchEventHandler.OnGiftedSubscription; p();
                client.OnCommunitySubscription += TwitchEventHandler.OnCommunitySubscription; p();
                client.OnReSubscriber += TwitchEventHandler.OnReSubscriber; p();
                client.OnSuspended += TwitchEventHandler.OnSuspended; p();
                client.OnConnected += TwitchEventHandler.OnConnected; p();
                client.OnLog += TwitchEventHandler.OnLog; p();
                client.Connect(); p();

                botAlreadyConnected = true;
                // Console.Write("\n | Подготавливаем методы");
                // MiniGames.MiningGame.Main.AddHardware(); p();

                ConsoleUtil.LOG("\n- Заканчиваем");
                var lastChannel = NamesUtil.GetUsername(Channels.LastOrDefault(), Channels.LastOrDefault());
                var notFoundedChannels = new List<string>();
                last_channel_connected = lastChannel;
                string sendChannelsMsg = "";
                foreach (var channel in Channels)
                {
                    var channel2 = NamesUtil.GetUsername(channel, "NONE\n");
                    if (channel2 != "NONE\n")
                    {
                        sendChannelsMsg += $"{channel2}, ";
                    }
                    else
                    {
                        notFoundedChannels.Add(channel);
                    }
                }
                ConsoleUtil.LOG($"Подключаюсь к: {sendChannelsMsg.TrimEnd(',', ' ')}");
                foreach (var channel in Channels)
                {
                    var channel2 = NamesUtil.GetUsername(channel, "NONE\n");
                    if (channel2 != "NONE\n")
                    {
                        client.JoinChannel(channel2);
                    }
                }
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var channel in notFoundedChannels)
                {
                    ConsoleUtil.LOG("Не удалось найти пользователя с ID: " + channel, ConsoleColor.Red);
                }
                Console.ResetColor();
                BotEngine.isTwitchReady = true;
                ConsoleUtil.LOG("ДИСКОРД");
                ConsoleUtil.LOG("- Создание клиента Discord", WrapLine: false);
                var discordConfig = new DiscordSocketConfig
                {
                    MessageCacheSize = 1000,
                    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages | GatewayIntents.MessageContent
                };
                discordClient = new DiscordSocketClient(discordConfig); p();
                discordCommands = new CommandService(); p();
                discordServices = new ServiceCollection()
                    .AddSingleton(discordClient)
                    .AddSingleton(discordCommands)
                    .BuildServiceProvider(); p();

                ConsoleUtil.LOG("\n- Подписка на события Discord", WrapLine: false);
                discordClient.Log += DiscordEventHandler.LogAsync; p();
                discordClient.JoinedGuild += DiscordEventHandler.ConnectToGuilt; p();
                discordClient.Ready += DiscordWorker.ReadyAsync; p();
                discordClient.MessageReceived += DiscordWorker.MessageReceivedAsync; p();
                discordClient.SlashCommandExecuted += DiscordEventHandler.SlashCommandHandler; p();
                discordClient.ApplicationCommandCreated += DiscordEventHandler.ApplicationCommandCreated; p();
                discordClient.ApplicationCommandDeleted += DiscordEventHandler.ApplicationCommandDeleted; p();
                discordClient.ApplicationCommandUpdated += DiscordEventHandler.ApplicationCommandUpdated; p();
                discordClient.ChannelCreated += DiscordEventHandler.ChannelCreated; p();
                discordClient.ChannelDestroyed += DiscordEventHandler.ChannelDeleted; p();
                discordClient.ChannelUpdated += DiscordEventHandler.ChannelUpdated; p();
                discordClient.Connected += DiscordEventHandler.Connected; p();
                discordClient.ButtonExecuted += DiscordEventHandler.ButtonTouched; p();
                ConsoleUtil.LOG("\n- Регистрация команд Discord...");
                await DiscordWorker.RegisterCommandsAsync();
                ConsoleUtil.LOG("- Вход в Discord", WrapLine: false);
                await discordClient.LoginAsync(TokenType.Bot, BotDiscordToken); p();
                await discordClient.StartAsync(); p();
                ConsoleUtil.LOG("\nГОТОВО!");
            }
            catch (Exception ex)
            {
                LogWorker.Log(ex.Message, LogWorker.LogTypes.Err, "bot_connect");
                Console.ForegroundColor = ConsoleColor.Red;
                ConsoleUtil.LOG("Ошибка подключения! Рестарт...", ConsoleColor.Red);
                LogWorker.Log($"Ошибка подключения! Рестарт... " + ex.Message, LogWorker.LogTypes.Err, "bot_connect");
                Console.ResetColor();
                RestartPlease();
            }
        }
        public async Task StatusSender()
        {
            try
            {
                // Сколько времени работает
                var workTime = DateTime.Now - BotEngine.StartTime;
                // Пинг твича
                PingUtil twPing = new();
                await twPing.PingAsync("twitch.tv", 1000);
                if (!twPing.isSuccess)
                {
                    ConsoleServer.SendConsoleMessage("errors", "Cannot ping twitch: " + twPing.resultText);
                }
                // Пинг дискорда
                PingUtil dsPing = new();
                await dsPing.PingAsync("discord.com", 1000);
                if (!dsPing.isSuccess)
                {
                    ConsoleServer.SendConsoleMessage("errors", "Cannot ping discord: " + dsPing.resultText);
                }
                // Пинг гугла
                PingUtil glPing = new();
                await glPing.PingAsync("192.168.1.1", 1000);
                if (!glPing.isSuccess)
                {
                    await glPing.PingAsync("192.168.0.1", 1000);
                    if (!glPing.isSuccess)
                    {
                        ConsoleServer.SendConsoleMessage("errors", "Cannot ping ASP: " + dsPing.resultText);
                    }
                }
                // Память, занятая процессом
                Process process = Process.GetCurrentProcess();
                long workingAppSet = process.WorkingSet64 / (1024 * 1024);
                // Подготовка сообщения
                string message = $"/me glorp 📡 Твитч: {twPing.pingSpeed}ms · Дискорд: {dsPing.pingSpeed}ms · Глобальный: {glPing.pingSpeed}ms · {workTime.ToString(@"dd\:hh\:mm\.ss")} · {workingAppSet}мб";
                // Отправка сообщения
                await CommandUtil.ChangeNicknameColorAsync(TwitchLib.Client.Enums.ChatColorPresets.DodgerBlue);
                ChatUtil.SendMessage(BotNick, message, "", "", "", true);
                string newToken = "";
                try
                {
                    newToken = await tokenGetter.RefreshAccessToken();
                    if (newToken != null)
                    {
                        Bot.BotToken = newToken;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleUtil.LOG($"Ошибка получения нового токена!!! ({ex.Message})", BG: ConsoleColor.Red, FG: ConsoleColor.Black);
                }
            }
            catch (Exception ex)
            {
                // Обработчик ошибок
                ConsoleUtil.LOG($"Не удалось отправить статус сообщение! ({ex.Message})", ConsoleColor.Red);
            }
        } // Отправка статус-сообщенеия в чат бота
        public static void RestartPlease()
        {
            // Создание процесса рестарта бота
            Thread thread = new(Restarter);
            // Запуск процесса
            thread.Start();
        } // Запуск рестарта
        static void Restarter()
        {
            ConsoleUtil.LOG("Рестарт через 3 сек.", WrapLine: false);
            Thread.Sleep(1000);
            Console.Write(".");
            Thread.Sleep(1000);
            Console.Write(".\n");
            Thread.Sleep(1000);
            ConsoleUtil.LOG("Рестарт случился!");
            // Попытка отключения всех каналов от клиента
            try
            {
                foreach (var channel in client.JoinedChannels)
                {
                    try
                    {
                        client.LeaveChannel(channel);
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception) { }
            // Попытка отключения всех EventHandler'ов
            try
            {
                client.OnJoinedChannel -= TwitchEventHandler.OnJoin;
                client.OnChatCommandReceived -= Commands.TwitchCommand;
                client.OnMessageReceived -= TwitchEventHandler.OnMessageReceived;
                client.OnMessageThrottled -= TwitchEventHandler.OnMessageThrottled;
                client.OnMessageSent -= TwitchEventHandler.OnMessageSend;
                client.OnAnnouncement -= TwitchEventHandler.OnAnnounce;
                client.OnBanned -= TwitchEventHandler.OnBanned;
                client.OnConnectionError -= TwitchEventHandler.OnConnectionError;
                client.OnContinuedGiftedSubscription -= TwitchEventHandler.OnContinuedGiftedSubscription;
                client.OnChatCleared -= TwitchEventHandler.OnChatCleared;
                client.OnDisconnected -= TwitchEventHandler.OnTwitchDisconnected;
                client.OnReconnected -= TwitchEventHandler.OnReconnected;
                client.OnError -= TwitchEventHandler.OnError;
                client.OnIncorrectLogin -= TwitchEventHandler.OnIncorrectLogin;
                client.OnLeftChannel -= TwitchEventHandler.OnLeftChannel;
                client.OnRaidNotification -= TwitchEventHandler.OnRaidNotification;
                client.OnNewSubscriber -= TwitchEventHandler.OnNewSubscriber;
                client.OnGiftedSubscription -= TwitchEventHandler.OnGiftedSubscription;
                client.OnCommunitySubscription -= TwitchEventHandler.OnCommunitySubscription;
                client.OnReSubscriber -= TwitchEventHandler.OnReSubscriber;
                client.OnSuspended -= TwitchEventHandler.OnSuspended;
                client.OnConnected -= TwitchEventHandler.OnConnected;
            }
            catch (Exception) { }
            // Наконец, отключение клиента
            try
            {
                client.Disconnect();
            }
            catch (Exception) { }
            // Полный сброс клиента
            client = new();
            // Подождем секунду...
            Thread.Sleep(1000);
            // Перезапуск через движок бота
            BotEngine.isNeedRestart = true;
        } // Процесс рестарта
    }
    public class DiscordWorker
    {
        public static async Task ReadyAsync()
        {
            try
            {
                ConsoleServer.SendConsoleMessage("discord", $"Подключен как @{Bot.discordClient.CurrentUser}!");

                foreach (var guild in Bot.discordClient.Guilds)
                {
                    ConsoleServer.SendConsoleMessage("discord", $"Подключен к серверу: {guild.Name}");
                    Bot.ServersConnected++;
                }
            }
            catch (Exception ex)
            {
                ConsoleUtil.ErrorOccured(ex, "DiscordWorker\\ReadyAsync");
            }
        }
        public static async Task MessageReceivedAsync(SocketMessage message)
        {
            try
            {
                if (!(message is SocketUserMessage msg) || message.Author.IsBot) return;
                OnMessageReceivedArgs e = default;
                CommandUtil.MessageWorker("ds" + message.Author.Id.ToString(), ((SocketGuildChannel)message.Channel).Guild.Id.ToString(), message.Author.Username.ToLower(), message.Content, e, ((SocketGuildChannel)message.Channel).Guild.Name, "ds", message.Channel.ToString());
            }
            catch (Exception ex)
            {
                ConsoleUtil.ErrorOccured(ex, $"DiscordWorker\\MessageReceivedAsync#{message.Content}");
            }
        }
        public static async Task RegisterCommandsAsync()
        {
            try
            {
                Bot.discordClient.Ready += RegisterSlashCommands;
                Bot.discordClient.MessageReceived += DiscordEventHandler.HandleCommandAsync;

                await Bot.discordCommands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: Bot.discordServices);
            }
            catch (Exception ex)
            {
                ConsoleUtil.ErrorOccured(ex, "DiscordWorker\\RegisterCommandsAsync");
            }
        }
        private static async Task RegisterSlashCommands()
        {
            // Удаление старых команд
            ConsoleServer.SendConsoleMessage("discord", "Удаление всех команд...");
            await Bot.discordClient.Rest.DeleteAllGlobalCommandsAsync();
            ConsoleServer.SendConsoleMessage("discord", "Регистрация команд...");
            await Bot.discordClient.Rest.CreateGlobalCommand(new SlashCommandBuilder()
                .WithName("ping")
                .WithDescription("Проверить статус работы бота.")
                .Build());
            await Bot.discordClient.Rest.CreateGlobalCommand(new SlashCommandBuilder()
                .WithName("status")
                .WithDescription("Проверить состояние бота. (Только для администраторов бота)")
                .Build());
            await Bot.discordClient.Rest.CreateGlobalCommand(new SlashCommandBuilder()
                .WithName("weather")
                .WithDescription("Проверить погоду")
                .AddOption("location", ApplicationCommandOptionType.String, "местоположение для проверки погоды", isRequired: false)
                .AddOption("showpage", ApplicationCommandOptionType.Integer, "показать погоду на странице", isRequired: false)
                .AddOption("page", ApplicationCommandOptionType.Integer, "показать страницу результата полученной погоды", isRequired: false)
                .Build());
            ConsoleServer.SendConsoleMessage("discord", "Все команды зарегистрированны!");
        }
    }
    public class DiscordEventHandler
    {
        public static Task LogAsync(LogMessage log)
        {
            try
            {
                ConsoleServer.SendConsoleMessage("discord", log.ToString().Replace("\n", " ").Replace("\r", ""));
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                ConsoleUtil.ErrorOccured(ex, $"DiscordEventHandler\\LogAsync#{log.Message}");
                return Task.CompletedTask;
            }
        }
        public static async Task ConnectToGuilt(SocketGuild g)
        {
            ConsoleServer.SendConsoleMessage("discord", $"Подключен к серверу: {g.Name}");
            Bot.ServersConnected++;
        }
        public static async Task HandleCommandAsync(SocketMessage arg)
        {
            try
            {
                var message = arg as SocketUserMessage;
                if (message == null || message.Author.IsBot) return;

                int argPos = 0;
                if (message.HasCharPrefix('#', ref argPos))
                {
                    var context = new SocketCommandContext(Bot.discordClient, message);
                    var result = await Bot.discordCommands.ExecuteAsync(context, argPos, Bot.discordServices);
                    if (!result.IsSuccess)
                    {
                        ConsoleUtil.LOG(result.ErrorReason, ConsoleColor.Red);
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleUtil.ErrorOccured(ex, $"DiscordEventHandler\\HandleCommandAsync");
            }
        }
        public static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            Commands.DiscordCommand(command);
        }
        public static async Task ApplicationCommandCreated(SocketApplicationCommand e)
        {
            ConsoleServer.SendConsoleMessage("commands", "Discord | Команда создана: /" + e.Name + " (" + e.Description + ")");
        }
        public static async Task ApplicationCommandDeleted(SocketApplicationCommand e)
        {
            ConsoleServer.SendConsoleMessage("commands", "Discord | Команда удалена: /" + e.Name + " (" + e.Description + ")");
        }
        public static async Task ApplicationCommandUpdated(SocketApplicationCommand e)
        {
            ConsoleServer.SendConsoleMessage("commands", "Discord | Команда обновлена: /" + e.Name + " (" + e.Description + ")");
        }
        public static async Task ChannelCreated(SocketChannel e)
        {
            ConsoleServer.SendConsoleMessage("info", "Discord | Создан новый канал: " + e.Id);
        }
        public static async Task ChannelDeleted(SocketChannel e)
        {
            ConsoleServer.SendConsoleMessage("info", "Discord | Канал был удален: " + e.Id);
        }
        public static async Task ChannelUpdated(SocketChannel e, SocketChannel a)
        {
            ConsoleServer.SendConsoleMessage("info", "Discord | Обновлен канал: " + e.Id + "/" + a.Id);
        }
        public static async Task Connected()
        {
            ConsoleServer.SendConsoleMessage("discord", "Подключен!");
        }
        public static async Task ButtonTouched(SocketMessageComponent e)
        {
            ConsoleServer.SendConsoleMessage("commands", "Discord | Была нажата кнопка. Пользователь: " + e.User + ", ID кнопки: " + e.Id + ", Сервер: " + ((SocketGuildChannel)e.Channel).Guild.Name);
        }
    }
}