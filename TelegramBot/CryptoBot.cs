﻿using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot;

namespace TelegramBotCrypto
{
    public class CryptoBot
    {
        TelegramBotClient botClient = new TelegramBotClient("7131736793:AAF2Ac1lzEsg0EwvNmj1E8a5eVjdF3YlJNs");
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        private readonly HttpClient httpClient = new HttpClient();
        private readonly ConcurrentDictionary<long, string> userStates = new ConcurrentDictionary<long, string>();
        private readonly ConcurrentDictionary<long, string> conversionFromCrypto = new ConcurrentDictionary<long, string>();
        private readonly ConcurrentDictionary<long, string> conversionToCrypto = new ConcurrentDictionary<long, string>();
        private readonly ConcurrentDictionary<long, decimal> conversionAmount = new ConcurrentDictionary<long, decimal>();

        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"Бот {botMe.Username} почав працювати");
            Console.ReadKey();
        }

        private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в телеграм бот АПІ:\n {apiRequestException.ErrorCode}" +
                $"\n{apiRequestException.Message}",
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
        }

        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text != null && message.Text.StartsWith("/start", StringComparison.InvariantCultureIgnoreCase))
            {
                await HandleStartCommandAsync(message);
            }
            else if (message.Text == "Показати терміни")
            {
                await HandleShowTermsCommandAsync(message);
            }
            else if (message.Text.StartsWith("Термін:", StringComparison.InvariantCultureIgnoreCase))
            {
                await HandleTermDetailCommandAsync(message);
            }
            else if (userStates.TryGetValue(message.Chat.Id, out string state))
            {
                if (state == "waiting_for_crypto_name")
                {
                    await HandleCryptoNameInputAsync(message);
                }
                else if (state == "waiting_for_conversion_from_crypto")
                {
                    await HandleConversionFromCryptoInputAsync(message);
                }
                else if (state == "waiting_for_conversion_to_crypto")
                {
                    await HandleConversionToCryptoInputAsync(message);
                }
                else if (state == "waiting_for_conversion_amount")
                {
                    await HandleConversionAmountInputAsync(message);
                }
            }
            else if (message.Text == "Отримати курс")
            {
                await HandleGetRateCommandAsync(message);
            }
            else if (message.Text == "Конвертація")
            {
                await HandleConvertCommandAsync(message);
            }
            else if (message.Text == "Створення криптогаманця")
            {
                await HandleCreateWalletCommandAsync(message);
            }
            else if (message.Text == "Показати команди")
            {
                await ShowCommandsAsync(message);
            }
            else
            {
                await HandleUnknownCommandAsync(message);
            }
        }

        private async Task HandleStartCommandAsync(Message message)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
            {
        new KeyboardButton[] { "Отримати курс", "Конвертація" },
        new KeyboardButton[] { "Показати терміни" }
    })
            {
                ResizeKeyboard = true
            };

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Привіт! Я Cryptobot! Мої функції: \n-Надавати тобі актуальну інформацію про курс криптовалюти \n-Конвертувати одну криптовалюту в іншу \n-Надавати інформацію про терміни криптовалют. \nОбери команду:",
                replyMarkup: replyKeyboardMarkup
            );
        }

        private async Task HandleGetRateCommandAsync(Message message)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Окей! Напишіть, будь ласка, повну назву криптовалюти, курс якої ви хочете отримати \n(Для прикладу: 'bitcoin')",
                replyMarkup: new ReplyKeyboardRemove()
            );

            userStates[message.Chat.Id] = "waiting_for_crypto_name";
        }

        private async Task HandleCryptoNameInputAsync(Message message)
        {
            var cryptoName = message.Text.ToLowerInvariant();
            var rate = await GetCryptoRateAsync(cryptoName);

            if (rate != null)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Курс {cryptoName}: {rate} USD"
                );

                userStates.TryRemove(message.Chat.Id, out _);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Вибачте, не вдалося отримати курс криптовалюти. Спробуйте ще раз."
                );

                userStates[message.Chat.Id] = "waiting_for_crypto_name";
            }
        }

        private async Task HandleConvertCommandAsync(Message message)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Окей! Напишіть, будь ласка, повну назву криптовалюти, яку ви хочете конвертувати.",
                replyMarkup: new ReplyKeyboardRemove()
            );

            userStates[message.Chat.Id] = "waiting_for_conversion_from_crypto";
        }

        private async Task HandleConversionFromCryptoInputAsync(Message message)
        {
            var fromCrypto = message.Text.ToLowerInvariant();
            conversionFromCrypto[message.Chat.Id] = fromCrypto;

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Напишіть, будь ласка, повну назву криптовалюти, в яку ви хочете конвертувати."
            );

            userStates[message.Chat.Id] = "waiting_for_conversion_to_crypto";
        }

        private async Task HandleConversionToCryptoInputAsync(Message message)
        {
            var toCrypto = message.Text.ToLowerInvariant();
            conversionToCrypto[message.Chat.Id] = toCrypto;

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Введіть кількість криптовалюти, яку ви хочете конвертувати."
            );

            userStates[message.Chat.Id] = "waiting_for_conversion_amount";
        }

        private async Task HandleConversionAmountInputAsync(Message message)
        {
            if (decimal.TryParse(message.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
            {
                conversionAmount[message.Chat.Id] = amount;

                var fromCrypto = conversionFromCrypto[message.Chat.Id];
                var toCrypto = conversionToCrypto[message.Chat.Id];

                var fromRate = await GetCryptoRateAsync(fromCrypto);
                var toRate = await GetCryptoRateAsync(toCrypto);

                if (fromRate.HasValue && toRate.HasValue)
                {
                    var convertedAmount = amount * fromRate.Value / toRate.Value;

                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Конвертація: {amount} {fromCrypto} дорівнює {convertedAmount} {toCrypto}."
                    );
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Вибачте, не вдалося отримати курси криптовалют. Спробуйте ще раз."
                    );
                }

                userStates.TryRemove(message.Chat.Id, out _);
                conversionFromCrypto.TryRemove(message.Chat.Id, out _);
                conversionToCrypto.TryRemove(message.Chat.Id, out _);
                conversionAmount.TryRemove(message.Chat.Id, out _);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Введіть коректну кількість криптовалюти."
                );

                userStates[message.Chat.Id] = "waiting_for_conversion_amount";
            }
        }

        private async Task HandleCreateWalletCommandAsync(Message message)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Окей! Будь ласка, сперше, введіть назву криптогаманця.",
                replyMarkup: new ReplyKeyboardRemove()
            );
        }

        private async Task HandleShowTermsCommandAsync(Message message)
        {
            var terms = await GetCryptoTermsAsync();
            var termNames = string.Join("\n", terms.Select(t => t.Term));

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"Список термінів:\n{termNames}\n\nВведіть 'Термін: [назва]' для отримання значення терміну."
            );
        }

        public async Task HandleTermDetailCommandAsync(Message message)
        {
            var termName = message.Text.Split(' ')[1];
            var term = await GetCryptoTermByNameAsync(termName);

            if (term != null)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"{term.Term}: {term.Definition}");
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Термін не знайдено.");
            }
        }


        private async Task<List<CryptoTerm>> GetCryptoTermsAsync()
        {
            var response = await httpClient.GetAsync("https://localhost:7201/CryptoTerms");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<CryptoTerm>>(responseBody);
        }

        public async Task<CryptoTerm> GetCryptoTermByNameAsync(string termName)
        {
            try
            {
                HttpClient client = new HttpClient();
                string url = $"https://localhost:7201/cryptoTerms/name/{termName}";
                string response = await client.GetStringAsync(url);

                CryptoTerm term = JsonConvert.DeserializeObject<CryptoTerm>(response);
                return term;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return null;
            }
        }




        private async Task ShowCommandsAsync(Message message)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "Отримати курс", "Конвертація", "Показати терміни" }
            })
            {
                ResizeKeyboard = true
            };

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Ось доступні команди:",
                replyMarkup: replyKeyboardMarkup
            );
        }

        private async Task HandleUnknownCommandAsync(Message message)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Вибачте, я не розумію цю команду. Будь ласка, оберіть одну з доступних функцій.",
                replyMarkup: new ReplyKeyboardRemove()
            );
        }

        private async Task<decimal?> GetCryptoRateAsync(string cryptoName)
        {
            try
            {
                var response = await httpClient.GetAsync($"https://localhost:7201/CryptoCurrency/price?currency={cryptoName}");
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var rate = decimal.Parse(responseBody, CultureInfo.InvariantCulture);
                return rate;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e.Message}");
                return null;
            }
        }
    }
}
