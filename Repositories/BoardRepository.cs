using BoardCreate.DataContext;
using Microsoft.EntityFrameworkCore;
using BoardCreate.Models;
using System.Data;
using BoardCreate.Controllers;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Diagnostics;
using BoardCreate.Models.Board;
using System.Transactions;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace BoardCreate.Repositories
{
    public class BoardRepository
    {
        private readonly ILogger<BoardRepository> _logger;
        private readonly BoardDbContext _boardDbContext;
        private readonly SectionTabsDbContext _sectionTabsDbContext;
        private readonly IHubContext<NotificationHub> _hubContext; // ✅ 추가됨
        private readonly string _connectionString;

        public BoardRepository(BoardDbContext boardDbContext,
                               SectionTabsDbContext sectionTabsDbContext,
                               IHubContext<NotificationHub> hubContext, // ✅ 추가됨
                               ILogger<BoardRepository> logger,
                               string connectionString)
        {
            _boardDbContext = boardDbContext;
            _sectionTabsDbContext = sectionTabsDbContext;
            _hubContext = hubContext; // ✅ 추가됨
            _logger = logger;
            _connectionString = connectionString;
        }
        /*
        public async Task<List<BoardSectionsModel>> GetSectionListRepository()
        {
            var sectionList = new List<BoardSectionsModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT IDX, SectionName, SectionStatus
                        FROM  Board.BoardSections 
                            WHERE SectionStatus = 0
                                ORDER BY SectionOrder ASC";
                var command = new SqlCommand(query, connection);
                try
                {
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            sectionList.Add(new BoardSectionsModel
                            {
                                IDX = reader.GetInt32(0),
                                SectionName = reader.GetString(1),
                                SectionStatus = reader.GetInt32(2)
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 예외 처리
                    throw new Exception("Error GetSectionListRepository : ", ex);
                }
            }
            return sectionList;
        }*/

        public async Task<List<BoardSectionsModel>> GetSectionListRepository()
        {
            var sectionList = new List<BoardSectionsModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
                WITH RankedBoards AS (
                    SELECT b.SectionIDX, b.IDX AS BoardIDX, b.Title, b.BoardCreatedAt, COUNT(c.IDX) AS CommentCount,
                        ROW_NUMBER() OVER (PARTITION BY b.SectionIDX ORDER BY b.BoardCreatedAt DESC) AS RowNum
                    FROM Board.Board b
	                LEFT JOIN Board.Comments c ON c.BoardIDX = b.IDX 
	                GROUP BY b.SectionIDX, b.IDX, b.Title, b.BoardCreatedAt
                )
                SELECT s.IDX , s.SectionName, b.BoardIDX, b.Title, b.BoardCreatedAt, b.CommentCount
                FROM Board.BoardSections s
                    LEFT JOIN RankedBoards b ON s.IDX = b.SectionIDX AND b.RowNum <= 8
                        WHERE s.SectionStatus = 0
                        ORDER BY s.SectionOrder ASC, b.BoardCreatedAt DESC;
                    ";
                var command = new SqlCommand(query, connection);
                try
                {
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            sectionList.Add(new BoardSectionsModel
                            {
                                IDX = reader.IsDBNull(reader.GetOrdinal("IDX")) ? 0 : reader.GetInt32(reader.GetOrdinal("IDX")),
                                SectionName = reader.IsDBNull(reader.GetOrdinal("SectionName")) ? string.Empty : reader.GetString(reader.GetOrdinal("SectionName")),
                                BoardIDX = reader.IsDBNull(reader.GetOrdinal("BoardIDX")) ? 0 : reader.GetInt32(reader.GetOrdinal("BoardIDX")),
                                Title = reader.IsDBNull(reader.GetOrdinal("Title")) ? string.Empty : reader.GetString(reader.GetOrdinal("Title")),
                                BoardCreatedAt = reader.IsDBNull(reader.GetOrdinal("BoardCreatedAt"))? DateTime.MinValue
                                    : reader.GetDateTime(reader.GetOrdinal("BoardCreatedAt")),
                                CommentCount = reader.IsDBNull(reader.GetOrdinal("CommentCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("CommentCount"))

                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error GetSectionListRepository : ", ex);
                }
            }
            return sectionList;
        }
        public async Task<List<SectionTabsModel>> GetSectionTabsLIstRepository(int BoardSectionIDX, int UserLevel)
        {
            var TabsList = new List<SectionTabsModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "";
                if (UserLevel < 3)
                {
                    query = @"
                        SELECT IDX, TabName, SectionIDX, TabStatus
                            FROM Board.SectionTabs 
                                WHERE SectionIDX = @BoardSectionIDX"; // 파라미터화
                }
                else
                {
                    query = @"
                        SELECT IDX, TabName, SectionIDX, TabStatus
                            FROM Board.SectionTabs 
                                WHERE SectionIDX = @BoardSectionIDX AND TabStatus = 0";
                }

                using (var command = new SqlCommand(query, connection))
                {

                    command.Parameters.AddWithValue("@BoardSectionIDX", BoardSectionIDX);

                    try
                    {
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                TabsList.Add(new SectionTabsModel
                                {
                                    IDX = reader.GetInt32(0),
                                    TabName = reader.GetString(1),
                                    SectionIDX = reader.GetInt32(2),
                                    TabStatus = reader.GetInt32(3)
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("GetSectionTabsListAllRepository ERROR : ", ex);
                    }
                }
            }
            return TabsList;
        }

        public async Task<BoardSectionsModel> GetSectionRepository(int SectionIDX)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT IDX, SectionName, SectionStatus, SectionOrder, SectionStartDate, SectionEndDate
                        FROM Board.BoardSections 
                            WHERE IDX = @sectionIDX";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@sectionIDX", SectionIDX);

                try
                {
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // 데이터 읽어서 객체에 담기
                            var boardSectionsModel = new BoardSectionsModel
                            {
                                /* 컬럼 순서 상관없음 안전함 대신 느림
                                IDX = reader.GetInt32(reader.GetOrdinal("IDX")),
                                SectionName = reader.GetString(reader.GetOrdinal("SectionName")),
                                SectionStatus = reader.GetInt32(reader.GetOrdinal("SectionStatus")),
                                SectionOrder = reader.GetInt32(reader.GetOrdinal("SectionOrder")),
                                SectionStartDate = reader.GetDateTime(reader.GetOrdinal("SectionStartDate")),
                                SectionEndDate = reader.GetDateTime(reader.GetOrdinal("SectionEndDate"))
                                */
                                IDX = reader.GetInt32(0),
                                SectionName = reader.GetString(1),
                                SectionStatus = reader.GetInt32(2),
                                SectionOrder = reader.GetInt32(3),
                                SectionStartDate = reader.GetDateTime(4),
                                SectionEndDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5) // NULL 확인
                            };

                            return boardSectionsModel;
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    throw new Exception("GetSectionRepository : ", ex);
                }
            }
        }

        public async Task<bool> SetBoardRepository(BoardModel boardModel)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO Board.Board
                    (SectionIDX, Tab, UserID, NickName, Title, Contents, BoardPrivate)
                    VALUES 
                    (@value1, @value2, @value3, @value4, @value5, @value6, @value7)";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@value1", boardModel.SectionIDX);
                command.Parameters.AddWithValue("@value2", boardModel.Tab);
                command.Parameters.AddWithValue("@value3", boardModel.UserID);
                command.Parameters.AddWithValue("@value4", boardModel.NickName);
                command.Parameters.AddWithValue("@value5", boardModel.Title);
                command.Parameters.AddWithValue("@value6", boardModel.Contents);
                command.Parameters.AddWithValue("@value7", boardModel.BoardPrivate);
                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    // 예외 처리
                    throw new Exception("Error SetBoardRepository : ", ex);
                }
            }
        }

        public async Task<int> GetBoardListCountRepository(int SectionIDX, int UserLevel, string BoardType)
        {
            await using var connection = new SqlConnection(_connectionString); // C# 8.0 이후 await using 사용 가능

            string query = @"
                SELECT COUNT(*) FROM Board.Board WHERE SectionIDX = @value1 AND Tab = @value2;
            ";

            await connection.OpenAsync(); 

            await using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@value1", SectionIDX);
            command.Parameters.AddWithValue("@value2", BoardType);

            try
            {
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetBoardListCountRepository: {ex.Message}");
                throw; // 원본 예외 그대로 던지기 (스택 트레이스 유지)
            }

        }
       

        public async Task<List<BoardModel>> GetBoardListRepository(int UserLevel, int boardListCount, BoardSundryModel boardSundryModel)
        {
            var BoardList = new List<BoardModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                int plusValue = (boardSundryModel.CurrentPage - 1) * boardSundryModel.PageSize;
                int start = 0 + plusValue;
                string query = "";

                bool hasUserID = !string.IsNullOrEmpty(boardSundryModel.UserID);

                string userExistsQuery = hasUserID ? @"
                    ,(SELECT COALESCE(SUM(CASE WHEN u.UserID = @value7 THEN 1 ELSE 0 END), 0)
                        FROM UserInfo.UserPreferences u 
                        WHERE u.BoardIDX = b.IDX) AS UserExists" : ", 0 AS UserExists";

                if (UserLevel < 3)
                {
                    query = $@"
                        SELECT b.IDX, b.SectionIDX, b.Tab, b.UserID, b.NickName, b.Title, b.Contents, b.BoardPrivate, b.ViewCount, b.BoardCreatedAt,
                            (SELECT COUNT(*) FROM UserInfo.UserPreferences u
                                WHERE u.BoardIDX = b.IDX AND u.LikeStatus = 0) AS BoardLikeCount
                            {userExistsQuery}
                                FROM Board.Board b
                                    WHERE SectionIDX = @value1 AND Tab = @value2
                                    ORDER BY IDX DESC
                                    OFFSET @value3 ROWS FETCH NEXT @value4 ROWS ONLY;";
                }
                /*
                  --,(@value5 - ((@value4 * (@value6 - 1)) + ROW_NUMBER() OVER (ORDER BY IDX DESC) - 1)) AS AdjustedIDX
                  -- 이부분 충돌이 있는지 오류가 계속 나서 다른 방식으로 함
               */
                else
                {
                    query = $@"
                        SELECT b.IDX, b.SectionIDX, b.Tab, b.UserID, b.NickName, b.Title, b.Contents, b.BoardPrivate, b.ViewCount, b.BoardCreatedAt,
                            (SELECT COUNT(*) FROM UserInfo.UserPreferences u
                                WHERE u.BoardIDX = b.IDX AND u.LikeStatus = 0) AS BoardLikeCount
                            {userExistsQuery}
                                FROM Board.Board b
                                    WHERE SectionIDX = @value1 AND BoardStatus = 0 AND Tab = @value2 
                                    ORDER BY IDX DESC
                                    OFFSET @value3 ROWS FETCH NEXT @value4 ROWS ONLY;";
                }

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@value1", boardSundryModel.SectionIDX);
                    command.Parameters.AddWithValue("@value2", boardSundryModel.SelectedTab);
                    command.Parameters.AddWithValue("@value3", start);
                    command.Parameters.AddWithValue("@value4", boardSundryModel.PageSize);
                    command.Parameters.AddWithValue("@value5", boardListCount);
                    command.Parameters.AddWithValue("@value6", boardSundryModel.CurrentPage);

                    if (hasUserID)
                    {
                        command.Parameters.AddWithValue("@value7", boardSundryModel.UserID);
                    }

                    await connection.OpenAsync();
                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int totalCount = boardListCount;
                            int currentOffset = (boardSundryModel.CurrentPage - 1) * boardSundryModel.PageSize;

                            while (await reader.ReadAsync())
                            {
                                int adjustedIDX = totalCount - currentOffset - BoardList.Count;

                                var boardModel = new BoardModel
                                {
                                    IDX = reader.GetInt32(0),
                                    SectionIDX = reader.GetInt32(1),
                                    Tab = reader.GetString(2),
                                    UserID = reader.GetString(3),
                                    NickName = reader.GetString(4),
                                    Title = reader.GetString(5),
                                    Contents = reader.GetString(6),
                                    BoardPrivate = reader.GetInt32(7),
                                    ViewCount = reader.GetInt32(8),
                                    BoardCreatedAt = reader.GetDateTime(9),
                                    BoardLikeCount = reader.GetInt32(10),
                                    AdjustedIDX = adjustedIDX
                                };
                                if (hasUserID)
                                {
                                    boardModel.UserExists = reader.GetInt32(11);
                                }

                                BoardList.Add(boardModel);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] GetBoardListRepository: {ex.Message}");
                        throw;
                    }
                }
            }
            return BoardList;
        }



        public async Task<BoardDetailModel> GetBoardDetailRepository(int BoardIDx, int ViewCountCheck)
        {
            var Board = new BoardModel();
            var CommentsList = new List<CommentsModel>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    string query = @"
                UPDATE Board.Board
                    SET ViewCount = ViewCount + CASE WHEN @ViewCountCheck = 0 THEN 1 ELSE 0 END
                    WHERE IDX = @value1;
            ";
                    /*
                    query += @"
                SELECT b.IDX, b.SectionIDX, b.Tab, b.UserID, b.NickName, b.Title, b.Contents, b.BoardPrivate, b.ViewCount, b.BoardCreatedAt,
                        m.IDX As UserIDX
                    FROM Board.Board b
                        LEFT JOIN UserInfo.Member m
                            ON b.UserID = m.UserID
                    WHERE b.IDX = @value1 AND b.BoardStatus = 0;
            ";
                    */
                    query += @"
                SELECT b.IDX, b.SectionIDX, b.Tab, b.UserID, b.NickName, b.Title, b.Contents, b.BoardPrivate, b.ViewCount, b.BoardCreatedAt, s.SectionName
                    FROM Board.Board b 
                        LEFT JOIN Board.BoardSections s
                        ON b.SectionIDX = s.IDX 
                            WHERE b.IDX = @value1 AND b.BoardStatus = 0;
            ";
                    query += @"
                IF EXISTS(SELECT 1 FROM Board.Comments WHERE BoardIDX = @value1)
                    SELECT c.IDX, c.BoardIDX, c.WriterIDX,
                        mWriter.UserID AS WriterID,
                        c.CommentsContent, c.CommentsCreatedAt, c.CommentsUpdatedAt, c.CommentsStatus
                    FROM Board.Comments c
                        LEFT JOIN UserInfo.Member mWriter
                            ON c.WriterIDX = mWriter.IDX
                    WHERE BoardIDX = @value1 AND TargetedCommentsIDX = 0;
                    --OFFSET 0 ROWS FETCH NEXT 4 ROWS ONLY;
            ";

                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction()) // 트랜잭션 시작
                    {
                        try
                        {
                            using (var command = new SqlCommand(query, connection, transaction)) // 트랜잭션 설정
                            {
                                command.Parameters.AddWithValue("@ViewCountCheck", ViewCountCheck);
                                command.Parameters.AddWithValue("@value1", BoardIDx);

                                using (var reader = await command.ExecuteReaderAsync())
                                {
                                    // 첫 번째 결과 집합: Board 데이터
                                    if (await reader.ReadAsync())
                                    {
                                        Board = new BoardModel
                                        {
                                            IDX = reader.GetInt32(0),
                                            SectionIDX = reader.GetInt32(1),
                                            Tab = reader.GetString(2),
                                            UserID = reader.GetString(3),
                                            NickName = reader.GetString(4),
                                            Title = reader.GetString(5),
                                            Contents = reader.GetString(6),
                                            BoardPrivate = reader.GetInt32(7),
                                            ViewCount = reader.GetInt32(8),
                                            BoardCreatedAt = reader.GetDateTime(9),
                                            SectionName = reader.GetString(10)
                                        };
                                    }

                                    // 두 번째 결과 집합: Comments 데이터
                                    if (await reader.NextResultAsync() && reader.HasRows)
                                    {
                                        while (await reader.ReadAsync())
                                        {
                                            CommentsList.Add(new CommentsModel
                                            {
                                                IDX = reader.GetInt32(0),
                                                BoardIDX = reader.GetInt32(1),
                                                WriterIDX = reader.GetInt32(2),
                                                WriterID = reader.GetString(3),
                                                CommentsContent = reader.GetString(4),
                                                CommentsCreatedAt = reader.GetDateTime(5),
                                                CommentsUpdatedAt = reader.GetDateTime(6),
                                                CommentsStatus = reader.GetBoolean(7) ? 1 : 0
                                            });
                                        }
                                    }
                                }
                            }

                            await transaction.CommitAsync(); // 트랜잭션 커밋
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"트랜잭션 중 오류 발생: {ex.Message}");
                            await transaction.RollbackAsync();
                            return null;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL 오류: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"오류 발생: {ex.Message}");
                throw;
            }

            return new BoardDetailModel
            {
                Board = Board,
                CommentsList = CommentsList
            };
        }


        public async Task<bool> UpdateUserPreferencesLikeStatusRepository(int BoardIDX, int UpdateType, string UserID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // UPDATE 쿼리 작성
                string query = @"
                    IF NOT EXISTS (SELECT 1 FROM UserInfo.UserPreferences WHERE UserID = @value2 AND BoardIDX = @value3)
                        BEGIN
                            RETURN;
                        END

                        UPDATE UserInfo.UserPreferences
                            SET LikeStatus = @value1
                            WHERE UserID = @value2 AND BoardIDX = @value3";
                var command = new SqlCommand(query, connection);

                // 파라미터 추가
                command.Parameters.AddWithValue("@value1", UpdateType);
                command.Parameters.AddWithValue("@value2", UserID);
                command.Parameters.AddWithValue("@value3", BoardIDX);

                try
                {
                    // 연결 열기
                    await connection.OpenAsync();

                    // 쿼리 실행
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    // 업데이트 성공 여부 반환
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error UpdateUserPreferencesLikeStatusRepository : {ex.Message}");
                }
            }
        }
        public async Task<int> CommentsInsertRepository(CommentsModel commentsModel, int CommentsType)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string targetedColumns = CommentsType == 1 ? "TargetedUserIDX, " : "";
                string targetedValues = CommentsType == 1 ? "@value4, " : "";

                string query = $@"
                    INSERT INTO Board.Comments
                        (BoardIDX, WriterIDX, TargetedCommentsIDX, {targetedColumns} CommentsContent)
                        VALUES (@value1, @value2, @value3, {targetedValues} @value5);
                    SELECT SCOPE_IDENTITY();
                ";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@value1", commentsModel.BoardIDX);
                    command.Parameters.AddWithValue("@value2", commentsModel.WriterIDX);
                    command.Parameters.AddWithValue("@value3", commentsModel.TargetedCommentsIDX);

                    if (CommentsType == 1)
                    {
                        command.Parameters.AddWithValue("@value4", commentsModel.TargetedUserIDX);
                    }
                    command.Parameters.AddWithValue("@value5", commentsModel.CommentsContent);

                    try
                    {
                        await connection.OpenAsync();
                        object result = await command.ExecuteScalarAsync();
                        if (result == null)
                        {
                            throw new Exception("Insert failed: No IDENTITY value was returned for CommentsInsertRepository.");
                        }
                        int commentId = Convert.ToInt32(result); // 삽입된 댓글 ID 반환

                        await SendCommentNotification(commentsModel, CommentsType);

                        return commentId;

                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error in CommentsInsertRepository: {ex.Message}");
                    }
                }
            }
        }

        private async Task SendCommentNotification(CommentsModel commentsModel, int CommentsType)
        {
            string recipientId = CommentsType == 1 ? commentsModel.TargetedUserIDX.ToString() : commentsModel.WriterIDX.ToString();
            string message = $"💬 새로운 댓글이 등록되었습니다: {commentsModel.CommentsContent}";

            if (NotificationHub.IsUserOnline(recipientId)) // ✅ 해당 사용자가 접속 중인지 확인
            {
                await _hubContext.Clients.User(recipientId).SendAsync("ReceiveNotification", new
                {
                    message = message,
                    url = $"/Board/Detail/{commentsModel.BoardIDX}" // ✅ 알림 클릭 시 이동할 URL
                });
            }
        }





        public async Task<List<CommentsModel>> CommentsReplyDynamicSelectRepository(int BoardIDX,int TargetedCommentsIDX, int CurrentComments,int CurrentCommentsPlus)
        {
            // TargetedCommentsIDX == 0이면 댓글 아니면 답글
            List<CommentsModel> commentsListModel = new List<CommentsModel>();
            using (var connection = new SqlConnection(_connectionString))
            {

                string ReplyCount = TargetedCommentsIDX == 0 ? ", (SELECT COUNT(*) FROM Board.Comments AS sub WHERE sub.TargetedCommentsIDX = c.IDX) AS ReplyCount" : "";
                string TargetedUserIDX = TargetedCommentsIDX == 0 ? "" : ",c.TargetedUserIDX";

                string query = $@"
                        SELECT 
                            c.IDX, c.BoardIDX, c.WriterIDX, mWriter.UserID AS WriterID, c.TargetedCommentsIDX, c.CommentsContent, c.CommentsUpdatedAt, ISNULL(CAST(cl.LikeStatus AS INT), 2) AS LikeStatus,
                            ISNULL(SUM(CASE WHEN cl.LikeStatus = 0 THEN 1 ELSE 0 END), 0) AS LikeCount,
                            ISNULL(SUM(CASE WHEN cl.LikeStatus = 1 THEN 1 ELSE 0 END), 0) AS BadCount
                            {TargetedUserIDX}
						    {ReplyCount}

                        FROM Board.Comments c
                            LEFT JOIN Board.CommentLikes cl
                                ON c.IDX = cl.CommentsIdx
                            LEFT JOIN UserInfo.Member mWriter
                                ON c.WriterIDX = mWriter.IDX
                        WHERE c.BoardIDX = @value1  AND c.CommentsStatus = 0 AND c.TargetedCommentsIDX = @value4  AND (mWriter.UserDelete = 0 OR mWriter.UserDelete IS NULL)
                        GROUP BY 
                            c.IDX, c.BoardIDX, c.WriterIDX, mWriter.UserID, c.TargetedCommentsIDX , c.CommentsContent, c.CommentsUpdatedAt, cl.LikeStatus {TargetedUserIDX}
                        ORDER BY c.IDX ASC
                        OFFSET @value2 ROWS FETCH NEXT @value3 ROWS ONLY;
                    ";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@value1", BoardIDX);
                    command.Parameters.AddWithValue("@value2", CurrentComments); // 시작 위치
                    command.Parameters.AddWithValue("@value3", CurrentCommentsPlus); // 가져올 개수
                    command.Parameters.AddWithValue("@value4", TargetedCommentsIDX); // 가져올 개수

                    try
                    {

                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (!reader.HasRows) 
                            {
                                return new List<CommentsModel>();
                            }

                            while (await reader.ReadAsync()) 
                            {
                                if (TargetedCommentsIDX == 0)
                                {
                                    commentsListModel.Add(new CommentsModel
                                    {
                                        IDX = reader.GetInt32(0),
                                        BoardIDX = reader.GetInt32(1),
                                        WriterIDX = reader.GetInt32(2),
                                        WriterID = reader.GetString(3),
                                        TargetedCommentsIDX = reader.GetInt32(4),
                                        CommentsContent = reader.GetString(5),
                                        CommentsUpdatedAt = reader.GetDateTime(6),
                                        LikeStatus = reader.GetInt32(7),
                                        LikeCount = reader.GetInt32(8),
                                        BadCount = reader.GetInt32(9),
                                        ReplyCount = reader.GetInt32(10)
                                    });
                                }
                                else
                                {
                                    commentsListModel.Add(new CommentsModel
                                    {
                                        IDX = reader.GetInt32(0),
                                        BoardIDX = reader.GetInt32(1),
                                        WriterIDX = reader.GetInt32(2),
                                        WriterID = reader.GetString(3),
                                        TargetedCommentsIDX = reader.GetInt32(4),
                                        CommentsContent = reader.GetString(5),
                                        CommentsUpdatedAt = reader.GetDateTime(6),
                                        LikeStatus = reader.GetInt32(7),
                                        LikeCount = reader.GetInt32(8),
                                        BadCount = reader.GetInt32(9),
                                        TargetedUserIDX = reader.GetInt32(10)
                                    });
                                }
                                
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine($"SQL 오류 발생: {ex.Message}");
                        throw new Exception($"Error CommentsDynamicSelectRepository sql Error : {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error CommentsDynamicSelectRepository : {ex.Message}");
                    }
                }
            }

            return commentsListModel;
        }


        public async Task<CommentsLikesModel> UpdateCommentsLikesRepository(int CommentsIdx, string UserID, int Likestype, int duplicateLikeStstusCheck)
        {
            CommentsLikesModel commentsLikesModel = new CommentsLikesModel();
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    BEGIN TRANSACTION;
                    BEGIN TRY
                    DECLARE @ProgressStatus INT = 0;
                    ";

                if (duplicateLikeStstusCheck == 2)
                {
                    query += @"
                        INSERT INTO Board.CommentLikes (CommentsIDX, UserID, LikeStatus) 
                                VALUES (@value1, @value2, @value3);
                    ";
                }
                else if (duplicateLikeStstusCheck == Likestype)
                {
                    query += @"
                        DELETE FROM Board.CommentLikes 
                            WHERE CommentsIDX = @value1 AND UserID = @value2;
                    ";
                }
                else if(duplicateLikeStstusCheck != Likestype)
                {
                    query += @"
                        UPDATE Board.CommentLikes 
                            SET LikeStatus = @value3 
                            WHERE CommentsIDX = @value1 AND UserID = @value2;
                    ";
                }
                query += @"
                    COMMIT TRANSACTION;
                        END TRY

                        BEGIN CATCH
                            ROLLBACK TRANSACTION;
                            SET @ProgressStatus = 1;
                        END CATCH;

                    IF NOT EXISTS (SELECT 1 FROM Board.CommentLikes WHERE CommentsIDX = @value1)
                        BEGIN
                            RETURN;
                        END;

                    SELECT 
                        @ProgressStatus AS ProgressStatus, 
                            SUM(CASE WHEN LikeStatus = 0 THEN 1 ELSE 0 END) AS LikeCount,
                            SUM(CASE WHEN LikeStatus = 1 THEN 1 ELSE 0 END) AS BadCount 
                    FROM Board.CommentLikes 
                    WHERE CommentsIDX = @value1 
                    GROUP BY CommentsIDX;
                    ";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@value1", CommentsIdx);
                    command.Parameters.AddWithValue("@value2", UserID);
                    command.Parameters.AddWithValue("@value3", Likestype);

                    try
                    {
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (!reader.HasRows)
                            {
                                return commentsLikesModel;
                            }
                            if (await reader.ReadAsync())
                            {
                                commentsLikesModel = new CommentsLikesModel
                                {
                                    ProgressStatus = reader.GetInt32(0),
                                    LikeCount = !reader.IsDBNull(1) ? reader.GetInt32(1) : 0,
                                    BadCount = !reader.IsDBNull(2) ? reader.GetInt32(2) : 0
                                };
                            }

                        }
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine($"UpdateCommentsLikesRepository SQL 오류 발생: {ex.Message}");
                        throw new Exception($"Error UpdateCommentsLikesRepository sql Error : {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"오류 발생: {ex.Message}");
                        throw new Exception("알 수 없는 오류가 발생했습니다.", ex);
                    }
                }
            }
            return commentsLikesModel;
        }

        public async Task<int> GetLikeStatusRepository(int CommentsIdx, string UserID)
        {
            int result = 2; // 기본값
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        CASE 
                            WHEN NOT EXISTS (SELECT 1 FROM Board.CommentLikes WHERE CommentsIDX = @value1 AND UserID = @value2) THEN 2
                            ELSE ISNULL(
                                (SELECT LikeStatus 
                                 FROM Board.CommentLikes 
                                 WHERE CommentsIDX = @value1 AND UserID = @value2), 2
                            )
                        END AS Result
                ";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@value1", CommentsIdx);
                command.Parameters.AddWithValue("@value2", UserID);

                try
                {
                    await connection.OpenAsync();
                    var dbResult = await command.ExecuteScalarAsync();
                    result = dbResult != null ? Convert.ToInt32(dbResult) : 2;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" GetLikeStatusRepository SQL 오류 발생: {ex.Message}");
                    throw new Exception($"Error GetLikeStatusRepository sql Error : {ex.Message}");
                }
            }
            return result;
        }

        public async Task<List<BoardModel>> GetBoardRecentListMyRepository(List<string> userRecent)
        {
            var BoardList = new List<BoardModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Board.Board WHERE ";
                List<string> conditions = new List<string>();
                List<SqlParameter> parameters = new List<SqlParameter>();

                for (int i = 0; i < userRecent.Count; i++)
                {
                    string[] parts = userRecent[i].Split('_');

                    int SectionIDX = int.Parse(parts[0]);
                    int BoardIDX = int.Parse(parts[1]);

                    string sectionParam = $"@Section{i}";
                    string boardParam = $"@Board{i}";

                    conditions.Add($"(SectionIDX = {sectionParam} AND IDX = {boardParam} AND BoardStatus = 0)");

                    parameters.Add(new SqlParameter(sectionParam, SectionIDX));
                    parameters.Add(new SqlParameter(boardParam, BoardIDX));
                }
                query += string.Join(" OR ", conditions);
                await connection.OpenAsync();
                Console.WriteLine(query);
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var boardModel = new BoardModel
                                {
                                    IDX = reader.GetInt32(reader.GetOrdinal("IDX")),
                                    SectionIDX = reader.GetInt32(reader.GetOrdinal("SectionIDX")),
                                    UserID = reader.GetString(reader.GetOrdinal("UserID")),
                                    Title = reader.GetString(reader.GetOrdinal("Title")),
                                    BoardPrivate = reader.GetInt32(reader.GetOrdinal("BoardPrivate")),
                                    ViewCount = reader.GetInt32(reader.GetOrdinal("ViewCount"))
                                };
                                BoardList.Add(boardModel);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] GetBoardRecentListRepository: {ex.Message}");
                        throw;
                    }
                }
            }

            return BoardList;
        }

        public async Task<List<BoardModel>> GetBoardRecentListAllMyRepository()
        {
            var BoardList = new List<BoardModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                    SELECT * FROM Board.Board WHERE BoardStatus = 0 
                        ORDER BY IDX DESC 
                            OFFSET 0 ROWS FETCH NEXT 9 ROWS ONLY;
                ";

                using (var command = new SqlCommand(query, connection))
                {
                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var boardModel = new BoardModel
                                {
                                    IDX = reader.GetInt32(reader.GetOrdinal("IDX")),
                                    SectionIDX = reader.GetInt32(reader.GetOrdinal("SectionIDX")),
                                    UserID = reader.GetString(reader.GetOrdinal("UserID")),
                                    Title = reader.GetString(reader.GetOrdinal("Title")),
                                    BoardPrivate = reader.GetInt32(reader.GetOrdinal("BoardPrivate")),
                                    ViewCount = reader.GetInt32(reader.GetOrdinal("ViewCount"))
                                };
                                BoardList.Add(boardModel);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] GetBoardRecentListRepository: {ex.Message}");
                        throw; 
                    }
                }
            }
            return BoardList;
        }






    }



}
/*
 
UpdateCommentsLikesRepository 의 최적화 전 버전 
IF EXISTS (SELECT 1 FROM Board.CommentLikes WHERE CommentsIdx = @CommentsIdx AND UserID = @UserID)
    BEGIN
        IF EXISTS (SELECT 1 FROM Board.CommentLikes WHERE CommentsIdx = @CommentsIdx AND UserID = @UserID AND LikeStatus = @Likestype)
            BEGIN
                DELETE FROM Board.CommentLikes WHERE CommentsIdx = @CommentsIdx AND UserID = @UserID AND LikeStatus = @LikeStatus
            END
        ELSE
            BEGIN
                UPDATE Board.CommentLikes
                SET LikeStatus = @Likestype
                WHERE CommentsIdx = @CommentsIdx AND UserID = @UserID;
            END
    END
ELSE
    BEGIN
        INSERT INTO Board.CommentLikes (CommentsIdx, UserID, LikeStatus)
        VALUES (@CommentsIdx, @UserID, @Likestype);
    END
*/