using APIChallongeClass;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace _7D___Quistis.DataBase
{
    public static class DBEngine
    {
        public static async Task<bool> StorePlayer(string discordNickname)
        {
            try
            {
                int userID = await GetPlayerId(discordNickname);
                if (userID != 0)
                {
                    return false;
                }

                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    await conn.OpenAsync();

                    string querry =
                        $"insert into sddb.players (player_nickname, scores) select '{discordNickname}', 0 where not exists(select player_nickname from sddb.players where player_nickname = '{discordNickname}');";

                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public static async Task<long> GetTotalPlayers()
        {
            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    await conn.OpenAsync();
                    string querry = "SELECT COUNT (*) FROM sddb.players";

                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        var userCount = await cmd.ExecuteScalarAsync();
                        return Convert.ToInt64(userCount);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return -1;
            }
        }

        public static async Task<string> DBGetConnectionString()
        {
            JSONReaderDBClass jsonDB = new JSONReaderDBClass("DB.json");
            await jsonDB.ReadJSON();
            return jsonDB.connectionString;
        }

        public static async Task<bool> AddSeasons()
        {
            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    DateTime dt = DateTime.Now;
                    DateTime dtEnding = dt.AddMonths(10);
                    await conn.OpenAsync();
                    string querry = $"insert into sddb.seasons (starting_date, ending_date) values ('{dt.Year}-{dt.Month}-{dt.Day}','{dtEnding.Year}-{dtEnding.Month}-{dtEnding.Day}');";

                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static async Task<DateTime> GetLastSeasons()
        {
            DateTime dt = DateTime.Now;
            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    await conn.OpenAsync();
                    string querry = "SELECT MAX(ending_date) AS ending_date FROM sddb.seasons;";
                    //send querry to DB
                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        var date = await cmd.ExecuteScalarAsync();

                        dt = DateTime.Parse(date.ToString());

                        //if today is higher than older ending_date, add new season 
                        if (dt < DateTime.Now)
                        {
                            bool success = await AddSeasons();
                            if(success)
                            {
                                dt = await GetLastSeasons();
                            }
                            else
                            {
                                throw new Exception("Couldn't add new season");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return dt;
        }

        public static async Task<int> GetSeasonId(DateTime datetime)
        {
            int id = 0;
            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    await conn.OpenAsync();

                    string querry = $"SELECT (season_id) FROM sddb.seasons where seasons.ending_date = '{datetime.Year}-{datetime.Month}-{datetime.Day}';";

                    //send querry to DB
                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        var answer = await cmd.ExecuteScalarAsync();
                        try
                        {
                            id = Convert.ToInt32(answer);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return id;
        }

        public static async Task<int> GetTypeId(string type)
        {
            int id = 0;
            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    await conn.OpenAsync();
                    string querry = $"SELECT (type_id) FROM sddb.tournaments_types where tournaments_types.name = '{type}';";

                    //send querry to DB
                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        var answer = await cmd.ExecuteScalarAsync();
                        try
                        {
                            id = Convert.ToInt32(answer);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return id;
        }

        public static async Task PostTournament(string type, string name)
        {
            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                { 
                    int typeId = await GetTypeId(type);
                    DateTime dt = await GetLastSeasons();
                    int seasonId = await GetSeasonId(dt.Date);
                    string querry = $"insert into sddb.tournaments(tournament_name,type_id,season_id) VALUES('{name}',{typeId},{seasonId});";
                    await conn.OpenAsync();
                    //send querry to DB
                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task<int> GetPlayerId(string playerName)
        {
            int id = -1;
            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    await conn.OpenAsync();
                    string querry = $"SELECT (player_id) FROM sddb.players where players.player_nickname = '{playerName}';";

                    //send querry to DB
                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        var answer = await cmd.ExecuteScalarAsync();
                        id = Convert.ToInt32(answer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return id;
        }

        public static async Task<int> GetLastTournamentId(string tournament)
        {
            int id = 0;
            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    await conn.OpenAsync();
                    string querry = $"SELECT MAX(tournament_id) AS tournament_id FROM sddb.tournaments where tournaments.tournament_name = '{tournament}';";

                    //send querry to DB
                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        var answer = await cmd.ExecuteScalarAsync();
                        try
                        {
                            id = Convert.ToInt32(answer);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return id;
        }

        public static async Task PostRegistration(string decklist, string tournamentName, string playerName)
        {
            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    int playerId = await GetPlayerId(playerName);
                    int tournamentId = await GetLastTournamentId(tournamentName);
                    string querry = $"insert into sddb.registrations(decklist,tournament_id,player_id) VALUES('{decklist}','{tournamentId}','{playerId}') ON CONFLICT(tournament_id, player_id) DO NOTHING;";
                    await conn.OpenAsync();
                    //send querry to DB
                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task PostMatch(int match_id, string tournament_name, string player1_name, string player2_name)
        {
            int player1_id = await GetPlayerId(player1_name);
            int player2_id = await GetPlayerId(player2_name);
            int tournament_id = await GetLastTournamentId(tournament_name);

            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    string querry = $"insert into sddb.matches(challonge_id,tournament_id,player1_id,player2_id) select {match_id},{tournament_id},{player1_id},{player2_id} where not exists (select challonge_id from sddb.matches where challonge_id = {match_id});";
                    await conn.OpenAsync();
                    //send querry to DB
                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public static async Task AddScoreToMatch(int match_id, int scoreJ1, int scoreJ2, string winnerName)
        {
            int winner_id = await GetPlayerId(winnerName);

            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    string querry = $"update sddb.matches set damages_player1 = {scoreJ1},damages_player2 = {scoreJ2},winner_id = {winner_id} where challonge_id ={match_id};";
                    await conn.OpenAsync();
                    //send querry to DB
                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task ChangePlayerPoints(int points, string playerName)
        {

            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    string querry = $"update sddb.players set scores = scores + {points} where player_nickname = '{playerName}';";
                    await conn.OpenAsync();
                    //send querry to DB
                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task ChangePlayerNameAndUsername(string playerSurname, string playerName, string playerNickName)
        {
            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    string querry = $"update sddb.players set player_name = '{playerName}', player_surname = '{playerSurname}' where player_nickname = '{playerNickName}';";
                    await conn.OpenAsync();
                    //send querry to DB
                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task AddBounty(string playerName)
        {
            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    string querry = $"update sddb.players set scores = scores + 1 where player_nickname = '{playerName}';";
                    await conn.OpenAsync();
                    //send querry to DB
                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task<int> GetScore(string playerName)
        {
            int id = -1;
            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    await conn.OpenAsync();
                    string querry = $"SELECT (scores) FROM sddb.players where players.player_nickname = '{playerName}';";

                    //send querry to DB
                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        var answer = await cmd.ExecuteScalarAsync();
                        id = Convert.ToInt32(answer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return id;
        }
    }
}
