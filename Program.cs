using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot_Month_Budget
{
    class Program
    {
        static GoogleHelper google;
        static bool success;
        public static ReceiverOptions More { get; private set; }

        static void Main(string[] args)
        {
            string tokenG = "JSON ИЗ GOOGLE CONSOLE!!!!!!";
            google = GoogleHelper.GetInstance(tokenG, "Траты за месяц");
            bool success = google.Start().Result;
            var client = new TelegramBotClient("ВАШ ТОКЕН БОТА!!!!!!!!");
            client.StartReceiving(Category, Error);
            Console.ReadLine();
        }

        private static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }

        async static Task Category(ITelegramBotClient client, Update update, CancellationToken token)
        {
            var message = update.Message;
            if (message.Text == "/start")
            {
                success = google.Start().Result;
                if (success)
                {
                    KeyboardButton[][] keyboardLol = new KeyboardButton[][] { new KeyboardButton[] { "Нет", "Да" }, new KeyboardButton[] { "Другая неделя", "Новый месяц" }, new KeyboardButton[] { "Посмотреть остаток" } };
                    ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardLol) { ResizeKeyboard = true };
                    await client.SendTextMessageAsync(
                        message.Chat.Id,
                        text: $"ЗДАРОВА!\nНа дворе {google.Month} неделя №{(google.Week + 1).ToString()}.\nТратил деньги?",
                        replyMarkup: replyKeyboardMarkup);
                }
                else
                {
                    await client.SendTextMessageAsync(
                        message.Chat.Id,
                        text: "Что-то пошло не по плану с гуглом.");
                }
            }

            if (message.Text == "Да")
            {
                KeyboardButton[][] keyboardCategory = new KeyboardButton[][] { new KeyboardButton[] { "Продукты", "Рестораны", "Развлечения" }, new KeyboardButton[] { "Одежда", "Косметика", "Машина" }, new KeyboardButton[] { "Здоровье", "Быт", "Другое" }, new KeyboardButton[] { "Другая неделя" } };
                ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardCategory) { ResizeKeyboard = true };
                await client.SendTextMessageAsync(
                    message.Chat.Id,
                    text: "Куда ты потратил деньги?",
                    replyMarkup: replyKeyboardMarkup);
            }
            if (message.Text == "Другая неделя")
            {
                int week = google.NextWeek();
                if (week != 0)
                {
                    KeyboardButton[][] keyboardLol = new KeyboardButton[][] { new KeyboardButton[] { "Нет", "Да" }, new KeyboardButton[] { "Другая неделя", "Новый месяц" }, new KeyboardButton[] { "Посмотреть остаток" } };
                    ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardLol) { ResizeKeyboard = true };
                    await client.SendTextMessageAsync(
                        message.Chat.Id,
                        text: $"Неделя №{week}.\nОпять потратил деньги?",
                        replyMarkup: replyKeyboardMarkup);
                }
                else
                {
                    KeyboardButton[] keyboardLol = new KeyboardButton[] { "Продолжим", "Новый месяц" };
                    ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardLol) { ResizeKeyboard = true };
                    await client.SendTextMessageAsync(
                        message.Chat.Id,
                        text: "Сейчас последняя неделя месяца.\nПродолжим здесь или откроем новый месяц?",
                        replyMarkup: replyKeyboardMarkup);
                }
            }
            if (message.Text == "Нет")
            {
                google.RefreshToken();
                KeyboardButton[][] keyboardLol = new KeyboardButton[][] { new KeyboardButton[] { "Нет", "Да" }, new KeyboardButton[] { "Другая неделя", "Новый месяц" }, new KeyboardButton[] { "Посмотреть остаток" } };
                ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardLol) { ResizeKeyboard = true };
                await client.SendTextMessageAsync(
                    message.Chat.Id,
                    text: "Пидора ответ.)\nОпять потратил деньги?",
                    replyMarkup: replyKeyboardMarkup);
            }
            if (message.Text == "Отмена")
            {
                KeyboardButton[][] keyboardLol = new KeyboardButton[][] { new KeyboardButton[] { "Нет", "Да" }, new KeyboardButton[] { "Другая неделя", "Новый месяц" }, new KeyboardButton[] { "Посмотреть остаток" } };
                ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardLol) { ResizeKeyboard = true };
                await client.SendTextMessageAsync(
                    message.Chat.Id,
                    text: "Пидора ответ.)\nОпять потратил деньги?",
                    replyMarkup: replyKeyboardMarkup);
            }
            if (message.Text == "Продукты" | message.Text == "Рестораны" | message.Text == "Развлечения" | message.Text == "Одежда" | message.Text == "Косметика" | message.Text == "Машина" | message.Text == "Здоровье" | message.Text == "Быт" | message.Text == "Другое")
            {
                string category = message.Text;
                google.NewCategory(category);
                KeyboardButton keyboardLol = new KeyboardButton("Отмена");
                ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardLol) { ResizeKeyboard = true };
                await client.SendTextMessageAsync(
                    message.Chat.Id,
                    text: "Введи сумму без копеек и шелухи.",
                    replyMarkup: replyKeyboardMarkup);
            }
            if (message.Text == "Новый месяц")
            {
                google.NextMonth();
                KeyboardButton[][] keyboardLol = new KeyboardButton[][] { new KeyboardButton[] { "Нет", "Да" }, new KeyboardButton[] { "Другая неделя", "Новый месяц" }, new KeyboardButton[] { "Посмотреть остаток" } };
                ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardLol) { ResizeKeyboard = true };
                await client.SendTextMessageAsync(
                    message.Chat.Id,
                    text: $"Готово.\nСейчас {google.Month}.\nОпять потратил деньги?",
                    replyMarkup: replyKeyboardMarkup);
            }
            if (message.Text == "Посмотреть остаток")
            {
                KeyboardButton[][] keyboardLol = new KeyboardButton[][] { new KeyboardButton[] { "Нет", "Да" }, new KeyboardButton[] { "Другая неделя", "Новый месяц" }, new KeyboardButton[] { "Посмотреть остаток" } };
                ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardLol) { ResizeKeyboard = true };
                await client.SendTextMessageAsync(
                    message.Chat.Id,
                    text: $"{google.Balance()}.\nОпять потратил деньги?",
                    replyMarkup: replyKeyboardMarkup);
            }

            else
            {
                string value = "";
                for (int i = 0; i < message.Text.Length; i++)
                {
                    if (Char.IsNumber(message.Text, i))
                    {
                        value = value + message.Text[i];
                    }
                }
                if (value != "")
                {
                    string result = google.Set(value);
                    if (result == "")
                    {
                        KeyboardButton[][] keyboardLol = new KeyboardButton[][] { new KeyboardButton[] { "Нет", "Да" }, new KeyboardButton[] { "Другая неделя", "Новый месяц" }, new KeyboardButton[] { "Посмотреть остаток" } };
                        ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardLol) { ResizeKeyboard = true };
                        await client.SendTextMessageAsync(
                            message.Chat.Id,
                            text: $"{value} потрачено на {google.Category}.\nЕщё тратил деньги?",
                            replyMarkup: replyKeyboardMarkup);
                    }
                    else
                    {
                        KeyboardButton[][] keyboardLol = new KeyboardButton[][] { new KeyboardButton[] { "Другая неделя", "Новый месяц" } };
                        ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardLol) { ResizeKeyboard = true };
                        await client.SendTextMessageAsync(
                            message.Chat.Id,
                            text: $"{result}",
                            replyMarkup: replyKeyboardMarkup);
                    }
                }
            }
        }
    }
}
