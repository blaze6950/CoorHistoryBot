using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CoorHistoryBot
{
    class Bbot
    {
        private bool _isCancel = false;
        private TelegramBotClient Bot;
        private List<User> _currentUsers;
        private List<Place> _buildingPlaces;
        private String _key;

        public void Start(String key)
        {
            _key = key;
            Task.Run(()=> { DoWorkAsync(); });
        }

        public Bbot()
        {
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
                            if (_currentUsers.Contains(update.Message.From))
                            {
                                
                            }
                            else
                            {
                                TakeLocationAsync(update);
                            }                            
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
                FindPlaces();
            }
        }

        private void FindPlaces()
        {
            //throw new NotImplementedException();
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
