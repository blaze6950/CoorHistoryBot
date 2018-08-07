using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
using System.Text;
using System.Threading.Tasks;
using CoorHistoryBot.Model;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CoorHistoryBot
{
    class Bbot
    {
        private bool _isCancel = false;
        private DbModel _dbModel;
        private TelegramBotClient Bot;
        private List<User> _currentUsers;
        private List<Place> _buildingPlaces;
        private String _key;

        public void Start(String key)
        {
            _key = key;
            //DoWorkAsync();
            Task.Run(()=> { DoWorkAsync(); });
        }

        public Bbot()
        {
            _dbModel = new DbModel();
            _currentUsers = new List<User>();
            _buildingPlaces = new List<Place>();
        }

        private async void DoWorkAsync()
        {
            try
            {
                Bot = new TelegramBotClient(_key); // инициализируем API
                await Bot.SetWebhookAsync("");

                int offset = 0; // отступ по сообщениям
                while (true)
                {
                    var updates = await Bot.GetUpdatesAsync(offset); // получаем массив обновлений

                    foreach (var update in updates) // Перебираем все обновления
                    {
                        var message = update.Message;
                        if (message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                        {
                            switch (message.Text)
                            {
                                case "/start":
                                    await StartActionAsync(update);
                                    break;
                                case "/test":
                                    await TestActionAsync(update);
                                    break;
                                case "/add":
                                    await AddActionAsync(update);
                                    break;
                                case "/stats":
                                    await StatsActionAsync(update);
                                    break;
                                case "/ok":
                                    await OkActionAsync(update);
                                    break;
                                default:
                                    await DefaultActionAsync(update);
                                    break;
                            }
                        }                        
                        else if (message.Type == Telegram.Bot.Types.Enums.MessageType.Photo)
                        {
                            if (_currentUsers.Contains(update.Message.From))
                            {
                                Place findPlace = null;
                                foreach (var buildingPlace in _buildingPlaces)
                                {
                                    if (buildingPlace.User == update.Message.From)
                                    {
                                        findPlace = buildingPlace;
                                        break;
                                    }
                                }
                                if (findPlace == null)
                                {
                                    findPlace = new Place(update.Message.From);
                                    _buildingPlaces.Add(findPlace);
                                }
                                findPlace.Photos.AddRange(update.Message.Photo);
                            }
                        }
                        else if (message.Type == Telegram.Bot.Types.Enums.MessageType.Location)
                        {
                            TakeLocationAsync(update);
                        }
                        offset = update.Id + 1;
                    }
                    if (_isCancel)
                    {
                        break;
                    }
                }
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task DefaultActionAsync(Update update)
        {
            if (_currentUsers.Contains(update.Message.From))
            {
                Place findPlace = null;
                foreach (var buildingPlace in _buildingPlaces)
                {
                    if (buildingPlace.User == update.Message.From)
                    {
                        findPlace = buildingPlace;
                        break;
                    }
                }
                if (findPlace == null)
                {
                    findPlace = new Place(update.Message.From);
                    _buildingPlaces.Add(findPlace);
                }
                findPlace.Caption.AppendLine(update.Message.Text);
            }
        }

        private async Task StartActionAsync(Update update)
        {
            var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                KeyboardButton.WithRequestLocation("Get places"),
                new KeyboardButton("/add"),
            });

            await Bot.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Choose",
                replyMarkup: RequestReplyKeyboard);
        }

        private async Task TestActionAsync(Update update)
        {
            var places = _dbModel.GetPlaces();
            StringBuilder newListBuilder = new StringBuilder();
            int i = 1;
            foreach (var place in places)
            {
                if (i > 20)
                {
                    break;
                }
                newListBuilder.AppendLine( "/" + i + place.Caption.ToString() + "\n");
                i++;
            }
            await Bot.SendTextMessageAsync(update.Message.Chat.Id, newListBuilder.ToString());
        }

        private void TakeLocationAsync(Update update)
        {            
            if (_currentUsers.Contains(update.Message.From))
            {
                Place findPlace = null;
                foreach (var buildingPlace in _buildingPlaces)
                {
                    if (buildingPlace.User == update.Message.From)
                    {
                        findPlace = buildingPlace;
                        break;
                    }
                }
                if (findPlace == null)
                {
                    findPlace = new Place(update.Message.From);
                    _buildingPlaces.Add(findPlace);
                }
                findPlace.Location = update.Message.Location;
                SaveDataToDb(findPlace);
            }
            else
            {
                FindPlaces(update);
            }
        }

        private async void FindPlaces(Update update)
        {
            GeoCoordinate userCurrentLocation = new GeoCoordinate(update.Message.Location.Latitude, update.Message.Location.Longitude);
            var newlist = _dbModel.GetPlaces();
            newlist.Sort((p1, p2) => {
                var p1Coord = new GeoCoordinate(p1.Location.Latitude, p1.Location.Longitude);
                var p2Coord = new GeoCoordinate(p2.Location.Latitude, p2.Location.Longitude);

                return (int)(userCurrentLocation.GetDistanceTo(p1Coord) - userCurrentLocation.GetDistanceTo(p2Coord));
            });
            StringBuilder newListBuilder = new StringBuilder();
            int i = 1;
            foreach (var place in newlist)
            {
                if (i > 20)
                {
                    break;
                }
                newListBuilder.AppendLine("/" + i + " " + place.Caption.ToString() + "\n");
                i++;
            }
            await Bot.SendTextMessageAsync(update.Message.Chat.Id, newListBuilder.ToString());
        }

        private void SaveDataToDb(Place newPlace)
        {
            _buildingPlaces.Remove(newPlace);
            // adding place to db
        }

        private async Task OkActionAsync(Update update)
        {
            if (_currentUsers.Contains(update.Message.From))
            {                
                await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Отправьте геолокацию этого места");
            }
        }

        private async Task StatsActionAsync(Update update)
        {
            throw new NotImplementedException();
        }

        private async Task AddActionAsync(Update update)
        {
            if (!_currentUsers.Contains(update.Message.From))
            {
                _currentUsers.Add(update.Message.From);
            }
            await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Отправьте фотографию с описанием и потом отправьте команду \"/ok\"");
        }

    }
}
