using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class RpsUdpServer
{
    private static UdpClient udpServer = null!;
    private static IPEndPoint clientEndPoint = null!;
    private static int rounds = 0;
    private static int score1 = 0, score2 = 0;
    private static bool gameFinished = false;

    static void Main()
    {
        udpServer = new UdpClient(5000);
        clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        Console.WriteLine("UDP сервер запущено на порту 5000...");

        while (!gameFinished)
        {
            string message = ReceiveMessage();
            if (message == "CONNECT_REQUEST")
            {
                Console.Write("Прийняти підключення? (Y/N): ");
                string input = Console.ReadLine()?.Trim().ToUpper();
                if (input == "Y")
                {
                    SendMessage("ACCEPT");
                    Console.WriteLine("[СЕРВЕР] Підключення прийнято.");
                    GameLoop();
                }
                else
                {
                    SendMessage("REJECT");
                    Console.WriteLine("[СЕРВЕР] Підключення відхилено.");
                }
            }
        }

        Console.WriteLine("Сервер завершив свою роботу.");
    }

    static void GameLoop()
    {
        while (rounds < 5 && !gameFinished)
        {
            string moves = ReceiveMessage();

            if (moves == "DRAW_REQUEST")
            {
                if (score1 == 1 && score2 == 1 || score1 == 2 && score2 == 2)
                {
                    SendMessage("DRAW_REQUEST");
                    string response = ReceiveMessage();
                    if (response == "DRAW_ACCEPT")
                    {
                        SendMessage("\nГра завершена нічиєю.");
                        Console.WriteLine($"\nГра завершена нічиєю. Фінальний рахунок: {score1}-{score2}");
                        EndGame();
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Нічия відхилена. Гра триває.");
                        continue;
                    }
                }
                else
                {
                    SendMessage("DRAW_REJECT");
                    Console.WriteLine("Запит на нічию відхилено: рахунок не 1-1 або 2-2.");
                }
            }

            if (moves == "LOSE")
            {
                SendMessage("\nОдин з гравців визнав поразку. Гра завершена!");
                Console.WriteLine("\nГра завершена через поразку одного з гравців.");
                EndGame();
                return;
            }

            string[] choices = moves.Split(',');

            if (choices.Length != 2) continue;

            string move1 = choices[0].Trim();
            string move2 = choices[1].Trim();

            Console.WriteLine($"\nРаунд {rounds + 1}: Гравець 1 - {move1} | Гравець 2 - {move2}");

            int result = GetRoundWinner(move1, move2);
            if (result == 1) score1++;
            if (result == 2) score2++;

            rounds++;
            string scoreMessage = $"Рахунок: {score1}-{score2}";
            SendMessage(scoreMessage);
            Console.WriteLine($"\nПоточний рахунок: Гравець 1 - {score1} | Гравець 2 - {score2}");
        }

        string finalMessage = score1 > score2 ? "Гравець 1 переміг!" :
                              score1 < score2 ? "Гравець 2 переміг!" : "Нічия!";
        SendMessage(finalMessage);
        Console.WriteLine("\nГра завершена. " + finalMessage);
        EndGame();
    }

    static void EndGame()
    {
        gameFinished = true;
        udpServer.Close();
    }

    static string ReceiveMessage()
    {
        if (gameFinished) return string.Empty;

        var receivedData = udpServer.Receive(ref clientEndPoint);
        string message = Encoding.UTF8.GetString(receivedData);
        Console.WriteLine($"\n[СЕРВЕР] Отримано повідомлення: {message}");
        return message;
    }

    static void SendMessage(string message)
    {
        if (gameFinished) return;

        byte[] data = Encoding.UTF8.GetBytes(message);
        udpServer.Send(data, data.Length, clientEndPoint);
    }

    static int GetRoundWinner(string move1, string move2)
    {
        if (move1 == move2)
            return 0;
        if ((move1 == "Камінь" && move2 == "Ножиці") ||
            (move1 == "Ножиці" && move2 == "Папір") ||
            (move1 == "Папір" && move2 == "Камінь"))
            return 1;
        return 2;
    }
}
