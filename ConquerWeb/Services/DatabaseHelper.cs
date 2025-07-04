using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using ConquerWeb.Models; // News ve Account modelleri i�in
using System;
using System.Data;
using Microsoft.Extensions.Caching.Memory;

namespace ConquerWeb.Services
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;
        private readonly IMemoryCache _cache;

        public DatabaseHelper(IConfiguration configuration, IMemoryCache cache)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _cache = cache;
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public void LogError(string message)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("INSERT INTO logs (log_data) VALUES (@message)", conn))
                    {
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"CRITICAL ERROR: Logging failed during database operation: {ex.Message} - Original Message: {message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CRITICAL ERROR: Unexpected logging error: {ex.Message} - Original Message: {message}");
                }
            }
        }

        public bool RegisterUser(string username, string password, string email, string ipAddress)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("INSERT INTO accounts (Username, Password, Email, IP) VALUES (@username, @password, @email, @ip)", conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@ip", ipAddress);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (MySqlException ex)
                {
                    if (ex.Number == 1062)
                    {
                        if (ex.Message.Contains("Username"))
                            LogError($"Registration Error (Username Already In Use): {username}");
                        else if (ex.Message.Contains("Email"))
                            LogError($"Registration Error (Email Already In Use): {email}");
                    }
                    LogError($"Database Error during Registration: {ex.Message} - Username: {username}, Email: {email}");
                    return false;
                }
                catch (Exception ex)
                {
                    LogError($"Unexpected Error during Registration: {ex.Message} - Username: {username}, Email: {email}");
                    return false;
                }
            }
        }

        public Account GetAccountByUsername(string username)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT UID, Username, Password, Email, Status, IP, SecretID, Creation_Date, Last_Login, Reset_Token, Reset_Token_Expiry FROM accounts WHERE Username = @username", conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Account
                            {
                                UID = reader.GetInt32("UID"),
                                Username = reader.GetString("Username"),
                                Password = reader.GetString("Password"),
                                Email = reader.GetString("Email"),
                                Status = reader.GetInt32("Status"),
                                IP = reader.IsDBNull("IP") ? null : reader.GetString("IP"),
                                SecretID = reader.IsDBNull("SecretID") ? null : reader.GetString("SecretID"),
                                Creation_Date = reader.GetDateTime("Creation_Date"),
                                Last_Login = reader.IsDBNull("Last_Login") ? (DateTime?)null : reader.GetDateTime("Last_Login"),
                                Reset_Token = reader.IsDBNull("Reset_Token") ? null : reader.GetString("Reset_Token"),
                                Reset_Token_Expiry = reader.IsDBNull("Reset_Token_Expiry") ? (DateTime?)null : reader.GetDateTime("Reset_Token_Expiry")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public Account GetAccountByUid(int uid)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT UID, Username, Password, Email, Status, IP, SecretID, Creation_Date, Last_Login, Reset_Token, Reset_Token_Expiry FROM accounts WHERE UID = @uid", conn))
                {
                    cmd.Parameters.AddWithValue("@uid", uid);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Account
                            {
                                UID = reader.GetInt32("UID"),
                                Username = reader.GetString("Username"),
                                Password = reader.GetString("Password"),
                                Email = reader.GetString("Email"),
                                Status = reader.GetInt32("Status"),
                                IP = reader.IsDBNull("IP") ? null : reader.GetString("IP"),
                                SecretID = reader.IsDBNull("SecretID") ? null : reader.GetString("SecretID"),
                                Creation_Date = reader.GetDateTime("Creation_Date"),
                                Last_Login = reader.IsDBNull("Last_Login") ? (DateTime?)null : reader.GetDateTime("Last_Login"),
                                Reset_Token = reader.IsDBNull("Reset_Token") ? null : reader.GetString("Reset_Token"),
                                Reset_Token_Expiry = reader.IsDBNull("Reset_Token_Expiry") ? (DateTime?)null : reader.GetDateTime("Reset_Token_Expiry")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public Account GetAccountByEmail(string email)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT UID, Username, Password, Email, Status, IP, SecretID, Creation_Date, Last_Login, Reset_Token, Reset_Token_Expiry FROM accounts WHERE Email = @email", conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Account
                            {
                                UID = reader.GetInt32("UID"),
                                Username = reader.GetString("Username"),
                                Password = reader.GetString("Password"),
                                Email = reader.GetString("Email"),
                                Status = reader.GetInt32("Status"),
                                IP = reader.IsDBNull("IP") ? null : reader.GetString("IP"),
                                SecretID = reader.IsDBNull("SecretID") ? null : reader.GetString("SecretID"),
                                Creation_Date = reader.GetDateTime("Creation_Date"),
                                Last_Login = reader.IsDBNull("Last_Login") ? (DateTime?)null : reader.GetDateTime("Last_Login"),
                                Reset_Token = reader.IsDBNull("Reset_Token") ? null : reader.GetString("Reset_Token"),
                                Reset_Token_Expiry = reader.IsDBNull("Reset_Token_Expiry") ? (DateTime?)null : reader.GetDateTime("Reset_Token_Expiry")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public void UpdatePassword(int uid, string newPassword)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("UPDATE accounts SET Password = @newPassword WHERE UID = @uid", conn))
                {
                    cmd.Parameters.AddWithValue("@newPassword", newPassword);
                    cmd.Parameters.AddWithValue("@uid", uid);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void SetResetToken(int uid, string token, DateTime expiry)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("UPDATE accounts SET Reset_Token = @token, Reset_Token_Expiry = @expiry WHERE UID = @uid", conn))
                {
                    cmd.Parameters.AddWithValue("@token", token);
                    cmd.Parameters.AddWithValue("@expiry", expiry);
                    cmd.Parameters.AddWithValue("@uid", uid);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Account GetAccountByResetToken(string token)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT UID, Username, Password, Email, Status, IP, SecretID, Creation_Date, Last_Login, Reset_Token, Reset_Token_Expiry FROM accounts WHERE Reset_Token = @token AND Reset_Token_Expiry > NOW()", conn))
                {
                    cmd.Parameters.AddWithValue("@token", token);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Account
                            {
                                UID = reader.GetInt32("UID"),
                                Username = reader.GetString("Username"),
                                Password = reader.GetString("Password"),
                                Email = reader.GetString("Email"),
                                Status = reader.GetInt32("Status"),
                                IP = reader.IsDBNull("IP") ? null : reader.GetString("IP"),
                                SecretID = reader.IsDBNull("SecretID") ? null : reader.GetString("SecretID"),
                                Creation_Date = reader.GetDateTime("Creation_Date"),
                                Last_Login = reader.IsDBNull("Last_Login") ? (DateTime?)null : reader.GetDateTime("Last_Login"),
                                Reset_Token = reader.IsDBNull("Reset_Token") ? null : reader.GetString("Reset_Token"),
                                Reset_Token_Expiry = reader.IsDBNull("Reset_Token_Expiry") ? (DateTime?)null : reader.GetDateTime("Reset_Token_Expiry")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public void ClearResetToken(int uid)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("UPDATE accounts SET Reset_Token = NULL, Reset_Token_Expiry = NULL WHERE UID = @uid", conn))
                {
                    cmd.Parameters.AddWithValue("@uid", uid);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateLastLogin(int uid, string ipAddress)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("UPDATE accounts SET Last_Login = CURRENT_TIMESTAMP, IP = @ip WHERE UID = @uid", conn))
                {
                    cmd.Parameters.AddWithValue("@ip", ipAddress);
                    cmd.Parameters.AddWithValue("@uid", uid);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Character GetCharacterByUserId(int userId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT UID, Name, Level, Experience, Spouse, Body, Face, Hair, Silvers, WHSilvers, CPs, GuildID, Version, Map, X, Y, Job, PreviousJob1, PreviousJob2, Strength, Agility, Vitality, Spirit, ExtraStats, Life, Mana, VirtuePoints, DBScrolls, VIPLevelToReceive, VIPDaysToReceive, DoubleExp, WHPassword, VIPLevel, VIP, PumpkinPoints, TreasurePoints, CTBPoints, MetScrolls, DragonGems, PhoenixGems, RainbowGems, KylinGems, FuryGems, VioletGems, MoonGems, TortoiseGems, Dragonballs, Paradises, GarmentToken, OnlineTime, CurrentKills, Nobility, PKPoints, VotePoints, ClassicPoints, PassiveSkills, HeavensBlessing, LastLogin, BotjailedTime, MutedRecord, MutedTime, CurrentHonor, TotalHonor, TotalWins, TotalLosses FROM characters WHERE UID = @userId", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Character
                            {
                                UID = reader.GetInt32("UID"),
                                Name = reader.GetString("Name"),
                                Level = reader.GetInt16("Level"),
                                Experience = reader.GetInt64("Experience"),
                                Spouse = reader.IsDBNull("Spouse") ? null : reader.GetString("Spouse"),
                                Body = reader.GetInt16("Body"),
                                Face = reader.GetInt16("Face"),
                                Hair = reader.GetInt16("Hair"),
                                Silvers = reader.GetInt64("Silvers"),
                                WHSilvers = reader.GetInt64("WHSilvers"),
                                CPs = reader.GetInt32("CPs"),
                                GuildID = reader.GetInt32("GuildID"),
                                Version = reader.GetInt32("Version"),
                                Map = reader.GetInt32("Map"),
                                X = reader.GetInt16("X"),
                                Y = reader.GetInt16("Y"),
                                Job = reader.GetByte("Job"),
                                PreviousJob1 = reader.GetByte("PreviousJob1"),
                                PreviousJob2 = reader.GetByte("PreviousJob2"),
                                Strength = reader.GetInt16("Strength"),
                                Agility = reader.GetInt16("Agility"),
                                Vitality = reader.GetInt16("Vitality"),
                                Spirit = reader.GetInt16("Spirit"),
                                ExtraStats = reader.GetInt16("ExtraStats"),
                                Life = reader.GetInt16("Life"),
                                Mana = reader.GetInt16("Mana"),
                                VirtuePoints = reader.GetInt32("VirtuePoints"),
                                DBScrolls = reader.GetInt32("DBScrolls"),
                                VIPLevelToReceive = reader.GetInt32("VIPLevelToReceive"),
                                VIPDaysToReceive = reader.GetInt32("VIPDaysToReceive"),
                                DoubleExp = reader.GetInt32("DoubleExp"),
                                WHPassword = reader.IsDBNull("WHPassword") ? null : reader.GetString("WHPassword"),
                                VIPLevel = reader.GetInt32("VIPLevel"),
                                VIP = reader.GetInt32("VIP"),
                                PumpkinPoints = reader.GetInt32("PumpkinPoints"),
                                TreasurePoints = reader.GetInt32("TreasurePoints"),
                                CTBPoints = reader.GetInt32("CTBPoints"),
                                MetScrolls = reader.GetInt32("MetScrolls"),
                                DragonGems = reader.GetInt32("DragonGems"),
                                PhoenixGems = reader.GetInt32("PhoenixGems"),
                                RainbowGems = reader.GetInt32("RainbowGems"),
                                KylinGems = reader.GetInt32("KylinGems"),
                                FuryGems = reader.GetInt32("FuryGems"),
                                VioletGems = reader.GetInt32("VioletGems"),
                                MoonGems = reader.GetInt32("MoonGems"),
                                TortoiseGems = reader.GetInt32("TortoiseGems"),
                                Dragonballs = reader.GetInt32("Dragonballs"),
                                Paradises = reader.GetInt32("Paradises"),
                                GarmentToken = reader.GetInt32("GarmentToken"),
                                OnlineTime = reader.GetInt32("OnlineTime"),
                                CurrentKills = reader.GetInt32("CurrentKills"),
                                Nobility = reader.GetByte("Nobility"),
                                PKPoints = reader.GetInt32("PKPoints"),
                                VotePoints = reader.GetInt32("VotePoints"),
                                ClassicPoints = reader.GetInt32("ClassicPoints"),
                                PassiveSkills = reader.GetInt32("PassiveSkills"),
                                HeavensBlessing = reader.GetInt32("HeavensBlessing"),
                                LastLogin = reader.IsDBNull("LastLogin") ? (DateTime?)null : reader.GetDateTime("LastLogin"),
                                BotjailedTime = reader.GetInt32("BotjailedTime"),
                                MutedRecord = reader.GetByte("MutedRecord"),
                                MutedTime = reader.GetInt32("MutedTime"),
                                CurrentHonor = reader.GetInt32("CurrentHonor"),
                                TotalHonor = reader.GetInt32("TotalHonor"),
                                TotalWins = reader.GetInt32("TotalWins"),
                                TotalLosses = reader.GetInt32("TotalLosses")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public List<TopPlayer> GetTopPlayersFromTopsTable(int topType = 1, int count = 30)
        {
            var topPlayers = new List<TopPlayer>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT id, toptype, Name, Level, Job, GuildName, Nobility, Param, VIPLevel, Spouse, Avatar FROM tops WHERE toptype = @toptype ORDER BY Param DESC LIMIT @count", conn))
                {
                    cmd.Parameters.AddWithValue("@toptype", topType);
                    cmd.Parameters.AddWithValue("@count", count);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            topPlayers.Add(new TopPlayer
                            {
                                Id = reader.GetInt32("id"),
                                TopType = reader.GetInt32("toptype"),
                                Name = reader.GetString("Name"),
                                Level = reader.GetInt16("Level"),
                                Job = reader.GetInt16("Job"),
                                GuildName = reader.IsDBNull("GuildName") ? null : reader.GetString("GuildName"),
                                Nobility = reader.GetByte("Nobility"),
                                Param = reader.GetInt64("Param"),
                                VIPLevel = reader.IsDBNull("VIPLevel") ? 0 : reader.GetInt32("VIPLevel"),
                                Spouse = reader.IsDBNull("Spouse") ? null : reader.GetString("Spouse"),
                                Avatar = reader.IsDBNull("Avatar") ? 0 : reader.GetInt32("Avatar")
                            });
                        }
                    }
                }
            }
            return topPlayers;
        }

        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT product_id, product_name, product_price, product_currency, product_desc, DBScrolls, product_image FROM vtm_store", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                ProductId = reader.GetInt32("product_id"),
                                ProductName = reader.GetString("product_name"),
                                ProductPrice = reader.GetDecimal("product_price"),
                                ProductCurrency = reader.GetString("product_currency"),
                                ProductDesc = reader.GetString("product_desc"),
                                DBScrolls = reader.GetInt32("DBScrolls"),
                                ProductImage = reader.GetString("product_image")
                            });
                        }
                    }
                }
            }
            return products;
        }

        public Product GetProductById(int productId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT product_id, product_name, product_price, product_currency, product_desc, DBScrolls, product_image FROM vtm_store WHERE product_id = @productId", conn))
                {
                    cmd.Parameters.AddWithValue("@productId", productId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Product
                            {
                                ProductId = reader.GetInt32("product_id"),
                                ProductName = reader.GetString("product_name"),
                                ProductPrice = reader.GetDecimal("product_price"),
                                ProductCurrency = reader.GetString("product_currency"),
                                ProductDesc = reader.GetString("product_desc"),
                                DBScrolls = reader.GetInt32("DBScrolls"),
                                ProductImage = reader.GetString("product_image")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool SavePaymentRecord(string characterName, string currency, decimal amount, string email, int? vipDays, int? dbScrolls, DateTime payDate, string paymentscol)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("INSERT INTO payments (CharacterName, Currency, Amount, Email, VIPDays, DBScrolls, PayDate, paymentscol) VALUES (@characterName, @currency, @amount, @email, @vipDays, @dbScrolls, @payDate, @paymentscol)", conn))
                    {
                        cmd.Parameters.AddWithValue("@characterName", characterName);
                        cmd.Parameters.AddWithValue("@currency", currency);
                        cmd.Parameters.AddWithValue("@amount", amount);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@vipDays", vipDays.HasValue ? (object)vipDays.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@dbScrolls", dbScrolls.HasValue ? (object)dbScrolls.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@payDate", payDate);
                        cmd.Parameters.AddWithValue("@paymentscol", paymentscol);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (MySqlException ex)
                {
                    LogError($"Payment Record Error (Database): {ex.Message} - Character: {characterName}, Email: {email}");
                    return false;
                }
                catch (Exception ex)
                {
                    LogError($"Unexpected Error during Payment Record: {ex.Message} - Character: {characterName}, Email: {email}");
                    return false;
                }
            }
        }

        public void UpdateCharacterDBScrolls(string playerName, int dbScrollsAmount)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("UPDATE characters SET DBScrolls = @dbScrollsAmount WHERE Name = @playerName", conn))
                    {
                        cmd.Parameters.AddWithValue("@dbScrollsAmount", dbScrollsAmount);
                        cmd.Parameters.AddWithValue("@playerName", playerName);
                        cmd.ExecuteNonQuery();
                    }
                    LogError($"Character DBScrolls updated: {playerName} - DBScrolls: {dbScrollsAmount}");
                }
                catch (Exception ex)
                {
                    LogError($"Error updating Character DBScrolls: {ex.Message} - Player: {playerName}, DBScrolls: {dbScrollsAmount}");
                }
            }
        }

        public List<News> GetLatestNews(int count = 3)
        {
            var newsList = new List<News>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT id, title, content, author, publish_date FROM news ORDER BY publish_date DESC LIMIT @count", conn))
                {
                    cmd.Parameters.AddWithValue("@count", count);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            newsList.Add(new News
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                Content = reader.GetString("content"),
                                Author = reader.GetString("author"),
                                Publish_Date = reader.GetDateTime("publish_date")
                            });
                        }
                    }
                }
            }
            return newsList;
        }

        public News GetNewsById(int newsId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT id, title, content, author, publish_date FROM news WHERE id = @newsId", conn))
                {
                    cmd.Parameters.AddWithValue("@newsId", newsId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new News
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                Content = reader.GetString("content"),
                                Author = reader.GetString("author"),
                                Publish_Date = reader.GetDateTime("publish_date")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public List<News> GetAllNews()
        {
            var newsList = new List<News>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT id, title, content, author, publish_date FROM news ORDER BY publish_date DESC", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            newsList.Add(new News
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                Content = reader.GetString("content"),
                                Author = reader.GetString("author"),
                                Publish_Date = reader.GetDateTime("publish_date")
                            });
                        }
                    }
                }
            }
            return newsList;
        }

        // YEN� EKLENEN METOT: Haber ekler
        public bool AddNews(News news)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO news (title, content, author, publish_date) VALUES (@title, @content, @author, @publish_date)";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", news.Title);
                        cmd.Parameters.AddWithValue("@content", news.Content);
                        cmd.Parameters.AddWithValue("@author", news.Author);
                        cmd.Parameters.AddWithValue("@publish_date", news.Publish_Date);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error adding news: {ex.Message} - Title: {news.Title}");
                    return false;
                }
            }
        }

        // YEN� EKLENEN METOT: Haber g�nceller
        public bool UpdateNews(News news)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE news SET title = @title, content = @content, author = @author, publish_date = @publish_date WHERE id = @id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", news.Title);
                        cmd.Parameters.AddWithValue("@content", news.Content);
                        cmd.Parameters.AddWithValue("@author", news.Author);
                        cmd.Parameters.AddWithValue("@publish_date", news.Publish_Date);
                        cmd.Parameters.AddWithValue("@id", news.Id);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error updating news with ID {news.Id}: {ex.Message} - Title: {news.Title}");
                    return false;
                }
            }
        }

        // YEN� EKLENEN METOT: Haber siler
        public bool DeleteNews(int newsId)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM news WHERE id = @id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", newsId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error deleting news with ID {newsId}: {ex.Message}");
                    return false;
                }
            }
        }

        public List<DownloadItem> GetAllDownloads()
        {
            var downloadList = new List<DownloadItem>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT id, title, description, file_name, file_size_mb, download_url, publish_date, category FROM downloads ORDER BY publish_date DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@count", 100); // Varsay�lan 100 olarak belirttim, iste�e ba�l�
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            downloadList.Add(new DownloadItem
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                Description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                                File_Name = reader.GetString("file_name"),
                                File_Size_MB = reader.GetDecimal("file_size_mb"),
                                Download_Url = reader.GetString("download_url"),
                                Publish_Date = reader.GetDateTime("publish_date"),
                                Category = reader.IsDBNull("category") ? null : reader.GetString("category")
                            });
                        }
                    }
                }
            }
            return downloadList;
        }

        public List<Guild> GetTopGuilds(int count = 10)
        {
            var guildList = new List<Guild>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT ID, Name, LeaderID, LeaderName, Bulletin, Fund, Wins, Members, LastWinner FROM guilds ORDER BY Members DESC LIMIT @count", conn))
                {
                    cmd.Parameters.AddWithValue("@count", count);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            guildList.Add(new Guild
                            {
                                ID = reader.GetInt32("ID"),
                                Name = reader.GetString("Name"),
                                LeaderID = reader.GetInt32("LeaderID"),
                                LeaderName = reader.GetString("LeaderName"),
                                Bulletin = reader.IsDBNull("Bulletin") ? null : reader.GetString("Bulletin"),
                                Fund = reader.GetInt64("Fund"),
                                Wins = reader.GetInt32("Wins"),
                                Members = reader.GetInt16("Members"),
                                LastWinner = reader.IsDBNull("LastWinner") ? null : reader.GetString("LastWinner")
                            });
                        }
                    }
                }
            }
            return guildList;
        }
        public List<PaymentRecord> GetPaymentRecordsByUsername(string username)
        {
            var paymentRecords = new List<PaymentRecord>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT ID, CharacterName, Currency, Amount, Email, VIPDays, DBScrolls, PayDate, paymentscol FROM payments WHERE CharacterName = @username ORDER BY PayDate DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            paymentRecords.Add(new PaymentRecord
                            {
                                ID = reader.GetInt32("ID"),
                                CharacterName = reader.GetString("CharacterName"),
                                Currency = reader.GetString("Currency"),
                                Amount = reader.GetDecimal("Amount"),
                                Email = reader.GetString("Email"),
                                VIPDays = reader.IsDBNull("VIPDays") ? (int?)null : reader.GetInt32("VIPDays"),
                                DBScrolls = reader.IsDBNull("DBScrolls") ? (int?)null : reader.GetInt32("DBScrolls"),
                                PayDate = reader.GetDateTime("PayDate"),
                                Paymentscol = reader.IsDBNull("paymentscol") ? null : reader.GetString("paymentscol")
                            });
                        }
                    }
                }
            }
            return paymentRecords;
        }
        public OnlineStats GetOnlinePlayerStats()
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                try
                {
                    using (var cmd = new MySqlCommand("SELECT `online`, `max` FROM `online` LIMIT 1", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new OnlineStats
                                {
                                    Online = reader.GetInt32("online"),
                                    Max = reader.GetInt32("max")
                                };
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    LogError($"Database Error getting online stats: {ex.Message}");
                }
                catch (Exception ex)
                {
                    LogError($"Unexpected Error getting online stats: {ex.Message}");
                }
            }
            return null;
        }

        public List<Account> GetAllAccounts()
        {
            var accounts = new List<Account>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT UID, Username, Email, Status, Creation_Date, Last_Login FROM accounts", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            accounts.Add(new Account
                            {
                                UID = reader.GetInt32("UID"),
                                Username = reader.GetString("Username"),
                                Email = reader.GetString("Email"),
                                Status = reader.GetInt32("Status"),
                                Creation_Date = reader.GetDateTime("Creation_Date"),
                                Last_Login = reader.IsDBNull("Last_Login") ? (DateTime?)null : reader.GetDateTime("Last_Login")
                            });
                        }
                    }
                }
            }
            return accounts;
        }

        public bool UpdateAccount(int uid, string newUsername, string newEmail, int newStatus)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE accounts SET Username = @username, Email = @email, Status = @status WHERE UID = @uid";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", newUsername);
                        cmd.Parameters.AddWithValue("@email", newEmail);
                        cmd.Parameters.AddWithValue("@status", newStatus);
                        cmd.Parameters.AddWithValue("@uid", uid);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (MySqlException ex)
                {
                    if (ex.Number == 1062)
                    {
                        if (ex.Message.Contains("Username"))
                            LogError($"Account Update Error (Username Already In Use): UID {uid}, Username: {newUsername}");
                        else if (ex.Message.Contains("Email"))
                            LogError($"Account Update Error (Email Already In Use): UID {uid}, Email: {newEmail}");
                    }
                    LogError($"Database Error during account update for UID {uid}: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    LogError($"Unexpected error during account update for UID {uid}: {ex.Message}");
                    return false;
                }
            }
        }

        public bool DeleteAccount(int uid)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmdChar = new MySqlCommand("DELETE FROM characters WHERE UID = @uid", conn))
                    {
                        cmdChar.Parameters.AddWithValue("@uid", uid);
                        cmdChar.ExecuteNonQuery();
                    }

                    using (var cmdAcc = new MySqlCommand("DELETE FROM accounts WHERE UID = @uid", conn))
                    {
                        cmdAcc.Parameters.AddWithValue("@uid", uid);
                        int rowsAffected = cmdAcc.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            LogError($"Account with UID {uid} deleted successfully.");
                            return true;
                        }
                        return false;
                    }
                }
                catch (MySqlException ex)
                {
                    LogError($"Database Error deleting account for UID {uid}: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    LogError($"Unexpected error deleting account for UID {uid}: {ex.Message}");
                    return false;
                }
            }
        }

        public List<LogEntry> GetLogs(int count = 100)
        {
            var logs = new List<LogEntry>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT id, log_data, timestamp FROM logs ORDER BY timestamp DESC LIMIT @count", conn))
                {
                    cmd.Parameters.AddWithValue("@count", count);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new LogEntry
                            {
                                Id = reader.GetInt32("id"),
                                LogData = reader.GetString("log_data"),
                                Timestamp = reader.GetDateTime("timestamp")
                            });
                        }
                    }
                }
            }
            return logs;
        }

        public int GetTotalUserCount()
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM accounts", conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
        public bool AddProduct(Product product)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO vtm_store (product_name, product_price, product_currency, product_desc, DBScrolls, product_image) VALUES (@productName, @productPrice, @productCurrency, @productDesc, @dbScrolls, @productImage)";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@productName", product.ProductName);
                        cmd.Parameters.AddWithValue("@productPrice", product.ProductPrice);
                        cmd.Parameters.AddWithValue("@productCurrency", product.ProductCurrency);
                        cmd.Parameters.AddWithValue("@productDesc", product.ProductDesc);
                        cmd.Parameters.AddWithValue("@dbScrolls", product.DBScrolls);
                        cmd.Parameters.AddWithValue("@productImage", product.ProductImage);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error adding product: {ex.Message} - Product: {product.ProductName}");
                    return false;
                }
            }
        }

        // Yeni metot: �r�n g�nceller
        public bool UpdateProduct(Product product)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE vtm_store SET product_name = @productName, product_price = @productPrice, product_currency = @productCurrency, product_desc = @productDesc, DBScrolls = @dbScrolls, product_image = @productImage WHERE product_id = @productId";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@productName", product.ProductName);
                        cmd.Parameters.AddWithValue("@productPrice", product.ProductPrice);
                        cmd.Parameters.AddWithValue("@productCurrency", product.ProductCurrency);
                        cmd.Parameters.AddWithValue("@productDesc", product.ProductDesc);
                        cmd.Parameters.AddWithValue("@dbScrolls", product.DBScrolls);
                        cmd.Parameters.AddWithValue("@productImage", product.ProductImage);
                        cmd.Parameters.AddWithValue("@productId", product.ProductId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error updating product with ID {product.ProductId}: {ex.Message} - Product: {product.ProductName}");
                    return false;
                }
            }
        }

        // Yeni metot: �r�n siler
        public bool DeleteProduct(int productId)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM vtm_store WHERE product_id = @productId";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@productId", productId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error deleting product with ID {productId}: {ex.Message}");
                    return false;
                }
            }
        }
    }
}