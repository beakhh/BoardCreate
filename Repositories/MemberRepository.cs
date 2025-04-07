using BoardCreate.DataContext;
using Microsoft.EntityFrameworkCore;
using BoardCreate.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using BoardCreate.Models.Member;
using Microsoft.AspNetCore.SignalR;

namespace BoardCreate.Repositories
{
    public class MemberRepository
    {
        private readonly ILogger<MemberRepository> _logger;
        private readonly MemberDbContext _memberDbContext;
        private readonly IHubContext<NotificationHub> _hubContext; // ✅ 변경됨
        private readonly string _connectionString;

        public MemberRepository(MemberDbContext memberDbContext,
                                IHubContext<NotificationHub> hubContext, // ✅ 변경됨
                                ILogger<MemberRepository> logger,
                                string connectionString)
        {
            _memberDbContext = memberDbContext;
            _hubContext = hubContext; // ✅ 변경됨
            _logger = logger;
            _connectionString = connectionString;
        }



        /*
        public async Task<int> SelectUserID(string userId)
        {
            var count = await _memberDbContext.Member
                .FromSqlRaw("SELECT COUNT(*) FROM UserInfo.Member WHERE UserID = {0}", userId).CountAsync();

            return count;
        }

        public async Task<int> SelectUserID(string userId)
        {
            var countResult = await _memberDbContext.Members
                .FromSqlRaw("SELECT COUNT(*) AS Count FROM UserInfo.Member WHERE UserID = {0}", userId)
                .Select(x => EF.Property<int>(x, "Count"))
                .FirstOrDefaultAsync();

            return countResult;
        }
        */
        // 아이디 중복검색
        public async Task<int> SelectUserID(string userId)
        {
            var countResult = await _memberDbContext.Member
                .FromSqlRaw("SELECT COUNT(*) AS Count FROM UserInfo.Member WHERE UserID = {0}", userId)
                .Select(x => EF.Property<int>(x, "Count"))
                .FirstOrDefaultAsync();
            return countResult;
        }
        public async Task<bool> SelectIdNickCheckAsync(string userId, string userNick)
        {
            int count = await _memberDbContext.Member
                .Where(m => m.UserId == userId || m.NickName == userNick)
                .CountAsync();

            return count == 0;
        }
        /*
        public async Task<List<MemberModel>> GetAllMembersAsync()
        {
            return await _memberDbContext.Members.FromSqlRaw("SELECT * FROM Members").ToListAsync();
        }

        public async Task AddMember(MemberModel member)
        {
            using (var connection = _memberDbContext.Database.GetDbConnection())
            {
                await connection.OpenAsync();

                var query = "INSERT INTO Members (Username, Password, Email) VALUES (@Username, @Password, @Email)";

                using (var command = new SqlCommand(query, (SqlConnection)connection))
                {
                    command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar) { Value = member.Username });
                    command.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar) { Value = member.Password });
                    command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar) { Value = member.Email });

                    await command.ExecuteNonQueryAsync();
                }
            }
        */
        public async Task AddMember(MemberModel model)
        {
            await _memberDbContext.Database.ExecuteSqlRawAsync(
                "INSERT INTO UserInfo.Member (UserID, UserSalt, UserPW, NickName, Gender) VALUES ({0}, {1}, {2}, {3}, {4})",
                model.UserId, model.UserSalt, model.UserPW, model.NickName, model.Gender
            );
        }
        public async Task<bool> Check_Id_Nick_duplication(string userValue, int type)
        {
            var result = _memberDbContext.Member.AsQueryable();

            if (type == 0 )
            {
                result = result.Where(m => m.UserId == userValue);
            }
            else if (type == 1)
            {
                result = result.Where(m => m.NickName == userValue);
            }

            int count = await result.CountAsync();

            return count == 0;
        }
        /*
        public async Task<MemberModel> GetUserByIdData(string UserValue, int type)
        {

            MemberModel? user = null;
            if (type == 0)
            {
                user = await _memberDbContext.Member.FirstOrDefaultAsync(m => m.UserId == UserValue);

            }
            else if (type == 1)
            {
                user = await _memberDbContext.Member.FirstOrDefaultAsync(m => m.NickName == UserValue);
            }

            return user;
        }
        */
        public async Task<bool> UserPreferencesDuplicateCehck(string userId, int boardIdx)
        {
            bool exists = false;

            string selectQuery = @"
                SELECT 1 
                    FROM UserInfo.UserPreferences 
                        WHERE BoardIDX = @value2 AND UserID = @value1";

            string updateQuery = @"
                UPDATE UserInfo.UserPreferences
                    SET LastVisitedDate  = @CurrentTime
                        WHERE BoardIDX = @value2 AND UserID = @value1";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@value1", userId);
                    command.Parameters.AddWithValue("@value2", boardIdx);

                    // SELECT 값 확인
                    var result = await command.ExecuteScalarAsync();
                    exists = result != null; // 데이터가 있으면 true
                }

                if (exists)
                {
                    using (var updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@value1", userId);
                        updateCommand.Parameters.AddWithValue("@value2", boardIdx);
                        updateCommand.Parameters.AddWithValue("@CurrentTime", DateTime.Now);

                        await updateCommand.ExecuteNonQueryAsync();
                    }
                }
            }

            return exists; // 데이터 존재 여부 반환
        }

        public async Task<bool> SetBoardDetailRepository(string userId, int boardIDX)
        {
            using (var connection = new SqlConnection(_connectionString))
            {

                string query = @"
                            INSERT INTO UserInfo.UserPreferences 
                                (UserID, BoardIDX) VALUES (@value1, @value2);

                            IF EXISTS 
                                ( 
                                SELECT 1 FROM UserInfo.UserPreferences
                                WHERE BoardIDX = @value2 GROUP BY BoardIDX
                                HAVING COUNT(*) >= 20
                                )
                            BEGIN
                                UPDATE Board.Board SET BoardPrivate = '3'
                                WHERE IDX = @value2;
                            END;
                            ";

                /*
                // SQL 쿼리 정의
                string query = @"
                            DECLARE @ViewCountCheck INT = 1;

                                IF NOT EXISTS (SELECT 1 FROM UserInfo.UserPreferences WHERE BoardIDX = @value2 AND UserID = @value1)
                                BEGIN
                                    SET @ViewCountCheck = 0;
                                    INSERT INTO UserInfo.UserPreferences (UserID, BoardIDX)
                                    VALUES (@value1, @value2);
                                END;

                                IF EXISTS ( SELECT 1 FROM UserInfo.UserPreferences
                                    WHERE BoardIDX = @value2 GROUP BY BoardIDX
                                        HAVING COUNT(*) >= 20
                                )
                                BEGIN
                                    UPDATE Board.Board SET BoardPrivate = '3'
                                    WHERE IDX = @value2;
                                END;

                            SELECT @ViewCountCheck AS ViewCountCheck;

                            ";
                */
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@value1", userId);
                    command.Parameters.AddWithValue("@value2", boardIDX);

                    try
                    {
                        await connection.OpenAsync();

                        // 트랜잭션 시작
                        using (var transaction = await connection.BeginTransactionAsync())
                        {
                            command.Transaction = (SqlTransaction)transaction;

                            // 쿼리 실행
                            var rowsAffected = await command.ExecuteNonQueryAsync();
                            Console.WriteLine($"Rows affected: {rowsAffected}");

                            // 트랜잭션 커밋
                            await transaction.CommitAsync();
                            return rowsAffected > 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Transaction Error: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        public async Task<UserPreferencesModel> GetUserPreferences(string userId, int boardIDX, int type)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = null;
                if (type == 0)
                {
                    query = @"
                        SELECT IDX, LikeStatus, FavoriteStatus
                            FROM UserInfo.UserPreferences 
                                WHERE UserID = @value1 AND BoardIDX = @value2
                        ";
                }
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@value1", userId);
                    command.Parameters.AddWithValue("@value2", boardIDX);

                    try
                    {
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                if (type == 0)
                                {
                                    var UserPreferences = new UserPreferencesModel
                                    {
                                        IDX = reader.GetInt32(0),
                                        // NULL 처리 및 BIT → int 변환
                                        LikeStatus = reader.IsDBNull(1) ? (int?)null : (reader.GetBoolean(1) ? 1 : 0),
                                        FavoriteStatus = reader.GetBoolean(2) ? 1 : 0 // BIT → int 변환
                                    };
                                    return UserPreferences;
                                }
                            }
                        }
                        return null;

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Transaction Error asdf: {ex.Message}");
                        return null;
                    }
                }
            }


        }





    }
}
