
using System;
using System.IO;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


using Telegram.Bot.Extensions.Polling;
using System.Threading;
using Telegram.Bot.Types.InputFiles;

using System.Reflection;
using ResultBot.Models;
using Newtonsoft.Json;
using System.Net.Http;





//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Services;
//using Google.Apis.Upload;
//using Google.Apis.Util.Store;
//using Google.Apis.YouTube.v3;
//using Google.Apis.YouTube.v3.Data;


namespace ResultBot
{


    class Bot
    {
        private  string _maxPlayListID;
        private  string _maxLikeVideoID;
        private  string _maxRequestID;

        TelegramBotClient botClient = new TelegramBotClient("5538327578:AAFlF5EVlJOsGxCicUBiweXj5E8PPt8NNQY");
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        public async Task Start()
        {
            Clients clients = new Clients();
            var result1 = await clients.Client.GetAsync($"/Values/lastid");
            int max = int.Parse(result1.Content.ReadAsStringAsync().Result);
            max++;
            _maxRequestID = max.ToString();

            var result2 = await clients.Client.GetAsync($"/LikeVideo/lastid");
            max = int.Parse(result2.Content.ReadAsStringAsync().Result);
            max++;
            _maxLikeVideoID = max.ToString();

            var result3 = await clients.Client.GetAsync($"/Playlist/lastid");
            max = int.Parse(result3.Content.ReadAsStringAsync().Result);
            max++;
            _maxPlayListID = max.ToString();
            Console.WriteLine("all preperations done");


            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();

            Console.WriteLine($"Bot {botMe.Username} start working");

            Console.ReadKey();

        }

        private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Error in API {apiRequestException.ErrorCode}\n" + $"{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                await HandlerMessageAsync(botClient, update.Message);

            }

            if (update.Type == UpdateType.CallbackQuery)
            {
                await HandlerCallbackQueryAsync(botClient, update.CallbackQuery);
            }
        }

        private async Task HandlerCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
           
            int max = 0;
            Clients clients = new Clients();
            CallBackD callBack  = JsonConvert.DeserializeObject<CallBackD>(callbackQuery.Data);
            switch (callBack.Do)
            {
                case "Like":
                    //    / YouTubeApi / videoinfo ? userId = qwerqw & userName = qwerqw & VideoId = CHekNnySAfM

                    var result = await clients.Client.GetAsync($" /YouTubeApi/videoinfo?userId={callbackQuery.From.Id}&userName={callbackQuery.From.Username}&VideoId={callBack.Idv}");

                    LikeVideo likeVideo = JsonConvert.DeserializeObject<LikeVideo>(result.Content.ReadAsStringAsync().Result);

                    likeVideo.Id = _maxLikeVideoID;
                    max = int.Parse(_maxLikeVideoID);
                    max++;
                    _maxLikeVideoID = max.ToString();


                    var json = JsonConvert.SerializeObject(likeVideo);

                    var data = new StringContent(json, Encoding.UTF8, "application/json");

                    var post = await clients.Client.PostAsync("/LikeVideo/add", data);

                    post.EnsureSuccessStatusCode();

                    var postcontent = post.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(postcontent);

                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Added to Likes");
                    return;
                case "AddToP":

                    result = await clients.Client.GetAsync($" /Playlist/userplaylists?Userid={callbackQuery.From.Id}");

                    List<Playlist> playlists = new List<Playlist> { };

                    if (result.StatusCode!= System.Net.HttpStatusCode.NotFound)
                    {
                        playlists = JsonConvert.DeserializeObject<List<Playlist>>(result.Content.ReadAsStringAsync().Result);
                    }


                    List<List<InlineKeyboardButton>> buttons = new List<List<InlineKeyboardButton>> { };

                    foreach (var PlayL in playlists)
                    {
                        CallBackD callBack2 = new CallBackD()
                        {
                            Idv = callBack.Idv,
                            Idp = PlayL.Id,
                            Do = "AddToUserP"
                        };
                        var json1 = JsonConvert.SerializeObject(callBack2);
                       
                        buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData(PlayL.PlaylistName,json1),
                        }
                        );
                    }
                    CallBackD callBack1 = new CallBackD()
                    {
                        Idv = callBack.Idv,
                        Do = "AddPlaylist"
                    };
                    var json2 = JsonConvert.SerializeObject(callBack1);
                    buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData("Add playlist",json2),
                        }
                       );

                    InlineKeyboardMarkup keyboardMarkup = new InlineKeyboardMarkup
                        (
                        buttons
                        );

                    Console.WriteLine("Chose Playlist");

                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Chose Playlist", replyMarkup: keyboardMarkup);

                    return;

                case "AddPlaylist":
                    UserWatching.AddRecuestAsync(_maxRequestID, callbackQuery.From.Id.ToString(), callbackQuery.From.Username, "@AddPlaylist", callbackQuery.Message.Date.AddHours(3).ToString());
                    max = int.Parse(_maxRequestID);
                    max++;
                    _maxRequestID = max.ToString();

                    Console.WriteLine($"User {callbackQuery.From} AddPlaylist");
                    //---
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Type Playlist Name");

                    return;
                case "AddToUserP":
                    result = await clients.Client.GetAsync($" /YouTubeApi/videoinfo?userId={callbackQuery.From.Id}&userName={callbackQuery.From.Username}&VideoId={callBack.Idv}");

                    likeVideo = JsonConvert.DeserializeObject<LikeVideo>(result.Content.ReadAsStringAsync().Result);

                    result = await clients.Client.GetAsync($"Playlist/playlist?id={callBack.Idp}");

                    Playlist playlist;

                    if (result.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        playlist = JsonConvert.DeserializeObject<Playlist>(result.Content.ReadAsStringAsync().Result);

                        if (playlist.VideoIds[0]== "No videos")
                        {
                            playlist.VideoIds[0] = likeVideo.VideolId;
                            playlist.VideoTitles[0] = likeVideo.VideoTitle;
                        }
                        else if (playlist.VideoIds.Count>50)
                        {
                            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "You can not add more than 50 videos");
                            return;
                        }
                        else
                        {
                            playlist.VideoIds.Add(likeVideo.VideolId);
                            playlist.VideoTitles.Add(likeVideo.VideoTitle);
                        }
                        json = JsonConvert.SerializeObject(playlist);

                        data = new StringContent(json, Encoding.UTF8, "application/json");

                        post = await clients.Client.PostAsync("/Playlist/addplaylist", data);

                        post.EnsureSuccessStatusCode();

                        postcontent = post.Content.ReadAsStringAsync().Result;
                        Console.WriteLine(postcontent);

                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Added");

                    }
                    else
                    {
                        Console.WriteLine(result.StatusCode);
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Not added");

                    }
                    return;
                case "ShowPToUser":

                    result = await clients.Client.GetAsync($"Playlist/playlist?id={callBack.Idp}");

                    if (result.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        playlist = JsonConvert.DeserializeObject<Playlist>(result.Content.ReadAsStringAsync().Result);

                        buttons = new List<List<InlineKeyboardButton>> { };

                        CallBackD callBack4 = new CallBackD()
                        {
                            Idp = callBack.Idp,
                            Do = "DellP"
                        };
                        var json1 = JsonConvert.SerializeObject(callBack4);

                        buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData("Delete playlist",json1)
                        }
                       );

                        if (playlist.VideoIds[0] == "No videos")
                        {
                           
                        }
                        else
                        {
                            string req = "/YouTubeApi/videosinfo?";
                            req += $"Ids={playlist.VideoIds[0]}";
                            for (int i = 1; i < playlist.VideoIds.Count; i++)
                            {
                                req += $"&Ids={playlist.VideoIds[i]}";
                            }

                            result = await clients.Client.GetAsync(req);

                            List<LikeVideo> videos = JsonConvert.DeserializeObject<List<LikeVideo>>(result.Content.ReadAsStringAsync().Result);


                            for (int i = 0; i < videos.Count; i++)
                            {
                                buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithUrl($"{videos[i].VideoTitle}", @"https://www.youtube.com/watch?v=" + videos[i].VideolId)
                        }
                                );
                                callBack4 = new CallBackD()
                                {
                                    Idv = videos[i].VideolId,
                                    Idp = playlist.Id,
                                    Do = "Like"
                                };
                                json1 = JsonConvert.SerializeObject(callBack4);
                                CallBackD callBack2 = new CallBackD()
                                {
                                    Idv = videos[i].VideolId,
                                    Do = "Remove"
                                };
                                var json4 = JsonConvert.SerializeObject(callBack2);

                                buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData("Like",json1),
                            InlineKeyboardButton.WithCallbackData("Remove",json4)
                        }
                                );
                                Console.WriteLine(videos[i].VideoTitle);

                            }
                        }
                            keyboardMarkup = new InlineKeyboardMarkup
                                (
                                buttons
                                );
                            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, playlist.PlaylistName, replyMarkup: keyboardMarkup);
                            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "No videos");


                    }
                    else
                    {
                        Console.WriteLine(result.StatusCode);
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Error");

                    }

                    return;

                case "Remove":

                    result = await clients.Client.DeleteAsync($"/Playlist/deletevideos?Id={callBack.Idp}&videoId={callBack.Idv}");
                    result.EnsureSuccessStatusCode();

                    postcontent = result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(postcontent);

                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Video removed");


                    return;

                case "DellP":
                    result = await clients.Client.DeleteAsync($"/Playlist/deleteplaylist?id={callBack.Idp}");
                    result.EnsureSuccessStatusCode();

                    postcontent = result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(postcontent);

                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Playlist removed");
                    return;

                case "UnLike":
                    result = await clients.Client.DeleteAsync($"/LikeVideo/dell?userId={callbackQuery.From.Id}&VideoId={callBack.Idv}");
                    result.EnsureSuccessStatusCode();

                    postcontent = result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(postcontent);

                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Removed from likes");
                    return;


            }

        }

        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {
            Clients clients = new Clients();
            int max = 0;
            switch (message.Text)
            {
                case "/start":
                    UserWatching.AddRecuestAsync(_maxRequestID, message.From.Id.ToString(), message.From.Username, "/start", message.Date.AddHours(3).ToString());
                    max = int.Parse(_maxRequestID);
                    max++;
                    _maxRequestID = max.ToString();

                    Console.WriteLine($"User {message.From} /start");
                    //---
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Chose command:");
                   // await botClient.SendTextMessageAsync(message.Chat.Id, "/dirty");
                    await botClient.SendTextMessageAsync(message.Chat.Id, "/keyboard");

                    return;
                case "/keyboard":
                    UserWatching.AddRecuestAsync(_maxRequestID, message.From.Id.ToString(), message.From.Username, "/keyboard", message.Date.AddHours(3).ToString());
                    max = int.Parse(_maxRequestID);
                    max++;
                    _maxRequestID = max.ToString();

                    Console.WriteLine($"User {message.From} /keyboard");
                    //---
                    ReplyKeyboardMarkup replyKeyboardMarkup =
                    new
                    (
                    new[]
                    {
                        new KeyboardButton[] { "Search", "Artist" },
                        new KeyboardButton[] { "Trend", "Genres" },
                        new KeyboardButton[] { "Likes", "Playlists", "Friends" }
                    }
                    )
                    {
                        ResizeKeyboard = true
                    };

                    await botClient.SendTextMessageAsync(message.Chat.Id, "Chose option", replyMarkup: replyKeyboardMarkup);
                    return;
                case "Search":
                    UserWatching.AddRecuestAsync(_maxRequestID, message.From.Id.ToString(), message.From.Username, "@Serch", message.Date.AddHours(3).ToString());
                    max = int.Parse(_maxRequestID);
                    max++;
                    _maxRequestID = max.ToString();

                    Console.WriteLine($"User {message.From} Serch");
                    //---
                    await botClient.SendTextMessageAsync(message.Chat.Id, "What to serch?");
                    return;
                case "Trend":
                    UserWatching.AddRecuestAsync(_maxRequestID, message.From.Id.ToString(), message.From.Username, "@Trend", message.Date.AddHours(3).ToString());
                    max = int.Parse(_maxRequestID);
                    max++;
                    _maxRequestID = max.ToString();

                    Console.WriteLine($"User {message.From} Trend");
                    //---

                    var rTrend = await clients.Client.GetAsync($"/YouTubeApi/trendsbyrequest");

                    List<Models.Video> videos = JsonConvert.DeserializeObject<List<Models.Video>>(rTrend.Content.ReadAsStringAsync().Result);

                    List<List<InlineKeyboardButton>> buttons = new List<List<InlineKeyboardButton>> { };

                    for (int i = 0; i < videos.Count; i++)
                    {
                        buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithUrl($"{videos[i].VideoTitle}", @"https://www.youtube.com/watch?v=" + videos[i].VideoId)
                        }
                        );
                        CallBackD callBack4 = new CallBackD()
                        {
                            Idv = videos[i].VideoId,
                            Do = "Like"
                        };
                        var json1 = JsonConvert.SerializeObject(callBack4);
                        CallBackD callBack2 = new CallBackD()
                        {
                            Idv = videos[i].VideoId,
                            Do = "AddToP"
                        };
                        var json4 = JsonConvert.SerializeObject(callBack2);

                        buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData("Like",json1),
                            InlineKeyboardButton.WithCallbackData("Add to playlist",json4)
                        }
                        );
                        Console.WriteLine(videos[i].VideoTitle);

                    }


                    InlineKeyboardMarkup keyboardMarkup = new InlineKeyboardMarkup
                        (
                        buttons
                        );
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Today music", replyMarkup: keyboardMarkup);
                    return;



                case "Artist":
                    UserWatching.AddRecuestAsync(_maxRequestID, message.From.Id.ToString(), message.From.Username, "@Artist", message.Date.AddHours(3).ToString());
                    max = int.Parse(_maxRequestID);
                    max++;
                    _maxRequestID = max.ToString();

                    Console.WriteLine($"User {message.From} Artist");
                    //---
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Type Artist Name");
                    return;

                case "Friends":
                    UserWatching.AddRecuestAsync(_maxRequestID, message.From.Id.ToString(), message.From.Username, "@Friends", message.Date.AddHours(3).ToString());
                    max = int.Parse(_maxRequestID);
                    max++;
                    _maxRequestID = max.ToString();
    
                    Console.WriteLine($"User {message.From} Friends");
                    //---


                    var ll = await clients.Client.GetAsync($"/LikeVideo/likemates?userId={message.From.Id}");

                    if (ll.StatusCode == System.Net.HttpStatusCode.OK&&ll.Content!=null)
                    {
                        var LikeMates = JsonConvert.DeserializeObject<List<List<LikeVideo>>>(ll.Content.ReadAsStringAsync().Result);

                        foreach (var userData in LikeMates)
                        {
                            buttons = new List<List<InlineKeyboardButton>> { };

                            Console.WriteLine(userData[0].UserName);

                            for (int i = 0; i < userData.Count; i++)
                            {
                                buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithUrl($"{userData[i].VideoTitle}", @"https://www.youtube.com/watch?v=" + userData[i].VideolId)
                        }
                                );
                                CallBackD callBack4 = new CallBackD()
                                {
                                    Idv = userData[i].VideolId,
                                    Do = "Like"
                                };
                                var json1 = JsonConvert.SerializeObject(callBack4);
                                CallBackD callBack2 = new CallBackD()
                                {
                                    Idv = userData[i].VideolId,
                                    Do = "AddToP"
                                };
                                var json4 = JsonConvert.SerializeObject(callBack2);

                                buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData("Like",json1),
                            InlineKeyboardButton.WithCallbackData("Add to playlist",json4)
                        }
                                );
                                Console.WriteLine(userData[i].VideoTitle);

                            }


                            keyboardMarkup = new InlineKeyboardMarkup
                                (
                                buttons
                                );
                            await botClient.SendTextMessageAsync(message.Chat.Id, userData[0].UserName, replyMarkup: keyboardMarkup);

                        }


                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "You have no Likemates(");

                    }

                    return;


                case "Likes":
                    UserWatching.AddRecuestAsync(_maxRequestID, message.From.Id.ToString(), message.From.Username, "@Likes", message.Date.AddHours(3).ToString());
                    max = int.Parse(_maxRequestID);
                    max++;
                    _maxRequestID = max.ToString();

                    Console.WriteLine($"User {message.From} Likes");
                    //---
                  
                    var lll = await clients.Client.GetAsync($"/LikeVideo/userlikevideos?userId={message.From.Id}");

                    if (lll.StatusCode == System.Net.HttpStatusCode.OK && lll.Content != null)
                    {
                        var Likevideos = JsonConvert.DeserializeObject<List<LikeVideo>>(lll.Content.ReadAsStringAsync().Result);

                        buttons = new List<List<InlineKeyboardButton>> { };

                        for (int i = 0; i < Likevideos.Count; i++)
                        {
                            buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithUrl($"{Likevideos[i].VideoTitle}", @"https://www.youtube.com/watch?v=" + Likevideos[i].VideolId)
                        }
                            );
                            CallBackD callBack4 = new CallBackD()
                            {
                                Idv = Likevideos[i].VideolId,
                                Do = "UnLike"
                            };
                            var json1 = JsonConvert.SerializeObject(callBack4);
                            CallBackD callBack2 = new CallBackD()
                            {
                                Idv = Likevideos[i].VideolId,
                                Do = "AddToP"
                            };
                            var json4 = JsonConvert.SerializeObject(callBack2);

                            buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData("DisLike",json1),
                            InlineKeyboardButton.WithCallbackData("Add to playlist",json4)
                        }
                            );
                            Console.WriteLine(Likevideos[i].VideoTitle);

                        }


                        keyboardMarkup = new InlineKeyboardMarkup
                            (
                            buttons
                            );
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Liked videos:", replyMarkup: keyboardMarkup);

                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "You have no Likes(");

                    }

                    return;

                case "Playlists":
                    UserWatching.AddRecuestAsync(_maxRequestID, message.From.Id.ToString(), message.From.Username, "@Playlists", message.Date.AddHours(3).ToString());
                    max = int.Parse(_maxRequestID);
                    max++;
                    _maxRequestID = max.ToString();

                    Console.WriteLine($"User {message.From} Playlists");
                    //---

                    var result1 = await clients.Client.GetAsync($" /Playlist/userplaylists?Userid={message.From.Id}");

                    List<Playlist> playlists = new List<Playlist> { };

                    if (result1.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        playlists = JsonConvert.DeserializeObject<List<Playlist>>(result1.Content.ReadAsStringAsync().Result);
                    }

                    buttons = new List<List<InlineKeyboardButton>> { };

                    foreach (var PlayL in playlists)
                    {
                        CallBackD callBack2 = new CallBackD()
                        {

                            Idp = PlayL.Id,
                            Do = "ShowPToUser"
                        };
                        var json1 = JsonConvert.SerializeObject(callBack2);

                        buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData(PlayL.PlaylistName,json1),
                        }
                        );
                    }
                    CallBackD callBack1 = new CallBackD()
                    {

                        Do = "AddPlaylist"
                    };
                    var json2 = JsonConvert.SerializeObject(callBack1);
                    buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData("Add playlist",json2),
                        }
                       );

                    keyboardMarkup = new InlineKeyboardMarkup
                         (
                         buttons
                         );

                    Console.WriteLine("Show Playlist");

                    await botClient.SendTextMessageAsync(message.Chat.Id, "Your playlists:", replyMarkup: keyboardMarkup);


                    return;

                case "Genres":
                    UserWatching.AddRecuestAsync(_maxRequestID, message.From.Id.ToString(), message.From.Username, "@Genres", message.Date.AddHours(3).ToString());
                    max = int.Parse(_maxRequestID);
                    max++;
                    _maxRequestID = max.ToString();

                    Console.WriteLine($"User {message.From} Genres");
                    //---

                    var result5 = await clients.Client.GetAsync($"/YouTubeApi/genresbyrequest");

                    List<List<Models.Video>> genres = new List<List<Models.Video>> { };

                    if (result5.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        genres = JsonConvert.DeserializeObject<List<List<Models.Video>>>(result5.Content.ReadAsStringAsync().Result);
                        if(genres.Count != 0)
                        {

                            for (int j = 0; j < genres.Count; j++)
                            {
                                buttons = new List<List<InlineKeyboardButton>> { };

                                for (int i = 0; i < genres[j].Count; i++)
                                {
                                    buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithUrl($"{genres[j][i].VideoTitle}", @"https://www.youtube.com/watch?v=" + genres[j][i].VideoId)
                        }
                                    );
                                    CallBackD callBack4 = new CallBackD()
                                    {
                                        Idv = genres[j][i].VideoId,
                                        Do = "Like"
                                    };
                                    var json1 = JsonConvert.SerializeObject(callBack4);
                                    CallBackD callBack2 = new CallBackD()
                                    {
                                        Idv = genres[j][i].VideoId,
                                        Do = "AddToP"
                                    };
                                    var json4 = JsonConvert.SerializeObject(callBack2);

                                    buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData("Like",json1),
                            InlineKeyboardButton.WithCallbackData("Add to playlist",json4)
                        }
                                    );
                                    Console.WriteLine(genres[j][i].VideoTitle);

                                }


                                keyboardMarkup = new InlineKeyboardMarkup
                                    (
                                    buttons
                                    );

                                switch (j)
                                {
                                    case 0:
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Classical", replyMarkup: keyboardMarkup);
                                        break;
                                    case 1:
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Pop", replyMarkup: keyboardMarkup);
                                        break;
                                    case 2:
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Country", replyMarkup: keyboardMarkup);
                                        break;
                                    case 3:
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Reggae", replyMarkup: keyboardMarkup);
                                        break;
                                    case 4:
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Rock", replyMarkup: keyboardMarkup);
                                        break;
                                    case 5:
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Jazz", replyMarkup: keyboardMarkup);
                                        break;
                                    case 6:
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Hip hop", replyMarkup: keyboardMarkup);
                                        break;
                                    case 7:
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Electronic", replyMarkup: keyboardMarkup);
                                        break;
                                }

                            }
                            Console.WriteLine("Show Genres");
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Not today");
                            Console.WriteLine("Error");
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Not today");
                        Console.WriteLine("Error");
                    }

                    return;

            }

            var result = await clients.Client.GetAsync($"/Values/lastmode?userId={message.From.Id}");

            string Mode = result.Content.ReadAsStringAsync().Result;

            switch (Mode)
            {
                case "@Serch":
                    UserWatching.AddRecuestAsync(_maxRequestID, message.From.Id.ToString(), message.From.Username, message.Text, message.Date.AddHours(3).ToString());
                    max = int.Parse(_maxRequestID);
                    max++;
                    _maxRequestID = max.ToString();

                    Console.WriteLine($"User {message.From} {message.Text}");
                    //---

                    result = await clients.Client.GetAsync($"/YouTubeApi/videosbyrequest?request={message.Text}");

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        List<Models.Video> videos = JsonConvert.DeserializeObject<List<Models.Video>>(result.Content.ReadAsStringAsync().Result);

                        if (videos.Count != 0)
                        {
                            List<List<InlineKeyboardButton>> buttons = new List<List<InlineKeyboardButton>> { };

                            for (int i = 0; i < videos.Count; i++)
                            {
                                buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithUrl($"{videos[i].VideoTitle}", @"https://www.youtube.com/watch?v=" + videos[i].VideoId)
                        }
                                );
                                CallBackD callBack4 = new CallBackD()
                                {
                                    Idv = videos[i].VideoId,
                                    Do = "Like"
                                };
                                var json1 = JsonConvert.SerializeObject(callBack4);
                                CallBackD callBack2 = new CallBackD()
                                {
                                    Idv = videos[i].VideoId,
                                    Do = "AddToP"
                                };
                                var json4 = JsonConvert.SerializeObject(callBack2);

                                buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData("Like",json1),
                            InlineKeyboardButton.WithCallbackData("Add to playlist",json4)
                        }
                                );
                                Console.WriteLine(videos[i].VideoTitle);

                            }


                            InlineKeyboardMarkup keyboardMarkup = new InlineKeyboardMarkup
                                (
                                buttons
                                );
                            await botClient.SendTextMessageAsync(message.Chat.Id, "music", replyMarkup: keyboardMarkup);
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "No result, say in ather words");
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "No result, say in ather words");
                    }

                    return;


                case "@AddPlaylist":

                    Playlist playlist = new Playlist()
                    {
                        Id = _maxPlayListID,
                        UserId = message.From.Id.ToString(),
                        UserName = message.From.Username,
                        PlaylistName = message.Text,
                        VideoIds = new List<string> { "No videos"},
                        VideoTitles = new List<string> { "No videos" }
                    };

                    max = int.Parse(_maxPlayListID);
                    max++;
                    _maxPlayListID = max.ToString();


                    var json = JsonConvert.SerializeObject(playlist);

                    var data = new StringContent(json, Encoding.UTF8, "application/json");

                    var post = await clients.Client.PostAsync("/Playlist/addplaylist", data);

                    post.EnsureSuccessStatusCode();

                    var postcontent = post.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(postcontent);

                    await botClient.SendTextMessageAsync(message.Chat.Id, "Playlist added");

                    return;

                case "@Artist":

                    UserWatching.AddRecuestAsync(_maxRequestID, message.From.Id.ToString(), message.From.Username, message.Text, message.Date.AddHours(3).ToString());
                    max = int.Parse(_maxRequestID);
                    max++;
                    _maxRequestID = max.ToString();

                    Console.WriteLine($"User {message.From} {message.Text}");
                    //---

                    string arrt = "";
                    if (message.Text[0] == '*')
                    {
                        for (int i = 1; i < message.Text.Length; i++)
                        {
                            arrt += message.Text[i];
                        }
                    }
                    else
                    {
                        arrt = message.Text;
                    }

                    result = await clients.Client.GetAsync($"/YouTubeApi/artistbyrequest?artist={arrt}");

                    if(result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var videos = JsonConvert.DeserializeObject<List<Models.Video>>(result.Content.ReadAsStringAsync().Result);

                        if (videos.Count!=0)
                        {
                            var buttons = new List<List<InlineKeyboardButton>> { };

                            for (int i = 0; i < (videos.Count > 20 ? 20 : videos.Count); i++)
                            {
                                buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithUrl($"{videos[i].VideoTitle}", @"https://www.youtube.com/watch?v=" + videos[i].VideoId)
                        }
                                );
                                CallBackD callBack4 = new CallBackD()
                                {
                                    Idv = videos[i].VideoId,
                                    Do = "Like"
                                };
                                var json1 = JsonConvert.SerializeObject(callBack4);
                                CallBackD callBack2 = new CallBackD()
                                {
                                    Idv = videos[i].VideoId,
                                    Do = "AddToP"
                                };
                                var json4 = JsonConvert.SerializeObject(callBack2);

                                buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData("Like",json1),
                            InlineKeyboardButton.WithCallbackData("Add to playlist",json4)
                        }
                                );
                                Console.WriteLine(videos[i].VideoTitle);

                            }


                            var keyboardMarkup = new InlineKeyboardMarkup
                                (
                                buttons
                                );
                            await botClient.SendTextMessageAsync(message.Chat.Id, videos[0].ChannelTitle, replyMarkup: keyboardMarkup);

                            if (message.Text[0] == '*')
                            {
                                string WhatToCopy = "";
                                for (int i = 0; i < (videos.Count > 20 ? 20 : videos.Count); i++)
                                {
                                    WhatToCopy += $"https://www.youtube.com/watch?v={videos[i].VideoId} \n";
                                    if (i % 5 == 4)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, WhatToCopy);
                                        WhatToCopy = "";
                                    }
                                }
                            }
                            
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "No result, say in ather words");

                        }
                        
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "No result, say in ather words");
                    }

                    return;


            }


        }

    }
}
