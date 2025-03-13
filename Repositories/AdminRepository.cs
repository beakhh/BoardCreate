using BoardCreate.DataContext;
using Microsoft.EntityFrameworkCore;
using BoardCreate.Models;
using System.Data;
using BoardCreate.Controllers;
using Microsoft.Data.SqlClient;
using BoardCreate.Models.Board;

namespace BoardCreate.Repositories
{
    public class AdminRepository 
    {
        private readonly ILogger<AdminRepository> _logger;
        private readonly BoardSectionsDbContext _boardSectionDbContext;
        private readonly SectionTabsDbContext _sectionTabsDbContext;
        private readonly string _connectionString;

        public AdminRepository(BoardSectionsDbContext boardSectionDbContext, SectionTabsDbContext sectionTabsDbContext, ILogger<AdminRepository> logger,  string connectionString)
        {
            _boardSectionDbContext = boardSectionDbContext;
            _connectionString = connectionString;
            _sectionTabsDbContext = sectionTabsDbContext;
            _logger = logger;
        }

        public async Task<bool> SectionDuplicateCheck(string SectionName) 
        {
            var duplicateCheck = await _boardSectionDbContext.BoardSections
                .AnyAsync(b => b.SectionName == SectionName);
            /*
            var duplicateCheck = await _boardSectionDbContext.BoardSections
                .FromSqlRaw("SELECT * FROM Board.BoardSections WHERE SectionsName = {0}", model.SectionName)
                .AnyAsync();
            */
            return duplicateCheck;
        }
        
        public async Task<int> GetSectionOrderMax()
        {
            int maxSectionOrder = 0;
            if (await _boardSectionDbContext.BoardSections.AnyAsync())
            {
                maxSectionOrder = await _boardSectionDbContext.BoardSections
                    .MaxAsync(t => t.SectionOrder);
            }

            return maxSectionOrder;
        }
        public async Task<int> GetSectionIDXMax()
        {
            int maxSectionIDX = 0;
            if (await _boardSectionDbContext.BoardSections.AnyAsync())
            {
                maxSectionIDX = await _boardSectionDbContext.BoardSections
                    .MaxAsync(t => t.IDX);
            }
            return maxSectionIDX;
        }

        public async Task<bool> SectionCreate(string SectionName, int SectionOrder)
        {
            var result = await _boardSectionDbContext.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Board.BoardSections (SectionName , SectionOrder) VALUES ({0}, {1})",
                    SectionName, SectionOrder
                );
            return result > 0;
        }

        public async Task<bool> SectionTabsCreate(int createSectionIDX) // 기본 숙지요구
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // 삽입 쿼리
                string query = "INSERT INTO Board.SectionTabs (SectionIDX, TabStatus) VALUES (@SectionIDX, @Status)";
                var command = new SqlCommand(query, connection);

                // 파라미터 추가
                command.Parameters.AddWithValue("@SectionIDX", createSectionIDX);
                command.Parameters.AddWithValue("@Status", 0); // Status를 0으로 설정

                try
                {
                    // 데이터베이스 연결 열기
                    await connection.OpenAsync();

                    // 삽입 실행
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    // 삽입 성공 여부 반환
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    // 예외 처리
                    Console.WriteLine($" SectionTabsCreate Error: {ex.Message}");
                    return false;
                }
            }
        }


        public async Task<List<BoardSectionsModel>> GetSectionListAll(int statusTyle)
        {
            var sectionList = new List<BoardSectionsModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "";

                if (statusTyle == 0)
                {
                    query = "SELECT IDX, SectionName, SectionStatus, SectionOrder, SectionStartDate, SectionEndDate " +
                                "FROM Board.BoardSections " +
                                    "WHERE SectionStatus = 0 OR SectionStatus = 1 " +
                                        "ORDER BY SectionOrder ASC";
                }
                else if (statusTyle == 1)
                {
                    query = "SELECT IDX, SectionName, SectionStatus, SectionOrder, SectionStartDate, SectionEndDate " +
                                "FROM Board.BoardSections " +
                                    "WHERE SectionStatus = 2 OR SectionStatus = 3 " +
                                        "ORDER BY SectionOrder ASC";
                }


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
                                SectionStatus = reader.GetInt32(2),
                                SectionOrder = reader.GetInt32(3),
                                SectionStartDate = reader.GetDateTime(4),
                                SectionEndDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5) // NULL 확인
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error fetching sections", ex);
                }
            }

            return sectionList;
        }
        public async Task<List<SectionTabsModel>> GetSectionTabsListAllRepository(int sectionIdx)
        {
            var tabsList = new List<SectionTabsModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                // 기본 쿼리 작성
                string query = @"
                    SELECT IDX, TabName, SectionIDX, TabStatus
                        FROM Board.SectionTabs";

                if (sectionIdx != -1)
                {
                    query += " WHERE SectionIDX = @SectionIDX";
                }

                using (var command = new SqlCommand(query, connection))
                {
                    // 파라미터 추가
                    if (sectionIdx != -1)
                    {
                        command.Parameters.Add(new SqlParameter("@SectionIDX", SqlDbType.Int) { Value = sectionIdx });
                    }

                    try
                    {
                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                // 모델 추가
                                tabsList.Add(new SectionTabsModel
                                {
                                    IDX = reader.GetInt32(reader.GetOrdinal("IDX")), //이러면 순서 안 정해도 알아서 찾아감
                                    TabName = reader.GetString(reader.GetOrdinal("TabName")),
                                    SectionIDX = reader.GetInt32(reader.GetOrdinal("SectionIDX")),
                                    TabStatus = reader.GetInt32(reader.GetOrdinal("TabStatus"))
                                });
                            }
                        } 
                    }
                    catch (Exception ex)
                    {
                        // 상세 예외 메시지 포함
                        throw new Exception("--------Error GetSectionTabsListAllRepository : ", ex);
                    }
                }
            }

            return tabsList;
        }


        public async Task<bool> SetSectionOrderUpDownUpdate(int sectionOrder, int sectionOrderResult) // 기본 숙지 요구
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // UPDATE 쿼리 작성
                string query = @"
                        UPDATE Board.BoardSections
                        SET SectionOrder = 
                            CASE 
                                WHEN SectionOrder = @SectionOrder THEN @SectionOrderResult
                                WHEN SectionOrder = @SectionOrderResult THEN @SectionOrder
                                ELSE SectionOrder
                            END
                        WHERE SectionOrder IN (@SectionOrder, @SectionOrderResult)";
                var command = new SqlCommand(query, connection);

                // 파라미터 추가
                command.Parameters.AddWithValue("@SectionOrder", sectionOrder);
                command.Parameters.AddWithValue("@SectionOrderResult", sectionOrderResult);

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
                    // 예외 처리
                    throw new Exception("Error updating section order", ex);
                }
            }
        }

        public async Task<bool> SectionStatusUpdateRepository(int SliderIDX, int SliderValue,int MaxSectionOrder, int sliderV)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // 기본 쿼리
                string query = @"
                    UPDATE Board.BoardSections
                        SET SectionStatus = @SectionStatus";

                        // sliderV에 따라 동적으로 추가
                        if (sliderV == 1)
                        {
                            query += @", SectionOrder = 0";
                        }
                        else if (sliderV == 2)
                        {
                            query += @", SectionOrder = @MaxSectionOrder";
                        }

                        query += @"
                            WHERE IDX = @IDX";

                var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@SectionStatus", SliderValue);
                command.Parameters.AddWithValue("@IDX", SliderIDX);

                if (sliderV == 2)
                {
                    command.Parameters.AddWithValue("@MaxSectionOrder", MaxSectionOrder);
                }
                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    // 예외 처리
                    throw new Exception("Error updating section order", ex);
                }

            }
        }
        public async Task<bool> SectionNameUpdateRepository(int SectionIdx, string SectionNameValue)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE Board.BoardSections SET SectionName = @SectionName
                    WHERE IDX = @IDX";
                var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@SectionName", SectionNameValue);
                command.Parameters.AddWithValue("@IDX", SectionIdx);
                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    // 예외 처리
                    throw new Exception("Error updating section order", ex);
                }
            }
        }

        public async Task<BoardSectionsModel> GetBoardSectionRepository(int SectionIDX)
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
                    throw new Exception("GetBoardSectionRepository : ", ex);
                }
            }
        }
        public async Task<bool> AdminTabsInsertRepository(int SectionIDX, string TabName)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO Board.SectionTabs (TabName, SectionIDX) 
                        VALUES (@value1, @value2)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@value1", TabName);
                    command.Parameters.AddWithValue("@value2", SectionIDX);

                    try
                    {
                        await connection.OpenAsync();
                        var rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($" AdminTabsInsertRepository Error: {ex.Message}");
                        return false;
                    }
                }
            }
        }
        public async Task<bool> AdminTabsStatusUpdateRepository(int CheckedIDX, int CheckedNumber) 
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE Board.SectionTabs SET TabStatus = @value1
                    WHERE IDX = @value2";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@value1", CheckedNumber);
                    command.Parameters.AddWithValue("@value2", CheckedIDX);
                    try
                    {
                        await connection.OpenAsync();
                        var rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($" AdminTabsStatusUpdateRepository Error: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        /*
        public async Task<List<BoardSectionsModel>> GetSectionListAll()
        {
            return await _boardSectionDbContext.BoardSections
                .Select(b => new BoardSectionsModel
                {
                    IDX = b.IDX,
                    SectionName = b.SectionName,
                    SectionStatus = b.SectionStatus,
                    SectionOrder = b.SectionOrder,
                    SectionStartDate = b.SectionStartDate,
                    SectionEndDate = b.SectionEndDate
                })
                .ToListAsync();
        }*/
    }
}
