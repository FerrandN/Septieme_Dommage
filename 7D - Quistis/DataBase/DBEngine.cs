using APIChallongeClass;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Text;
using System.Text.RegularExpressions;
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
                int seasonID = await GetSeasonId(await GetLastSeasons());
                if (userID != 0)
                {
                    return false;
                }

                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    await conn.OpenAsync();

                    string querry =
                        $"insert into sddb.players (player_nickname, bonus) select '{discordNickname}', 0 where not exists(select player_nickname from sddb.players where player_nickname = '{discordNickname}');";

                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

				using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
				{
					await conn.OpenAsync();

					string querry =
						$"insert into sddb.bonus (player_id, season_id) select {userID}, {seasonID} where not exists(select player_id, season_id  from sddb.bonus where player_id = {userID} and season_id = {seasonID});";

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

        public static async Task PostTournament(string type, string name, DateTime time, string url)
        {
            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                { 
                    int typeId = await GetTypeId(type);
                    DateTime dt = await GetLastSeasons();
                    int seasonId = await GetSeasonId(dt.Date);
                    string querry = $"insert into sddb.tournaments(tournament_name,type_id,season_id,starting_date, tournament_url) VALUES('{name}','{typeId}','{seasonId}','{time.Year}-{time.Month}-{time.Day},'{url}');";
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
                    string querry = $"SELECT MAX(tournament_id) AS tournament_id FROM sddb.tournaments where tournaments.tournament_url = '{tournament}';";

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
                    string querry = $"update sddb.players set bonus = bonus + {points} where player_nickname = '{playerName}';";
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

		public static async Task<int> GetBonusId(string playerName)
		{
			int id = 0;
			try
			{
				int userID = await GetPlayerId(playerName);
				int seasonID = await GetSeasonId(await GetLastSeasons());


				using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
				{
					string querry = $"select bonus_id from sddb.bonus b where (player_id = {userID} and season_id = {seasonID});";
                    await conn.OpenAsync();
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

		public static async Task ChangePlayerBonus(int points, string playerName)
		{
			try
			{
				int bonusID = await GetBonusId(playerName);

				using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
				{
					string querry = $"update sddb.bonus set bonus_value = bonus_value + {points} where bonus_id = '{bonusID}';";
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
		public static async Task AddTournamentPoints(int points, string playerName, string tournamentname)
        {
            try
            {
                int tournamentid = await GetLastTournamentId(tournamentname);
                int playerid = await GetPlayerId(playerName);
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    string querry = $"UPDATE sddb.registrations set players_points_won = {points} where sddb.registrations.player_id = {playerid} and tournament_id = {tournamentid};";
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

		public static async Task ChangeMatchesScore(int matchId, int scoreJ1, int scoreJ2, string winner)
		{
			try
			{
                int id = await GetPlayerId(winner);
				using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
				{
					string querry = $"update sddb.matches set winner_id = '{id}', damages_player1 = '{scoreJ1}', damages_player2 = '{scoreJ2}' where challonge_id  = '{matchId}';";
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
            int playerId = await GetPlayerId(playerName);
            int seasonId = await GetSeasonId(await GetLastSeasons());
            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    string querry = $"update sddb.bonus set bonus_value = bonus_value + 1 where player_id = '{playerId}' and season_id = '{seasonId}';";
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
            int playerId = await GetPlayerId(playerName);
            int seasonId = await GetSeasonId(await GetLastSeasons());
            int score = 0;
            try
            {
                using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    await conn.OpenAsync();
                    string querry = $" SELECT SUM(r.players_points_won) + COALESCE(p.bonus, 0) + COALESCE(b.bonus_value, 0) AS playertotalscoreofseason from sddb.registrations r join sddb.players p on p.player_id = r.player_id join sddb.bonus b on b.player_id  = p.player_id join sddb.tournaments t on r.tournament_id = t.tournament_id join sddb.seasons s on s.season_id = t.season_id where p.player_id = '{playerId}' and s.season_id = '{seasonId}' group by p.bonus, b.bonus_value ;";

                    //send querry to DB
                    using (var cmd = new NpgsqlCommand(querry, conn))
                    {
                        var answer = await cmd.ExecuteScalarAsync();
                        score = Convert.ToInt32(answer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return score;
        }

		public static async Task AddLogs(string commandName, string userName, string optionsName, string optionValues)
		{
			try
			{
				using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
				{
					string querry = $"insert into sddb.logs (command_name, user_name, option_name, value) values('{commandName}','{userName}','{optionsName}','{optionValues}');";
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

		public static async Task PostEndDate(string tournamentName)
		{
			try
			{
				using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
				{
                    DateTime time = DateTime.Now;
					int tournamentId = await GetLastTournamentId(tournamentName);
					string querry = $"update sddb.tournaments set ending_date = ('{time.Year}-{time.Month}-{time.Day}') where tournament_id  = {tournamentId};";
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

		public static async Task Merge2Players(string userToSave, string userToDelete)
		{
            int userToSaveID = await GetPlayerId(userToSave);
            int userToDeleteID = await GetPlayerId(userToDelete);

			try
			{
				using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
				{
					string querry = $"update sddb.matches set player1_id = '{userToSaveID}' where player1_id = '{userToDeleteID}'; update sddb.matches set player2_id = '{userToSaveID}' where player2_id = '{userToDeleteID}'; update sddb.registrations  set player_id = '{userToSaveID}' where player_id = '{userToDeleteID}';";
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

		public static async Task<List<(string decklist, string playerNickname)>> GetDeckList(string tournamentUrl)
		{
			int tournamentId = await GetLastTournamentId(tournamentUrl);
			List<(string decklist, string playerNickname)> results = new List<(string decklist, string playerNickname)>();
			try
            {
				using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
                {
                    await conn.OpenAsync();
                    string query = $"SELECT decklist, p.player_nickname FROM sddb.registrations JOIN sddb.players p ON sddb.registrations.player_id = p.player_id JOIN sddb.tournaments t ON sddb.registrations.tournament_id = t.tournament_id WHERE t.tournament_id = {tournamentId};";

					//send query to DB
					using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string decklist = reader.GetString(0);
                                string playerNickname = reader.GetString(1);
                                results.Add((decklist, playerNickname));
                            }

                            foreach (var result in results)
                            {
                                Console.WriteLine($"Decklist: {result.decklist}, Player Nickname: {result.playerNickname}");
                            }
                        }
                    }
                }
			}
			catch (Exception ex)
			{
				// Return a failed task with the exception
				var tcs = new TaskCompletionSource<List<(string decklist, string playerNickname)>>();
				tcs.SetException(ex);
				return await tcs.Task;
			}

			return results;
		}

		public static async Task UpdateAllUrl(string tournamentName, string tournamentUrl)
		{
			try
			{
				using (var conn = new NpgsqlConnection(await DBGetConnectionString()))
				{
					DateTime time = DateTime.Now;
					int tournamentId = await GetLastTournamentId(tournamentName);
					string querry = $"update sddb.tournaments set tournament_url = ('{tournamentUrl}') where tournament_id  = {tournamentId};";
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
	}
}
