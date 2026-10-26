using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class PollRepository : IPollRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PollRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Poll?> GetUnansweredInRoomAsync(int roomId, int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT p.id, p.ref_room, p.name, p.thanks_message, p.type, p.reward_type, p.reward
            FROM polls p
            WHERE p.ref_room = @roomId AND p.enabled = '1'
              AND EXISTS (SELECT 1 FROM polls_questions q WHERE q.ref_poll = p.id)
              AND NOT EXISTS (SELECT 1 FROM users_answered_polls a WHERE a.ref_poll = p.id AND a.ref_user = @userId)
            ORDER BY p.id
            LIMIT 1;
            """, connection);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@userId", userId);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? new Poll(
                reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3),
                reader.GetString(4) == "quiz" ? PollType.Quiz : PollType.Survey,
                reader.GetString(5) switch { "furni" => PollRewardType.Furni, "badge" => PollRewardType.Badge, _ => PollRewardType.None },
                reader.GetString(6))
            : null;
    }

    public async Task<IReadOnlyList<PollQuestion>> GetQuestionsAsync(int pollId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var options = new Dictionary<int, List<PollOption>>();
        await using (var optionCommand = new MySqlCommand(
            """
            SELECT o.ref_question, o.number, o.content, o.is_correct
            FROM polls_question_options o
            INNER JOIN polls_questions q ON q.id = o.ref_question
            WHERE q.ref_poll = @pollId
            ORDER BY o.ref_question, o.number;
            """, connection))
        {
            optionCommand.Parameters.AddWithValue("@pollId", pollId);
            await using MySqlDataReader reader = await optionCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                int question = reader.GetInt32(0);
                if (!options.TryGetValue(question, out List<PollOption>? list))
                {
                    options[question] = list = [];
                }

                list.Add(new PollOption(reader.GetInt32(1), reader.GetString(2), reader.GetString(3) == "1"));
            }
        }

        var questions = new List<PollQuestion>();
        await using var command = new MySqlCommand(
            "SELECT id, number, type, content, selection_min, selection_max FROM polls_questions WHERE ref_poll = @pollId ORDER BY number;", connection);
        command.Parameters.AddWithValue("@pollId", pollId);
        await using MySqlDataReader questionReader = await command.ExecuteReaderAsync(cancellationToken);
        while (await questionReader.ReadAsync(cancellationToken))
        {
            int id = questionReader.GetInt32(0);
            questions.Add(new PollQuestion(
                id, questionReader.GetInt32(1),
                questionReader.GetString(2) switch { "multiple" => PollQuestionType.Multiple, "text" => PollQuestionType.Text, _ => PollQuestionType.Single },
                questionReader.GetString(3), questionReader.GetInt32(4), questionReader.GetInt32(5),
                options.TryGetValue(id, out List<PollOption>? found) ? found : []));
        }

        return questions;
    }

    public async Task<bool> CompleteAsync(
        int userId, int pollId, IReadOnlyList<PollAnswer> answers, int questionsTotal, bool rewardGiven, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var finished = new MySqlCommand(
            """
            INSERT IGNORE INTO users_answered_polls (ref_user, ref_poll, correct_answers, questions_total, reward_given)
            VALUES (@userId, @pollId, @correct, @total, @reward);
            """, connection, transaction))
        {
            finished.Parameters.AddWithValue("@userId", userId);
            finished.Parameters.AddWithValue("@pollId", pollId);
            finished.Parameters.AddWithValue("@correct", answers.Count(a => a.IsCorrect));
            finished.Parameters.AddWithValue("@total", questionsTotal);
            finished.Parameters.AddWithValue("@reward", rewardGiven ? "1" : "0");
            if (await finished.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }
        }

        foreach (PollAnswer answer in answers)
        {
            await using var insert = new MySqlCommand(
                "INSERT INTO users_poll_answers (ref_user, ref_question, answer, is_correct) VALUES (@userId, @question, @answer, @correct);",
                connection, transaction);
            insert.Parameters.AddWithValue("@userId", userId);
            insert.Parameters.AddWithValue("@question", answer.QuestionId);
            insert.Parameters.AddWithValue("@answer", answer.Answer);
            insert.Parameters.AddWithValue("@correct", answer.IsCorrect ? "1" : "0");
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }
}
